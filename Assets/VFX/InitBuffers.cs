using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public class InitBuffers : MonoBehaviour
{
    [SerializeField] private VisualEffect _effect;
    private GraphicsBuffer _buffer;
    private readonly int bufferProperty = Shader.PropertyToID("PCVelocity");
    private int pointCount = 3000;

    // Start is called before the first frame update
    void Start()
    {
        _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pointCount, Marshal.SizeOf(new Vector3()));

        if (_effect != null)
        {
            _effect.SetGraphicsBuffer(bufferProperty, _buffer);
        }

        Vector3[] points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            points[i] = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        }
        _buffer.SetData(points);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

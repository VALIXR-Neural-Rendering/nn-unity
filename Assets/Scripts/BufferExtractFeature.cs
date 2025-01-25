using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BufferExtractFeature : ScriptableRendererFeature
{
    // Exposed Settings
    public RenderPassEvent bufferPassEvent = RenderPassEvent.AfterRenderingOpaques;
    public LayerMask bufferMask = 1;
    public bool showInSceneView = false;

    // Feature Methods
    private BufferPass m_BufferPass;

    public override void Create()
    {
        m_BufferPass = new BufferPass(bufferMask, name + "_buffer");
        m_BufferPass.renderPassEvent = bufferPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return; // Ignore feature for editor/inspector previews & asset thumbnails
        if (!showInSceneView && cameraType == CameraType.SceneView) return;
        renderer.EnqueuePass(m_BufferPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_BufferPass?.Dispose();
    }
}
using UnityEngine.Rendering.Universal;
using UnityEngine;
using Unity.Sentis;

[System.Serializable]
public class Settings
{
    public bool showInSceneView = true;
    public RenderPassEvent _event = RenderPassEvent.AfterRenderingOpaques;

    [Header("Draw Renderers Settings")]
    public LayerMask layerMask = 1;
    public LayerMask bufferMask = 1;

    [Header("Blit Settings")]
    public Material blitMaterial;
    public Material compositeMaterial;
    public bool enableCustomBuffer = false;

    [Header("Neural Network Settings")]
    public ModelAsset estimationModel;
}
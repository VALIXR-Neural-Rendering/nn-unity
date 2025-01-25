using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NeuralPPFeature : ScriptableRendererFeature
{
    // Exposed Settings
    public Settings settings = new Settings();

    // Feature Methods
    private SceneColorPass m_SceneColorPass;
    private SceneDepthPass m_SceneDepthPass;
    private OverlayColorPass m_RenderPass;
    private OverlayDepthPass m_DepthPass;
    private NeuralPPPass m_NeuralPass;
    private DepthCompositingPass m_DepthCompositingPass;
    //private BufferPass m_BufferPass;      // For custom buffer loading

    public override void Create()
    {
        m_SceneColorPass = new SceneColorPass(settings, name + "_scenecol");
        m_SceneColorPass.renderPassEvent = settings._event;

        m_SceneDepthPass = new SceneDepthPass(settings, name + "_scenedepth");
        m_SceneDepthPass.renderPassEvent = settings._event;

        m_RenderPass = new OverlayColorPass(settings, name+"_color");
        m_RenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        m_DepthPass = new OverlayDepthPass(settings, name + "_depth");
        m_DepthPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        m_NeuralPass = new NeuralPPPass(settings, name + "_neural");
        m_NeuralPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        m_DepthCompositingPass = new DepthCompositingPass(settings, name + "_depthcomposit");
        m_DepthCompositingPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        //m_BufferPass = new BufferPass(settings, name + "_buffer");
        //m_BufferPass.renderPassEvent = settings._event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return; // Ignore feature for editor/inspector previews & asset thumbnails
        if (!settings.showInSceneView && cameraType == CameraType.SceneView) return;
        //renderer.EnqueuePass(m_BufferPass);
        renderer.EnqueuePass(m_SceneColorPass);
        renderer.EnqueuePass(m_SceneDepthPass);
        renderer.EnqueuePass(m_RenderPass);
        renderer.EnqueuePass(m_DepthPass);
        renderer.EnqueuePass(m_NeuralPass);
        renderer.EnqueuePass(m_DepthCompositingPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_RenderPass?.Dispose();
        m_DepthPass?.Dispose();
        m_SceneColorPass?.Dispose();
        m_SceneDepthPass?.Dispose();
        m_NeuralPass?.Dispose();
        m_DepthCompositingPass?.Dispose();
        //m_BufferPass?.Dispose();
    }
}
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections.Generic;

// This render pass performs the depth compositing of the non-overlay
// and the post-processed overlay layer and outputs to a RenderTexture
class DepthCompositingPass : ScriptableRenderPass
{
    private Settings settings;
    private ProfilingSampler _profilingSampler;
    private Material dcMaterial;
    private RTHandle ovColor, ovDepth, nonOvColor, nonOvDepth, rtOut, rtCam;
    private readonly string ovColID = "_NeuralPPTexture", ovDepthID = "_OvDepthTexture", nonOvColID = "_NonOvColorTexture", nonOvDepthID = "_NonOvDepthTexture", outID = "_OutputTexture";

    public DepthCompositingPass(Settings settings, string name)
    {
        this.settings = settings;
        dcMaterial = settings.compositeMaterial;
        _profilingSampler = new ProfilingSampler(name);

        ovColor = RTHandles.Alloc(ovColID, name: ovColID);
        ovDepth = RTHandles.Alloc(ovDepthID, name: ovDepthID);
        nonOvColor = RTHandles.Alloc(nonOvColID, name: nonOvColID);
        nonOvDepth = RTHandles.Alloc(nonOvDepthID, name: nonOvDepthID);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        // Set up custom color and depth target buffer (to render objects into)
        RenderingUtils.ReAllocateIfNeeded(ref rtOut, desc, name: outID);

        // Setup camera target
        rtCam = renderingData.cameraData.renderer.cameraColorTargetHandle;

        ConfigureTarget(rtOut);
        ConfigureClear(ClearFlag.All, new Color(0, 0, 0, 0));
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            // Empty command buffer run for correct scope title in the profiler
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            if (dcMaterial == null)
            {
                Debug.LogError("Material uninitialized.");
                return;
            }

            Blitter.BlitTexture(cmd, rtOut, rtOut, dcMaterial, 0);
            cmd.SetGlobalTexture(outID, rtOut);

            // Also blit to screen
            Blitter.BlitCameraTexture(cmd, rtOut, rtCam);
        }
        // Execute Command Buffer one last time and release it
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose()
    {
        ovColor?.Release();
        ovDepth?.Release();
        nonOvColor?.Release();
        nonOvDepth?.Release();
    }
}
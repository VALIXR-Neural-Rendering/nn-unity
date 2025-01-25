using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections.Generic;

// This render pass renders skybox and the non-overlay layers to a RenderTexture
class SceneColorPass : ScriptableRenderPass
{
    private Settings settings;
    private FilteringSettings filteringSettings;
    private ProfilingSampler _profilingSampler;
    private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();
    private RTHandle rtColor, rtDepth;
    private readonly string colID = "_NonOvColorTexture";
    private DrawSkyboxPass skyboxPass;

    public SceneColorPass(Settings settings, string name)
    {
        this.settings = settings;
        LayerMask allExceptLayer = (LayerMask) (0xFFFFFFFF ^ ((1 << settings.layerMask)));
        filteringSettings = new FilteringSettings(RenderQueueRange.all, ~(settings.layerMask | settings.bufferMask));

        // Use default tags
        shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));
        shaderTagsList.Add(new ShaderTagId("UniversalForward"));
        shaderTagsList.Add(new ShaderTagId("UniversalForwardOnly"));


        _profilingSampler = new ProfilingSampler(name);
        skyboxPass = new DrawSkyboxPass(settings._event);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        // Set up custom color and depth target buffer (to render objects into)
        RenderingUtils.ReAllocateIfNeeded(ref rtColor, desc, name: colID);

        // Using camera's depth target (that way we can ZTest with scene objects still)
        rtDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;

        ConfigureTarget(rtColor, rtDepth);
        ConfigureClear(ClearFlag.Color, new Color(0, 0, 0, 0));
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            // Empty command buffer run for correct scope title in the profiler
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            skyboxPass.Execute(context, ref renderingData);
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            // Pass our custom target to shaders as a Global Texture reference
            cmd.SetGlobalTexture(colID, rtColor);
        }
        // Execute Command Buffer one last time and release it
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose()
    {
        rtColor?.Release();
        rtDepth?.Release();
    }
}
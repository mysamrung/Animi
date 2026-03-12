using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using static WaterRenderPass;

public class WaterEffectRenderPass : ScriptableRenderPass {

    [System.Serializable]
    public class Setting {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader rippleShader;
        public int resolution;
        public float fadeRange;
        public float renderRange;
    }

    internal class WaterEffectRenderPassPassData {
        internal RendererListHandle rendererList;
        internal TextureHandle prevFrameTextureHandle;
        internal TextureHandle curFrameTextureHandle;
        internal Material material;
    }

    public class WaterEffectResultData : ContextItem {
        public Vector4 rendererCoords;

        public override void Reset() {
            rendererCoords = Vector4.zero;
        }
    }

    private const string RenderTag = "WaterEffect";
    private const string PassName = "WaterEffect Pass";

    public static readonly int _WaterDynamicEffectsBufferID = Shader.PropertyToID("_WaterDynamicEffectsBuffer");
    public static readonly int _WaterDynamicEffectsCoordsID = Shader.PropertyToID("_WaterDynamicEffectsCoords");
    public static readonly int _PrevFrameTextureID = Shader.PropertyToID("_PrevFrameTexture");

    private readonly RenderQueueRange _renderQueueRange = RenderQueueRange.all;
    private readonly ShaderTagId _shaderTagId = new ShaderTagId(RenderTag);

    private FilteringSettings _filteringSettings;

    private Vector4 rendererCoords;
    private Vector3 centerPosition;

    private Matrix4x4 projection { set; get; }
    private Matrix4x4 view { set; get; }

    private static readonly Quaternion viewRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
    private static readonly Vector3 viewScale = new Vector3(1, 1, -1);
    private static Rect viewportRect;


    private Setting setting;

    private RTHandle previousFrameWaveRT;
    private Material rippleMaterial;

    public WaterEffectRenderPass(Setting setting)
    {
        Dispose();

        _filteringSettings = new FilteringSettings(_renderQueueRange);
        renderPassEvent = setting.renderPassEvent;
        this.setting = setting;

        if (rippleMaterial == null && setting.rippleShader != null)
            rippleMaterial = new Material(setting.rippleShader);
    }

    public void Dispose() {
        if(previousFrameWaveRT != null ) 
            previousFrameWaveRT.Release();

        if(rippleMaterial != null)
            CoreUtils.Destroy(rippleMaterial);
    }

    private static Vector3 StabilizeProjection(Vector3 pos, float texelSize)
    {
        float Snap(float coord, float cellSize) => Mathf.FloorToInt(coord / cellSize) * (cellSize) + (cellSize * 0.5f);
        return new Vector3(Snap(pos.x, texelSize), Snap(pos.y, texelSize), Snap(pos.z, texelSize));
    }

    private void SetupProjection(RasterCommandBuffer cmd, Camera camera)
    {
        centerPosition = camera.transform.position + (camera.transform.forward * (setting.renderRange - setting.fadeRange));

        centerPosition = StabilizeProjection(centerPosition, (setting.renderRange * 2f) / setting.resolution);

        var frustumHeight = setting.renderRange * 2f;
        centerPosition += (Vector3.up * frustumHeight * 0.5f);

        projection = Matrix4x4.Ortho(-setting.renderRange, setting.renderRange, -setting.renderRange, setting.renderRange, 0.03f, frustumHeight);

        view = Matrix4x4.TRS(centerPosition, viewRotation, viewScale).inverse;

        cmd.SetViewProjectionMatrices(view, projection);

        //viewportRect.width = setting.resolution;
        //viewportRect.height = setting.resolution;
        //cmd.SetViewport(viewportRect);

        rendererCoords.x = centerPosition.x - setting.renderRange;
        rendererCoords.y = centerPosition.z - setting.renderRange;
        rendererCoords.z = setting.renderRange * 2f;
        rendererCoords.w = 1f; //Enable in shaderx
    }


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        
        //  RenderingData???????AContextContainer???????????K?v???f?[?^???B??????????????
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        WaterEffectResultData waterEffectData = frameData.Create<WaterEffectResultData>();

        RenderTextureDescriptor textureProperties = new RenderTextureDescriptor(setting.resolution, setting.resolution, RenderTextureFormat.RFloat);
        TextureHandle currentFrametexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties, "_WaterDynamicEffects Texture", false);
        TextureHandle temptexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties, "_Temp Texture", true);
        TextureHandle previousFrameWavTextureHandle;

        var prevViewMatrix = cameraData.GetViewMatrix();
        var prevProjMatrix = cameraData.GetProjectionMatrix();

        if(previousFrameWaveRT == null) {
            previousFrameWaveRT = RTHandles.Alloc(
                width: setting.resolution,
                height: setting.resolution,
                colorFormat: GraphicsFormat.R32_SFloat,
                useDynamicScale: false,
                name: "PrevFrame Wave"
            );

            previousFrameWavTextureHandle = renderGraph.ImportTexture(previousFrameWaveRT);

            using (var builder = renderGraph.AddRasterRenderPass<WaterEffectRenderPassPassData>(PassName, out var passData)) {
                builder.SetRenderAttachment(previousFrameWavTextureHandle, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((WaterEffectRenderPassPassData data, RasterGraphContext context) => {
                    using (new ProfilingScope(context.cmd, profilingSampler)) {
                        context.cmd.ClearRenderTarget(false, true, Color.black);
                    }
                });
            }
        }
        else {

            previousFrameWavTextureHandle = renderGraph.ImportTexture(previousFrameWaveRT);
        }
        
        using (var builder = renderGraph.AddRasterRenderPass<WaterEffectRenderPassPassData>(PassName, out var passData)) {
            passData.prevFrameTextureHandle = previousFrameWavTextureHandle;
            passData.material = rippleMaterial;

            builder.UseTexture(previousFrameWavTextureHandle, AccessFlags.Read);
            
            builder.SetRenderAttachment(temptexture, 0, AccessFlags.Write);
            
            builder.AllowPassCulling(false);

            SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
            DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(_shaderTagId, renderingData, cameraData, lightData, sortingCriteria);

            RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawSettings, _filteringSettings);
            passData.rendererList = renderGraph.CreateRendererList(rendererListParams);

            builder.UseRendererList(passData.rendererList);

            builder.SetRenderFunc((WaterEffectRenderPassPassData data, RasterGraphContext context) => {
                using (new ProfilingScope(context.cmd, profilingSampler)) {
                    context.cmd.ClearRenderTarget(false, true, Color.black);
                    //Blitter.BlitTexture(context.cmd, data.prevFrameTextureHandle, new Vector4(1, 1, 0, 0), passData.material, 1);

                    SetupProjection(context.cmd, cameraData.camera);
                    context.cmd.DrawRendererList(data.rendererList);

                    waterEffectData.rendererCoords = rendererCoords;
                }
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<WaterEffectRenderPassPassData>(PassName, out var passData)) {
            passData.prevFrameTextureHandle = previousFrameWavTextureHandle;
            passData.curFrameTextureHandle = temptexture;
            passData.material = rippleMaterial;

            builder.UseTexture(previousFrameWavTextureHandle, AccessFlags.Read);
            builder.UseTexture(temptexture, AccessFlags.Read);

            builder.SetRenderAttachment(currentFrametexture, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetGlobalTextureAfterPass(temptexture, _WaterDynamicEffectsBufferID);
            builder.SetRenderFunc((WaterEffectRenderPassPassData data, RasterGraphContext context) => {
                using (new ProfilingScope(context.cmd, profilingSampler)) {
                    rippleMaterial.SetTexture(_PrevFrameTextureID, data.prevFrameTextureHandle);
                    Blitter.BlitTexture(context.cmd, data.curFrameTextureHandle, new Vector4(1, 1, 0, 0), passData.material, 0);
                }
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<WaterEffectRenderPassPassData>(PassName, out var passData)) {
            passData.curFrameTextureHandle = currentFrametexture;
            builder.UseTexture(currentFrametexture, AccessFlags.Read);

            builder.SetRenderAttachment(previousFrameWavTextureHandle, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((WaterEffectRenderPassPassData data, RasterGraphContext context) => {
                using (new ProfilingScope(context.cmd, profilingSampler)) {
                    context.cmd.ClearRenderTarget(false, true, Color.black);
                    Blitter.BlitTexture(context.cmd, data.curFrameTextureHandle, new Vector4(1, 1, 0, 0), 0, false);
                }
            });
        }
    }
}

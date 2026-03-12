using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class LightVolumeRenderPass : ScriptableRenderPass {
    private const string PassTag = "LightVolume RenderGraph";

    private Material _material;
    private LightVolume _lightVolume;

    public LightVolumeRenderPass(RenderPassEvent passEvent, Shader shader) {
        renderPassEvent = passEvent;

        if (shader != null)
            _material = CoreUtils.CreateEngineMaterial(shader);
    }

    public void Dispose() {
        if(_material != null ) 
            CoreUtils.Destroy(_material);
    }

    class PassData {
        public Material material;
        public TextureHandle source;
        public TextureHandle temp1;
        public TextureHandle temp2;
        public bool useBlurSmoothing;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph,
        ContextContainer frameData) {
        if (_material == null)
            return;

        var cameraData = frameData.Get<UniversalCameraData>();
        if (!cameraData.postProcessEnabled)
            return;

        var volumeStack = VolumeManager.instance.stack;
        _lightVolume = volumeStack.GetComponent<LightVolume>();

        if (_lightVolume == null || !_lightVolume.active || _lightVolume.Intensity.value <= 0)
            return;

        var resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle source = resourceData.activeColorTexture;

        // ---------- Create temporary textures ----------

        TextureDesc desc = source.GetDescriptor(renderGraph);
        desc.width = Mathf.FloorToInt(desc.width * _lightVolume.DownScale.value);
        desc.height = Mathf.FloorToInt(desc.height * _lightVolume.DownScale.value);
        desc.msaaSamples = MSAASamples.None;
        desc.depthBufferBits = 0;
        desc.clearBuffer = true;
        desc.clearColor = Color.clear;
        desc.name = "LightVolumeTemp1";

        TextureHandle temp1 = renderGraph.CreateTexture(desc);

        var descBlur = source.GetDescriptor(renderGraph);
        descBlur.width = Mathf.FloorToInt(desc.width * _lightVolume.BlurDownScale.value);
        descBlur.height = Mathf.FloorToInt(desc.height * _lightVolume.BlurDownScale.value);
        descBlur.msaaSamples = MSAASamples.None;
        descBlur.depthBufferBits = 0;
        descBlur.clearColor = Color.clear;
        descBlur.clearBuffer = true;

        TextureHandle temp2 = renderGraph.CreateTexture(descBlur);

        // ---------- Matrices ----------
        Matrix4x4 view = cameraData.camera.worldToCameraMatrix;
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(
            cameraData.camera.projectionMatrix, true);

        Matrix4x4 vp = proj * view;

        // ---------- Light Direction ----------
        Vector3 lightDirScreen = Vector3.zero;

        if (RenderSettings.sun != null) {
            Vector3 lightDirWorld = RenderSettings.sun.transform.forward;
            Vector3 pointInLightDir =
                cameraData.camera.transform.position + lightDirWorld;

            lightDirScreen =
                cameraData.camera.WorldToViewportPoint(pointInLightDir);

            lightDirScreen =
                lightDirScreen.normalized *
                Mathf.Sign(lightDirScreen.z);

            lightDirScreen *= -1;
        }

        // ============================================================
        // PASS
        // ============================================================

        // -------- Set material params --------

        _material.SetColor("_ColorMin", _lightVolume.MinColor.value);
        _material.SetColor("_ColorMax", _lightVolume.MaxColor.value);
        _material.SetFloat("_Extinction", _lightVolume.Extinction.value);
        _material.SetFloat("_Intensity", _lightVolume.Intensity.value);
        _material.SetFloat("_IntensityAmp", _lightVolume.IntensityAmplify.value);
        _material.SetFloat("_MaxIntensity", _lightVolume.MaxIntensity.value);
        _material.SetInt("_StepCount", _lightVolume.StepCount.value);
        _material.SetMatrix("_CameraVP", vp);
        _material.SetMatrix("_CameraIVP", vp.inverse);
        _material.SetVector("_LightDirectionScreenSpace", lightDirScreen);
        _material.SetFloat("_BlurOffset", _lightVolume.BlurOffset.value);
        _material.SetInt("_SampleCount", _lightVolume.SampleCount.value);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassTag, out var passData)) {
            passData.material = _material;
            passData.source = source;

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(temp1, 0, AccessFlags.Write);
            builder.UseAllGlobalTextures(true);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) => {
                var cmd = ctx.cmd;
                var mat = data.material;

                Blitter.BlitTexture(cmd, data.source, new Vector4(1, 1, 0, 0), mat, 0);
            });
        }

        if (_lightVolume.SampleCount.value > 0) {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassTag, out var passData)) {
                passData.material = _material;
                passData.source = source;
                passData.temp1 = temp1;
                passData.temp2 = temp2;

                builder.UseTexture(temp1, AccessFlags.Read);

                builder.SetRenderAttachment(temp2, 0, AccessFlags.WriteAll);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) => {
                    var cmd = ctx.cmd;
                    var mat = data.material;

                    Blitter.BlitTexture(cmd, data.temp1, new Vector4(1, 1, 0, 0), mat, 1);
                });
            }
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassTag, out var passData)) {
            passData.material = _material;
            passData.source = source;
            passData.temp1 = temp1;
            passData.temp2 = temp2;

            builder.UseTexture(temp2, AccessFlags.Read);

            builder.SetRenderAttachment(source, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) => {
                var cmd = ctx.cmd;
                var mat = data.material;

                Blitter.BlitTexture(cmd, data.temp2, new Vector4(1, 1, 0, 0), mat, 2);
            });
        }
    }
}
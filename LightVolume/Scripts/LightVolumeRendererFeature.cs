using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightVolumeRendererFeature : ScriptableRendererFeature {

    [System.Serializable]
    public class Setting {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    [SerializeField]
    private Setting _setting = new Setting();

    private LightVolumeRenderPass _lightVolumeRenderPass;

    public override void Create() {
        this.name = "HeightFog";

        // •`‰æƒpƒX‚Ì¶¬
        _lightVolumeRenderPass = new LightVolumeRenderPass(
            _setting.renderPassEvent,
            _setting.shader
        );
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(_lightVolumeRenderPass);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // ‚±‚±‚ÅPass‚Ì”jŠüˆ—‚ğŒÄ‚Ño‚·
            _lightVolumeRenderPass.Dispose();
        }
    }
}

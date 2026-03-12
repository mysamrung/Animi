using UnityEngine;
using UnityEngine.Rendering;
public class LightVolume : VolumeComponent {

    [Header("Light Volue")]
    public ColorParameter MinColor = new ColorParameter(new Color(0, 0, 0, 0));
    public ColorParameter MaxColor = new ColorParameter(new Color(0, 0, 0, 0));
    public FloatParameter Extinction = new FloatParameter(0.0f);
    public FloatParameter Intensity = new FloatParameter(0.0f);
    public FloatParameter IntensityAmplify = new FloatParameter(1.0f);
    public FloatParameter MaxIntensity = new FloatParameter(1.0f);
    public ClampedIntParameter StepCount = new ClampedIntParameter(100, 1, 500);
    public FloatParameter DownScale = new FloatParameter(1.0f);

    [Header("Blur")]
    public MinIntParameter SampleCount = new MinIntParameter(4, 0);
    public FloatParameter BlurOffset = new FloatParameter(0.05f);
    public FloatParameter BlurDownScale = new FloatParameter(1.0f);
}

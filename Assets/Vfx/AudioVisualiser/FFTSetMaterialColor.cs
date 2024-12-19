using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FFTSetMaterialColor : MonoBehaviour
{
    public FrequencyBandAnalyser _FFT;
    public FrequencyBandAnalyser.Bands _FreqBands = FrequencyBandAnalyser.Bands.Eight;
    public int _FrequencyBandIndex = 0;

    public string _ColorName = "_EmissionColor";
    public Color _Col;
    public float _StrengthScalar = 1;

    MeshRenderer _MeshRenderer;

    private void Start()
    {
        _MeshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        // calculate strength based on the frequency
        float bandValue = _FFT.GetBandValue(_FrequencyBandIndex, _FreqBands);
        float strength = (bandValue + 0.001f) * _StrengthScalar;

        // Send strength and color to shader
        _MeshRenderer.material.SetFloat("_Strength", strength);
        _MeshRenderer.material.SetColor("_BaseColor", _Col);
    }
}

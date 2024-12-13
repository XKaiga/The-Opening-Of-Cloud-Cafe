using UnityEngine;
using UnityEngine.InputSystem.Android;

public class FFTObjectArray : MonoBehaviour
{
    public enum CircleDraw
    {
        Full,
        UpperHalf,
        LeftHalf,
        RightHalf,
        BottomHalf,
        FirstQuarter,
        SecondQuarter,
        ThirdQuarter,
        ForthQuarter
    }

    [Header("FFT")]
    private FrequencyBandAnalyser _FFT;
    public FrequencyBandAnalyser.Bands _FreqBands = FrequencyBandAnalyser.Bands.Eight;
    GameObject[] _FFTGameObjects;
    public GameObject _ObjectToSpawn;

    [Header("Object Array")]
    public CircleDraw circleDraw = CircleDraw.SecondQuarter;
    public float _Spacing = 1;
    public Vector3 _ScaleStrength = Vector3.up;
    Vector3 _BaseScale;

    [Header("Emission col")]
    public Color _EmissionCol;
    public float _EmissionStrength = 1;

    private void Start()
    {
        _FFT = FindObjectOfType<FrequencyBandAnalyser>();

        _FFTGameObjects = new GameObject[(int)_FreqBands];
        _BaseScale = _ObjectToSpawn.transform.localScale;

        float angleSpacing = 0f;
        float angleOffset = 0f;

        switch (circleDraw)
        {
            case CircleDraw.Full:
                angleSpacing = (2f * Mathf.PI) / (int)_FreqBands; // Full circle
                angleOffset = 0;
                break;
            case CircleDraw.UpperHalf:
                angleSpacing = Mathf.PI / (int)_FreqBands; // Right half (-π/2 to π/2)
                angleOffset = -Mathf.PI / 2f;
                break;
            case CircleDraw.BottomHalf:
                angleSpacing = Mathf.PI / (int)_FreqBands; // Left half (π/2 to 3π/2)
                angleOffset = Mathf.PI / 2f;
                break;
            case CircleDraw.LeftHalf:
                angleSpacing = Mathf.PI / (int)_FreqBands; // Bottom half (π to 2π)
                angleOffset = Mathf.PI;
                break;
            case CircleDraw.RightHalf:
                angleSpacing = Mathf.PI / (int)_FreqBands; // Upper half
                angleOffset = 0;
                break;
            case CircleDraw.FirstQuarter:
                angleSpacing = (Mathf.PI / 2f) / (int)_FreqBands; // First quarter
                angleOffset = 0;
                break;
            case CircleDraw.SecondQuarter:
                angleSpacing = (Mathf.PI / 2f) / (int)_FreqBands; // Second quarter
                angleOffset = 3 * Mathf.PI / 2f;
                break;
            case CircleDraw.ThirdQuarter:
                angleSpacing = (Mathf.PI / 2f) / (int)_FreqBands; // Third quarter
                angleOffset = Mathf.PI;
                break;
            case CircleDraw.ForthQuarter:
                angleSpacing = (Mathf.PI / 2f) / (int)_FreqBands; // Fourth quarter
                angleOffset = Mathf.PI / 2f;
                break;
            default:
                break;
        }


        // Generate 1/4 Circle
        for (int i = 0; i < _FFTGameObjects.Length; i++)
        {
            float angle = (i * angleSpacing) + angleOffset;
            float x = Mathf.Sin(angle) * _Spacing;
            float y = Mathf.Cos(angle) * _Spacing;

            GameObject newFFTObject = Instantiate(_ObjectToSpawn);
            newFFTObject.transform.SetParent(transform);
            newFFTObject.transform.localPosition = new Vector3(x, y, 0);

            // Rotate Object
            newFFTObject.transform.LookAt(transform.position);
            newFFTObject.transform.localRotation *= Quaternion.Euler(-90, 0, 0);

            // Add FFTSetMaterialColor Comp.
            FFTSetMaterialColor setMaterialCol = newFFTObject.AddComponent<FFTSetMaterialColor>();
            setMaterialCol._Col = _EmissionCol;
            setMaterialCol._StrengthScalar = _EmissionStrength;
            setMaterialCol._FFT = _FFT;
            setMaterialCol._FreqBands = _FreqBands;
            setMaterialCol._FrequencyBandIndex = i;

            _FFTGameObjects[i] = newFFTObject;
        }
    }

    private void Update()
    {
        for (int i = 0; i < _FFTGameObjects.Length; i++)
        {
            _FFTGameObjects[i].transform.localScale = _BaseScale + (_ScaleStrength * _FFT.GetBandValue(i, _FreqBands));
        }
    }
}

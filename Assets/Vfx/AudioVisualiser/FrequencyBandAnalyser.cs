using UnityEngine;

public class FrequencyBandAnalyser : MonoBehaviour
{
    public enum Bands
    {
        Eight = 8,
        SixtyFour = 64,
    }

    private AudioSource _AudioSource;

    [SerializeField] private float _SmoothDownRate = 0;
    [SerializeField] private float _Scalar = 1;

    [SerializeField] private bool _DrawGizmos = false;

    private float[] _Samples;
    private float[] _SampleBuffer;

    private int _FrequencyBins = 512;
    [HideInInspector]
    public float[] _FreqBands8;
    [HideInInspector]
    public float[] _FreqBands64;

    void Start()
    {
        _AudioSource = GetComponent<AudioSource>();

        _FreqBands8 = new float[8];
        _FreqBands64 = new float[64];
        _Samples = new float[_FrequencyBins];
        _SampleBuffer = new float[_FrequencyBins];
    }


    void UpdateFreqBands8()
    {
        // 22050 / 512 = 43hz per sample

        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == 7)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += _Samples[count] * (count + 1);
                count++;
            }

            average /= count;
            _FreqBands8[i] = average;
        }
    }

    void UpdateFreqBands64()
    {
        // 22050 / 512 = 43hz per sample

        int count = 0;
        int sampleCount = 1;
        int power = 0;

        for (int i = 0; i < 64; i++)
        {
            float average = 0;

            if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56)
            {
                power++;
                sampleCount = (int)Mathf.Pow(2, power);
                if (power == 3)
                    sampleCount -= 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += _Samples[count] * (count + 1);
                count++;
            }

            average /= count;
            _FreqBands64[i] = average;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //---   POPULATE SAMPLES
        _AudioSource.GetSpectrumData(_SampleBuffer, 0, FFTWindow.BlackmanHarris);

        for (int i = 0; i < _Samples.Length; i++)
        {
            if (_SampleBuffer[i] > _Samples[i])
                _Samples[i] = _SampleBuffer[i];
            else
                _Samples[i] = Mathf.Lerp(_Samples[i], _SampleBuffer[i], Time.deltaTime * _SmoothDownRate);
        }

        UpdateFreqBands8();
        UpdateFreqBands64();
    }

    public float GetBandValue(int index, Bands bands)
    {
        if (bands == Bands.Eight)
        {
            return _FreqBands8[index];
        }
        
        return _FreqBands64[index];
    }

    private void OnDrawGizmos()
    {
        if (_DrawGizmos && Application.isPlaying)
        {
            float linearXPos0;
            float linearXPos1;



            for (int i = 1; i < 63; i++)
            {
                linearXPos0 = (float)(i - 1) / 63f;
                linearXPos1 = (float)(i) / 63f;

                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position + new Vector3(linearXPos0 * 4, _FreqBands64[i - 1] * _Scalar, 0), transform.position + new Vector3(linearXPos1 * 4, _FreqBands64[i] * _Scalar, 0));
            }
        }
    }
}

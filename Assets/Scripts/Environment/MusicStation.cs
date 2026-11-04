using UnityEngine;

public enum InstrumentType
{
    AcousticGuitar,
    BassGuitar,
    ElectricGuitar,
    Warhorn,
    AmpStatic
}

[RequireComponent(typeof(AudioSource))]
public class InteractiveAudioObject : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private InstrumentType instrumentType;
    [SerializeField] private float frequency = 220f; // Pitch for instruments (Bass: 110, Horn: 140, Guitar: 220)
    [SerializeField] private float duration = 2.0f;
    [Range(0f, 1f)] [SerializeField] private float volume = 0.75f;

    private const int SampleRate = 44100;
    private AudioClip generatedClip;
    private AudioSource audioSource;
    private bool isToggledOn = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 3D positional sound

        if (instrumentType == InstrumentType.AmpStatic)
        {
            audioSource.loop = true;
        }

        generatedClip = GenerateAudioClip();
        audioSource.clip = generatedClip;
    }

    private void OnMouseDown()
    {
        Interact();
    }

    // Call this method from custom interaction scripts
    public void Interact()
    {
        // Record the use in StationTracker through the MusicStation parent
        StationInteractable station = GetComponentInParent<StationInteractable>();
        if (station != null)
        {
            station.OnPlayerInteracted();
        }

        if (instrumentType == InstrumentType.AmpStatic)
        {
            isToggledOn = !isToggledOn;
            if (isToggledOn)
            {
                audioSource.volume = volume;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
        else
        {
            audioSource.PlayOneShot(generatedClip, volume);
        }
    }

    private AudioClip GenerateAudioClip()
    {
        int sampleCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float lastAmpSample = 0f; // Filter state for amp noise

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float phase = 2f * Mathf.PI * frequency * t;
            float sample = 0f;

            switch (instrumentType)
            {
                case InstrumentType.AcousticGuitar:
                    float acousticEnvelope = Mathf.Min(1f, t / 0.002f) * Mathf.Exp(-4f * t);
                    float acousticWave = Mathf.Sin(phase) + 0.6f * Mathf.Sin(2f * phase)+ 0.4f * Mathf.Sin(3f * phase) + 0.2f * Mathf.Sin(4f * phase);
                    sample = (acousticWave / 2.2f) * acousticEnvelope;
                    break;

                case InstrumentType.BassGuitar:
                    float bassEnvelope = Mathf.Min(1f, t / 0.005f) * Mathf.Exp(-2.5f * t);
                    float bassWave = Mathf.Sin(phase) + 0.8f * Mathf.Sin(0.5f * phase) + 0.3f * Mathf.Sin(2f * phase);
                    sample = (bassWave / 2.1f) * bassEnvelope;
                    break;

                case InstrumentType.ElectricGuitar:
                    float electricEnvelope = Mathf.Min(1f, t / 0.003f) * Mathf.Exp(-1.8f * t);
                    float rawSignal = Mathf.Sin(phase) + 0.5f * Mathf.Sin(2f * phase) + 0.3f * Mathf.Sin(3f * phase);
                    sample = Mathf.Clamp(rawSignal * 3.0f, -1.0f, 1.0f) * electricEnvelope;
                    break;

                case InstrumentType.Warhorn:
                    float hornEnvelope = Mathf.SmoothStep(0f, 1f, t / 0.3f) * Mathf.SmoothStep(1f, 0f, (t - (duration - 0.4f)) / 0.4f);
                    float pitchSwell = 1.0f + 0.05f * Mathf.Exp(-8f * t);
                    float hornPhase = 2f * Mathf.PI * (frequency * pitchSwell) * t;
                    float hornWave = Mathf.Sin(hornPhase) + 0.7f * Mathf.Sin(2f * hornPhase) + 0.5f * Mathf.Sin(3f * hornPhase) + 0.4f * Mathf.Sin(4f * hornPhase) + 0.3f * Mathf.Sin(5f * hornPhase);
                    sample = (hornWave / 2.9f) * hornEnvelope;
                    break;

                case InstrumentType.AmpStatic:
                    float whiteNoise = Random.Range(-1.0f, 1.0f);
                    lastAmpSample = Mathf.Lerp(lastAmpSample, whiteNoise, 0.15f); // Low-pass warm filter
                    float mainsHum = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.15f; // 60Hz hum
                    sample = lastAmpSample + mainsHum;
                    break;
            }

            float fadeOut = Mathf.Clamp01((duration - t) / 0.05f);
            samples[i] = sample * fadeOut;
        }

        AudioClip clip = AudioClip.Create($"{instrumentType}_Clip", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

// Detects a basket. 
// Two thin invisible trigger boxes, one ABOVE
// the rim and one BELOW. A hoop only counts if it enters the top zone first and the
// bottom zone shortly after
 
//access number of hoops scored HoopScorer.TotalHoopsScored
public class HoopScorer : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Empty object at the centre of the ring.")]
    public Transform rimCenter;
    [Tooltip("Inner radius of the ring in metres.")]
    public float rimRadius = 0.2f;

    [Header("Zones")]
    [Tooltip("Distance of each zone from the rim plane. Must stay below half the ball diameter.")]
    public float zoneOffset = 0.11f;
    [Tooltip("Thickness of each zone. Must be thinner than the ball so it cannot skip one.")]
    public float zoneThickness = 0.06f;
    [Tooltip("Max seconds between touching the top and the bottom zone.")]
    public float maxPassTime = 1.0f;

    [Header("Audio Settings")]
    [Range(0f, 1f)][SerializeField] private float soundVolume = 0.75f;
    [SerializeField] private float dingFrequency = 880f; // Fixed sound pitch
    public static int TotalHoopsScored { get; private set; }
    public int Score { get; private set; }

    //Fired when a ball scores
    public event Action<Pickable> Scored;

    // Ball -> time it entered the top zone.
    readonly Dictionary<Pickable, float> topEntryTime = new Dictionary<Pickable, float>();

    private AudioSource audioSource;
    private AudioClip dingClip;
    private const int SampleRate = 44100;

    void Start()
    {
        if (rimCenter == null)
        {
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f; // 3D sound at hoop position
        audioSource.playOnAwake = false;

        // Pregenerate the ding clip at startup
        dingClip = GenerateDingClip(dingFrequency);

        CreateZone("TopZone", +zoneOffset, true);
        CreateZone("BottomZone", -zoneOffset, false);
    }

    void CreateZone(string zoneName, float yOffset, bool isTop)
    {
        var go = new GameObject(zoneName);
        go.transform.SetParent(transform, false);
        go.transform.position = rimCenter.position + Vector3.up * yOffset; // always world-up
        go.transform.rotation = Quaternion.identity;
        go.layer = 2;

        // Convert world-size to local-size
        Vector3 s = transform.lossyScale;
        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(rimRadius * 2f / s.x, zoneThickness / s.y, rimRadius * 2f / s.z);

        var relay = go.AddComponent<HoopZoneRelay>();
        relay.owner = this;
        relay.isTop = isTop;
    }

    public void ZoneEntered(bool isTop, Pickable ball)
    {
        if (isTop)
        {
            topEntryTime[ball] = Time.time;
            return;
        }

        // Bottom zone: only a score if the ball recently came through the top zone.
        if (topEntryTime.TryGetValue(ball, out float t) && Time.time - t <= maxPassTime)
        {
            topEntryTime.Remove(ball);
            Score++;
            TotalHoopsScored++;
            Debug.Log("Score = " + Score);

            PlayDingSound();
            Scored?.Invoke(ball);
        }
    }

    private void PlayDingSound()
    {
        if (dingClip != null)
        {
            audioSource.PlayOneShot(dingClip, soundVolume);
        }
    }

    //AI generated audio ding
    private AudioClip GenerateDingClip(float frequency)
    {
        float duration = 0.35f;
        int sampleCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float phase = 2f * Mathf.PI * frequency * t;

            // Warm bell tone using fundamental frequency + light octave harmonic
            float wave = Mathf.Sin(phase) + 0.3f * Mathf.Sin(2f * phase);

            // Fast attack with an exponential decay for a crisp metallic ding
            float envelope = Mathf.Min(1f, t / 0.002f) * Mathf.Exp(-6f * t);

            // Short fade-out at clip end to prevent clicking
            float fadeOut = Mathf.Clamp01((duration - t) / 0.03f);

            samples[i] = (wave / 1.3f) * envelope * fadeOut;
        }

        AudioClip clip = AudioClip.Create("ScoreDing", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void OnDrawGizmosSelected()
    {
        if (rimCenter == null) return;
        Vector3 size = new Vector3(rimRadius * 2f, zoneThickness, rimRadius * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(rimCenter.position + Vector3.up * zoneOffset, size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(rimCenter.position - Vector3.up * zoneOffset, size);
    }
}

// Tiny helper on each zone: sends trigger events to the HoopScorer.
public class HoopZoneRelay : MonoBehaviour
{
    [HideInInspector] public HoopScorer owner;
    [HideInInspector] public bool isTop;

    void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponentInParent<Pickable>();
        if (ball != null) owner.ZoneEntered(isTop, ball);
    }
}
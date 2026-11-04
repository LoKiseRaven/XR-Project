using TMPro;
using UnityEngine;

/*
    Chrono shown on the HUD. Put it on a TMP text of the HUD prefab.
    The time is kept in a static field, which survives scene changes (like SceneTransition),
    so each scene's HUD carries on from where the previous one stopped.
*/

public class HudTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    // Seconds since the game started, shared by every HUD
    public static float ElapsedTime { get; private set; }
    public static bool IsRunning { get; set; } = true;

    // Static fields aren't reset when Play Mode starts if Domain Reload is turned off, so reset them here
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetTimer()
    {
        ElapsedTime = 0f;
        IsRunning = true;
    }

    void Awake()
    {
        if (timerText == null)
        {
            timerText = GetComponent<TMP_Text>();
        }
    }

    void Update()
    {
        if (IsRunning)
        {
            ElapsedTime += Time.deltaTime;
        }

        int minutes = (int)(ElapsedTime / 60f);
        int seconds = (int)(ElapsedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}

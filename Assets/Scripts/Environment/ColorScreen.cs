using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorScreen : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TMP_Text instructionText;
    // Shows the grid texture, one pixel per cell
    [SerializeField] private RawImage gridImage;
    [SerializeField] private Image[] progressBoxes;
    [SerializeField] private Color emptyBoxColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color filledBoxColor = Color.white;

    [Header("Grid")]
    // 0, 1, 2 (red, green, blue), the same indexes the buttons send to SubmitAnswer
    [SerializeField] private Color[] colors = { Color.red, Color.green, Color.blue };
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    // How many more cells the dominant colour has compared to an even split (smaller = harder)
    [SerializeField] private int minMargin = 1;
    [SerializeField] private int maxMargin = 3;

    [Header("Task")]
    [SerializeField] private int answersToWin = 5;
    public UnityEvent onCompleted;

    // Light flashed green or red after each answer
    [Header("Feedback")]
    [SerializeField] private Light feedbackLight;
    [SerializeField] private Color goodAnswerColor = Color.green;
    [SerializeField] private Color badAnswerColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;

    private Coroutine flashRoutine;
    private Texture2D gridTexture;
    // Index of the colour that appears the most
    private int dominantColor;
    private int goodAnswers;
    private bool isCompleted;

    void Start()
    {
        // A tiny texture with one pixel per cell. Point filtering keeps the cells sharp once stretched on the screen
        gridTexture = new Texture2D(gridWidth, gridHeight);
        gridTexture.filterMode = FilterMode.Point;
        gridTexture.wrapMode = TextureWrapMode.Clamp;
        gridImage.texture = gridTexture;

        if (feedbackLight != null)
        {
            feedbackLight.enabled = false;
        }

        UpdateProgress();
        GenerateGrid();
    }

    // Called by the coloured buttons (Interactable onPressed), with the index of their colour (0, 1, 2 for red, green, blue)
    public void SubmitAnswer(int colorIndex)
    {
        if (isCompleted)
        {
            return;
        }

        bool isCorrect = colorIndex == dominantColor;
        if (isCorrect)
        {
            goodAnswers++;
        }
        else
        {
            goodAnswers = 0;
        }

        FlashLight(isCorrect);
        UpdateProgress();

        // Task completed: hide the grid, show the message and unlock the door
        if (goodAnswers >= answersToWin)
        {
            isCompleted = true;
            gridImage.gameObject.SetActive(false);
            instructionText.text = "Task completed. Door unlocked";
            Debug.Log("Color task completed");
            onCompleted.Invoke();
            return;
        }

        GenerateGrid();
    }

    void GenerateGrid()
    {
        int cellCount = gridWidth * gridHeight;

        int dominantCount = Mathf.CeilToInt(cellCount / 3f) + Random.Range(minMargin, maxMargin + 1);
        int remaining = cellCount - dominantCount;
        int secondCount = Random.Range(Mathf.Max(0, remaining - (dominantCount - 1)), dominantCount);
        int thirdCount = remaining - secondCount;

        // Random dominant colour, the two others are the next ones in the array
        dominantColor = Random.Range(0, colors.Length);
        int secondColor = (dominantColor + 1) % colors.Length;
        int thirdColor = (dominantColor + 2) % colors.Length;

        // Fill the cells in order (dominant, then second, then third colour)
        int[] cells = new int[cellCount];
        for (int i = 0; i < cellCount; i++)
        {
            if (i < dominantCount) cells[i] = dominantColor;
            else if (i < dominantCount + secondCount) cells[i] = secondColor;
            else cells[i] = thirdColor;
        }

        // Shuffle so the colours are mixed on the screen
        for (int i = cellCount - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }

        Color[] pixels = new Color[cellCount];
        for (int i = 0; i < cellCount; i++)
        {
            pixels[i] = colors[cells[i]];
        }
        gridTexture.SetPixels(pixels);
        gridTexture.Apply();

        Debug.Log($"New grid: {dominantCount} / {secondCount} / {thirdCount}, answer = {dominantColor}");
    }

    void FlashLight(bool isCorrect)
    {
        if (feedbackLight == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashRoutine(isCorrect ? goodAnswerColor : badAnswerColor));
    }

    IEnumerator FlashRoutine(Color color)
    {
        feedbackLight.color = color;
        feedbackLight.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        feedbackLight.enabled = false;
        flashRoutine = null;
    }

    // Fill one box per good answer in a row
    void UpdateProgress()
    {
        for (int i = 0; i < progressBoxes.Length; i++)
        {
            progressBoxes[i].color = i < goodAnswers ? filledBoxColor : emptyBoxColor;
        }
    }
}

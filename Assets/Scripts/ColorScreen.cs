using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorScreen : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private RawImage gridImage;
    [SerializeField] private Image[] progressBoxes;
    [SerializeField] private Color emptyBoxColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color filledBoxColor = Color.white;

    [Header("Grid")]
    // 0, 1, 2 (red, green, blue)
    [SerializeField] private Color[] colors = { Color.red, Color.green, Color.blue };
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    // How many more cells the dominant colour has compared to an even split (smaller = harder)
    [SerializeField] private int minMargin = 1;
    [SerializeField] private int maxMargin = 3;

    [Header("Task")]
    [SerializeField] private int answersToWin = 5;
    public UnityEvent onCompleted;

    private Texture2D gridTexture;
    private int dominantColor;
    private int goodAnswers;
    private bool isCompleted;

    void Start()
    {
        gridTexture = new Texture2D(gridWidth, gridHeight);
        gridTexture.filterMode = FilterMode.Point;
        gridTexture.wrapMode = TextureWrapMode.Clamp;
        gridImage.texture = gridTexture;

        UpdateProgress();
        GenerateGrid();
    }

    public void SubmitAnswer(int colorIndex)
    {
        if (isCompleted)
        {
            return;
        }

        if (colorIndex == dominantColor)
        {
            goodAnswers++;
        }
        else
        {
            goodAnswers = 0;
        }

        UpdateProgress();

        if (goodAnswers >= answersToWin)
        {
            isCompleted = true;
            Debug.Log("Color task completed");
            onCompleted.Invoke();
            return;
        }

        GenerateGrid();
    }

    void GenerateGrid()
    {
        int cellCount = gridWidth * gridHeight;

        // Dominant colour gets slightly more than a third, the two others share the rest
        int dominantCount = Mathf.CeilToInt(cellCount / 3f) + Random.Range(minMargin, maxMargin + 1);
        int remaining = cellCount - dominantCount;
        // Both other counts stay strictly below the dominant one
        int secondCount = Random.Range(Mathf.Max(0, remaining - (dominantCount - 1)), dominantCount);
        int thirdCount = remaining - secondCount;

        dominantColor = Random.Range(0, colors.Length);
        int secondColor = (dominantColor + 1) % colors.Length;
        int thirdColor = (dominantColor + 2) % colors.Length;

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

    void UpdateProgress()
    {
        for (int i = 0; i < progressBoxes.Length; i++)
        {
            progressBoxes[i].color = i < goodAnswers ? filledBoxColor : emptyBoxColor;
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class CaptchaCategory
{
    public string name;
    public Sprite[] images;
}

public class CaptchaScreen : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text inputText;
    [SerializeField] private Image[] progressBoxes;
    [SerializeField] private Color emptyBoxColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color filledBoxColor = Color.white;

    [Header("Text captcha")]
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TMP_Text captchaText;
    [SerializeField] private int textLength = 6;
    [SerializeField] private float maxRotation = 25f;
    private const string Characters = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    [Header("Image captcha")]
    [SerializeField] private GameObject imagePanel;
    [SerializeField] private Image[] gridCells;
    [SerializeField] private CaptchaCategory[] categories;
    [SerializeField] private int minTargets = 2;
    [SerializeField] private int maxTargets = 4;

    [Header("Task")]
    [SerializeField] private int answersToWin = 5;
    [SerializeField] private int maxInputLength = 12;
    public UnityEvent onCompleted;

    private PlayerController player;
    private PlayerInteraction playerInteraction;

    private bool isTyping;
    private int typingStartFrame;
    private string currentInput = "";

    private bool isImageCaptcha;
    private string textAnswer;
    private HashSet<int> imageAnswer = new HashSet<int>();

    private int goodAnswers;
    private bool isCompleted;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        playerInteraction = FindObjectOfType<PlayerInteraction>();

        UpdateProgress();
        GenerateCaptcha();
        UpdateInputText();
    }

    public void StartTyping()
    {
        if (isCompleted || isTyping)
        {
            return;
        }

        isTyping = true;
        typingStartFrame = Time.frameCount;
        SetPlayerControl(false);
        UpdateInputText();
    }

    void StopTyping()
    {
        isTyping = false;
        SetPlayerControl(true);
        UpdateInputText();
    }

    void SetPlayerControl(bool enabled)
    {
        player.enabled = enabled;
        playerInteraction.enabled = enabled;

        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        // In LateUpdate so PlayerController, re-enabled here, doesn't also read this Escape and unlock the cursor
        if (isTyping && Input.GetKeyDown(KeyCode.Escape))
        {
            StopTyping();
        }
    }

    void Update()
    {
        if (!isTyping || Time.frameCount == typingStartFrame)
        {
            return;
        }

        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (currentInput.Length > 0)
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r')
            {
                SubmitAnswer();
                return;
            }
            else if (char.IsLetterOrDigit(c) && currentInput.Length < maxInputLength)
            {
                currentInput += char.ToUpperInvariant(c);
            }
        }

        UpdateInputText();
    }

    void SubmitAnswer()
    {
        if (IsAnswerCorrect())
        {
            goodAnswers++;
        }
        else
        {
            goodAnswers = 0;
        }

        currentInput = "";
        UpdateProgress();

        if (goodAnswers >= answersToWin)
        {
            isCompleted = true;
            StopTyping();
            textPanel.SetActive(false);
            imagePanel.SetActive(false);
            instructionText.text = "Task completed. Door unlocked";
            Debug.Log("Captcha task completed");
            onCompleted.Invoke();
            return;
        }

        GenerateCaptcha();
        UpdateInputText();
    }

    bool IsAnswerCorrect()
    {
        if (!isImageCaptcha)
        {
            return currentInput == textAnswer;
        }

        // Order doesn't matter: "258" and "852" are both fine
        HashSet<int> typedCells = new HashSet<int>();
        foreach (char c in currentInput)
        {
            if (c >= '1' && c <= '9')
            {
                typedCells.Add(c - '1');
            }
        }
        return typedCells.SetEquals(imageAnswer);
    }

    void GenerateCaptcha()
    {
        // Image captchas need at least 2 categories (the target and the others)
        isImageCaptcha = categories.Length >= 2 && Random.value < 0.5f;

        textPanel.SetActive(!isImageCaptcha);
        imagePanel.SetActive(isImageCaptcha);

        if (isImageCaptcha)
        {
            GenerateImageCaptcha();
        }
        else
        {
            GenerateTextCaptcha();
        }
    }

    void GenerateTextCaptcha()
    {
        instructionText.text = "Type the characters below";

        StringBuilder answer = new StringBuilder();
        StringBuilder display = new StringBuilder();
        for (int i = 0; i < textLength; i++)
        {
            char c = Characters[Random.Range(0, Characters.Length)];
            answer.Append(c);

            // Each character gets its own rotation, height and size
            string rotation = Random.Range(-maxRotation, maxRotation).ToString("0", CultureInfo.InvariantCulture);
            string offset = Random.Range(-0.2f, 0.2f).ToString("0.00", CultureInfo.InvariantCulture);
            int size = Random.Range(80, 125);
            display.Append($"<rotate={rotation}><voffset={offset}em><size={size}%>{c}</size></voffset></rotate>");
        }
        textAnswer = answer.ToString();
        captchaText.text = display.ToString();

        Debug.Log("New text captcha, answer = " + textAnswer);
    }

    void GenerateImageCaptcha()
    {
        int target = Random.Range(0, categories.Length);
        instructionText.text = $"Type the numbers of every image with a {categories[target].name}.";

        imageAnswer.Clear();
        int targetCount = Random.Range(minTargets, maxTargets + 1);
        while (imageAnswer.Count < targetCount)
        {
            imageAnswer.Add(Random.Range(0, gridCells.Length));
        }

        for (int i = 0; i < gridCells.Length; i++)
        {
            int category = target;
            if (!imageAnswer.Contains(i))
            {
                category = (target + Random.Range(1, categories.Length)) % categories.Length;
            }

            Sprite[] images = categories[category].images;
            gridCells[i].sprite = images[Random.Range(0, images.Length)];
        }

        Debug.Log("New image captcha, answer = " + string.Join("", SortedAnswer()));
    }

    List<int> SortedAnswer()
    {
        List<int> cells = new List<int>();
        foreach (int cell in imageAnswer)
        {
            cells.Add(cell + 1);
        }
        cells.Sort();
        return cells;
    }

    void UpdateInputText()
    {
        if (isCompleted)
        {
            inputText.text = "";
        }
        else if (isTyping)
        {
            inputText.text = "> " + currentInput + "_   (Enter = validate, Esc = leave)";
        }
        else
        {
            inputText.text = "Click to type";
        }
    }

    void UpdateProgress()
    {
        for (int i = 0; i < progressBoxes.Length; i++)
        {
            progressBoxes[i].color = i < goodAnswers ? filledBoxColor : emptyBoxColor;
        }
    }
}

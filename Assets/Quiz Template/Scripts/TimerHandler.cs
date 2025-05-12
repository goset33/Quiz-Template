using System.Collections;
using TMPro;
using UnityEngine;

// Таймер не тестировался
public class TimerHandler : MonoBehaviour
{
    public static float time = 0f; // Время в секундах
    public static float maxTime = 0f;

    public static bool isRunning = false;

    private TextMeshProUGUI textComponent;

    private Coroutine routine = null;
    private readonly WaitForSeconds waitFor = new WaitForSeconds(1f);

    private void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        QuestionController.NextQuestionLoaded += WhenNextQuestion;
        QuestionController.WrongAnswer += OnWrongAnswer;
    }

    private void OnDisable()
    {
        StopTime();
        QuestionController.NextQuestionLoaded -= WhenNextQuestion;
        QuestionController.WrongAnswer -= OnWrongAnswer;
    }

    /// <summary>
    /// Инициализирует таймер. Вызывать при инициализации окна теста
    /// </summary>
    /// <param name="_maxTime">Начальное время на таймере</param>
    /// <param name="isGlobal">True - время общее для всех вопросов. False - время отдельно для каждого вопроса</param>
    public void InitializeTime(float _maxTime, bool isGlobal)
    {
        if (isRunning) return;

        time = _maxTime;
        isRunning = true;
        if (!isGlobal)
        {
            maxTime = _maxTime;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(TimerCoroutine());
    }

    /// <summary>
    /// Останавливает и обнуляет таймер
    /// </summary>
    public void StopTime()
    {
        isRunning = false;
        time = 0f;

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    IEnumerator TimerCoroutine()
    {
        while (time > 0 && isRunning)
        {
            yield return waitFor;
            time -= 1f;
            textComponent.text = ConvertSecondsToClock(time);
        }

        if (time <= 0f)
        {
            isRunning = false;
            StopTime();
        }
    }

    public void OnWrongAnswer()
    {
        if (maxTime == 0f) return;

        StopCoroutine(routine);
        routine = null;
    }

    public void WhenNextQuestion()
    {
        if (maxTime == 0f) return;

        time = maxTime;
        routine ??= StartCoroutine(TimerCoroutine());
    }

    private string ConvertSecondsToClock(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int minutes = totalSeconds / 60;
        int remaining = totalSeconds % 60;

        return $"{minutes:D2}:{remaining:D2}";
    }
}

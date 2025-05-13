using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimerHandler : MonoBehaviour
{
    private float time = 0f; // Время в секундах
    private float maxTime = 0f;

    public static bool isRunning = false;
    public static event Action OnTimeEnd;

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

        maxTime = 0f;
        time = _maxTime;
        isRunning = true;
        if (!isGlobal)
        {
            maxTime = _maxTime;
        }

        textComponent.text = ConvertSecondsToClock(time);

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

    /// <summary>
    /// Восстанавливает время на таймере
    /// </summary>
    /// <param name="addingTime">Время, которое нужно восстановить</param>
    public void RestoreSomeTime(float addingTime)
    {
        if (maxTime == 0f)
        {
            InitializeTime(addingTime, true);
        }
        else
        {
            time = addingTime;
            isRunning = true;
            textComponent.text = ConvertSecondsToClock(time);
            routine = StartCoroutine(TimerCoroutine());
        }
    }

    /// <summary>
    /// Основная корутина таймера
    /// </summary>
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
            OnTimeEnd?.Invoke();
            isRunning = false;
            StopTime();
        }
    }

    public void OnWrongAnswer()
    {
        StopCoroutine(routine);
        routine = null;
    }

    public void WhenNextQuestion()
    {
        if (maxTime != 0f)
        {
            time = maxTime;
            isRunning = true;
        }

        textComponent.text = ConvertSecondsToClock(time);
        routine ??= StartCoroutine(TimerCoroutine());
    }

    /// <summary>
    /// Конвертирует время в секундах в часы формата "00:00"
    /// </summary>
    /// <param name="seconds">Время в секундах</param>
    /// <returns>Время формата "00:00"</returns>
    private string ConvertSecondsToClock(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int minutes = totalSeconds / 60;
        int remaining = totalSeconds % 60;

        return $"{minutes:D2}:{remaining:D2}";
    }
}

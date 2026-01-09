using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TimerHandler : MonoBehaviour
{
    private float time = 0f; // Время в секундах
    private float maxTime = 0f;

    public static bool isRunning = false;
    public static event Action OnTimeEnd;

    public bool IsEnabled => timerText.parent.visible;
    private Label timerText;

    private Coroutine routine = null;
    private readonly WaitForSeconds waitFor = new WaitForSeconds(1f);

    public void InitTimer(Label text)
    {
        timerText = text;

        QuestionController.NextQuestionLoaded += WhenNextQuestion;
        QuestionController.WrongAnswer += OnWrongAnswer;
    }

    /// <summary>
    /// Останавливает и обнуляет таймер
    /// </summary>
    public void ResetTime()
    {
        isRunning = false;
        time = 0f;

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void OnDisable()
    {
        ResetTime();
        QuestionController.NextQuestionLoaded -= WhenNextQuestion;
        QuestionController.WrongAnswer -= OnWrongAnswer;
    }

    public void ChangeVisibility(bool newState)
    {
        timerText.parent.visible = newState;
    }

    /// <summary>
    /// Инициализирует таймер. Вызывать при инициализации окна теста
    /// </summary>
    /// <param name="_maxTime">Начальное время на таймере</param>
    /// <param name="isGlobal">True - время общее для всех вопросов. False - время отдельно для каждого вопроса</param>
    public void SetTime(float _maxTime, bool isGlobal)
    {
        if (isRunning || !IsEnabled) return;

        maxTime = 0f;
        time = _maxTime;
        isRunning = true;
        if (!isGlobal)
        {
            maxTime = _maxTime;
        }

        timerText.text = ConvertSecondsToClock(time);

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(TimerCoroutine());
    }

    /// <summary>
    /// Ставит и снимает время с паузы
    /// </summary>
    public void ChangeTimePauseState()
    {
        if (!IsEnabled) return;

        isRunning = !isRunning;
        if (!isRunning)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }
        else if (time != 0f)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(TimerCoroutine());
        }
    }

    /// <summary>
    /// Восстанавливает время на таймере
    /// </summary>
    /// <param name="addingTime">Время, которое нужно восстановить</param>
    public void RestoreSomeTime(float addingTime)
    {
        if (!IsEnabled) return;

        if (maxTime == 0f)
        {
            SetTime(addingTime, true);
        }
        else
        {
            time = addingTime;
            isRunning = true;
            timerText.text = ConvertSecondsToClock(time);
            routine = StartCoroutine(TimerCoroutine());
        }
    }

    /// <summary>
    /// Основная корутина таймера
    /// </summary>
    IEnumerator TimerCoroutine()
    {
        while (time > 0f && isRunning)
        {
            yield return waitFor;
            time -= 1f;
            timerText.text = ConvertSecondsToClock(time);
        }

        if (time <= 0f)
        {
            OnTimeEnd?.Invoke();
            isRunning = false;
            ResetTime();
        }
    }

    public void OnWrongAnswer()
    {
        if (routine == null || !IsEnabled) return;

        StopCoroutine(routine);
        routine = null;
    }

    public void WhenNextQuestion()
    {
        if (!IsEnabled) return;

        if (maxTime != 0f)
        {
            time = maxTime;
            isRunning = true;
        }

        timerText.text = ConvertSecondsToClock(time);
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

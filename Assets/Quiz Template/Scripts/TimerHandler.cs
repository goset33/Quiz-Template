using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TimerHandler : MonoBehaviour
{
	public static event Action OnTimeEnd; // Вызывается когда время истекло

    private Label timerText;

	private float currentTime = 0f;	// Текущее время (в секундах)
    private float perQuestionTime = 0f; // Если локальный режим, запоминаем заданное время для восстановления. Игнор при глобальном режиме

	private bool isGlobalMode = true; // Режим: true = глобальный таймер (не сбрасывается между вопросами), false = локальный (восстанавливается)
    private bool isPaused = false;

	private Coroutine timerCoroutine = null;
	private readonly WaitForSeconds oneSecond = new WaitForSeconds(1f);

	public bool IsEnabled
	{
		get
		{
			if (timerText == null || timerText.parent == null) return false;
			return !timerText.parent.ClassListContains("hided");
		}
	}

	/// <summary>
	/// Инициализация. Передавайте Label, который будет показывать время.
	/// Подписывает внутренние обработчики на QuestionController события.
	/// </summary>
	public void InitTimer(Label label)
	{
		timerText = label ?? throw new ArgumentNullException(nameof(label));
		UpdateLabelText();

		QuestionController.NextQuestionLoaded += WhenNextQuestion;
		QuestionController.OnAnswered += OnAnswered;
	}

	void OnDisable()
	{
		QuestionController.NextQuestionLoaded -= WhenNextQuestion;
		QuestionController.OnAnswered -= OnAnswered;
		ResetTime(); 
	}

	/// <summary>
	/// Полный сброс таймера: останавливает корутину, очищает текущее время и снимает паузу.
	/// Выполняется всегда (даже если визуально скрыт)
	/// </summary>
	public void ResetTime()
	{
		isPaused = false;
		currentTime = 0f;
		perQuestionTime = 0f;
		isGlobalMode = true;

		if (timerCoroutine != null)
		{
			StopCoroutine(timerCoroutine);
			timerCoroutine = null;
		}

		UpdateLabelText();
	}

	/// <summary>
	/// Отображение/скрытие таймера визуально.
	/// Если переключаем в скрытое - останавливаем internal coroutine (без изменения currentTime).
	/// Если переключаем в видимое - пытаемся возобновить корутину (если есть время и таймер не в паузе).
	/// </summary>
	public void ChangeVisibility(bool visible)
	{
		if (timerText == null || timerText.parent == null) return;

        ResetTime();

        if (visible)
		{
			timerText.parent.RemoveFromClassList("hided");
		}
		else
		{
			timerText.parent.AddToClassList("hided");
		}
    }

    /// <summary>
    /// Устанавливает время и режим, и запускает таймер
    /// </summary>
    /// <param name="seconds">начальное время в секундах.</param>
    /// <param name="globalMode">true = глобальный таймер; false = локальный (перезагружается на perQuestionTime при следующем вопросе</param>
    public void SetTime(float seconds, bool globalMode)
	{
		if (!IsEnabled) return;

		seconds = Mathf.Max(0f, seconds);

		isGlobalMode = globalMode;
		currentTime = seconds;

		if (!isGlobalMode)
			perQuestionTime = seconds;
		else
			perQuestionTime = 0f;

		isPaused = false;

		UpdateLabelText();
		RestartCoroutineIfNeeded();
	}

	/// <summary>
	/// Добавляет/восстанавливает время на таймере
	/// </summary>
	public void RestoreSomeTime(float addingTime)
	{
		if (!IsEnabled) return;

		addingTime = Mathf.Max(0f, addingTime);

        currentTime += addingTime;

        UpdateLabelText();
		RestartCoroutineIfNeeded();
	}

	/// <summary>
	/// Явная пауза
	/// </summary>
	public void Pause()
	{
		isPaused = true;
		if (timerCoroutine != null)
		{
			StopCoroutine(timerCoroutine);
			timerCoroutine = null;
		}
	}

	/// <summary>
	/// Явное возобновление 
	/// </summary>
	public void Resume()
	{
		isPaused = false;
		if (IsEnabled && currentTime > 0f && timerCoroutine == null)
		{
			timerCoroutine = StartCoroutine(TimerCoroutine());
		}
	}

	/// <summary>
	/// Вызывается, когда пользователь ответил: ставим таймер на паузу.
	/// </summary>
	private void OnAnswered()
	{
		if (!IsEnabled) return;
		Pause();
	}

	/// <summary>
	/// Вызывается при загрузке следующего вопроса:
	/// - Снимаем паузу (Resume).
	/// - Если локальный режим — восстанавливаем время на perQuestionTime.
	/// </summary>
	private void WhenNextQuestion()
	{
		if (!IsEnabled) return;

		if (!isGlobalMode)
		{
			currentTime = perQuestionTime;
		}

		Resume();
		UpdateLabelText();
	}

	/// <summary>
	/// Перезапускает/запускает корутину таймера, если нужно.
	/// </summary>
	private void RestartCoroutineIfNeeded()
	{
		if (timerCoroutine != null)
		{
			StopCoroutine(timerCoroutine);
			timerCoroutine = null;
		}

		if (!isPaused && IsEnabled && currentTime > 0f)
		{
			timerCoroutine = StartCoroutine(TimerCoroutine());
		}
	}

	/// <summary>
	/// Основная корутина таймера.
	/// Каждую секунду уменьшает currentTime, обновляет визуал.
	/// Останавливается, если:
	/// - время закончилось (тогда вызывает OnTimeEnd),
	/// - элемент стал скрыт (IsEnabled == false) — корутина корректно завершится,
	/// - явно остановлена извне (StopCoroutine).
	/// </summary>
	private IEnumerator TimerCoroutine()
	{
		while (currentTime > 0f)
		{
			if (!IsEnabled)
			{
				timerCoroutine = null;
				yield break;
			}

			yield return oneSecond;

			if (isPaused || !IsEnabled)
			{
				timerCoroutine = null;
				yield break;
			}

#if UNITY_EDITOR
			if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.T))
			{
				currentTime = 0f;
			}
#endif

			currentTime = Mathf.Max(0f, currentTime - 1f);
			UpdateLabelText();

			if (currentTime <= 0f)
			{
				Debug.Log("Время закончилось; вызов OnTimeEnd");
                OnTimeEnd?.Invoke();

                ResetTime();
				yield break;
			}
		}

		timerCoroutine = null;
	}

	/// <summary>
	/// Обновляет текст в Label (MM:SS)
	/// </summary>
	private void UpdateLabelText()
	{
		if (timerText == null) return;

		int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(currentTime));
		int minutes = totalSeconds / 60;
		int seconds = totalSeconds % 60;

		timerText.text = $"{minutes:D2}:{seconds:D2}";
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using YG;
using Random = UnityEngine.Random;

public class QuestionController : AbstractController
{
	private GameManager gameManager;

	public static event Action<int, int, bool> QuestionsEnded;
	public static event Action AllReady, NextQuestionLoaded, OnAnswered;

	private int rightIndex;

	private int quizHardness; // 0 - FTUE, дальше как обычно
	private int[] questionsHardness = null; // Массив, равный количеству вопросов и показывающий уровень сложности каждого вопроса
	private int QuestionDifficult => questionsHardness[currentQuestion - 1]; // Сложность текущего вопроса
	private int AccruedCash => gameManager.config.cashAddCount[quizHardness - 1];

	private readonly string[] difficultClasses = new string[4] { "easy-background", "medium-background", "hard-background", "boss-background" };
	private int reviveCount = 0;
	private bool isAnswerShowed = false;
	private bool isInitializing = false;
	private List<IQuestion> cards = new();

	public int currentQuestion; // Хранит текущий номер вопроса НАЧИНАЯ С 1. (В коде требуется отнимать 1)
	public int rightAnswers;
	public bool isWinning = true;

	private HeartContainer heartContainer;
	private TimerHandler timerHandler;

	private Button hintButton;
	private Label timerText;
	private VisualElement heartContainerElement;
	private VisualElement difficultBackground;
	private Label difficultText, questionText;
	private List<GradientButton> answerButtons = new();
	private Button showRightButton, nextButton;
	
	[SerializeField] private VisualTreeAsset loseCounterElement;

	[Header("Locales")]
	[SerializeField] private LocalizedString[] difficultiesLocales;
	[SerializeField] private LocalizedString[] backInMenuLocales, showAnswerLocales, lackOfLivesLocales, outOfTimeLocales, getHintLocales;

	public override void Init()
	{
		base.Init();
		gameManager = GameManager.Instance;

		heartContainer = GetComponent<HeartContainer>();
		timerHandler = GetComponent<TimerHandler>();

		hintButton = root.Q<Button>("Hint");
		timerText = root.Q<Label>("TimeText");
		heartContainerElement = root.Q<VisualElement>("HeartContainer");
		difficultBackground = root.Q<VisualElement>("DifficultBackground");
		difficultText = root.Q<Label>("DifficultText");
		questionText = root.Q<Label>("QuestionText");

		answerButtons = root.Query<GradientButton>("VariantButton").ToList();

		showRightButton = root.Q<Button>("ShowRightButton");
		nextButton = root.Q<GradientButton>("NextButton");

		for (int i = 0;  i < answerButtons.Count; i++)
		{
			answerButtons[i].GradientFrom = gameManager.questionConfig.defaultAnswerFrom;
            answerButtons[i].GradientTo = gameManager.questionConfig.defaultAnswerTo;

            int index = i;
			answerButtons[i].clicked += () => DefaultAnswer(index);
		}

		nextButton.clicked += NextButtonPressed;
		showRightButton.clicked += TellRightAnswerPressed;
		hintButton.clicked += GetHintPressed;

        showRightButton.AddToClassList("hided");
		nextButton.AddToClassList("hided");

		heartContainer.InitializeHearts(heartContainerElement.Children());
		timerHandler.InitTimer(timerText);
	}

	public override async Task ChangeVisibilityStateAsync(bool newState)
	{
		TimerHandler.OnTimeEnd -= OnTimeEnd;
		YG2.onErrorRewardedAdv -= OnAdError;

		base.ChangeVisibilityState(newState);
	
		if (newState)
		{
			if (isInitializing)
			{
				Debug.LogWarning("[QuestionController] InitGame already in progress, skipping...");
				return;
			}

			TimerHandler.OnTimeEnd += OnTimeEnd;
			YG2.onErrorRewardedAdv += OnAdError;
			
			if (gameManager.chosenQuiz == null)
			{
				Debug.LogError("[QuestionController] chosenQuiz is null, cannot initialize game");
				return;
			}

			isInitializing = true;
			try
			{
				await InitGame(gameManager.chosenQuiz);
			}
			catch (Exception ex)
			{
				Debug.LogError($"InitGame failed: {ex}");
				GameManager.Instance.OpenWindow<MenuController>();
			}
			finally
			{
				isInitializing = false;
			}
		}
		else
		{
			isInitializing = false;
			timerHandler.ResetTime();
		}
	}

	/// <summary>
	/// Инициализация контроллера. Вызывается автоматически при включении объекта со скриптом
	/// </summary>
	/// <param name="quizCard">Сам экземпляр квиза</param>
	private async Task InitGame(QuizCard quizCard)
	{
		quizHardness = gameManager.GetQuizHardness();

		if (quizHardness == 0) YG2.saves.isFirstQuiz = false;

		QuestionContainer container = quizCard.testContainer;
		if (container == null)
		{
			gameManager.InvokeNotification(2);
			GameManager.Instance.OpenWindow<MenuController>();
            AllReady?.Invoke();
            return;
		}

		await container.LoadQuestionsAsync();
		List<IQuestion> allPool = MixQuestions(new(container.Questions));

		int lookupIndex = (quizHardness != 0) ? (quizHardness - 1) : quizHardness;

		int amount = quizCard.questionsAmount[lookupIndex];
		cards = amount == 0 ? allPool : new List<IQuestion>(allPool.Take(amount));

		int[] difficulties = gameManager.config.questionsHardness[quizHardness];
		difficulties.MultiplyArray(Mathf.RoundToInt(quizCard.questionsAmount[lookupIndex] / 10f));
		questionsHardness = difficulties.SelectMany((x, i) => Enumerable.Repeat(i, x)).OrderBy(_ => Random.value).Take(cards.Count).ToArray();

		heartContainer.ResetHearts();
		timerHandler.ResetTime();

		timerHandler.ChangeVisibility(quizHardness > 1);
		if (quizHardness > 1)
		{
			float T = gameManager.config.questionTimer;
			float time = quizHardness == 2 ? T : (quizHardness == 3 ? T / 2f : T / 10f);
			bool isGlobal = quizHardness < 4;

			timerHandler.SetTime(time, isGlobal);
		}

		ClearScreen();
		reviveCount = 0;
		currentQuestion = 1;
		rightAnswers = 0;
		isWinning = true;

		// Для нейронки
		//string json = await AIRequestHandler.GenerateQuestionsAsync(quizCard.names[0], quizCard.questionsAmount[hardness]);
		//cards = AIAnswerParser.ParseJsonAnswer(json);
		//cards = MixQuestions(cards);

		Dictionary<string, object> data = new() { { "Имя квиза", gameManager.chosenQuiz.GetName() }, { "Уровень сложности квиза", quizHardness } };
		YG2.MetricaSend("QuizStart", data);

		LoadNextQuestion(cards[currentQuestion - 1]);
		AllReady?.Invoke();
	}

	/// <summary>
	/// Метод для рандомизации входящего списка из IQuestion
	/// </summary>
	/// <param name="inputQuestions">Входной, не рандомизированный список</param>
	/// <returns>Рандомизированный список</returns>
	private List<IQuestion> MixQuestions(List<IQuestion> inputQuestions)
	{
		if (!gameManager.shouldShuffle) return inputQuestions;

		List<IQuestion> questions = new(inputQuestions);
		int n = questions.Count;

		// Базовая рандомизация (алгоритм Фишера-Йейтса)
		for (int i = n - 1; i > 0; i--)
		{
			int j = Random.Range(0, i + 1);
			IQuestion temp = questions[i];
			questions[i] = questions[j];
			questions[j] = temp;
		}

		return questions;
	}

	/// <summary>
	/// Переносит данные из класса вопроса в интерфейс
	/// </summary>
	/// <param name="card">Карточка вопроса</param>
	private void LoadNextQuestion(IQuestion card)
	{
		if (!isWinning)
		{
			Finish();
			return;
		}

		difficultText.text = difficultiesLocales[QuestionDifficult].GetLocalizedString();
		difficultBackground.AddToClassList(difficultClasses[QuestionDifficult]);
		questionText.text = card.QuestionText;
		//counterText.text = $"{currentQuestion}/{cards.Count}";
		if (QuestionDifficult == 3 && hintButton != null)
		{
			hintButton.AddToClassList("hint-button--blocked");
		}

		MainTypeQuestion question = card as MainTypeQuestion;
		var wrongs = question.WrongAnswers.OrderBy(_ => Random.value).Take(Mathf.Min(QuestionDifficult + 1, 3)).ToList(); // Неправильные ответы рандомно
		List<string> allAnswers = new(wrongs.Append(question.RightAnswer).OrderBy(_ => Random.value)); //  Все рандомные варианты ответов
		for (int i = 0; i < allAnswers.Count; i++)
		{
			GradientButton button = answerButtons[i];
			button.RemoveFromClassList("hided");
			button.text = allAnswers[i];

			if (allAnswers[i] == question.RightAnswer)
			{
				rightIndex = i;
				SoundManager.Instance.AddUniqueSoundToButton(button, 1);
#if UNITY_EDITOR
				print("Right index: " + i);
#endif
			}
			else
			{
				SoundManager.Instance.AddUniqueSoundToButton(button, 2);
			}
		}

		NextQuestionLoaded?.Invoke();
	}

	/// <summary>
	/// Обработка нажатия кнопки при вопросе типов 1, 2
	/// </summary>
	/// <param name="index">Индекс кнопки</param>
	private void DefaultAnswer(int index)
	{
		if (nextButton != null && !nextButton.ClassListContains("hided")) return;

		bool isRight = index == rightIndex;
		var colorFrom = isRight ? gameManager.questionConfig.rightAnswerFrom : gameManager.questionConfig.wrongAnswerFrom;
		var colorTo = isRight ? gameManager.questionConfig.rightAnswerTo : gameManager.questionConfig.wrongAnswerTo;

		answerButtons[index].GradientFrom = colorFrom;
		answerButtons[index].GradientTo = colorTo;

		for (int i = 0; i < answerButtons.Count; i++)
		{
			if (i != index)
			{
				answerButtons[i].AddToClassList("variant-button--inactive");
			}
			else
			{
				answerButtons[i].AddToClassList("variant-button--choosed");
			}
		}

		Answered(isRight);
	}

	/// <summary>
	/// События после выбора варианта ответа
	/// </summary>
	/// <param name="isRight">Правильно ли ответил игрок</param>
	private void Answered(bool isRight)
	{
		Dictionary<string, object> data = new() {
			{ "Правильный ответ?", isRight },
			{ "Уровень сложности вопроса", QuestionDifficult },
			{ "Текст вопроса", cards[currentQuestion - 1].QuestionText } };
		YG2.MetricaSend("GivesAnswer", data);	

		foreach (Button button in answerButtons)
		{
			button.pickingMode = PickingMode.Ignore;
		}

		nextButton.RemoveFromClassList("hided");

        OnAnswered?.Invoke();
        if (isRight)
		{
			rightAnswers++;
            nextButton.AddToClassList("next-button--big");
            gameManager.AddExperience(1);
			GameManager.ChangeCash(AccruedCash);
			print("Right!");
		}
		else
		{
			heartContainer.TakeOneDamage();

			if (heartContainer.AliveHeartCount == 0)
			{
				Debug.Log("Сердца закончились, проигрыш");
				if (reviveCount < 2)
				{
					reviveCount++;
					gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Big, loseCounterElement, new LoseCounter(), lackOfLivesLocales), WhenDeathButtonPressed);
				}
				else
				{
                    Debug.Log("Возрождений больше нет, сердца кончились, конец раунда");
                    isWinning = false;
				}
			}

			showRightButton.RemoveFromClassList("hided");
            print("Incorrect!");
		}
	}

	private void WhenDeathButtonPressed(int buttonIndex)
	{
		if (buttonIndex == 0)
		{
			isWinning = false;
			Finish();
		}
		else if (buttonIndex == 1)
		{
			YG2.RewardedAdvShow("0", () => heartContainer.HealOneHeart());
		}
	}

	private void OnTimeEnd()
	{
		if (reviveCount < 2)
		{
			reviveCount++;
			gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Big, loseCounterElement, new LoseCounter(), outOfTimeLocales), TimeEndButtonPressed);
		}
		else
		{
			Debug.Log("Возрождений больше нет, время закончилось, конец раунда");
            isWinning = false;
		}
	}

	private void TimeEndButtonPressed(int buttonIndex)
	{
		if (buttonIndex == 0)
		{
			isWinning = false;
			Finish();
		}
		else if (buttonIndex == 1)
		{
			float T = gameManager.config.questionExtraTime;
			float time = quizHardness == 2 ? T : (quizHardness == 3 ? T / 2f : T / 10f);
			YG2.RewardedAdvShow("3", () => timerHandler.RestoreSomeTime(time));
		}
	}

	/// <summary>
	/// Вызывается при нажатии кнопки следующего вопроса
	/// </summary>
	public void NextButtonPressed()
	{
		ClearScreen();
		YG2.InterstitialAdvShow();

		if (YG2.envir.payload == "AdminPanel-Shift+Tab"
#if UNITY_EDITOR
			|| true
#endif
		   )
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Tab))
			{
				currentQuestion = cards.Count;
				gameManager.AddExperience(currentQuestion);
			}
		}


		if (currentQuestion != cards.Count) // Если вопрос был не последний
		{
			currentQuestion++;
			LoadNextQuestion(cards[currentQuestion - 1]);
		}
		else
		{
			Finish();
		}
	}

	/// <summary>
	/// Метод показывает правильный ответ на вопрос
	/// </summary>
	private void ShowRightAnswer()
	{
		isAnswerShowed = true;
		if (cards[currentQuestion - 1] is MainTypeQuestion)
		{
			answerButtons[rightIndex].RemoveFromClassList("variant-button--inactive");
			answerButtons[rightIndex].AddToClassList("variant-button--choosed");

            answerButtons[rightIndex].GradientFrom = gameManager.questionConfig.rightAnswerFrom;
			answerButtons[rightIndex].GradientTo = gameManager.questionConfig.rightAnswerTo;
		}
	}

	/// <summary>
	/// Метод вызывается при нажатии кнопки показа правильного ответа
	/// </summary>
	public void TellRightAnswerPressed()
	{
		if (isAnswerShowed) return;

        timerHandler.Pause();
        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, showAnswerLocales), GetAnswerTold);
	}

	private void GetAnswerTold(int pressedIndex)
	{
		if (pressedIndex == 1)
		{
			YG2.RewardedAdvShow("4", ShowAnswerTold);
		}
	}

	private void ShowAnswerTold()
	{
        Dictionary<string, object> data = new() { 
			{ "Имя квиза", gameManager.chosenQuiz.GetName() }, 
			{ "Уровень сложности квиза", quizHardness }, 
			{ "Уровень сложности вопроса", QuestionDifficult },
			{ "Текст вопроса", cards[currentQuestion - 1].QuestionText } };
		YG2.MetricaSend("AnswerTold", data);
		ShowRightAnswer();
	}

	/// <summary>
	/// Метод вызывается при нажатии кнопки подсказки
	/// </summary>
	public void GetHintPressed()
	{
		if (QuestionDifficult == 3 || isAnswerShowed) return;

		timerHandler.Pause();
		gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Medium, getHintLocales), GetHint);
	}

	private void GetHint(int pressedIndex)
	{
        if (pressedIndex == 1)
		{
			YG2.RewardedAdvShow("2", ShowHintAnswer);
		}
	}

	private void ShowHintAnswer()
	{
        timerHandler.Resume();

        Dictionary<string, object> data = new() { 
			{ "Имя квиза", gameManager.chosenQuiz.GetName() }, 
			{ "Уровень сложности квиза", quizHardness }, 
			{ "Уровень сложности вопроса", QuestionDifficult },
			{ "Текст вопроса", cards[currentQuestion - 1].QuestionText }};
		YG2.MetricaSend("HintUsed", data);

		ShowRightAnswer();
	}

	/// <summary>
	/// Чистит экран и обнуляет все что нужно обнулить. Вызывать после каждого вопроса
	/// </summary>
	private void ClearScreen()
	{
		isAnswerShowed = false;

		showRightButton.AddToClassList("hided");
        nextButton.AddToClassList("hided");

        hintButton.RemoveFromClassList("hint-button--blocked");
		nextButton.RemoveFromClassList("next-button--big");

		foreach (var difficultClass in difficultClasses)
		{
			difficultBackground.RemoveFromClassList(difficultClass);
		}

		answerButtons.ForEach(button =>
		{
			button.RemoveFromClassList("variant-button--inactive");
			button.RemoveFromClassList("variant-button--choosed");
			button.AddToClassList("hided");
			button.GradientFrom = gameManager.questionConfig.defaultAnswerFrom;
			button.GradientTo = gameManager.questionConfig.defaultAnswerTo;
			button.pickingMode = PickingMode.Position;

			SoundManager.Instance.UnsubscribeSoundFromButton(button);
		});
	}

	/// <summary>
	/// Вызывает конец теста
	/// </summary>
	private void Finish()
	{
        //if (isWinning) gameManager.IncrementQuizHardness();

        Dictionary<string, object> data = new() {
            { "Имя квиза", GameManager.Instance.chosenQuiz.GetName() },
            { "Уровень сложности квиза", quizHardness },
            { "Количество верных ответов", rightAnswers },
            { "Количество возрождений", reviveCount },
            { "Игрок прошел квиз?", isWinning } };
        YG2.MetricaSend("QuizEnded", data);

        QuestionsEnded?.Invoke(rightAnswers, cards.Count, isWinning);

		timerHandler.ResetTime();
		ClearScreen();
	}

	/// <summary>
	/// Обрабатывает ошибки в показе рекламы за вознаграждение
	/// </summary>
	/// <param name="id">ID показанной рекламы</param>
	private void OnAdError(string id)
	{
		if (id == "0" || id == "3")
		{
			reviveCount--;
			isWinning = false;
		}
	}

	protected override void BackInMenu()
	{
        timerHandler.Pause(); 
		gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, backInMenuLocales), BackToMenuAgreed);
	}

	private void BackToMenuAgreed(int pressedIndex)
	{
        timerHandler.ResetTime(); 
		if (pressedIndex == 1)
		{
			Dictionary<string, object> data = new() { 
				{ "Имя квиза", gameManager.chosenQuiz.GetName() }, 
				{ "Уровень сложности квиза", quizHardness }, 
				{ "Номер последнего вопроса", currentQuestion },    
				{ "Сложность последнего вопроса", QuestionDifficult },
				{ "Текст последнего вопроса", cards[currentQuestion - 1].QuestionText },
				{ "Количество возрождений", reviveCount },
				{ "Количество сердец", heartContainer.AliveHeartCount } };
			YG2.MetricaSend("QuizLeave", data);
			base.BackInMenu();
		}
	}
}

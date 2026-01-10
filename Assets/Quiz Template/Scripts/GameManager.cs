using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using YG;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	public static string Language => YG2.lang;

	public static CompositeDisposable disposables = new CompositeDisposable();

	public GameConfig config;

	[Header("Choose Settings")]
	public List<QuizCard> quizzes = new();
	public Dictionary<string, QuizCardSaveData> quizCardLevelProgress = new();
	[HideInInspector] public QuizCard chosenQuiz = null;

	[Header("Questions Settings")]
	public QuestionConfig questionConfig;
	public bool shouldShuffle; // Следует ли рандомизировать порядок вопросов

	[Header("Controllers")]
	[SerializeField] private LoadController loadController;
	[SerializeField] private TimelessController timelessController;
	//[SerializeField] private HardnessController hardnessController;
	[SerializeField] private ResultController resultController;

	private Dictionary<Type, AbstractController> controllers = new Dictionary<Type, AbstractController>();
	private Type currentWindowType;

	private float timer = 0f; // Я мог бы использовать Time.realtimeSinceStartupAsDouble и не мучаться, но рот я ебал юнитеков и баг, которому уже 5 лет

	/// <summary>
	/// Bootstrap для всей игры. На старте запускает инициализацию меню 
	/// </summary>
	private void Awake()
	{
		// Место для дебаг строк
		
		// Удалить потом обязательно

		// Настройка локализации
		LocalizationSettings.InitializationOperation.Completed += LoadLocale;

		// Присвоение инстанса и подписки на ивенты
		Instance = this;

		MenuController.GameStarted += OpenWindow<ChooseController>;
		MenuController.SettingsOpened += OpenWindow<SettingsController>;
		MenuController.LeaderboardOpened += OpenWindow<LeaderboardController>;
		ChooseController.QuizChoosed += OnQuizChoosed;
		//HardnessController.LevelChoosed += OnLevelChoosed;
		QuestionController.AllReady += OnQuestionsLoaded;
		QuestionController.QuestionsEnded += OnQuestionsSolved;

		YG2.onGetSDKData += AfterSDKInitializing;

		// Настройка квизов в окне выбора квизов
		// Наверняка можно упростить, но я уже не помню как там всё выглядит после временной интеграции избранных квизов. Работает и славно
		if (YG2.saves.otherCards == null || YG2.saves.otherCards.Length != quizzes.Count)
		{
			YG2.saves.otherCards = quizzes.ConvertToNames();
			YG2.saves.favoriteCards.Clear();
		}

		var allControllers = FindObjectsByType<AbstractController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (var controller in allControllers)
		{
			controllers[controller.GetType()] = controller;
			controller.Init();

			if (controller is not LoadController && controller is not TimelessController)
			{
				controller.ChangeVisibilityState(false);
			}
		}
		controllers[typeof(MenuController)].ChangeVisibilityState(true);
		currentWindowType = typeof(MenuController);
	}

	private void AfterSDKInitializing()
	{
		YG2.MetricaSend("GameEnter");

		SoundManager.Instance.Init();

		StartCoroutine(SaveCoroutine());

		InitializeAndLoadLevelProgress();
	}

	/// <summary>
	/// Инициализирует словарь quizCardLevelProgress
	/// </summary>
	private void InitializeAndLoadLevelProgress()
	{
		quizCardLevelProgress.Clear();
		YG2.saves.quizCards ??= new List<QuizCardSaveData>();

		// Подтягивание данных из YG2.saves.quizCards в quizCardProgress
		foreach (QuizCardSaveData savedData in YG2.saves.quizCards)
		{
			if (savedData == null || string.IsNullOrEmpty(savedData.cardId)) continue;

			if (!quizCardLevelProgress.ContainsKey(savedData.cardId))
			{
				quizCardLevelProgress.Add(savedData.cardId, savedData);
			}
			else
			{
				Debug.LogWarning($"Найден дупликат cardId '{savedData.cardId}' в YG2.saves.quizCards");
			}
		}

		// Создание новых экземпляров данных в YG2.saves.quizCards по quizzes
		bool newProgressDataCreated = false;
		foreach (QuizCard quizTemplate in quizzes)
		{
			if (quizTemplate == null) continue;

			string cardId = quizTemplate.GetName();
			if (!quizCardLevelProgress.ContainsKey(cardId))
			{
				QuizCardSaveData newSave = new QuizCardSaveData(cardId);
				quizCardLevelProgress.Add(cardId, newSave);

				newProgressDataCreated = true;
			}
		}

		// Пересохранение списка YG2.saves.quizCards, если он был модифицирован
		if (newProgressDataCreated)
		{
			SaveQuizCardProgress();
		}
	}

	/// <summary>
	/// Загружает локаль сразу после инициализации пакета Localization
	/// </summary>
	private void LoadLocale(AsyncOperationHandle<LocalizationSettings> handle)
	{
		LocalizationSettings.InitializationOperation.Completed -= LoadLocale;
		if (Language == "ru")
		{
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
		}
		else
		{
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
		}

		loadController.EndLoad();
	}

	/// <summary>
	/// Вызывается перед выходом из игры. Обнуляет всё и делает отписки от ивентов
	/// </summary>
	public void OnGameQuit()
	{
		AIRequestHandler.Dispose();
		disposables?.Dispose();
			
		StopAllCoroutines();

		YG2.MetricaSend("SessionTime", new Dictionary<string, object> { { "Время в секундах", timer } });

		MenuController.GameStarted -= OpenWindow<ChooseController>;
		MenuController.SettingsOpened -= OpenWindow<SettingsController>;
		MenuController.LeaderboardOpened -= OpenWindow<LeaderboardController>;
		ChooseController.QuizChoosed -= OnQuizChoosed;
		//HardnessController.LevelChoosed -= OnLevelChoosed;
		QuestionController.AllReady -= OnQuestionsLoaded;
		QuestionController.QuestionsEnded -= OnQuestionsSolved;

		YG2.onGetSDKData -= AfterSDKInitializing;
		if (LocalizationSettings.Instance != null)
		{
			LocalizationSettings.InitializationOperation.Completed -= LoadLocale;
		}
	}

	private void Update()
	{
		timer += Time.deltaTime;
	}

	/// <summary>
	/// Сохраняет конкретно YG2.saves.quizCards в соответствии с quizCardProgress
	/// </summary>
	private void SaveQuizCardProgress()
	{
		YG2.saves.quizCards.Clear();
		foreach (QuizCardSaveData saveData in quizCardLevelProgress.Values)
		{
			YG2.saves.quizCards.Add(saveData);
		}
	}

	IEnumerator SaveCoroutine()
	{
		YieldInstruction waiter = new WaitForSeconds(5f);
		while (true)
		{
			YG2.SaveProgress();
			YG2.SetLeaderboard("Stars", YG2.saves.cash);
			yield return waiter;
		}
	}

	public static bool HaveEnoughCash(int cost)
	{
		if (cost < 0)
		{
			return YG2.saves.cash >= Math.Abs(cost);
		}
		return true;
	}

	public static void ChangeCash(int cost)
	{
		if (!HaveEnoughCash(cost)) return;

		YG2.saves.cash += cost;
	}

	public void AddExperience(int amount)
	{
		QuizCardSaveData data = quizCardLevelProgress[chosenQuiz.GetName()];
		bool leveledUp = data.AddExperience(amount);
		Debug.Log($"Card '{chosenQuiz.GetName()}': EXP {data.exp}/{data.maxExp}, Level {data.level}. Leveled up: {leveledUp}");

		SaveQuizCardProgress();
	}

	public void InvokePopup(PopupSettings settings, Action<int> callback)
	{
		timelessController.CreatePopup(settings, callback);
	}

	public void InvokeNotification(int notifyIndex)
	{
		timelessController.CreateNotification(config.notifyLocales[notifyIndex]);
	}

	/// <summary>
	/// Возвращает сложность текущего квиза
	/// </summary>
	/// <returns>
	/// 0 - FTUE; 1-4 - Как обычно
	/// </returns>
	public int GetQuizHardness()
	{
		if (YG2.saves.isFirstQuiz) return 0;

		QuizCardSaveData data = quizCardLevelProgress[chosenQuiz.GetName()];
		return data.level;
	}

	private void OnQuestionsLoaded()
	{
		loadController.EndLoad();
	}

	/// <summary>
	/// Изменяет позицию карточки квиза в массиве, не трогая отображение
	/// </summary>
	/// <param name="card">Сама карточка</param>
	[Obsolete]
	public void SetAsFavorite(QuizCard card)
	{
		string name = card.GetName();

		int currentIndex = Array.IndexOf(YG2.saves.otherCards, name);
		if (currentIndex != -1)
		{
			YG2.saves.otherCards[currentIndex] = null;
			YG2.saves.favoriteCards.Add(name);
		}
		else
		{
			int originalIndex = quizzes.IndexOf(card);
			YG2.saves.otherCards[originalIndex] = name;
			YG2.saves.favoriteCards.Remove(name);
		}
	}

	// --- Функции переходов между экранами ---

	public void OpenWindow<T>() where T : AbstractController
	{
		if (currentWindowType != null)
		{
			controllers[currentWindowType].ChangeVisibilityState(false);
		}

        currentWindowType = typeof(T);
        controllers[typeof(T)].ChangeVisibilityState(true);

		YG2.InterstitialAdvShow();
	}

	public async Task OpenWindowAsync<T>() where T : AbstractController
	{
		if (currentWindowType != null)
			controllers[currentWindowType].ChangeVisibilityState(false);

        currentWindowType = typeof(T);
        await controllers[typeof(T)].ChangeVisibilityStateAsync(true);

		YG2.InterstitialAdvShow();
	}

	private void OnQuizChoosed(QuizCard obj)
	{
		chosenQuiz = obj;
		loadController.StartLoad(async () => {
			await OpenWindowAsync<QuestionController>();
		});
	}

	private void OnQuestionsSolved(int arg1, int arg2, bool isItGood)
	{
		OpenWindow<ResultController>();
		resultController.LoadResult(arg1, arg2, isItGood);
	}
}
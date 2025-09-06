using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static string Language => YG2.lang;

    public enum GameState
    {
        InMainMenu,
        ChoosingQuiz,
        ChoosingHardness,
        SolvingQuestions,
        GettingResults
    }
    private GameState state;

    public GameConfig config;

    [Header("Choose Settings")]
    public List<QuizCard> quizzes = new();
    public Dictionary<string, QuizCardSaveData> quizCardLevelProgress = new();
    [HideInInspector] public QuizCard chosenQuiz = null;

    [Header("Questions Settings")]
    public QuestionConfig questionConfig;
    public bool shouldShuffle; // Следует ли рандомизировать порядок вопросов

    // Идея: Сейчас переменные используются только для переключения окон. В целом можно передавать ивенты типа Action<Transform, int> и тогда убрать все эти зависимости
    [Header("Controllers")]
    [SerializeField] private LoadController loadController;
    [SerializeField] private TimelessController timelessController;
    [SerializeField] private MenuController menuController;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private LeaderboardController leaderboardController;
    [SerializeField] private ChooseController chooseController;
    [SerializeField] private HardnessController hardnessController;
    [SerializeField] private QuestionController questionController;
    [SerializeField] private ResultController resultController;

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

        MenuController.GameStarted += GetIntoGame;
        MenuController.SettingsOpened += OpenSettings;
        MenuController.LeaderboardOpened += OpenLeaderboard;
        QuizCardSetter.QuizChoosed += OnQuizChoosed;
        //HardnessController.LevelChoosed += OnLevelChoosed;
        QuestionController.AllReady += OnQuestionsLoaded;
        QuestionController.QuestionsEnded += OnQuestionsSolved;

        YG2.onGetSDKData += AfterSDKInitializing;
        YG2.onGetLeaderboard += leaderboardController.OnLeaderboardInitialized;

        // Настройка квизов в окне выбора квизов
        if (YG2.saves.otherCards == null || YG2.saves.otherCards.Length != quizzes.Count)
        {
            YG2.saves.otherCards = quizzes.ConvertToNames();
            YG2.saves.favoriteCards.Clear();
        }
    }

    private void AfterSDKInitializing()
    {
        YG2.MetricaSend("GameEnter");

        SoundManager.Instance.SetMusicVolume(YG2.saves.musicVolume);
        SoundManager.Instance.SetVfxVolume(YG2.saves.vfxVolume);

        StartCoroutine(SaveCoroutine());

        InitializeAndLoadLevelProgress();

        if (string.IsNullOrEmpty(YG2.saves.nickname))
        {
            GetPlayerName();
        }
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

    public void OnGameQuit()
    {
        AIRequestHandler.Dispose();
            
        StopAllCoroutines();

        YG2.MetricaSend("SessionTime", new Dictionary<string, object> { { "Время в секундах", timer } });

        MenuController.GameStarted -= GetIntoGame;
        MenuController.SettingsOpened -= OpenSettings;
        QuizCardSetter.QuizChoosed -= OnQuizChoosed;
        //HardnessController.LevelChoosed -= OnLevelChoosed;
        QuestionController.AllReady -= OnQuestionsLoaded;
        QuestionController.QuestionsEnded -= OnQuestionsSolved;

        YG2.onGetSDKData -= AfterSDKInitializing;
        YG2.onGetLeaderboard -= leaderboardController.OnLeaderboardInitialized;
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

    private void GetPlayerName()
    {
        if (YG2.player.auth && !YG2.player.name.Equals("anonymous"))
        {
            YG2.saves.nickname = YG2.player.name;
            return;
        }

        timelessController.ActivateTheNameWindow();
    }

    public GameState GetGameState() { return state; }

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

    public void InvokePopup(PopupSettings settings)
    {
        timelessController.CreatePopup(settings);
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

    private void OnQuestionsLoaded()
    {
        loadController.EndLoad();
    }

    // --- Функции переходов между экранами ---

    public void ReturnToMenu(Transform currentController)
    {
        YG2.InterstitialAdvShow();
        currentController.parent.gameObject.SetActive(false);
        menuController.transform.parent.gameObject.SetActive(true);
    }

    private void GetIntoGame()
    {
        menuController.transform.parent.gameObject.SetActive(false);
        chooseController.transform.parent.gameObject.SetActive(true);
    }

    private void OpenSettings()
    {
        menuController.transform.parent.gameObject.SetActive(false);
        settingsController.transform.parent.gameObject.SetActive(true);
    }

    private void OpenLeaderboard()
    {
        menuController.transform.parent.gameObject.SetActive(false);
        leaderboardController.transform.parent.gameObject.SetActive(true);
    }

    private void OnQuizChoosed(QuizCard obj)
    {
        chosenQuiz = obj;
        loadController.StartLoad(() => {
            chooseController.transform.parent.gameObject.SetActive(false);
            questionController.transform.parent.gameObject.SetActive(true);
        });
    }

    //private void OnLevelChoosed(int obj)
    //{
    //    chosenHardnessIndex = obj;
    //    hardnessController.transform.parent.gameObject.SetActive(false);
    //    questionController.transform.parent.gameObject.SetActive(true);
    //}

    private void OnQuestionsSolved(int arg1, int arg2, int arg3, bool isItGood)
    {
        YG2.InterstitialAdvShow();
        questionController.transform.parent.gameObject.SetActive(false);
        resultController.transform.parent.gameObject.SetActive(true);
        resultController.Init(arg1, arg2, arg3, isItGood);
    }
}
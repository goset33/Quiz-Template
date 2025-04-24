using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using YG;

public class GameManager : MonoBehaviour
{
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
    [HideInInspector] public QuizCard chosenQuiz = null;

    [Header("Questions Settings")]
    public QuestionConfig questionConfig;
    public int startHeartsCount = 3; // Сколько сердец будет у игрока на старте
    public bool shouldShuffle; // Следует ли рандомизировать порядок вопросов

    // Идея: Сейчас переменные используются только для переключения окон. В целом можно передавать ивенты типа Action<Transform, int> и тогда убрать все эти зависимости
    [Header("Controllers")]
    [SerializeField] private TimelessController timelessController;
    [SerializeField] private MenuController menuController;
    [SerializeField] private ChooseController chooseController;
    [SerializeField] private HardnessController hardnessController;
    [SerializeField] private QuestionController questionController;
    [SerializeField] private ResultController resultController;

    /// <summary>
    /// Bootstrap для всей игры. На старте запускает инициализацию меню 
    /// </summary>
    private void Awake()
    {
        // Место для дебаг строк
        YG2.saves.level = 1;
        YG2.saves.experience = 0;
        YG2.saves.requiredExp = 100;
        // Удалить потом обязательно

        // Настройка локализации
        if (Language == "ru")
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }
        else
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
        }

        // Присвоение контроллерам и подписки на ивенты
        MenuController.gameManager = this;
        ChooseController.gameManager = this;
        HardnessController.gameManager = this;
        QuestionController.gameManager = this;
        ResultController.gameManager = this;

        MenuController.GameStarted += GetIntoGame;
        QuizCardSetter.QuizChoosed += OnQuizChoosed;
        //HardnessController.LevelChoosed += OnLevelChoosed;
        QuestionController.QuestionsEnded += OnQuestionsSolved;

        // Настройка квизов в окне выбора квизов
        if (YG2.saves.otherCards == null || YG2.saves.otherCards.Length != quizzes.Count)
        {
            YG2.saves.otherCards = quizzes.ConvertToNames();
            YG2.saves.favoriteCards.Clear();
        }
    }

    private void OnDisable()
    {
        AIRequestHandler.Dispose();

        MenuController.GameStarted -= GetIntoGame;
        QuizCardSetter.QuizChoosed -= OnQuizChoosed;
        //HardnessController.LevelChoosed -= OnLevelChoosed;
        QuestionController.QuestionsEnded -= OnQuestionsSolved;
    }

    public GameState GetGameState() { return state; }

    public static bool HaveEnoughCash(int cost)
    {
        if (cost < 0 && YG2.saves.cash < Math.Abs(cost))
        {
            return false;
        }
        return true;
    }

    public static void ChangeCash(int cost)
    {
        if (!HaveEnoughCash(cost)) return;

        YG2.saves.cash += cost;
        YG2.SaveProgress();
    }

    public void AddExperience(int addedExp)
    {
        chooseController.levelHandler.AddExp(addedExp);
    }

    public void InvokePopup(PopupSettings settings)
    {
        timelessController.CreatePopup(settings);
    }

    public void InvokeNotification(int notifyIndex)
    {
        timelessController.CreateNotification(config.notifyLocales[notifyIndex]);
    }

    public static int GetQuizHardness(QuizCard quizCard)
    {
        if (YG2.saves.levelsHardness.ContainsKey(quizCard.names[0]))
        {
            return YG2.saves.levelsHardness[quizCard.names[0]];
        }
        return 0;
    }

    public static void IncrementQuizHardness(QuizCard quizCard)
    {
        int curr = GetQuizHardness(quizCard);
        if (curr == 0)
        {
            YG2.saves.levelsHardness.Add(quizCard.names[0], curr + 1);
        }
        else if (curr != 2)
        {
            YG2.saves.levelsHardness[quizCard.names[0]]++;
        }
        YG2.SaveProgress();
    }

    /// <summary>
    /// Изменяет позицию карточки квиза в массиве, не трогая отображение
    /// </summary>
    /// <param name="card">Сама карточка</param>
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
        YG2.SaveProgress();
    }

    // Функции переходов между экранами
    public void ReturnToMenu(Transform currentController)
    {
        YG2.InterstitialAdvShow();
        currentController.parent.gameObject.SetActive(false);
        menuController.transform.parent.gameObject.SetActive(true);
    }

    public void GetIntoGame()
    {
        menuController.transform.parent.gameObject.SetActive(false);
        chooseController.transform.parent.gameObject.SetActive(true);
        chooseController.RedrawOrder(null);
    }

    private void OnQuizChoosed(QuizCard obj)
    {
        chosenQuiz = obj;
        chooseController.transform.parent.gameObject.SetActive(false);
        questionController.transform.parent.gameObject.SetActive(true);

        //hardnessController.transform.parent.gameObject.SetActive(true);
    }

    //private void OnLevelChoosed(int obj)
    //{
    //    chosenHardnessIndex = obj;
    //    hardnessController.transform.parent.gameObject.SetActive(false);
    //    questionController.transform.parent.gameObject.SetActive(true);
    //}

    private void OnQuestionsSolved(int arg1, int arg2, bool isItGood)
    {
        YG2.InterstitialAdvShow();
        questionController.transform.parent.gameObject.SetActive(false);
        resultController.transform.parent.gameObject.SetActive(true);
        resultController.Init(arg1, arg2, isItGood);
    }
}
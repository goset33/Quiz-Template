using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YG;

public class GameManager : MonoBehaviour
{
    public static string Language => YG2.lang;

    public HashSet<DoubleInt> OpenedLevels => YG2.saves.openedLevels;

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
    [HideInInspector] public int chosenQuizIndex = -1;

    [Header("Questions Settings")]
    public QuestionConfig questionConfig;
    public int startHeartsCount = 3; // Сколько сердец будет у игрока на старте
    public bool shouldShuffle; // Следует ли рандомизировать порядок вопросов
    [HideInInspector] public int chosenHardnessIndex = -1;

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

        MenuController.gameManager = this;
        ChooseController.gameManager = this;
        HardnessController.gameManager = this;
        QuestionController.gameManager = this;
        ResultController.gameManager = this;

        MenuController.GameStarted += GetIntoGame;
        QuizCardSetter.QuizChoosed += OnQuizChoosed;
        HardnessController.LevelChoosed += OnLevelChoosed;
        QuestionController.QuestionsEnded += OnQuestionsSolved;

        if (YG2.saves.otherCards.Count != quizzes.Count)
        {
            YG2.saves.otherCards = new(quizzes);
            YG2.saves.favoriteCards.Clear();
        }

        // Пытки в нейро
        //string ans = await AIRequestHandler.GenerateQuestionsAsync("Minectaft", 5);
        //AIAnswerParser.ParseJsonAnswer(ans);
    }

    private void OnDisable()
    {
        // Пытки в нейро
        AIRequestHandler.Dispose();

        MenuController.GameStarted -= GetIntoGame;
        QuizCardSetter.QuizChoosed -= OnQuizChoosed;
        HardnessController.LevelChoosed -= OnLevelChoosed;
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

    /// <summary>
    /// Проверяет в сохранениях, был ли открыт уровень
    /// </summary>
    /// <param name="quizIndex">Индекс квиза</param>
    /// <param name="levelIndex">Индекс уровня сложности</param>
    public bool IsLevelWasOpened(int quizIndex, int levelIndex)
    {
        return OpenedLevels.Any(obj => obj.first == quizIndex && obj.second == levelIndex);
    }

    /// <summary>
    /// Изменяет позицию карточки квиза в массиве, не трогая отображение
    /// </summary>
    /// <param name="card">Сама карточка</param>
    public void SetAsFavorite(QuizCard card)
    {
        List<QuizCard> list = YG2.saves.otherCards;

        // Находим текущую позицию объекта в списке
        int currentIndex = list.IndexOf(card);
        if (currentIndex != -1)
        {
            print("to favs");
            list[currentIndex] = null;
            YG2.saves.favoriteCards.Add(card);
        }
        else
        {
            print("move back");
            int originalIndex = quizzes.IndexOf(card);
            list[originalIndex] = card;
            YG2.saves.favoriteCards.Remove(card);
        }
        YG2.SaveProgress();
    }

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
        chooseController.RedrawOrder();
    }

    private void OnQuizChoosed(int obj)
    {
        chosenQuizIndex = obj;
        chooseController.transform.parent.gameObject.SetActive(false);
        hardnessController.transform.parent.gameObject.SetActive(true);
    }

    private void OnLevelChoosed(int obj)
    {
        chosenHardnessIndex = obj;
        hardnessController.transform.parent.gameObject.SetActive(false);
        questionController.transform.parent.gameObject.SetActive(true);
    }

    private void OnQuestionsSolved(int arg1, int arg2, bool isItGood)
    {
        YG2.InterstitialAdvShow();
        questionController.transform.parent.gameObject.SetActive(false);
        resultController.transform.parent.gameObject.SetActive(true);
        resultController.Init(arg1, arg2, isItGood);
    }
}
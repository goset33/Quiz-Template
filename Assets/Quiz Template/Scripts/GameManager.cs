using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YG;

public class GameManager : MonoBehaviour
{
    public static string Language => YandexGame.lang;
    public HashSet<DoubleInt> OpenedLevels => YandexGame.savesData.openedLevels;

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
    private async void Awake()
    {
        // Место для дебаг строк
        YandexGame.savesData.level = 1;
        YandexGame.savesData.experience = 0;
        YandexGame.savesData.requiredExp = 100;
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

        if (YandexGame.savesData.otherCards.Count != quizzes.Count)
        {
            YandexGame.savesData.otherCards = new(quizzes);
            YandexGame.savesData.favoriteCards.Clear();
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
        if (cost < 0 && YandexGame.savesData.cash < Math.Abs(cost))
        {
            return false;
        }
        return true;
    }

    public static void ChangeCash(int cost)
    {
        if (!HaveEnoughCash(cost)) return;

        YandexGame.savesData.cash += cost;
        YandexGame.SaveProgress();
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
        List<QuizCard> list = YandexGame.savesData.otherCards;

        // Находим текущую позицию объекта в списке
        int currentIndex = list.IndexOf(card);
        if (currentIndex != -1)
        {
            print("to favs");
            list[currentIndex] = null;
            YandexGame.savesData.favoriteCards.Add(card);
        }
        else
        {
            print("move back");
            int originalIndex = quizzes.IndexOf(card);
            list[originalIndex] = card;
            YandexGame.savesData.favoriteCards.Remove(card);
        }
        YandexGame.SaveProgress();
    }

    public void ReturnToMenu(Transform currentController)
    {
        YandexGame.FullscreenShow();
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
        YandexGame.FullscreenShow();
        questionController.transform.parent.gameObject.SetActive(false);
        resultController.transform.parent.gameObject.SetActive(true);
        resultController.Init(arg1, arg2, isItGood);
    }
}
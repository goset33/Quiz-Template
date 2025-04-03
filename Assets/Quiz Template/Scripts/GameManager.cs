using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YG;

public class GameManager : MonoBehaviour
{
    public string Language => YandexGame.lang;
    public HashSet<DoubleInt> OpenedLevels => YandexGame.savesData.openedLevels;

    public enum GameState
    {
        ChoosingQuiz,
        ChoosingLevel,
        SolvingQuestions,
        GettingResults
    }
    private GameState state;

    public GameConfig config;

    [Header("Choose Settings")]
    public QuizCard[] quizzes;
    [HideInInspector] public int chosenQuizIndex = -1;

    [Header("Questions Settings")]
    public QuestionConfig questionConfig;
    public int startHeartsCount = 3; // Сколько сердец будет у игрока на старте
    public bool shouldShuffle; // Следует ли рандомизировать порядок вопросов
    [HideInInspector] public int chosenLevelIndex = -1;

    // Идея: Сейчас переменные используются только для переключения окон. В целом можно передавать ивенты типа Action<Transform, int> и тогда убрать все эти зависимости
    [Header("Controllers")]
    [SerializeField] TimelessController timelessController;
    [SerializeField] ChooseController chooseController;
    [SerializeField] MenuController menuController;
    [SerializeField] QuestionController questionController;
    [SerializeField] ResultController resultController;

    /// <summary>
    /// Bootstrap для всей игры. На старте запускает инициализацию меню 
    /// </summary>
    private void Awake()
    {
        // Место для дебаг строк
        YandexGame.savesData.level = 1;
        YandexGame.savesData.experience = 0;
        YandexGame.savesData.requiredExp = 100;
        // Удалить потом обязательно

        ChooseController.gameManager = this;
        MenuController.gameManager = this;
        QuestionController.gameManager = this;
        ResultController.gameManager = this;

        ChooseController.QuizChoosed += OnQuizChoosed;
        MenuController.LevelChoosed += OnLevelChoosed;
        QuestionController.QuestionsEnded += OnQuestionsSolved;

        chooseController.OnGameStart(quizzes);
    }

    private void OnDisable()
    {
        ChooseController.QuizChoosed -= OnQuizChoosed;
        MenuController.LevelChoosed -= OnLevelChoosed;
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

    public void ReturnToMenu(Transform currentController)
    {
        YandexGame.FullscreenShow();
        currentController.parent.gameObject.SetActive(false);
        chooseController.transform.parent.gameObject.SetActive(true);
    }

    private void OnQuizChoosed(int obj)
    {
        chosenQuizIndex = obj;
        chooseController.transform.parent.gameObject.SetActive(false);
        menuController.transform.parent.gameObject.SetActive(true);
    }

    private void OnLevelChoosed(int obj)
    {
        chosenLevelIndex = obj;
        menuController.transform.parent.gameObject.SetActive(false);
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
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

    [Header("Controllers")]
    [SerializeField] TimelessController timelessController;
    [SerializeField] ChooseController chooseController;
    [SerializeField] MenuController menuController;
    [SerializeField] QuestionController questionController;
    [SerializeField] ResultController resultController;

    /// <summary>
    /// Bootstrap для всей игры. На старте запускает инициализацию меню 
    /// </summary>
    public void Awake()
    {
        // Место для дебаг строк
        YandexGame.savesData.level = 1;
        YandexGame.savesData.experience = 0;
        YandexGame.savesData.requiredExp = 1;
        // Удалить потом обязательно

        ChooseController.gameManager = this;
        MenuController.gameManager = this;
        QuestionController.gameManager = this;
        ResultController.gameManager = this;
        chooseController.OnGameStart(quizzes);
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
    /// Общая функция для переключения всех окон в игре.
    /// </summary>
    /// <param name="currentController">Контроллер, вызвавший функцию</param>
    /// <param name="requredState">Состояние, на которое требуется переключться</param>
    /// <param name="integer">В некоторых случаях требует число</param>
    /// <exception>
    /// В случае, например, отсутствия integer когда он нужен перенаправляет игрока на окно выбора теста
    /// </exception>
    public void ChangeActiveWindow(Transform currentController, GameState requredState, int? integer)
    {
        state = requredState;
        currentController.parent.gameObject.SetActive(false);
        if (requredState == GameState.ChoosingLevel && integer.HasValue)
        {
            chosenQuizIndex = integer.Value;
            menuController.transform.parent.gameObject.SetActive(true);
            return;
        }
        else if (requredState == GameState.SolvingQuestions) 
        {
            if (integer.HasValue)
            {
                chosenLevelIndex = integer.Value;
            }
            if (chosenLevelIndex != -1)
            {
                questionController.transform.parent.gameObject.SetActive(true);
                return;
            }
        }
        else if (requredState == GameState.GettingResults && integer.HasValue)
        {
            resultController.transform.parent.gameObject.SetActive(true);
            resultController.Init(integer.Value);
            return;
        }
        chooseController.transform.parent.gameObject.SetActive(true);
        state = GameState.ChoosingQuiz;
    }
}
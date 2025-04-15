using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
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
    public QuizCard[] quizzes;
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

    private AILoader loader = new AILoader();

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

        if (YandexGame.savesData.realQuizzesSequence == null || YandexGame.savesData.realQuizzesSequence.Length > quizzes.Length)
        {
            YandexGame.savesData.realQuizzesSequence = new int[quizzes.Length];
            for (int i = 0; i < quizzes.Length; i++)
            {
                YandexGame.savesData.realQuizzesSequence[i] = i;
            }
        }

        // Пытки в нейро
        try
        {
            await loader.LoadAsync();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("Загрузка отменена.");
        }
    }

    private void OnDisable()
    {
        // Пытки в нейро
        loader.Dispose();

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
    public void UpdateQuizCardPosition(QuizCard card)
    {
        int index = GetIndexFromCard(card);
        PasteObjectAsFirst(index, YandexGame.savesData.realQuizzesSequence);
        YandexGame.SaveProgress();
    }

    /// <summary>
    /// Ищет квиз в массиве квизов и возвращает его индекс
    /// </summary>
    /// <returns>Индекс квиза в массиве</returns>
    private int GetIndexFromCard(QuizCard card)
    {
        for (int i = 0; i < quizzes.Length; i++)
        {
            if (quizzes[i] == card)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Перемещает объект в начало массива, передвигая все остальные элементы на 1
    /// </summary>
    /// <typeparam name="T">QuizCard</typeparam>
    /// <param name="obj">Объект</param>
    /// <param name="array">Массив где содержится объект</param>
    private void PasteObjectAsFirst<T>(T obj, T[] array)
    {
        int maxIndex = -1;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Equals(obj))
            {
                maxIndex = i;
                break;
            }
        }

        for (int i = maxIndex; i > 0; i--)
        {
            array[i] = array[i - 1];
        }
        array[0] = obj;
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
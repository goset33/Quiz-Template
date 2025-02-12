using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YG;

// TODO: Метод нажатия на кнопки тестов, запуск меню сложности, запоминание всех выборов
public class GameManager : MonoBehaviour
{
    public string Language => YandexGame.lang;
    public HashSet<DoubleInt> OpenedLevels => YandexGame.savesData.openedLevels;

    public enum GameState
    {
        ChoosingQuiz,
        InLevelMenu,
        SolvingQuestions,
        GettingResults
    }
    private GameState state;

    [Header("Music settings")]
    public GameObject musicButton;
    public Sprite[] musicSprites = new Sprite[2];

    [Header("Choose settings")]
    public QuizCard[] quizzes;
    [HideInInspector] public int chosenQuizIndex = -1;

    [Header("Questions settings")]
    public QuestionConfig questionConfig;
    public bool shouldShuffle; // Следует ли рандомизировать порядок вопросов
    [HideInInspector] public int chosenLevelIndex = -1;

    [Header("Controllers")]
    public ChooseController chooseController;
    public MenuController menuController;
    public QuestionController questionController;
    public ResultController resultController;

    // Bootstrap для всей игры. На старте запускает инициализацию меню 
    public void Awake()
    {
        OpenedLevels.Clear(); // Убрать
        YandexGame.savesData.passedLevels = 0; // ! УБРАТЬ ПЕРЕД РЕЛИЗОМ ТРИ СТРОКИ
        YandexGame.SaveProgress(); // ! СТРОКИ ДЛЯ ДЕБАГА

        ChooseController.gameManager = this;
        MenuController.gameManager = this;
        QuestionController.gameManager = this;
        ResultController.gameManager = this;
        chooseController.OnGameStart(quizzes);

        if (!YandexGame.savesData.isMusicPlaying)
        {
            GetComponent<AudioSource>().Stop();
            musicButton.GetComponent<Image>().sprite = musicSprites[0];
            musicButton.transform.localScale = new Vector3(0.94f, 0.94f, 0.94f);
        }
        else
        {
            musicButton.GetComponent<Image>().sprite = musicSprites[1];
            musicButton.transform.localScale = Vector3.one;
        }
    }

    public GameState GetGameState() { return state; }

    public static bool ChangeCash(int cost)
    {
        if (cost < 0 && YandexGame.savesData.cash < Math.Abs(cost))
        {
            return false;
        }
        YandexGame.savesData.cash += cost;
        YandexGame.SaveProgress();
        return true;
    }

    public bool IsLevelWasOpened(int quizIndex, int levelIndex)
    {
        return OpenedLevels.Any(obj => obj.first == quizIndex && obj.second == levelIndex);
    }

    private void LoadNewWindow(Transform oldWindow, Transform newWindow)
    {
        oldWindow.parent.gameObject.SetActive(false);
        newWindow.parent.gameObject.SetActive(true);
    }

    // Общая функция для основного потока переключения окон
    // Основной поток: Выбор теста -> Выбор сложности -> Сам тест -> Результат -> Сам тест...
    public void NextStep(int requiredInt, Transform currentWindow)
    {
        if (currentWindow.TryGetComponent(out ChooseController _))
        {
            chosenQuizIndex = requiredInt;
            state = GameState.InLevelMenu;
            LoadNewWindow(currentWindow, menuController.transform);
            menuController.Init();
        }
        else if (currentWindow.TryGetComponent(out MenuController _))
        {
            chosenLevelIndex = requiredInt;
            state = GameState.SolvingQuestions;
            LoadNewWindow(currentWindow, questionController.transform);
            questionController.Init(quizzes[chosenQuizIndex].testContainer);
        }
        else if (currentWindow.TryGetComponent(out QuestionController _))
        {
            state = GameState.GettingResults;
            LoadNewWindow(currentWindow, resultController.transform);
            resultController.Init(requiredInt);
        }
        else if (currentWindow.TryGetComponent(out ResultController _))
        {
            state = GameState.SolvingQuestions;
            LoadNewWindow(currentWindow, questionController.transform);
            questionController.Init(quizzes[chosenQuizIndex].testContainer);
        }
    }

    public void RestartLastQuiz(Transform window) { NextStep(chosenLevelIndex, window); }

    // Общая функция для всех кнопок возвращения в меню
    public void BackInMenu(Transform lastWindow)
    {
        state = GameState.ChoosingQuiz;
        LoadNewWindow(lastWindow, chooseController.transform);
    }

    // Функция для обработки нажатия кнопки включения/выключения музыки
    public void MusicButtonPressed()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource.isPlaying && YandexGame.savesData.isMusicPlaying)
        {
            YandexGame.savesData.isMusicPlaying = false;
            YandexGame.SaveProgress();

            audioSource.Stop();
            musicButton.GetComponent<Image>().sprite = musicSprites[0];
            musicButton.transform.localScale = new Vector3(0.94f, 0.94f, 0.94f);
        }
        else
        {
            YandexGame.savesData.isMusicPlaying = true;
            YandexGame.SaveProgress();

            audioSource.Play();
            musicButton.GetComponent<Image>().sprite = musicSprites[1];
            musicButton.transform.localScale = Vector3.one;
        }
    }
}
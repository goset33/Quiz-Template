using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

        // Ставить локализацию

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

    // Проверяет в сохранениях был ли уровен открыт
    public bool IsLevelWasOpened(int quizIndex, int levelIndex)
    {
        return OpenedLevels.Any(obj => obj.first == quizIndex && obj.second == levelIndex);
    }

    // Общая функция для переключения всех окон в игре.
    // В случае ошибки (например отсутствия integer когда он нужен) перенаправляет игрока на окно выбора теста
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
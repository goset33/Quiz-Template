using DG.Tweening.Core.Easing;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using YG;
using Random = UnityEngine.Random;

public class QuestionController : MonoBehaviour
{
    public static GameManager gameManager;
    public static event Action<int, int, bool> QuestionsEnded;
    public static event Action NextQuestionLoaded, WrongAnswer;

    private int rightIndex;
    private List<GameObject> choosedSequence = new(); // Хранит кнопки в последовательности нажатия
    private GameObject[] rightSequence = new GameObject[0]; // Хранит кнопки в правильной последовательности

    private int hardness; // 0 - FTUE, дальше как обычно
    private int[] questionsHardness = null; // Массив, равный количеству вопросов и показывающий уровень сложности каждого вопроса
    private int reviveCount = 2;
    private bool isAnswerShowed = false;
    private List<IQuestion> cards = new();

    public int currentQuestion;
    public int rightAnswers;
    public bool isWinning = true;
    [SerializeField] private GameObject loseCounterPrefab;

    [Space]
    [SerializeField] private GameObject showRightButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private TextMeshProUGUI questionText, counterText;
    [SerializeField] private HeartContainer heartContainer;
    [SerializeField] private TimerHandler timerHandler;
    //[SerializeField] private Image imageField;
    [SerializeField] private RectTransform answersParent;
    [SerializeField] private GameObject answerButtonPrefab;

    [Header("Locales")]
    [SerializeField] private LocalizedString[] backInMenuLocales;
    [SerializeField] private LocalizedString[] showAnswerLocales, lackOfLivesLocales, outOfTimeLocales, getHintLocales;

    private async void OnEnable()
    {
        TimerHandler.OnTimeEnd += OnTimeEnd;
        await Init(gameManager.chosenQuiz);
    }

    private void OnDisable()
    {
        TimerHandler.OnTimeEnd -= OnTimeEnd;
    }

    /// <summary>
    /// Инициализация контроллера. Вызывается автоматически при включении объекта со скриптом
    /// </summary>
    /// <param name="quizCard">Сам экземпляр квиза</param>
    private async Task Init(QuizCard quizCard)
    {
        hardness = gameManager.GetQuizHardness();

        if (hardness == 0) YG2.saves.isFirstQuiz = false;

        QuestionContainer container = quizCard.testContainer;
        if (container == null)
        {
            gameManager.InvokeNotification(2);
            gameManager.ReturnToMenu(transform);
            return;
        }

        await container.LoadQuestionsAsync();
        List<IQuestion> allPool = new(container.Questions);
        if (gameManager.shouldShuffle)
        {
            allPool = MixQuestions(allPool);
        }

        int lookupIndex = (hardness != 0) ? (hardness - 1) : hardness;

        int amount = quizCard.questionsAmount[lookupIndex];
        cards = amount == 0 ? allPool : new List<IQuestion>(allPool.Take(amount));

        int[] difficulties = gameManager.config.questionsHardness[hardness];
        difficulties.MultiplyArray(Mathf.RoundToInt(quizCard.questionsAmount[lookupIndex] / 10f));
        questionsHardness = difficulties.SelectMany((x, i) => Enumerable.Repeat(i, x)).OrderBy(_ => Random.value).Take(cards.Count).ToArray();

        timerHandler.gameObject.SetActive(hardness > 1);
        if (hardness > 1)
        {
            float T = gameManager.config.questionTimer;
            float time = hardness == 2 ? T : (hardness == 3 ? T / 2f : T / 10f);
            bool isGlobal = hardness < 4;

            timerHandler.InitializeTime(time, isGlobal);
        }

        heartContainer.InitializeHearts(gameManager.config.harndessHeartCount[lookupIndex]);

        ClearScreen();
        reviveCount = 0;
        currentQuestion = 1;
        rightAnswers = 0;
        isWinning = true;

        // Для нейронки
        //string json = await AIRequestHandler.GenerateQuestionsAsync(quizCard.names[0], quizCard.questionsAmount[hardness]);
        //cards = AIAnswerParser.ParseJsonAnswer(json);
        //cards = MixQuestions(cards);

        LoadNextQuestion(cards[currentQuestion - 1]);
    }

    /// <summary>
    /// Метод для рандомизации входящего списка из IQuestion
    /// </summary>
    /// <param name="inputQuestions">Входной, не рандомизированный список</param>
    /// <returns>Рандомизированный список</returns>
    private List<IQuestion> MixQuestions(List<IQuestion> inputQuestions)
    {
        if (!gameManager.shouldShuffle) return inputQuestions;

        List<IQuestion> questions = new(inputQuestions);
        int n = questions.Count;

        // Базовая рандомизация (алгоритм Фишера-Йейтса)
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            IQuestion temp = questions[i];
            questions[i] = questions[j];
            questions[j] = temp;
        }

        // Поиск что нужно заменить и замена
        for (int i = 0; i < n; i++)
        {
            IQuestion current = questions[i];
            IQuestion prev = i > 0 ? questions[i - 1] : null;
            IQuestion next = i < n - 1 ? questions[i + 1] : null;

            if (current.GetType() == typeof(MainTypeQuestion)) continue;

            if (prev != null && prev.GetType() == current.GetType() ||
                next != null && next.GetType() == current.GetType())
            {
                bool replaced = false;
                for (int j = 0; j < n && !replaced; j++)
                {
                    IQuestion replaceable = questions[j];
                    IQuestion prev1 = j > 0 ? questions[j - 1] : null;
                    IQuestion next1 = j < n - 1 ? questions[j + 1] : null;

                    if ((prev1 == null || prev1.GetType() != current.GetType())
                        && (next1 == null || next1.GetType() != current.GetType()))
                    {
                        questions[j] = current;
                        questions[i] = replaceable;
                        replaced = true;
                    }
                }
                if (!replaced)
                {
                    Debug.LogWarning($"Не удалось заменить вопрос {current} на позиции {i}");
                }
            }
        }

        return questions;
    }

    /// <summary>
    /// Переносит данные из класса вопроса в интерфейс
    /// </summary>
    /// <param name="card">Карточка вопроса</param>
    private void LoadNextQuestion(IQuestion card)
    {
        questionText.text = card.QuestionText;
        //imageField.sprite = card.Image;
        counterText.text = $"{currentQuestion}/{cards.Count}";

        if (card is MainTypeQuestion question)
        {
            var wrongs = question.WrongAnswers.OrderBy(_ => Random.value).Take(Mathf.Min(questionsHardness[currentQuestion - 1] + 1, 3)).ToList(); // Неправильные ответы рандомно
            List<string> allAnswers = new(wrongs.Append(question.RightAnswer).OrderBy(_ => Random.value)); // Все рандомные варианты ответов
            for (int i = 0; i < allAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() => DefaultAnswer(index));
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allAnswers[i];

                if (allAnswers[i] == question.RightAnswer)
                {
                    rightIndex = i;
                    print("Right index: " + i);
                }
            }
        }
        else if (card is CounterQuestion)
        {
            choosedSequence.Clear();
            rightSequence = new GameObject[hardness + 2];

            List<string> answers = new(card.AllAnswers.Take(Mathf.Min(questionsHardness[currentQuestion - 1] + 2, 4)));
            List<string> randomizedAnswers = answers.OrderBy(_ => Random.value).ToList();
            for (int i = 0; i < randomizedAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = randomizedAnswers[i];
                rightSequence[answers.IndexOf(randomizedAnswers[i])] = button;
                button.GetComponent<Button>().onClick.AddListener(() => CountAnswer(button));
            }
        }
        else if (card is ConnectQuestion)
        {

        }

        NextQuestionLoaded?.Invoke();
    }

    // Обработка нажатия кнопки при вопросе типов 1, 2
    private void DefaultAnswer(int index)
    {
        if (nextButton.activeSelf) return;

        Image pressedButtonImage = answersParent.GetChild(index).GetComponent<Image>();
        bool isRight = index == rightIndex;
        var sprite = isRight ? gameManager.questionConfig.rightAnswerSprite : gameManager.questionConfig.wrongAnswerSprite;
        var color = isRight ? gameManager.questionConfig.rightButtonColor : gameManager.questionConfig.wrongButtonColor;

        if (sprite != null) pressedButtonImage.sprite = sprite;
        pressedButtonImage.color = color;

        Answered(isRight);
    }

    // Обработка нажатия кнопки при вопросе типа 3
    private void CountAnswer(GameObject pressedButton)
    {
        if (nextButton.activeSelf) return;

        UpdateButtonIndexes(pressedButton);
        if (choosedSequence.Count == answersParent.childCount)
        {
            int rightCounter = 0;
            for (int i = 0; i < choosedSequence.Count; i++)
            {
                if (choosedSequence[i] == rightSequence[i])
                {
                    rightCounter++;
                }
            }

            bool isRight = rightCounter == answersParent.childCount;
            if (isRight)
            {
                for (int i = 0; i < answersParent.childCount; i++)
                {
                    Image image = answersParent.GetChild(i).GetComponent<Image>();
                    image.sprite = gameManager.questionConfig.rightAnswerSprite;
                    image.color = gameManager.questionConfig.rightButtonColor;
                }
            }
            else
            {
                for (int i = 0; i < answersParent.childCount; i++)
                {
                    Image image = answersParent.GetChild(i).GetComponent<Image>();
                    image.sprite = gameManager.questionConfig.wrongAnswerSprite;
                    image.color = gameManager.questionConfig.wrongButtonColor;
                }
            }
            Answered(isRight);
        }
    }

    // Метод обновляет индексы на кнопках для типа 3
    private void UpdateButtonIndexes(GameObject changingButton)
    {
        if (changingButton != null)
        {
            GameObject changedText = changingButton.transform.GetChild(1).gameObject;
            changedText.SetActive(!changedText.activeSelf);
            if (changedText.activeSelf)
            {
                choosedSequence.Add(changingButton);
            }
            else
            {
                choosedSequence.Remove(changingButton);
            }
        }
        else
        {
            foreach (GameObject obj in choosedSequence)
            {
                obj.transform.GetChild(1).gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < choosedSequence.Count; i++)
        {
            choosedSequence[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
        }
    }

    /// <summary>
    /// Общие действия когда игрок дает ответ на вопрос
    /// </summary>
    /// <param name="isRight">Был ли ответ верным</param>
    private void Answered(bool isRight)
    {
        if (isRight)
        {
            rightAnswers++;
            gameManager.AddExperience(1);
            GameManager.ChangeCash(1);
            NextButtonPressed();
            print("Right!");
        }
        else
        {
            WrongAnswer?.Invoke();
            heartContainer.TakeOneDamage();
            if (heartContainer.HeartCount == 0)
            {
                if (reviveCount < 2)
                {
                    reviveCount++;
                    gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Medium, loseCounterPrefab, lackOfLivesLocales));
                    TimelessController.OnButtonPressed += WhenDeathButtonPressed;
                }
                else
                {
                    isWinning = false;
                    currentQuestion = cards.Count;
                }
            }

            showRightButton.SetActive(true);
            nextButton.SetActive(true);
            print("Incorrect!");
        }
    }

    private void WhenDeathButtonPressed(int buttonIndex)
    {
        TimelessController.OnButtonPressed -= WhenDeathButtonPressed;
        if (buttonIndex == 0)
        {
            isWinning = false;
            Finish();
        }
        else if (buttonIndex == 1)
        {
            YG2.RewardedAdvShow("0", () => heartContainer.HealOneHeart());
        }
    }

    private void OnTimeEnd()
    {
        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Medium, loseCounterPrefab, outOfTimeLocales));
        TimelessController.OnButtonPressed += TimeEndButtonPressed;
    }

    private void TimeEndButtonPressed(int buttonIndex)
    {
        if (buttonIndex == 0)
        {
            isWinning = false;
            Finish();
        }
        else if (buttonIndex == 1)
        {
            float T = gameManager.config.questionExtraTime;
            float time = hardness == 2 ? T : (hardness == 3 ? T / 2f : T / 10f);
            YG2.RewardedAdvShow("3", () => timerHandler.RestoreSomeTime(time));
        }
    }

    /// <summary>
    /// Вызывается при нажатии кнопки следующего вопроса
    /// </summary>
    public void NextButtonPressed()
    {
        ClearScreen();
        YG2.InterstitialAdvShow();

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Tab))
        {
            currentQuestion = cards.Count;
            gameManager.AddExperience(currentQuestion);
        }
#endif

        if (currentQuestion != cards.Count) // Если вопрос был не последний
        {
            currentQuestion++;
            LoadNextQuestion(cards[currentQuestion - 1]);
        }
        else
        {
            Finish();
        }
    }

    /// <summary>
    /// Показывает правильный ответ за валюту
    /// </summary>
    /// <param name="buttonIndex">0 = нет, 1 = да</param>
    public void BuyRightAnswerButtonPressed()
    {
        if (isAnswerShowed) return;

        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, showAnswerLocales));
        TimelessController.OnButtonPressed += BuyRightAnswer;
    }

    private void BuyRightAnswer(int pressedIndex)
    {
        TimelessController.OnButtonPressed -= BuyRightAnswer;
        if (pressedIndex != 1) return;

        if (GameManager.HaveEnoughCash(-1))
        {
            GameManager.ChangeCash(-1);
            ShowRightAnswer();
        }
        else
        {
            gameManager.InvokeNotification(0);
        }
    }

    private void ShowRightAnswer()
    {
        isAnswerShowed = true;
        if (cards[currentQuestion - 1] is MainTypeQuestion)
        {
            Image rightButton = answersParent.GetChild(rightIndex).GetComponent<Image>();
            rightButton.sprite = gameManager.questionConfig.rightAnswerSprite;
            rightButton.color = gameManager.questionConfig.rightButtonColor;
        }
        else if (cards[currentQuestion - 1] is CounterQuestion)
        {
            choosedSequence = rightSequence.ToList();

            for (int i = 0; i < choosedSequence.Count; i++)
            {

                Image button = answersParent.GetChild(i).GetComponent<Image>();
                button.sprite = gameManager.questionConfig.rightAnswerSprite;
                button.color = gameManager.questionConfig.rightButtonColor;
            }
            UpdateButtonIndexes(null);
        }
    }

    public void GetHintPressed()
    {
        if (hardness == 4) return;

        timerHandler.PauseTime();
        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, getHintLocales));
        TimelessController.OnButtonPressed += GetHint;
    }

    private void GetHint(int pressedIndex)
    {
        TimelessController.OnButtonPressed -= GetHint;
        timerHandler.PauseTime();
        if (pressedIndex == 1)
        {
            YG2.RewardedAdvShow("2", ShowRightAnswer);
        }
    }

    /// <summary>
    /// Чистит экран и обнуляет все что нужно обнулить. Вызывать после каждого вопроса
    /// </summary>
    private void ClearScreen()
    {
        isAnswerShowed = false;
        nextButton.SetActive(false);
        showRightButton.SetActive(false);
        for (int i = 0; i < answersParent.childCount; i++)
        {
            Destroy(answersParent.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Вызывает конец теста
    /// </summary>
    private void Finish()
    {
        //if (isWinning) gameManager.IncrementQuizHardness();

        QuestionsEnded?.Invoke(rightAnswers, cards.Count, isWinning);
    }

    public void MenuButtonPressed()
    {
        timerHandler.PauseTime();
        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, backInMenuLocales));
        TimelessController.OnButtonPressed += BackToMenu;
    }

    private void BackToMenu(int pressedIndex)
    {
        TimelessController.OnButtonPressed -= BackToMenu;
        timerHandler.PauseTime();
        if (pressedIndex == 1)
        {
            gameManager.ReturnToMenu(transform);
        }
    }
}

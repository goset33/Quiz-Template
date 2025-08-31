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
    private GameManager gameManager;

    public static event Action<int, int, int, bool> QuestionsEnded;
    public static event Action AllReady, NextQuestionLoaded, WrongAnswer;

    private int rightIndex;
    private List<GameObject> choosedSequence = new(); // Хранит кнопки в последовательности нажатия
    private GameObject[] rightSequence = new GameObject[0]; // Хранит кнопки в правильной последовательности

    private int quizHardness; // 0 - FTUE, дальше как обычно
    private int[] questionsHardness = null; // Массив, равный количеству вопросов и показывающий уровень сложности каждого вопроса
    private int QuestionDifficult => questionsHardness[currentQuestion - 1];
    private int AccruedCash => gameManager.config.cashAddCount[quizHardness - 1];

    private readonly Color[] difficultColors = new Color[4] { Color.green, Color.yellow, Color.red, new(105f, 0f, 198f) };
    private int reviveCount = 2;
    private bool isAnswerShowed = false;
    private List<IQuestion> cards = new();

    public int currentQuestion; // Хранит текущий номер вопроса НАЧИНАЯ С 1. (В коде требуется отнимать 1)
    public int rightAnswers;
    public bool isWinning = true;
    [SerializeField] private GameObject loseCounterPrefab;

    [Space]
    [SerializeField] private GameObject showRightButton;
    [SerializeField] private GameObject nextButton, hintCross;
    [SerializeField] private TextMeshProUGUI difficultText, questionText, counterText;
    [SerializeField] private HeartContainer heartContainer;
    [SerializeField] private TimerHandler timerHandler;
    //[SerializeField] private Image imageField;
    [SerializeField] private RectTransform answersParent;
    [SerializeField] private GameObject answerButtonPrefab;

    [Header("Locales")]
    [SerializeField] private LocalizedString[] difficultiesLocales;
    [SerializeField] private LocalizedString[] backInMenuLocales, showAnswerLocales, lackOfLivesLocales, outOfTimeLocales, getHintLocales;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    private async void OnEnable()
    {
        TimerHandler.OnTimeEnd += OnTimeEnd;
        YG2.onErrorRewardedAdv += OnAdError;
        await Init(gameManager.chosenQuiz);
    }

    private void OnDisable()
    {
        TimerHandler.OnTimeEnd -= OnTimeEnd;
        YG2.onErrorRewardedAdv -= OnAdError;
    }

    /// <summary>
    /// Инициализация контроллера. Вызывается автоматически при включении объекта со скриптом
    /// </summary>
    /// <param name="quizCard">Сам экземпляр квиза</param>
    private async Task Init(QuizCard quizCard)
    {
        quizHardness = gameManager.GetQuizHardness();

        if (quizHardness == 0) YG2.saves.isFirstQuiz = false;

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

        int lookupIndex = (quizHardness != 0) ? (quizHardness - 1) : quizHardness;

        int amount = quizCard.questionsAmount[lookupIndex];
        cards = amount == 0 ? allPool : new List<IQuestion>(allPool.Take(amount));

        int[] difficulties = gameManager.config.questionsHardness[quizHardness];
        difficulties.MultiplyArray(Mathf.RoundToInt(quizCard.questionsAmount[lookupIndex] / 10f));
        questionsHardness = difficulties.SelectMany((x, i) => Enumerable.Repeat(i, x)).OrderBy(_ => Random.value).Take(cards.Count).ToArray();

        timerHandler.gameObject.SetActive(quizHardness > 1);
        if (quizHardness > 1)
        {
            float T = gameManager.config.questionTimer;
            float time = quizHardness == 2 ? T : (quizHardness == 3 ? T / 2f : T / 10f);
            bool isGlobal = quizHardness < 4;

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

        Dictionary<string, object> data = new() { { "Имя квиза", gameManager.chosenQuiz.GetName() }, { "Уровень сложности квиза", quizHardness } };
        YG2.MetricaSend("QuizStart", data);

        LoadNextQuestion(cards[currentQuestion - 1]);
        AllReady?.Invoke();
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

        return questions;
    }

    /// <summary>
    /// Переносит данные из класса вопроса в интерфейс
    /// </summary>
    /// <param name="card">Карточка вопроса</param>
    private void LoadNextQuestion(IQuestion card)
    {
        if (!isWinning)
        {
            Finish();
            return;
        }

        difficultText.text = difficultiesLocales[QuestionDifficult].GetLocalizedString();
        difficultText.color = difficultColors[QuestionDifficult];
        questionText.text = card.QuestionText;
        //imageField.sprite = card.Image;
        counterText.text = $"{currentQuestion}/{cards.Count}";
        if (QuestionDifficult == 3)
        {
            hintCross.SetActive(true);
        }

        if (card is MainTypeQuestion question)
        {
            var wrongs = question.WrongAnswers.OrderBy(_ => Random.value).Take(Mathf.Min(QuestionDifficult + 1, 3)).ToList(); // Неправильные ответы рандомно
            List<string> allAnswers = new(wrongs.Append(question.RightAnswer).OrderBy(_ => Random.value)); // Все рандомные варианты ответов
            for (int i = 0; i < allAnswers.Count; i++)
            {
                Button button = Instantiate(answerButtonPrefab, answersParent).GetComponent<Button>();

                int index = i;
                button.onClick.AddListener(() => DefaultAnswer(index));
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allAnswers[i];

                if (allAnswers[i] == question.RightAnswer)
                {
                    rightIndex = i;
                    SoundManager.Instance.AddUniqueSoundToButton(button, 1);
#if UNITY_EDITOR
                    print("Right index: " + i);
#endif
                }
                else
                {
                    SoundManager.Instance.AddUniqueSoundToButton(button, 2);
                }
            }
        }
        else if (card is CounterQuestion)
        {
            choosedSequence.Clear();
            rightSequence = new GameObject[quizHardness + 2];

            List<string> answers = new(card.AllAnswers.Take(Mathf.Min(QuestionDifficult + 2, 4)));
            List<string> randomizedAnswers = answers.OrderBy(_ => Random.value).ToList();
            for (int i = 0; i < randomizedAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = randomizedAnswers[i];
                rightSequence[answers.IndexOf(randomizedAnswers[i])] = button;
                button.GetComponent<Button>().onClick.AddListener(() => CountAnswer(button));
            }
        }

        NextQuestionLoaded?.Invoke();
    }

    /// <summary>
    /// Обработка нажатия кнопки при вопросе типов 1, 2
    /// </summary>
    /// <param name="index">Индекс кнопки</param>
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

    /// <summary>
    /// Обработка нажатия кнопки при вопросе типа 3
    /// </summary>
    /// <param name="pressedButton">Экземпляр нажатой кнопки</param>
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

    /// <summary>
    /// Метод обновляет индексы на кнопках для типа 3
    /// </summary>
    /// <param name="changingButton">Экземпляр нажатой кнопки</param>
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
        Dictionary<string, object> data = new() { { "Правильный ответ?", isRight }, { "Уровень сложности вопроса", QuestionDifficult }, { "Текст вопроса", cards[currentQuestion - 1].QuestionText } };
        YG2.MetricaSend("GivesAnswer", data);

        if (isRight)
        {
            rightAnswers++;
            gameManager.AddExperience(1);
            GameManager.ChangeCash(AccruedCash);
            NextButtonPressed();
            print("Right!");
        }
        else
        {
            WrongAnswer?.Invoke();
            heartContainer.TakeOneDamage();

            foreach (Button button in answersParent.GetComponentsInChildren<Button>())
            {
                button.onClick.RemoveAllListeners();
            }

            if (heartContainer.HeartCount == 0)
            {
                if (reviveCount < 2)
                {
                    reviveCount++;
                    gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Medium, loseCounterPrefab, lackOfLivesLocales));
                    TimelessController.OnPopupButtonPressed += WhenDeathButtonPressed;
                }
                else
                {
                    isWinning = false;
                }
            }

            showRightButton.SetActive(true);
            nextButton.SetActive(true);
            print("Incorrect!");
        }
    }

    private void WhenDeathButtonPressed(int buttonIndex)
    {
        TimelessController.OnPopupButtonPressed -= WhenDeathButtonPressed;
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
        if (reviveCount < 2)
        {
            reviveCount++;
            gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Medium, loseCounterPrefab, outOfTimeLocales));
            TimelessController.OnPopupButtonPressed += TimeEndButtonPressed;
        }
        else
        {
            isWinning = false;
        }
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
            float time = quizHardness == 2 ? T : (quizHardness == 3 ? T / 2f : T / 10f);
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

        if (YG2.envir.payload == "~AdminPanel~-Shift+Tab"
#if UNITY_EDITOR
            || true
#endif
           )
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Tab))
            {
                currentQuestion = cards.Count;
                gameManager.AddExperience(currentQuestion);
            }
        }


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
    /// Метод показывает правильный ответ на вопрос
    /// </summary>
    private void ShowRightAnswer()
    {
        isAnswerShowed = true;
        if (cards[currentQuestion - 1] is MainTypeQuestion)
        {
            Image rightButton = answersParent.GetChild(rightIndex).GetComponent<Image>();
            rightButton.sprite = gameManager.questionConfig.rightAnswerSprite != null ? gameManager.questionConfig.rightAnswerSprite : rightButton.sprite;
            rightButton.color = gameManager.questionConfig.rightButtonColor;
        }
        else if (cards[currentQuestion - 1] is CounterQuestion)
        {
            choosedSequence = rightSequence.ToList();

            for (int i = 0; i < choosedSequence.Count; i++)
            {
                Image button = answersParent.GetChild(i).GetComponent<Image>();
                button.sprite = gameManager.questionConfig.rightAnswerSprite != null ? gameManager.questionConfig.rightAnswerSprite : button.sprite;
                button.color = gameManager.questionConfig.rightButtonColor;
            }
            UpdateButtonIndexes(null);
        }
    }

    /// <summary>
    /// Метод вызывается при нажатии кнопки показа правильного ответа
    /// </summary>
    public void TellRightAnswerPressed()
    {
        if (isAnswerShowed) return;

        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, showAnswerLocales));
        TimelessController.OnPopupButtonPressed += GetAnswerTold;
    }

    private void GetAnswerTold(int pressedIndex)
    {
        TimelessController.OnPopupButtonPressed -= GetAnswerTold;
        timerHandler.ChangeTimePauseState();
        if (pressedIndex == 1)
        {
            YG2.RewardedAdvShow("4", ShowAnswerTold);
        }
    }

    private void ShowAnswerTold()
    {
        Dictionary<string, object> data = new() { 
            { "Имя квиза", gameManager.chosenQuiz.GetName() }, 
            { "Уровень сложности квиза", quizHardness }, 
            { "Уровень сложности вопроса", QuestionDifficult },
            { "Текст вопроса", cards[currentQuestion - 1].QuestionText } };
        YG2.MetricaSend("AnswerTold", data);
        ShowRightAnswer();
    }

    /// <summary>
    /// Метод вызывается при нажатии кнопки подсказки
    /// </summary>
    public void GetHintPressed()
    {
        if (QuestionDifficult == 3 || isAnswerShowed) return;

        timerHandler.ChangeTimePauseState();
        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, getHintLocales));
        TimelessController.OnPopupButtonPressed += GetHint;
    }

    private void GetHint(int pressedIndex)
    {
        TimelessController.OnPopupButtonPressed -= GetHint;
        timerHandler.ChangeTimePauseState();
        if (pressedIndex == 1)
        {
            YG2.RewardedAdvShow("2", ShowHintAnswer);
        }
    }

    private void ShowHintAnswer()
    {
        Dictionary<string, object> data = new() { 
            { "Имя квиза", gameManager.chosenQuiz.GetName() }, 
            { "Уровень сложности квиза", quizHardness }, 
            { "Уровень сложности вопроса", QuestionDifficult },
            { "Текст вопроса", cards[currentQuestion - 1].QuestionText }};
        YG2.MetricaSend("HintUsed", data);
        ShowRightAnswer();
    }

    /// <summary>
    /// Чистит экран и обнуляет все что нужно обнулить. Вызывать после каждого вопроса
    /// </summary>
    private void ClearScreen()
    {
        isAnswerShowed = false;
        nextButton.SetActive(false);
        showRightButton.SetActive(false);
        hintCross.SetActive(false);
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

        QuestionsEnded?.Invoke(rightAnswers, cards.Count, reviveCount, isWinning);
    }

    /// <summary>
    /// Обрабатывает ошибки в показе рекламы за вознаграждение
    /// </summary>
    /// <param name="id">ID показанной рекламы</param>
    private void OnAdError(string id)
    {
        if (id == "0" || id == "3")
        {
            reviveCount--;
            isWinning = false;
        }
    }

    public void MenuButtonPressed()
    {
        timerHandler.ChangeTimePauseState();
        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, backInMenuLocales));
        TimelessController.OnPopupButtonPressed += BackToMenu;
    }

    private void BackToMenu(int pressedIndex)
    {
        TimelessController.OnPopupButtonPressed -= BackToMenu;
        timerHandler.ChangeTimePauseState();
        if (pressedIndex == 1)
        {
            Dictionary<string, object> data = new() { 
                { "Имя квиза", gameManager.chosenQuiz.GetName() }, 
                { "Уровень сложности квиза", quizHardness }, 
                { "Номер последнего вопроса", currentQuestion },    
                { "Сложность последнего вопроса", QuestionDifficult },
                { "Текст последнего вопроса", cards[currentQuestion - 1].QuestionText },
                { "Количество возрождений", reviveCount },
                { "Количество сердец", heartContainer.HeartCount } };
            YG2.MetricaSend("QuizLeave", data);
            gameManager.ReturnToMenu(transform);
        }
    }
}

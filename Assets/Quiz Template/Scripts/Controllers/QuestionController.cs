using System;
using System.Collections.Generic;
using System.Linq;
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

    private int rightIndex;
    private List<GameObject> choosedSequence = new(); // Хранит кнопки в последовательности нажатия
    private GameObject[] rightSequence = new GameObject[0]; // Хранит кнопки в правильной последовательности

    private int hardness;
    private int reviveCount = 2;
    private bool isAnswerShowed = false;
    private List<IQuestion> cards = new();

    public int currentQuestion;
    public int rightAnswers;
    [SerializeField] private GameObject loseCounterPrefab;

    [Space]
    [SerializeField] private GameObject showRightButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private TextMeshProUGUI questionText, counterText;
    [SerializeField] private HeartContainer heartContainer;
    [SerializeField] private Image imageField;
    [SerializeField] private RectTransform answersParent;
    [SerializeField] private GameObject answerButtonPrefab;

    [Header("Locales")]
    [SerializeField] private LocalizedString[] backInMenuLocales;
    [SerializeField] private LocalizedString[] showAnswerLocales, lackOfLivesLocales;

    private void OnEnable()
    {
        Init(gameManager.chosenQuiz);
    }

    /// <summary>
    /// Инициализация контроллера. Вызывается автоматически при включении объекта со скриптом
    /// </summary>
    /// <param name="quizCard">Сам экземпляр квиза</param>
    private async void Init(QuizCard quizCard)
    {
        //List<IQuestion> allPool = new(container.QuestionCards);
        //if (gameManager.shouldShuffle)
        //{
        //    allPool = MixQuestions(allPool);
        //}

        //int amount = 0;
        //if (gameManager.chosenHardnessIndex == 0)
        //{
        //    amount = container.easyAmount;
        //}
        //else if (gameManager.chosenHardnessIndex == 1)
        //{
        //    amount = container.mediumAmount;
        //}
        //else if (gameManager.chosenHardnessIndex == 2)
        //{
        //    amount = container.hardAmount;
        //}
        //cards = amount == 0 ? allPool : new List<IQuestion>(allPool.Take(amount));

        ClearScreen();
        heartContainer.InitializeHearts(gameManager.startHeartsCount);

        reviveCount = 0;
        currentQuestion = 1;
        rightAnswers = 0;

        hardness = GameManager.GetQuizHardness(quizCard);
        string json = await AIRequestHandler.GenerateQuestionsAsync(quizCard.names[0], quizCard.questionsAmount[hardness]);
        cards = AIAnswerParser.ParseJsonAnswer(json);
        cards = MixQuestions(cards);
        
        LoadNextQuestion(cards[currentQuestion - 1]);
    }

    /// <summary>
    /// Метод для рандомизации входящего списка из IQuestion
    /// </summary>
    /// <param name="inputQuestions">Входной, не рандомизированный список</param>
    /// <returns>Рандомизированный список</returns>
    private List<IQuestion> MixQuestions(List<IQuestion> inputQuestions)
    {
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
        imageField.sprite = card.Image;
        counterText.text = $"{currentQuestion}/{cards.Count}";

        if (card is MainTypeQuestion question)
        {
            List<string> wrongs = new(question.WrongAnswers.OrderBy(_ => Random.value).Take(hardness + 1)); // Рандомные неправильные ответы, отрезанные по сложности уровня
            List<string> allAnswers = new(wrongs.Append(question.RightAnswer).OrderBy(_ => Random.value)); // Рандомные варианты ответов
            for (int i = 0; i < allAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() => DefaultAnswer(index));
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allAnswers[i];

                if (allAnswers[i] == question.RightAnswer)
                {
                    rightIndex = i;
                    print("Right index was set: " + i);
                }
            }
        }
        else if (card is CounterQuestion)
        {
            choosedSequence.Clear();
            rightSequence = new GameObject[hardness + 2];

            List<string> answers = new(card.AllAnswers.Take(hardness + 2));
            List<string> randomizedAnswers = new(answers.OrderBy(_ => Random.value));
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
    }

    // Обработка нажатия кнопки при вопросе типов 1, 2
    private void DefaultAnswer(int index)
    {
        if (nextButton.activeSelf) return;

        Image pressedButtonImage = answersParent.GetChild(index).GetComponent<Image>();
        bool isRight = index == rightIndex;
        if (isRight)
        {
            pressedButtonImage.sprite = gameManager.questionConfig.rightAnswerSprite;
            pressedButtonImage.color = gameManager.questionConfig.rightButtonColor;
        }
        else
        {
            pressedButtonImage.sprite = gameManager.questionConfig.wrongAnswerSprite;
            pressedButtonImage.color = gameManager.questionConfig.wrongButtonColor;
        }
        Answered(isRight);
    }

    // Обработка нажатия кнопки при вопросе типа 3
    private void CountAnswer(GameObject pressedButton)
    {
        if (nextButton.activeSelf) return;

        UpdateButtonIndexes(pressedButton);
        if (choosedSequence.Count == hardness + 2)
        {
            int rightCounter = 0;
            for (int i = 0; i < choosedSequence.Count; i++)
            {
                if (choosedSequence[i] == rightSequence[i])
                {
                    rightCounter++;
                }
            }

            bool isRight = rightCounter == hardness + 2;
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
        nextButton.SetActive(true);
        
        if (isRight)
        {
            rightAnswers++;
            GameManager.ChangeCash(1);
            print("Right!");
        }
        else
        {
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
                    QuestionsEnded?.Invoke(rightAnswers, cards.Count, false);
                }
            }

            showRightButton.SetActive(true);
            print("Incorrect!");
        }
    }

    private void WhenDeathButtonPressed(int buttonIndex)
    {
        TimelessController.OnButtonPressed -= WhenDeathButtonPressed;
        if (buttonIndex == 0)
        {
            QuestionsEnded?.Invoke(rightAnswers, cards.Count, false);
        }
        else if (buttonIndex == 1)
        {
            YG2.onRewardAdv += ReviveAfterLose;
            YG2.RewardedAdvShow("0");
        }
    }

    private void ReviveAfterLose(string id)
    {
        YG2.onRewardAdv -= ReviveAfterLose;
        if (id != "0") return;

        heartContainer.HealOneHeart();
    }

    /// <summary>
    /// Вызывается при нажатии кнопки следующего вопроса
    /// </summary>
    public void NextButtonPressed()
    {
        ClearScreen();
        YG2.InterstitialAdvShow();

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Tab)) currentQuestion = cards.Count;
#endif

        if (currentQuestion != cards.Count) // Если вопрос был не последний
        {
            currentQuestion++;
            LoadNextQuestion(cards[currentQuestion - 1]);
        }
        else
        {
            GameManager.IncrementQuizHardness(gameManager.chosenQuiz);
            QuestionsEnded?.Invoke(rightAnswers, cards.Count, true);
        }
    }

    /// <summary>
    /// Вызывается при нажатии кнопки показа правильного варианта ответа
    /// </summary>
    public void ShowRightAnswerButtonPressed()
    {
        if (isAnswerShowed) return;

        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, showAnswerLocales));
        TimelessController.OnButtonPressed += ShowRightAnswer;
    }

    /// <summary>
    /// Показывает правильный ответ
    /// </summary>
    /// <param name="buttonIndex">0 = нет, 1 = да</param>
    private void ShowRightAnswer(int buttonIndex)
    {
        TimelessController.OnButtonPressed -= ShowRightAnswer;
        if (buttonIndex != 1) return;

        if (GameManager.HaveEnoughCash(-1))
        {
            isAnswerShowed = true;
            GameManager.ChangeCash(-1);
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
        else
        {
            gameManager.InvokeNotification(0);
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

    public void MenuButtonPressed()
    {
        gameManager.InvokePopup(new PopupSettings(PopupSettings.PopupSize.Small, backInMenuLocales));
        TimelessController.OnButtonPressed += BackInMenu;
    }

    public void BackInMenu(int pressedIndex)
    {
        TimelessController.OnButtonPressed -= BackInMenu;
        if (pressedIndex == 1)
        {
            gameManager.ReturnToMenu(transform);
        }
    }
}

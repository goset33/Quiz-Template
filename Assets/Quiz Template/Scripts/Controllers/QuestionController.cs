using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionController : MonoBehaviour
{
    public static GameManager gameManager;

    public int currentQuestion;
    public int rightAnswers;

    private int rightIndex;

    private List<GameObject> choosedSequence = new(); // Хранит кнопки в последовательности нажатия
    private GameObject[] rightSequence = new GameObject[0]; // Хранит кнопки в правильной последовательности

    private bool isAnswerShowed = false;
    private List<IQuestion> cards = new();

    [Space]
    public GameObject inMenuWindow;
    public GameObject showRightButton, nextButton;
    public TextMeshProUGUI questionText, counterText;
    public Image image;
    public RectTransform answersParent;
    public GameObject answerButtonPrefab;

    private void OnEnable()
    {
        Init(gameManager.quizzes[gameManager.chosenQuizIndex].testContainer);
    }

    // Инициализация контроллера. Вызывается автоматически при включении объекта со скриптом
    // Принимает на вход контейнер с вопросами, на которые потребуется отвечать пользователю
    private void Init(CardsContainer container)
    {
        List<IQuestion> allPool = new(container.QuestionCards);
        if (gameManager.shouldShuffle)
        {
            allPool = MixQuestions(allPool);
        }

        int amount = 0;
        switch (gameManager.chosenLevelIndex)
        {
            case 0:
                amount = container.easyAmount;
                break;
            case 1:
                amount = container.mediumAmount;
                break;
            case 2:
                amount = container.hardAmount;
                break;
        }
        cards = amount == 0 ? allPool : new List<IQuestion>(allPool.Take(amount));

        currentQuestion = 1;
        rightAnswers = 0;
        LoadNextQuestion(cards[currentQuestion - 1]);
    }

    // Метод для рандомизации входящего списка из IQuestion
    // TODO: Он не совсем работает. Тупой Qwen
    private List<IQuestion> MixQuestions(List<IQuestion> inputQuestions)
    {
        List<IQuestion> questions = new(inputQuestions);
        int n = questions.Count;

        for (int i = n - 1; i > 0; i--)
        {
            // Базовая рандомизация
            int j = Random.Range(0, i + 1);
            IQuestion temp = questions[i];
            questions[i] = questions[j];
            questions[j] = temp;

            // Проверяем правила размещения
            if (i > 0 &&
                ((questions[i].GetType() == questions[i - 1].GetType() &&
                  (questions[i] is CounterQuestion || questions[i] is ConnectQuestion)) ||
                 (questions[i] is MainTypeQuestion mainA && questions[i - 1] is MainTypeQuestion mainB &&
                  mainA.Type == 2 && mainB.Type == 2)))
            {
                // Находим безопасный индекс для обмена
                int swapIndex = -1;
                for (int k = i + 1; k < n; k++)
                {
                    if (!(questions[k].GetType() == questions[i - 1].GetType() &&
                          (questions[k] is CounterQuestion || questions[k] is ConnectQuestion)) &&
                        !(questions[k] is MainTypeQuestion mainK && questions[i - 1] is MainTypeQuestion mainPrev &&
                          mainK.Type == 2 && mainPrev.Type == 2))
                    {
                        swapIndex = k;
                        break;
                    }
                }

                // Если найден безопасный индекс, производим обмен
                if (swapIndex != -1)
                {
                    IQuestion swapTemp = questions[i];
                    questions[i] = questions[swapIndex];
                    questions[swapIndex] = swapTemp;
                }
            }
        }

        return questions;
    }

    // Переносит данные из класса вопроса в интерфейс
    private void LoadNextQuestion(IQuestion card)
    {
        questionText.text = card.QuestionText;
        image.sprite = card.Image;
        counterText.text = $"{currentQuestion}/{cards.Count}";

        if (card is MainTypeQuestion question)
        {
            List<string> wrongs = new(question.WrongAnswers.OrderBy(_ => Random.value).Take(gameManager.chosenLevelIndex + 1)); // Рандомные неправильные ответы, отрезанные по сложности уровня
            List<string> allAnswers = new(wrongs.Append(question.RightAnswer).OrderBy(_ => Random.value)); // Рандомные варианты ответов
            for (int i = 0; i < allAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() => MainTypeAnswer(index));
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
            rightSequence = new GameObject[gameManager.chosenLevelIndex + 2];

            List<string> answers = new(card.AllAnswers.Take(gameManager.chosenLevelIndex + 2));
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
    private void MainTypeAnswer(int index)
    {
        if (nextButton.activeSelf) return;

        nextButton.SetActive(true);
        Image pressedButtonImage = answersParent.GetChild(index).GetComponent<Image>();
        if (index == rightIndex)
        {
            rightAnswers++;
            GameManager.ChangeCash(1);

            pressedButtonImage.sprite = gameManager.questionConfig.rightAnswerSprite;
            pressedButtonImage.color = gameManager.questionConfig.rightButtonColor;
            print("Right!");
        }
        else
        {
            showRightButton.SetActive(true);
            pressedButtonImage.sprite = gameManager.questionConfig.wrongAnswerSprite;
            pressedButtonImage.color = gameManager.questionConfig.wrongButtonColor;
            print("Incorrect!");
        }
    }

    // Обработка нажатия кнопки при вопросе типа 3
    private void CountAnswer(GameObject pressedButton)
    {
        if (nextButton.activeSelf) return;

        UpdateButtonIndexes(pressedButton);
        if (choosedSequence.Count == gameManager.chosenLevelIndex + 2)
        {
            int rightCounter = 0;
            for (int i = 0; i < choosedSequence.Count; i++)
            {
                if (choosedSequence[i] == rightSequence[i])
                {
                    rightCounter++;
                }
            }

            nextButton.SetActive(true);
            if (rightCounter == gameManager.chosenLevelIndex + 2)
            {
                rightAnswers++;
                GameManager.ChangeCash(1);
                for (int i = 0; i < answersParent.childCount; i++)
                {
                    Image image = answersParent.GetChild(i).GetComponent<Image>();
                    image.sprite = gameManager.questionConfig.rightAnswerSprite;
                    image.color = gameManager.questionConfig.rightButtonColor;
                }
            }
            else
            {
                showRightButton.SetActive(true);
                for (int i = 0; i < answersParent.childCount; i++)
                {
                    Image image = answersParent.GetChild(i).GetComponent<Image>();
                    image.sprite = gameManager.questionConfig.wrongAnswerSprite;
                    image.color = gameManager.questionConfig.wrongButtonColor;
                }
            }
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

    // Чистит экран и обнуляет все что нужно обнулить. Вызывать после каждого вопроса
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

    // Вызывается при нажатии кнопки следующего вопроса
    public void NextButtonPressed()
    {
        ClearScreen();
        if (currentQuestion != cards.Count) // Если вопрос был не последний
        {
            currentQuestion++;
            LoadNextQuestion(cards[currentQuestion - 1]);
        }
        else
        {
            gameManager.ChangeActiveWindow(transform, GameManager.GameState.GettingResults, rightAnswers);
        }
    }

    // Вызывается при нажатии кнопки показа правильного варианта ответа
    public void ShowRightAnswer()
    {
        if (GameManager.HaveEnoughCash(-1) && !isAnswerShowed)
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
    }

    public void MenuButtonPressed()
    {
        inMenuWindow.SetActive(!inMenuWindow.activeSelf);
    }

    public void BackInMenu()
    {
        ClearScreen();
        inMenuWindow.SetActive(false);
        gameManager.ChangeActiveWindow(transform, GameManager.GameState.ChoosingQuiz, null);
    }
}

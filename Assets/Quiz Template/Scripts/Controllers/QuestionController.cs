using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionController : MonoBehaviour
{
    public static GameManager gameManager;

    private int rightIndex;
    private List<DoubleInt> choosedSequence = new(); // В данном случае DoubleInt.first хранит правильный индекс, а DoubleInt.second индекс нажатой очереди

    private List<IQuestion> cards = new();

    public int currentQuestion;
    public int rightAnswers;

    [Space]
    public GameObject inMenuWindow;
    public GameObject showRightButton, nextButton;
    public TextMeshProUGUI questionText, counterText;
    public Image image;
    public RectTransform answersParent;
    public GameObject answerButtonPrefab;

    // Инициализация контроллера. Обязательно вызвать ПЕРЕД началом каждого ответа на вопросы
    // Принимает на вход контейнер с вопросами, на которые потребуется отвечать пользователю
    public void Init(CardsContainer container)
    {
        List<IQuestion> allPool = new(container.QuestionCards);
        if (gameManager.shouldShuffle)
        {
            int n = allPool.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                IQuestion temp = allPool[i];
                allPool[i] = allPool[j];
                allPool[j] = temp;
            }
        }
        cards = container.shouldCutCardPool ? new List<IQuestion>(allPool.Take(container.numberOfQuestions)) : allPool;

        currentQuestion = 1;
        rightAnswers = 0;
        LoadNextQuestion(cards[currentQuestion - 1]);
    }

    private void ClearScreen()
    {
        nextButton.SetActive(false);
        showRightButton.SetActive(false);
        for (int i = 0; i < answersParent.childCount; i++)
        {
            Destroy(answersParent.GetChild(i).gameObject);
        }
    }

    // Функция используется для занесения данных из входной переменной card в интерфейс
    private void LoadNextQuestion(IQuestion card)
    {
        questionText.text = card.QuestionText;
        image.sprite = card.Image;
        counterText.text = $"{currentQuestion}/{cards.Count}";

        if (card is MainTypeQuestion)
        {
            List<string> wrongs = new(card.OtherAnswers.OrderBy(_ => Random.value).Take(gameManager.chosenLevelIndex + 1)); // Рандомные неправильные ответы, отрезанные по сложности уровня
            List<string> allAnswers = new(wrongs.Append(card.FirstAnswer).OrderBy(_ => Random.value)); // Рандомные варианты ответов
            for (int i = 0; i < allAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() => MainTypeAnswer(index));
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allAnswers[i];

                if (allAnswers[i] == card.FirstAnswer)
                {
                    rightIndex = i;
                    print("Right index was set: " + i);
                }
            }
        }
        else if (card is CounterQuestion)
        {
            choosedSequence.Clear();
            List<string> answers = new(card.OtherAnswers.Take(gameManager.chosenLevelIndex + 2));
            List<string> randomizedAnswers = new(answers.OrderBy(_ => Random.value));
            for (int i = 0; i < randomizedAnswers.Count; i++)
            {
                GameObject button = Instantiate(answerButtonPrefab, answersParent);
                int realIndex = answers.IndexOf(randomizedAnswers[i]);
                int workIndex = i;
                button.GetComponent<Button>().onClick.AddListener(() => CountAnswer(realIndex, workIndex)); 
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = randomizedAnswers[i];
            }
        }
    }

    private void MainTypeAnswer(int index)
    {
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

    private void CountAnswer(int rightIndex, int pressedIndex)
    {
        choosedSequence.Add(new DoubleInt(rightIndex, pressedIndex));
        UpdateButtonIndexes(pressedIndex);
        if (choosedSequence.Count == gameManager.chosenLevelIndex + 2)
        {
            int rightCounter = 0;
            for (int i = 0; i < choosedSequence.Count; i++)
            {
                if (choosedSequence[i].first == i)
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

    private void UpdateButtonIndexes(int changedIndex)
    {
        GameObject changed = answersParent.GetChild(changedIndex).GetChild(1).gameObject;
        changed.SetActive(!changed.activeSelf);
        for (int i = 0; i < choosedSequence.Count; i++)
        {
            GameObject numberText = answersParent.GetChild(choosedSequence[i].second).GetChild(1).gameObject;
            if (numberText.activeSelf)
            {
                numberText.GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            }
        }
    }

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
            gameManager.NextStep(rightAnswers, transform);
        }
    }

    public void ShowRightAnswer()
    {
        if (GameManager.ChangeCash(-1)) // TODO: Кнопку можно тыкать бесконечное кол-во раз пока деньги не кончатся
        {
            // TODO: Для 3 типа не обрабатывается правильно показ ответа
            Image rightButton = answersParent.GetChild(rightIndex).GetComponent<Image>();
            rightButton.sprite = gameManager.questionConfig.rightAnswerSprite;
            rightButton.color = gameManager.questionConfig.rightButtonColor;
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
        gameManager.BackInMenu(transform);
    }
}

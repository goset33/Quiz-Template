using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz Card", menuName = "Quiz Objects/Quiz Card", order = 51)]
public class QuizCard : ScriptableObject
{
    // Количество вопросов, которое должно быть в каждой сложности (Легкая, средняя, жесткая)
    public int[] questionsAmount = new int[3];

    public string[] names = new string[1]; // Первое указанное имя будет использоваться в промпте
    public Sprite image; // Как парсить картинку в карточку квиза, но не используя Sprite (Он не сериализуется YG2)?
                         // 1. byte[] | 2. string base64 (byte[])
                         // А надо еще как-то из инспектора картинку задавать
                         // Вероятно нужно разделить ScriptableObject и данные
                         // Класс CardContent? (По типу бывшего QuestionContainer)
                         // А как еще на счет GUID (UUID)?


    //public QuestionContainer testContainer;
}

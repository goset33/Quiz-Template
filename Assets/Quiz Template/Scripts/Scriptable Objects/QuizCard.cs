using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocalizedQuestionContainer
{
    public string language; // "ru", "en"
    public QuestionContainer container;
}

[CreateAssetMenu(fileName = "New Quiz Card", menuName = "Quiz Objects/Quiz Card", order = 51)]
public class QuizCard : ScriptableObject
{
    // Количество вопросов для каждого уровня сложности (Easy, Medium, Hard, Boss)
    // Если 0, то используются все доступные вопросы
    public int[] questionsAmount = new int[4];
    public string[] names = new string[1]; // Первое имя будет использоваться в промпте
    public Sprite image;

    public const int MIN_EXP = 30; // Начальное значение опыта для уровня 1

    [Header("Localization")]
    // Список контейнеров вопросов для разных языков
    public List<LocalizedQuestionContainer> localizedContainers = new List<LocalizedQuestionContainer>();

    public string GetName()
    {
        return names[0];
    }

    public QuestionContainer GetContainerForLanguage(string language)
    {
        foreach (var localized in localizedContainers)
        {
            if (localized.container != null && 
                string.Equals(localized.language, language, StringComparison.OrdinalIgnoreCase))
            {
                return localized.container;
            }
        }

        Debug.LogError($"[QuizCard] No QuestionContainer found for language '{language}'");
        return null;
    }
}

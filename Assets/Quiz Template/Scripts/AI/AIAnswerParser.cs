using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Класс парсит JSON, который возвращает AIRequestHandler.GenerateQuestionsAsync в список из IQuestion
/// </summary>
public static class AIAnswerParser
{
    //Все еще не идеальный парсинг
    public static List<IQuestion> ParseJsonAnswer(string json)
    {
        var jsonObject = JsonConvert.DeserializeObject<QuestionsRoot>(json);
        List<IQuestion> questions = new();

        foreach (var questionData in jsonObject.Questions)
        {
            questions.Add(new MainTypeQuestion(questionData.Question, null, questionData.Answers));
        }

        return questions;
    }

    private class QuestionsRoot
    {
        public List<QuestionData> Questions { get; set; }
    }

    private class QuestionData
    {
        public string Question { get; set; } // Текст вопроса
        public string[] Answers { get; set; } // 4 варианта ответа (1 правильный, 3 неправильных)
    }
}

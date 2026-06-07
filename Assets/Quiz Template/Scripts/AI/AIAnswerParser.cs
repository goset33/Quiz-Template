using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
///  ласс парсит JSON, который возвращает <see cref="AIRequestHandler.GenerateQuestionsAsync"/> в список из <see cref="IQuestion"/>
/// </summary>
public static class AIAnswerParser
{
    //¬се еще не идеальный парсинг
    public static List<IQuestion> ParseJsonAnswer(string json)
    {
        var jsonObject = JsonConvert.DeserializeObject<QuestionsRoot>(json);
        List<IQuestion> questions = new();

        foreach (var questionData in jsonObject.Questions)
        {
            questions.Add(new MainTypeQuestion(questionData.Question, null, questionData.RightAnswer, questionData.WrongAnswers));
        }

        return questions;
    }

    private class QuestionsRoot
    {
        public List<QuestionData> Questions { get; set; }
    }

    private class QuestionData
    {
        public string Question { get; set; } // “екст вопроса
        public string RightAnswer { get; set; } // 1 правильный вариант ответа
        public string[] WrongAnswers { get; set; } // 3 неправильных варианта ответа
    }
}

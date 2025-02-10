using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExcelDataParser
{
    public List<IQuestion> ParseQuestions(List<Dictionary<string, string>> sheetData)
    {
        List<IQuestion> questions = new();
        foreach (Dictionary<string, string> row in sheetData)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(Convert.FromBase64String(row["Изображение"]));

            int type = int.Parse(row["Тип вопроса"]);
            List<string> otherAnswers = new() { row["Неправильный ответ 1"], row["Неправильный ответ 2"], row["Неправильный ответ 3"] };
            Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            IQuestion card = null;
            if (type == 1 || type == 2)
            {
                card = new MainTypeQuestion(row["Вопрос"], image, row["Правильный ответ"], otherAnswers);
            }
            else if (type == 3)
            {
                card = new CounterQuestion(row["Вопрос"], image, row["Правильный ответ"], otherAnswers);
            }
            questions.Add(card);
        }
        return questions;
    }

    public List<IQuestion> ParseQuestions(string path, string sheet)
    {
        return ParseQuestions(new ExcelReader(path).ReadSheet(sheet));
    }
}

using System;
using System.Collections.Generic;
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
            string[] allAnswers = new string[4] { row["Правильный ответ"], row["Неправильный ответ 1"], row["Неправильный ответ 2"], row["Неправильный ответ 3"] };
            Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            IQuestion card = null;
            if (type == 1)
            {
                card = new MainTypeQuestion(row["Вопрос"], image, allAnswers);
            }
            else if (type == 2)
            {
                card = new PictureQuestion(row["Вопрос"], image, allAnswers);
            }
            else if (type == 3)
            {
                card = new CounterQuestion(row["Вопрос"], image, allAnswers);
            }
            else if (type == 4)
            {
                card = new ConnectQuestion(row["Вопрос"], image, allAnswers);
            }
            questions.Add(card);
        }
        return questions;
    }

    public List<IQuestion> ParseQuestions(string path, string sheet)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(sheet)) return null;

        return ParseQuestions(new ExcelReader(path).ReadSheet(sheet));
    }
}

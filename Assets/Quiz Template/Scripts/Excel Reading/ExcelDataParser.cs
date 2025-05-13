using System.Collections.Generic;

/// <summary>
/// Класс парсит данные, полученные из ExcelReader.ReadSheet в адекватный формат списка из IQuestion
/// </summary>
public static class ExcelDataParser
{
    public static List<IQuestion> ParseQuestions(List<Dictionary<string, string>> sheetData)
    {
        List<IQuestion> questions = new();
        foreach (Dictionary<string, string> row in sheetData)
        {
            //Texture2D texture = new Texture2D(1, 1);
            //texture.LoadImage(Convert.FromBase64String(row["Изображение"]));

            //int type = int.Parse(row["Тип вопроса"]);
            //Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            string[] allAnswers = new string[4] { row["Правильный ответ"], row["Ответ 2"], row["Ответ 3"], row["Ответ 4"] };
            IQuestion card = new MainTypeQuestion(row["Вопрос"], null, allAnswers);

            //IQuestion card = null;
            //if (type == 1)
            //{
            //    card = new MainTypeQuestion(row["Вопрос"], image, allAnswers);
            //}
            //else if (type == 2)
            //{
            //    card = new PictureQuestion(row["Вопрос"], image, allAnswers);
            //}
            //else if (type == 3)
            //{
            //    card = new CounterQuestion(row["Вопрос"], image, allAnswers);
            //}
            //else if (type == 4)
            //{
            //    card = new ConnectQuestion(row["Вопрос"], image, allAnswers);
            //}
            questions.Add(card);
        }
        return questions;
    }

    public static List<IQuestion> ParseQuestions(byte[] file, string sheet)
    {
        if (file == null || file.Length == 0 || string.IsNullOrEmpty(sheet)) return null;

        return ParseQuestions(ExcelReader.ReadSheet(file, sheet));
    }
}

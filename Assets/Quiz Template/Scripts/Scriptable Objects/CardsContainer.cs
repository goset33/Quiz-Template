using UnityEngine;

[CreateAssetMenu(fileName = "New Question Container", menuName = "Quiz Objects/Question Container", order = 51)]
public class CardsContainer : ScriptableObject
{
    // Количество вопросов в каждой сложности. Если 0 то все
    public int easyAmount, mediumAmount, hardAmount = 0;

    public IQuestion[] QuestionCards
    {
        get { return ExcelDataParser.ParseQuestions(filePath, sheetName).ToArray(); }
    }

    // Ниже: поля с данными таблицы для импорта
    [HideInInspector] public string sheetName;
    [HideInInspector] public string FileName
    { 
        get { return filePath.Substring(filePath.LastIndexOf("/") + 1); }
        set { FileName = value; }
    }
    [HideInInspector] public string filePath = "";
    public string FilePath
    {
        get { return filePath; }
        set { filePath = value; }
    }
}

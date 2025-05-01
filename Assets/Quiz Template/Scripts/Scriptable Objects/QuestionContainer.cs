using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Question Container", menuName = "Quiz Objects/Question Container", order = 51)]
public class QuestionContainer : ScriptableObject
{
    public string fileName;
    [HideInInspector] public string sheetName;

    private IQuestion[] cachedQuestions;
    public IQuestion[] Questions => cachedQuestions;

    /// <summary>
    /// Асинхронный метод загрузки вопросов в переменную cachedQuestions
    /// Необходимо вызывать перед использованием Questions, так как иначе там будет null
    /// </summary>
    public async Task LoadQuestionsAsync()
    {
        if (cachedQuestions != null) return;

        byte[] fileData = await ExcelLoader.LoadFileAsync(fileName);

        if (fileData == null)
        {
            Debug.LogError($"Не удалось загрузить файл {fileName}");
            return;
        }

        var questions = ExcelDataParser.ParseQuestions(fileData, sheetName).ToArray();
        cachedQuestions = questions;
    }

    public async Task<byte[]> GetFileAsync()
    {
        return await ExcelLoader.LoadFileAsync(fileName);
    }
}

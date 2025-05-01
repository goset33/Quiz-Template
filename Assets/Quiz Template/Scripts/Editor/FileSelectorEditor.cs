using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestionContainer))]
public class FileSelectorEditor : Editor
{
    private string[] sheetNames = null;
    private int selectedSheetIndex = 0;
    private bool isLoading = false;

    private string lastLoadedFileName = string.Empty;

    private QuestionContainer script;

    public override void OnInspectorGUI()
    {
        script = (QuestionContainer)target;
        DrawDefaultInspector();

        // Показываем текущий выбранный лист 
        if (sheetNames != null && sheetNames.Length > 0 && lastLoadedFileName.Equals(script.fileName))
        {
            selectedSheetIndex = EditorGUILayout.Popup("Выбранный лист", selectedSheetIndex, sheetNames);
            script.sheetName = sheetNames[selectedSheetIndex];
            EditorUtility.SetDirty(script);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Выбранный лист", script.sheetName);
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Space();

        // Кнопка появляется только если нужно перезагрузить листы
        bool shouldShowButton = sheetNames == null || !script.fileName.Equals(lastLoadedFileName);

        if (shouldShowButton && GUILayout.Button("Загрузить имена листов"))
        {
            LoadSheetsAsync(script);
        }

        if (isLoading)
        {
            EditorGUILayout.LabelField("Загрузка листов...");
        }
    }

    private async void LoadSheetsAsync(QuestionContainer script)
    {
        if (isLoading || string.IsNullOrEmpty(script.fileName)) return;

        isLoading = true;
        Repaint();

        byte[] data = await script.GetFileAsync();
        if (data != null && data.Length > 0)
        {
            try
            {
                sheetNames = ExcelReader.GetAllSheetNames(data);
                lastLoadedFileName = script.fileName;

                if (sheetNames != null && sheetNames.Length > 0)
                {
                    selectedSheetIndex = System.Array.IndexOf(sheetNames, script.sheetName);
                    if (selectedSheetIndex < 0) selectedSheetIndex = 0;
                    EditorUtility.SetDirty(script);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Ошибка чтения листов: {ex.Message}");
                sheetNames = null;
            }
        }
        else
        {
            Debug.LogError("Не удалось загрузить файл для получения списка листов.");
            sheetNames = null;
        }

        isLoading = false;
        Repaint();
    }
}
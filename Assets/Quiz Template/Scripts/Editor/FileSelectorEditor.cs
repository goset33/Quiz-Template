using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardsContainer))]
public class FileSelectorEditor : Editor
{
    private string[] sheetNames;
    private int selectedSheetIndex = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        CardsContainer script = (CardsContainer) target;

        // Поле для отображения выбранного файла
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Выбранная таблица", script.FileName);
        EditorGUI.EndDisabledGroup();

        // Загрузка имен листов
        if (!string.IsNullOrEmpty(script.FilePath) && sheetNames == null)
        {
            try
            {
                sheetNames = ExcelReader.GetAllSheetNames(script.FilePath);
                selectedSheetIndex = System.Array.IndexOf(sheetNames, script.sheetName);
                if (selectedSheetIndex == -1)
                {
                    selectedSheetIndex = 0;
                }
            }
            catch
            {
                Debug.LogError("Не удалось загрузить имена листов из файла.");
                sheetNames = null;
            }
        }

        // Создание выпадающего списка для выбора листа
        if (sheetNames != null && sheetNames.Length > 0)
        {
            selectedSheetIndex = EditorGUILayout.Popup("Выбранный лист", selectedSheetIndex, sheetNames);
            script.sheetName = sheetNames[selectedSheetIndex];
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Выбранный лист", "");
            EditorGUI.EndDisabledGroup();
        }

        if (GUILayout.Button("Выбрать .xlsx файл"))
        {
            string filePath = EditorUtility.OpenFilePanel("Выберите .xlsx файл", "", "xlsx");
            if (!string.IsNullOrEmpty(filePath))
            {
                sheetNames = null;
                selectedSheetIndex = 0;

                string relativePath = filePath.Replace(Application.dataPath, "Assets");
                script.FilePath = relativePath;
                EditorUtility.SetDirty(script);
            }
        }

        if (!string.IsNullOrEmpty(script.FilePath))
        {
            EditorGUILayout.LabelField($"Путь до файла: {script.FilePath}");
        }
    }
}
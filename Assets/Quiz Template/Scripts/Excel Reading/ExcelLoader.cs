using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class ExcelLoader
{
    public static async Task<byte[]> LoadFileAsync(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Sheets", fileName);

        // Проверяем, существует ли файл (работает только в редакторе и на Standalone)
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Файл '{fileName}' не найден по пути: {filePath}");
            return null;
        }
#endif

        using (UnityWebRequest www = UnityWebRequest.Get(filePath))
        {
            www.downloadHandler = new DownloadHandlerBuffer();

            var tcs = new TaskCompletionSource<byte[]>();

            www.SendWebRequest().completed += (op) =>
            {
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Ошибка загрузки файла '{fileName}': {www.error}");
                    tcs.SetException(new System.Exception(www.error));
                }
                else if (www.downloadedBytes == 0)
                {
                    Debug.LogError($"Файл '{fileName}' загружен, но данные отсутствуют (пустой файл).");
                    tcs.SetException(new System.Exception("Пустой файл"));
                }
                else
                {
                    Debug.Log($"Файл '{fileName}' успешно загружен.");
                    tcs.SetResult(www.downloadHandler.data);
                }

                www.Dispose();
            };

            return await tcs.Task;
        }
    }
}
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Класс отвечает за создание запросов к нейросети
/// </summary>
public static class AIRequestHandler
{
    private static readonly CancellationTokenSource cts = new CancellationTokenSource();

    private static HttpClient httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    /// <summary>
    /// Создает и отправляет промпт на сервера OpenRouter
    /// </summary>
    /// <param name="gameName">Имя игры</param>
    /// <param name="questionCount">Количество генерируемых вопросов</param>
    /// <returns>Ответ нейросети</returns>
    public static async Task<string> GenerateQuestionsAsync(string gameName, int questionCount)
    {
        TextAsset keyFile = (TextAsset) AssetDatabase.LoadAssetAtPath("Assets/ApiKey.txt", typeof(TextAsset));
        string apiKey = keyFile.text;

        TextAsset promptFile = (TextAsset) AssetDatabase.LoadAssetAtPath("Assets/AI Prompt.txt", typeof(TextAsset));
        Debug.Log(promptFile.text);
        string prompt = promptFile.text.Replace("{0}", gameName).Replace("{1}", questionCount.ToString()).Replace("{2}", GameManager.Language);
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Version = HttpVersion.Version10,
            RequestUri = new Uri("https://openrouter.ai/api/v1/chat/completions"),
            Headers =
            {
                { HttpRequestHeader.Authorization.ToString(), $"Bearer {apiKey}" },
                { HttpRequestHeader.ContentType.ToString(), "application/json" }
            },
            Content = new StringContent(prompt, Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
        string answer = await GetClearStringFromResponse(response);

        var responseContent = await response.Content.ReadAsStringAsync();
        Debug.Log($"Ответ сервера: {responseContent}");

        return answer;
    }

    /// <summary>
    /// Парсит из сырых данных сам ответ нейросети
    /// </summary>
    /// <param name="response">Сырой ответ сервера</param>
    /// <returns>Ответ нейронки</returns>
    private static async Task<string> GetClearStringFromResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            JObject parsedResponse = JObject.Parse(responseContent);

            var choices = parsedResponse["choices"];
            if (choices == null) return "*** 1";
            if (choices.Count() == 0) return "*** 2";
            if (choices[0] == null) return "*** 3";
            var message = choices![0]!["message"];
            if (message == null) return "*** 4";
            if (message["content"] == null) return "*** 5";
            var answer = message["content"]!.ToString();

            return answer;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        return $"Error: {response.StatusCode}, Content: {errorContent}";
    }

    /// <summary>
    /// Отменяет текущие и будущие запросы на сервер. НЕОБХОДИМО вызывать перед закрытием игры
    /// </summary>
    public static void Dispose()
    {
        // Отменяем все запросы
        cts.Cancel();
        cts.Dispose();

        // Освобождаем HttpClient
        httpClient.CancelPendingRequests();
        httpClient.Dispose();
    }
}


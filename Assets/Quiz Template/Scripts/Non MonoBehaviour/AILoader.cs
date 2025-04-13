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

public class AILoader : IDisposable
{
    private static readonly CancellationTokenSource cts = new CancellationTokenSource();
    private static string apiKey;

    private static HttpClient httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(60),
    };

    private static string questionTemplate = @"{
        ""model"": ""google/gemini-2.0-flash-exp:free"",
        ""messages"": [
            {
                ""role"": ""system"",
                ""content"": ""You are a specialized assistant for generating questions for the game {0}. Follow these instructions carefully:\nGenerate exactly 5 questions. Each question must have a unique structure and content to avoid repetition.\nFor type 1 questions:\n - Include one correct answer as the first option.\n - Provide three plausible but incorrect answers.\nFor type 3 questions:\n - Answers must form a logical sequence.\n - If 1-2 last answers are removed, the remaining sequence should still make sense.\nEnsure all questions are challenging and fit the theme of the game.\nUse the language specified as {1}.\nOutput MUST strictly follow the provided JSON schema.""
            }
        ],
        ""provider"": {
            ""require_parameters"": true
        },
        ""structured_outputs"": true,
        ""response_format"": {
            ""type"": ""json_schema"",
            ""json_schema"": {
                ""name"": ""Questions"",
                ""strict"": true,
                ""schema"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""questions"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""description"": ""One question card. Must contain a question, exactly 4 answer options, and a question type."",
                                ""properties"": {
                                    ""question"": {
                                        ""type"": ""string"",
                                        ""description"": ""A challenging question on a given topic. The difficulty level should be high.""
                                    },
                                    ""answers"": {
                                        ""type"": ""array"",
                                        ""minItems"": 4,
                                        ""maxItems"": 4,
                                        ""items"": {
                                            ""type"": ""string"",
                                            ""description"": ""Answer options. For type 1: 1 correct answer (first), 3 incorrect answers. For type 3: a logical sequence of 4 answers.""
                                        }
                                    },
                                    ""questionType"": {
                                        ""type"": ""number"",
                                        ""description"": ""Type of question. 1 = Default question (multiple-choice), 3 = Ordering question (sequence-based).""
                                    }
                                }
                            }
                        }
                    },
                    ""required"": [""questions""],
                    ""additionalProperties"": false
                }
            }
        }
    }";

    public async Task LoadAsync()
    {
        TextAsset file = (TextAsset) AssetDatabase.LoadAssetAtPath("Assets/ApiKey.txt", typeof(TextAsset));
        apiKey = file.text;

        string prompt = questionTemplate.Replace("{0}", "Minecraft").Replace("{1}", GameManager.Language);
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
        Debug.Log(answer);

        var responseContent = await response.Content.ReadAsStringAsync();
        Debug.Log($"Ответ сервера: {responseContent}");
    }

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

    public void Dispose()
    {
        // Отменяем все запросы
        cts.Cancel();
        cts.Dispose();

        // Освобождаем HttpClient
        httpClient.CancelPendingRequests();
        httpClient.Dispose();
    }
}


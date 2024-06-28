using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NAudio.Wave;

public class AIHelper
{
    static string system = @"You are now taking on the role of a professional interpreter. Please translate everything we discuss, tagging the recognized language at the beginning with tags like [de-de] for German or [en-gb] for English. Only output the translation. I am currently with friends and would like to communicate with them in English.However, my English isn't very good, so you need to translate their English sentences into German.\n\n
For example:\n
[de-de] Wie geht es dir?\n
[en-gb] I'm fine, how about you?";

    public static async Task<string> Ollama(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return string.Empty;

        // Erstelle den `prompt`-String


        var jsonData = JsonConvert.SerializeObject(new
        {
            model = "llama3",

            stream = false,
            prompt = $"",
            messages = new[]
            {
                    new { role = "system", content = $"{system}" },
                    new { role = "user", content = $"Prompt: {prompt}"}
            },
            options = new
            {
                max_tokens = 100
            },
            keep_alive = -1
        });
        string url = "http://localhost:11434";
        using (var httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync($"{url}/api/chat", content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<ResponseData2>(responseContent);
                    if (responseData == null) return "";
                    string messageContent = responseData.Message.Content;
                    string processedResponse = messageContent.Trim();
                    return processedResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return "";
    }

    public static async Task DoJob(string text)
    {

        var response = await Ollama(text);
        var audio = await PiperHelper.Piper(response);
        Console.WriteLine(response);
        AudioHelper.PlayAudio(audio);
    }

}

    public class MessageDetails
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
    public class ResponseData2
    {
        public string Model { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageDetails Message { get; set; }
        public bool Done { get; set; }
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
        public long PromptEvalDuration { get; set; }
        public int EvalCount { get; set; }
        public long EvalDuration { get; set; }
    }


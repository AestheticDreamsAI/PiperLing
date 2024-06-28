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

public class PiperLing
{
    static string system = @"";
    static string model = "";

    public static async Task Init(string sys= "You are now taking on the role of a professional interpreter. Please translate everything we discuss, tagging the recognized language of the translation at the beginning with tags like [de-de] for German or [en-gb] for English. Only output the translation. I am currently with friends and would like to communicate with them in English. However, my English isn't very good, so you need to translate their English sentences into German.\n\nFor example:\n[de-de] Wie geht es dir?\n[en-gb] I'm fine, how about you?", string m="llama3")
    {
        system = sys;
        model = m;
        await PiperHelper.Init();
        await Start();

    }

    private static async Task Start()
    {
        await Ollama("Initiating...");
        Console.Title = "PiperLing";
        Console.WriteLine(AI_Dollmetscher.Properties.Resources.logo);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ready...\n");
        Console.ForegroundColor = ConsoleColor.White;
        while (true)
        {
            try
            {
                var text = Console.ReadLine();
                await Translate(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error - {ex.Message}");
            }
        }
    }
    public static async Task<string> Ollama(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return string.Empty;

        // Erstelle den `prompt`-String


        var jsonData = JsonConvert.SerializeObject(new
        {
            model = model,

            stream = false,
            prompt = $"System: {system}",
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
                    if (messageContent.Contains("\n"))
                        messageContent = messageContent.Split('\n')[0];
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

    public static async Task Translate(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Thinking ...");
        var response = await Ollama(text);
        var audio = await PiperHelper.Speak(response);
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        Console.WriteLine(response);
        AudioHelper.PlayAudio(audio);
        Console.ForegroundColor= ConsoleColor.White;
        Console.WriteLine();
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


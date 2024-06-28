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
    static string apiExePath = AppDomain.CurrentDomain.BaseDirectory;
    static string modelPath = Path.Combine(apiExePath, $"model\\");
    static AudioListener audioListener;
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
        Console.WriteLine("______ _                 _     _             \r\n| ___ (_)               | |   (_)            \r\n| |_/ /_ _ __   ___ _ __| |    _ _ __   __ _ \r\n|  __/| | '_ \\ / _ \\ '__| |   | | '_ \\ / _` |\r\n| |   | | |_) |  __/ |  | |___| | | | | (_| |\r\n\\_|   |_| .__/ \\___|_|  \\_____/_|_| |_|\\__, |\r\n        | |   By AestheticDreamsAI      __/ |\r\n        |_|                            |___/ ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ready...");
        Console.ForegroundColor = ConsoleColor.White;
        audioListener = new AudioListener();
        audioListener.StartListening();
        //await ML.Run(mlContext,model,intents);
        while (true)
        {
           
        }
    }
    static async Task<string> Ollama(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return string.Empty;

        // Erstelle den `prompt`-String


        var jsonData = JsonConvert.SerializeObject(new
        {
            model = model,

            stream = false,
            messages = new[]
            {
                    new { role = "user", content = $"System: {system}\nTranslate the following: '{prompt}'."}
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
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        audioListener.StopListening();
        Console.WriteLine($"Input: {text}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Thinking ...");
        var response = await Ollama(text);
        var audio = await PiperHelper.Speak(response);
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        Console.WriteLine(response);
        AudioHelper.PlayAudio(audio);
        Console.ForegroundColor= ConsoleColor.White;
        audioListener.StartListening();
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


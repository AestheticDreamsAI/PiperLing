using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using AI_Dollmetscher;

using System.Threading.Tasks;
using System.IO;


public class PiperLing
{
    static string system = "";
    static string model = "";
    static string apiExePath = AppDomain.CurrentDomain.BaseDirectory;
    static string modelPath = Path.Combine(apiExePath, "model");
    static AudioListener audioListener;
    static Config config = new Config();
    static HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

    public static async Task<string> ProcessAudioFile(string filePath, string language)
    {
        // Implementiere hier die Logik zur Verarbeitung der Audio-Datei
        // und Rückgabe der Übersetzung
        // Beispiel: 
        // return "Translated text from the audio file";

        // Deine Logik hier
        await Task.Delay(1000); // Simuliere eine asynchrone Verarbeitung
        return "Translated text from the audio file";
    }
    public static async Task Init()
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (File.Exists("./config.json"))
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json"));
            model = config.llmModel;
            system = config.systemPrompt;
        }
        else
        {
            throw new Exception("Config file is missing ...");
        }

        await PiperHelper.Init();
        await Start();
    }

    private static async Task Start()
    {
        if (!model.StartsWith("gpt-"))
            await SendRequestToOllama("Are you ready?");

        Console.Title = "PiperLing";

        audioListener = new AudioListener();
        Header.Add("ready");

        Console.WriteLine($"LLM: {model.ToUpper()}");
        Console.ForegroundColor = ConsoleColor.White;
        audioListener.StartListening();

        while (true)
        {
            await Task.Delay(1000); // Non-blocking loop
        }
    }

    static async Task<string> SendRequestToOllama(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return string.Empty;

        var jsonData = JsonConvert.SerializeObject(new
        {
            model,
            stream = false,
            messages = new[]
            {
                new { role = "user", content = $"System: {system}\nTranslate the following: '{prompt}'." }
            },
            options = new
            {
                max_tokens = 100
            },
            keep_alive = -1
        });

        string url = "http://localhost:11434";
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{url}/v1/completion", content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<ResponseData2>(responseContent);
                if (responseData == null) return "";

                if (string.IsNullOrEmpty(responseData.Message.Content)) return "";
                string messageContent = responseData.Message.Content;
                if (messageContent.Contains("\n"))
                    messageContent = messageContent.Split('\n').FirstOrDefault(x => x.StartsWith("["));
                return messageContent?.Trim() ?? "";
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return "";
    }

    static string ReplaceUmlauts(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input
            .Replace("Ä", "Ae")
            .Replace("Ö", "Oe")
            .Replace("Ü", "Ue")
            .Replace("ä", "ae")
            .Replace("ö", "oe")
            .Replace("ü", "ue")
            .Replace("ß", "ss");
    }

    public static async Task Translate(string text)
    {
        try
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            audioListener.StopListening();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Input: {text}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Thinking ...");

            var response = model.Contains("gpt-")
                ? await SendRequestToGPT4o(text)
                : await SendRequestToOllama(text);

            var audio = await PiperHelper.Speak(ReplaceUmlauts(response));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(response);
            AudioHelper.PlayAudio(audio);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.ForegroundColor = ConsoleColor.White;
        audioListener.StartListening();
    }

    static async Task<string> SendRequestToGPT4o(string prompt)
    {
        if (string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(config.OpenaiApiKey))
            return string.Empty;

        var openaiUrl = "https://api.openai.com/v1/chat/completions";

        var jsonData = JsonConvert.SerializeObject(new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = system },
                new { role = "user", content = $"Translate the following: '{prompt}'." }
            },
            max_tokens = 100
        });

        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.OpenaiApiKey);

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(openaiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<ResponseData>(responseContent);
                return responseData?.choices[0]?.message.Content.Trim() ?? "";
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return "";
    }
}

public class Config
{
    public string llmModel { get; set; }
    public string OpenaiApiKey { get; set; }
    public string systemPrompt { get; set; }
}

public class Choice
{
    public MessageDetails message { get; set; }
}

public class ResponseData
{
    public List<Choice> choices { get; set; }
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

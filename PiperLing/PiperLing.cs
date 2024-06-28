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
using System.Net.Http.Json;
using AI_Dollmetscher;

public class PiperLing
{
    static string system = @"You are now taking on the role of a professional interpreter. Please translate everything we discuss, tagging the recognized language of the translation at the beginning with tags like [de-de] for German or [en-gb] for English. Only output the translation. I am currently with friends and would like to communicate with them in English. However, my English isn't very good, so you need to translate their English sentences into German.\n\nFor example:\n[de-de] Wie geht es dir?\n[en-gb] I'm fine, how about you?""";
    static string model = "";
    static string apiExePath = AppDomain.CurrentDomain.BaseDirectory;
    static string modelPath = Path.Combine(apiExePath, $"model\\");
    static AudioListener audioListener;
    static Config config = new Config();
    public static async Task Init(string m="llama3")
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (File.Exists("./config.json"))
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json"));
        model = m;
        await PiperHelper.Init();
        await Start();

    }

 
    private static async Task Start()
    {
        if(!model.StartsWith("gpt-"))
            await Ollama("Are you ready?");

        Console.Title = "PiperLing";

        audioListener = new AudioListener();
        Header.Add("ready");
     
        Console.WriteLine($"LLM: {model.ToUpper()} ");
        Console.ForegroundColor = ConsoleColor.White;    
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
                        messageContent = messageContent.Split('\n').Where(x=>x.StartsWith("[")).FirstOrDefault();
                    if (messageContent == null)
                        return "";
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
    static string ReplaceUmlauts(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        StringBuilder sb = new StringBuilder(input);
        sb.Replace("Ä", "Ae")
          .Replace("Ö", "Oe")
          .Replace("Ü", "Ue")
          .Replace("ä", "ae")
          .Replace("ö", "oe")
          .Replace("ü", "ue")
          .Replace("ß", "ss");

        return sb.ToString();
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
            var response = "";
            if(model.Contains("gpt-"))
             response = await GPT4o(text);
            else response = await Ollama(text);
            var audio = await PiperHelper.Speak(ReplaceUmlauts(response));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(response);
            AudioHelper.PlayAudio(audio);
        }
        catch { }
        Console.ForegroundColor= ConsoleColor.White;
        audioListener.StartListening();
    }

    static async Task<string> GPT4o(string prompt)
    {
        if (string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(config.OpenaiApiKey))
            return string.Empty;

        var openaiUrl = "https://api.openai.com/v1/chat/completions";

        var jsonData = JsonConvert.SerializeObject(new
        {
            model = model,
            messages = new[]
            {
            new { role = "system", content = system },
            new { role = "user", content = $"Translate the following: '{prompt}'." }
        },
            max_tokens = 100
        });

        using (var httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.OpenaiApiKey}");
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(openaiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<ResponseData>(responseContent);
                    if (responseData == null) return "";
                    string messageContent = responseData.choices[0].message.Content;
                    return messageContent.Trim();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return "";
    }
}


public class Config
{
    public string OpenaiApiKey { get; set; }
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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

    internal class PiperHelper
    {
    static string apiExePath = AppDomain.CurrentDomain.BaseDirectory;
    static string piperExePath = Path.Combine(apiExePath, "piper\\piper.exe");
    static string modelPath = Path.Combine(apiExePath, $"voices\\");

    public static async Task Init()
    {
        if (!Directory.Exists(modelPath))
        {
            Directory.CreateDirectory(modelPath);
        }

        if (!File.Exists(piperExePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            throw new Exception($"PiperTTS is missing: {piperExePath}");
        }
        if (Directory.GetFiles(modelPath, "*.onnx").Length < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            throw new Exception($"No piper voice models models detected: en-gb.onnx and en-gb.onnx.json\n{modelPath}");
        }

    }

  
        public static string ExtractLanguage(string input)
    {
        string pattern = @"\[(?<lang>[a-z]{2}-[a-z]{2})\]";
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            return match.Groups["lang"].Value;
        }
        else
        {
            return "Unknown";
        }
    }
    public static async Task<string> Speak(string text)
    {
        var lang = ExtractLanguage(text);
        string outputFilePath = System.IO.Path.GetTempPath() + $"\\{Guid.NewGuid().ToString()}.mp3";
        if (File.Exists($"{modelPath}{lang}.onnx"))
        {
            string command = $"echo \"{text.Replace($"[{lang}]", "").Trim()}\" | \"{piperExePath}\" --model \"{modelPath}{lang}.onnx\" --output_file \"{outputFilePath}\"";

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {

                RedirectStandardOutput = true,

                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    process.WaitForExit(); // Warte, bis der Prozess abgeschlossen ist
                    result = result.Replace("\r\n", "");
                    return outputFilePath;
                }
            }
        }
        else
        {
            Console.WriteLine($"{lang} not exists");
        }
            return "";
    }
}


﻿using NAudio.Wave;
using System.Collections.Concurrent;
using Whisper.net.Ggml;
using Whisper.net;
using Cherry_Lite;
using System.IO;
using System.Threading.Tasks;
using System;

public class AudioListener
{
    private bool listening = false;
    private ConcurrentQueue<byte[]> vadData = new ConcurrentQueue<byte[]>();
    private WaveInEvent waveIn;
    private const int sampleRate = 16000; // Whisper model's expected sample rate
    private GgmlType ggmlType = GgmlType.Base;
    private string modelFileName = ".\\model\\ggml-base.bin";
    private WhisperProcessor processor;
    private bool isProcessing = false; // Prevent overlapping processing
    private int recordDelay = 10;
    private int deviceId = 0;
    public AudioListener()
    {
        DeviceSelector();
        InitializeWhisper().Wait();
    }

    private void DeviceSelector()
    {

        for (int n = -1; n < WaveInEvent.DeviceCount; n++)
        {
            var capabilities = WaveInEvent.GetCapabilities(n);
            Console.WriteLine($"{n}: {capabilities.ProductName}");
        }

        // Benutzer nach der Auswahl des Geräts fragen
        Console.Write("Bitte wähle ein Aufnahmegerät aus (Nummer eingeben): ");
        
        deviceId = int.Parse(Console.ReadLine());
    }

    private void InitializeWaveInEvent()
    {
        waveIn?.Dispose();
        waveIn = new WaveInEvent
        {
            DeviceNumber = deviceId,
            WaveFormat = new WaveFormat(sampleRate, 1)
        };
        waveIn.DataAvailable += OnDataAvailable;
    }

    private async Task InitializeWhisper()
    {
        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        var whisperFactory = WhisperFactory.FromPath(modelFileName);
        processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();
    }

    public void StartListening()
    {
        if (!listening)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Listening...");
            InitializeWaveInEvent();
            listening = true;
            waveIn.StartRecording();
        }
        else
        {
            Console.WriteLine("Already listening.");
        }
    }

    public void StopListening()
    {
        if (listening)
        {
            waveIn.StopRecording();
            waveIn.Dispose(); // Dispose the waveIn to release resources
            listening = false;
            vadData = new ConcurrentQueue<byte[]>(); // Clear the queue to ensure fresh data on next start
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        byte[] buffer = new byte[e.BytesRecorded];
        Array.Copy(e.Buffer, buffer, e.BytesRecorded);
        vadData.Enqueue(buffer);

        if (vadData.Count > recordDelay && !isProcessing) // Prevent overlapping processing
        {
            isProcessing = true;
            Task.Run(() => ProcessAudio());
        }
    }

    private async Task ProcessAudio()
    {
        using (var wavStream = new MemoryStream())
        using (var waveFileWriter = new WaveFileWriter(wavStream, waveIn.WaveFormat))
        {
            try
            {
                while (vadData.TryDequeue(out var data))
                {
                    waveFileWriter.Write(data, 0, data.Length);
                }

                // Ensure all data is written
                waveFileWriter.Flush();
                wavStream.Seek(0, SeekOrigin.Begin);

                var output = "";
                await foreach (var result in processor.ProcessAsync(wavStream))
                {
                    output += $"{result.Text} ";
                }

                // Filter out text within square brackets and parentheses
                string filteredOutput = OutputCleaner.CleanString(output);

                if (!string.IsNullOrWhiteSpace(filteredOutput))
                {
                    StopListening(); // Stop listening after processing
                    await PiperLing.Translate(filteredOutput);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessAudio: {ex.Message}");
            }
            finally
            {
                isProcessing = false; // Reset the processing flag
            }
        }
    }

    private async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        string modelsPath = Path.GetDirectoryName(modelFileName);
        Console.WriteLine($"Downloading Whisper Model {fileName}");
        if (!Directory.Exists(modelsPath))
        {
            Directory.CreateDirectory(modelsPath);
        }

        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
        Console.WriteLine("Model downloaded.");
    }

    private void PlaySound(string filePath)
    {
        AudioHelper.PlayAudio(filePath);
    }
}

using NAudio.Wave;
using System.Collections.Concurrent;
using Whisper.net.Ggml;
using Whisper.net;
using Cherry_Lite;

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
    private int recordDelay = 50;
    public AudioListener()
    {
        InitializeWhisper().Wait();
    }

    private void InitializeWaveInEvent()
    {
        if (waveIn != null)
        {
            waveIn.Dispose();
        }

        waveIn = new WaveInEvent
        {
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
            //PlaySound(".\\data\\beep.mp3");
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
            //Console.WriteLine("Stopping recording...");
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
        MemoryStream wavStream = null;
        WaveFileWriter waveFileWriter = null;

        try
        {
            wavStream = new MemoryStream();
            waveFileWriter = new WaveFileWriter(wavStream, waveIn.WaveFormat);

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
            // Ensure resources are disposed of properly
            waveFileWriter?.Dispose();
            wavStream?.Dispose();
            isProcessing = false; // Reset the processing flag
        }
    }

    private async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
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

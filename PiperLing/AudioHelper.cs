using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    internal class AudioHelper
    {
        public static void PlayAudio(string url)
        {
            if (string.IsNullOrEmpty(url) || !File.Exists(url)) return;
            WaveOutEvent outputDevice = new WaveOutEvent();
            AudioFileReader audioFile = new AudioFileReader(url);

            outputDevice.Init(audioFile);
            //Program.appState.GifPath = Program.talkGif;

            // Event-Handler für PlaybackStopped hinzufügen
            outputDevice.PlaybackStopped += (sender, args) =>
            {
                // Code ausführen, wenn die Wiedergabe beendet ist
                //OnPlaybackEnded();
                // Aufräumen
                audioFile.Dispose();
                outputDevice.Dispose();
            };

            outputDevice.Play();
        }
    }
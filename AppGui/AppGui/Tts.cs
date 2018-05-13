using Microsoft.Speech.Synthesis;
using System;

namespace AppGui
{
    class Tts
    {
        SpeechSynthesizer tts = null;
        public Tts()
        {
            tts = new SpeechSynthesizer();
            tts.SelectVoice("Microsoft Server Speech Text to Speech Voice (pt-PT, Helia)");
            tts.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(tts_SpeakCompleted);
            tts.SetOutputToDefaultAudioDevice();
        }
        public void Speak(string text)
        {
            tts.SpeakAsync(text);
        }

        void tts_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {

        }
    }
}


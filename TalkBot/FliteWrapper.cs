
using System;
using System.Runtime.InteropServices;
using MonoTouch.AVFoundation;
using System.IO;
using MonoTouch.Foundation;

namespace TalkBot
{

    public static class FliteWrapper
    {
        [DllImport("__Internal")]
        static extern void flite_init ();

        [DllImport("__Internal")]
        static extern IntPtr register_cmu_us_kal ();

        [DllImport("__Internal")]
        static extern void flite_text_to_speech (string @from, IntPtr register_cmu_us_kal, string to);

        public static void Speak (string textToSpeak)
        {       
            string ttsFilePath = ConvertTextToWav (textToSpeak, "text.wav");
            
            AVAudioPlayer player = AVAudioPlayer.FromUrl (NSUrl.FromString (ttsFilePath));
            player.PrepareToPlay ();
            player.Play ();
        }
        
        public static NSData DataFromText (string text)
        {
            string ttsFilePath = ConvertTextToWav (text, "temp.wav");
            
            NSData data = NSData.FromFile (ttsFilePath);
            return data;
        }

        static string ConvertTextToWav (string text, string fileName)
        {
            var baseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");
            var tmpDir = Path.Combine (baseDir, "tmp");
            string ttsFilePath = Path.Combine (tmpDir, fileName);
            
            flite_init ();
            flite_text_to_speech (text, register_cmu_us_kal (), ttsFilePath);
            
            return ttsFilePath;
        }
    }
}

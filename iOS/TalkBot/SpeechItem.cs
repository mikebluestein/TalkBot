using System;
using MonoTouch.Foundation;
using MonoTouch.AVFoundation;

namespace TalkBot
{
    [Serializable]
    public class SpeechItem
    {
        public string Text {
            get;
            set;
        }

        public SpeechItem ()
        {   
        }

        public SpeechItem (string text)
        {
            this.Text = text;
        }

        public void Play ()
        {
            if (!String.IsNullOrEmpty (Text)) {
                var speechSynthesizer = new AVSpeechSynthesizer ();
                var speechUtterance = new AVSpeechUtterance (Text);
                speechSynthesizer.SpeakUtterance (speechUtterance);
            }
        }
    }
}


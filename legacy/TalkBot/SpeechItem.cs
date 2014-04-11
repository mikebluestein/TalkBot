using System;
using MonoTouch.Foundation;

namespace TalkBot
{
    [Serializable]
    public class SpeechItem
    {
        string _text;

        public string Text {
            get { return this._text; }
            set { _text = value; }
        }
        
        public SpeechItem ()
        {   
        }

        public SpeechItem (string text)
        {
            _text = text;
        }

        public void Play ()
        {
            FliteWrapper.Speak (_text);
        }
        
        public NSData ToData ()
        {
            return FliteWrapper.DataFromText (_text);
        }
    }
}


using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TalkBot
{
		[Activity (Label = "@string/app_name", MainLauncher = true, Theme = "@android:style/Theme.Holo.Light", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity, TextToSpeech.IOnInitListener
    {
        TextToSpeech speech;
        EditText textToSpeak;
        Button speechButton;
        ListView speechItemListView;
        ArrayAdapter<string> adapter;
        List<string> items;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);

            textToSpeak = FindViewById<EditText> (Resource.Id.textToSpeak);
            speechButton = FindViewById<Button> (Resource.Id.speechButton);
            speechItemListView = FindViewById<ListView> (Resource.Id.speechItemListView);

            speech = new TextToSpeech (this, this);
						speech.SetLanguage (Java.Util.Locale.Default); //translate to default locale

            speechButton.Click += (object sender, EventArgs e) => {

                string text = textToSpeak.Text;

                if (!String.IsNullOrEmpty (text)) {
                    speech.Speak (text, QueueMode.Add, null);
                } else {
										Toast.MakeText (this, Resource.String.enter_text_to_speak, ToastLength.Short).Show ();
                }
            };

            speechItemListView.ChoiceMode = ChoiceMode.Single;

            if (speechItemListView != null) {           
                speechItemListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
                    textToSpeak.Text = items [e.Position];
                    speechItemListView.SetSelection (e.Position);
										this.InvalidateOptionsMenu ();
                };
            }

						textToSpeak.TextChanged += (sender, e) => this.InvalidateOptionsMenu();
        }

        public override bool OnCreateOptionsMenu (IMenu menu)
        {

			MenuInflater.Inflate (Resource.Menu.main_menu, menu);
						int i = speechItemListView.CheckedItemPosition;
						
						//if no text don't allow saving
						if (string.IsNullOrWhiteSpace (textToSpeak.Text)) {
							menu.RemoveItem (Resource.Id.menu_save);
						}
				
						//if nothing selected don't allow deleting
						if (i < 0) {
							menu.RemoveItem (Resource.Id.menu_delete);
						}
				
            return true;
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            switch (item.ItemId) {
							case Resource.Id.menu_save:
								//check against no data entered
								if(string.IsNullOrWhiteSpace(textToSpeak.Text)){
									Toast.MakeText (this, Resource.String.enter_text_to_save, ToastLength.Short).Show ();
									return true;
								}
                items.Add (textToSpeak.Text);
                adapter.Add (textToSpeak.Text);
								this.InvalidateOptionsMenu ();
                return true;
							case Resource.Id.menu_delete:
                int i = speechItemListView.CheckedItemPosition;
                if (i >= 0) {
                    items.RemoveAt (i);
                    adapter.Clear ();
                    adapter.AddAll (items);
                    speechItemListView.SetItemChecked (-1, true);
										this.InvalidateOptionsMenu ();
                } else {
									Toast.MakeText (this, Resource.String.select_to_delete, ToastLength.Short).Show ();
                }
                return true;
            default:
                return base.OnOptionsItemSelected (item);
            }
        }

        protected override void OnResume ()
        {
            base.OnResume ();

            items = Util.ReadFromDisk () ?? new List<string> {
                "Please",
                "Thank You",
                "You're welcome",
                "$123.45",
                "Hello, how are you?",
                "1+1=2",
                "10%"
            };

            adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItemChecked, items);
            speechItemListView.Adapter = adapter;
						this.InvalidateOptionsMenu ();
        }

        protected override void OnPause ()
        {
            base.OnPause ();

            items.SaveToDisk ();
        }

        public void OnInit (OperationResult status)
        {
        }
    }

    public static class Util
    {
        public static void SaveToDisk (this List<string> speechItems)
        {
            using (Stream s = File.Open (SpeechItemsPath (), FileMode.Create)) {
                var bf = new BinaryFormatter ();
                bf.Serialize (s, speechItems);
            }
        }

        public static string SpeechItemsPath ()
        {
            string documentsDir = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            string speechItemsPath = Path.Combine (documentsDir, "speechitems.bin");

            return speechItemsPath;
        }

        public static List<string> ReadFromDisk ()
        {
            List<string> items = null;

            string path = SpeechItemsPath ();

            if (File.Exists (path)) {
                using (Stream stream = File.Open (path, FileMode.Open)) {
                    var bf = new BinaryFormatter ();
                    items = (List<string>)bf.Deserialize (stream);
                }
            }
            return items;
        }
    }
}
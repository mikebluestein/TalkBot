using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.iAd;

namespace TalkBot
{
    public partial class IPhoneViewController : UIViewController
    {
        SpeechItem currentSpeechItem;
        SpeechItemTableController savedItemsController;
        UIToolbar toolbar;

        public SpeechItemTableController SavedItemsController {
            get { return this.savedItemsController; }
        }

        #region Constructors

        public IPhoneViewController (IntPtr handle) : base(handle)
        {
            Initialize ();
        }

        [Export("initWithCoder:")]
        public IPhoneViewController (NSCoder coder) : base(coder)
        {
            Initialize ();
        }

        public IPhoneViewController () : base("IPhoneViewController", null)
        {
            Initialize ();
        }

        void Initialize ()
        {
        }

        #endregion

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            InitEventHandlers ();

            currentSpeechItem = new SpeechItem ();
            
            InitAdView ();
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            
            if ((savedItemsController != null) && (savedItemsController.SelectedSpeechItem != null)) {
                currentSpeechItem.Text = savedItemsController.SelectedSpeechItem.Text;
            }

            textField.BecomeFirstResponder ();
            
            textField.Text = currentSpeechItem.Text;
        }

        void InitEventHandlers ()
        {
            textField.ShouldReturn += o =>
            {
                currentSpeechItem.Play ();
                return true;
            };
            
            clearButton.TouchUpInside += (o, e) =>
            {
                textField.Text = "";
                currentSpeechItem.Text = "";
            };
            
            textField.EditingChanged += (o, e) => currentSpeechItem.Text = textField.Text;
            
            speakButton.TouchUpInside += (o, e) => currentSpeechItem.Play ();
        }

        void HandleSaveButtonClicked (object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty (currentSpeechItem.Text)) {
                if (savedItemsController == null)
                    savedItemsController = new SpeechItemTableController ();
                
                savedItemsController.SpeechItems.Add (new SpeechItem (currentSpeechItem.Text));
                
                UIAlertView savedAlert = new UIAlertView ("Talk Bot", "Item saved. Touch open \r\nto play back later", null, "OK");
                savedAlert.Show ();
            } else {
                UIAlertView promptNoText = new UIAlertView ("Talk Bot", "Please enter something \r\nfor me to save", null, "OK");
                promptNoText.Show ();
            }
        }

        void HandleOpenButtonClicked (object sender, EventArgs e)
        {
            if (savedItemsController == null)
                savedItemsController = new SpeechItemTableController ();
            
            PresentViewController (savedItemsController, true, null);
        }

        void InitAdView ()
        {
            adView.AdLoaded += delegate(object sender, EventArgs e) {
                placeholderLabel.Hidden = true;
                ((ADBannerView)sender).Hidden = false;
            };
            
            adView.FailedToReceiveAd += delegate(object sender, AdErrorEventArgs e) {
                ((ADBannerView)sender).Hidden = true;
                placeholderLabel.Hidden = false;
            };
        }
            
        public override UIView InputAccessoryView 
        {
            get 
            {
                var accessoryView = new UIView(new RectangleF(0,0,320,44));
                accessoryView.BackgroundColor = UIColor.White;   

                toolbar = new UIToolbar (new RectangleF (0, 0, 320, 44));
                toolbar.SetItems (new UIBarButtonItem[]{ 
                    new UIBarButtonItem ("Open", UIBarButtonItemStyle.Plain, HandleOpenButtonClicked),
                    new UIBarButtonItem (UIBarButtonSystemItem.Save, HandleSaveButtonClicked)
                }, false);

                accessoryView.Add (toolbar);

                return accessoryView;
            }
        }
    }
}


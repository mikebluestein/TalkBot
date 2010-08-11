
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.iAd;
using MonoTouch.MessageUI;

namespace TalkBot
{
    public partial class IPhoneViewController : UIViewController
    {
        SpeechItem _currentSpeechItem;
        SpeechItemTableController _savedItemsController;
        string _greeting = "Greetings, enter something for me to say";

        public SpeechItemTableController SavedItemsController {
            get { return this._savedItemsController; }
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
            _currentSpeechItem = new SpeechItem (_greeting);
        }

        #endregion

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            
            textField.BecomeFirstResponder ();
            
            InitEventHandlers ();
            
            _currentSpeechItem.Play ();
            _currentSpeechItem.Text = "";
            
            InitAdView ();
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            
            if ((_savedItemsController != null) && (_savedItemsController.SelectedSpeechItem != null)) {
                _currentSpeechItem.Text = _savedItemsController.SelectedSpeechItem.Text;
            }
            
            textField.Text = _currentSpeechItem.Text;
        }

        void InitEventHandlers ()
        {
            textField.ShouldReturn += o =>
            {
                _currentSpeechItem.Play ();
                return true;
            };
            
            clearButton.TouchUpInside += (o, e) =>
            {
                textField.Text = "";
                _currentSpeechItem.Text = "";
            };
            
            textField.EditingChanged += (o, e) => { _currentSpeechItem.Text = textField.Text; };
            
            speakButton.TouchUpInside += (o, e) => { _currentSpeechItem.Play (); };
            
            openButton.Clicked += HandleOpenButtonClicked;
            
            saveButton.Clicked += HandleSaveButtonClicked;
            
            shareButton.Clicked += HandleShareButtonClicked;
        }

        void HandleShareButtonClicked (object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty (_currentSpeechItem.Text)) {
                if (_savedItemsController != null)
                    _savedItemsController.SelectedSpeechItem = null;
                
                if (MFMailComposeViewController.CanSendMail) {
                    MFMailComposeViewController mail = new MFMailComposeViewController ();
                    mail.SetSubject ("message from TalkBot");
                    mail.SetMessageBody (_currentSpeechItem.Text, false);
                    mail.AddAttachmentData (_currentSpeechItem.ToData (), "audio/x-wav", "audio.wav");
                    mail.Finished += HandleMailFinished;
                    
                    this.PresentModalViewController (mail, true);
                    
                } else {
                    UIAlertView alert = new UIAlertView ("Talk Bot", "Could not send mail.", null, "OK", null);
                    alert.Show ();
                }
            } else {
                UIAlertView promptNoText = new UIAlertView ("Talk Bot", "Please enter something \r\nfor me to share", null, "OK");
                promptNoText.Show ();
            }
        }

        void HandleMailFinished (object sender, MFComposeResultEventArgs e)
        {
            if (e.Result == MFMailComposeResult.Sent) {
                UIAlertView alert = new UIAlertView ("Talk Bot", "Audio file added to outgoing mail.", null, "OK", null);
                alert.Show ();
            } else if (e.Result == MFMailComposeResult.Failed) {
                UIAlertView alert = new UIAlertView ("Talk Bot", "Could not send mail.", null, "OK", null);
                alert.Show ();
            }
            
            e.Controller.DismissModalViewControllerAnimated (true);
        }

        void HandleSaveButtonClicked (object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty (_currentSpeechItem.Text)) {
                if (_savedItemsController == null)
                    _savedItemsController = new SpeechItemTableController ();
                
                _savedItemsController.SpeechItems.Add (new SpeechItem (_currentSpeechItem.Text));
                
                UIAlertView savedAlert = new UIAlertView ("Talk Bot", "Item saved. Touch open \r\nto play back later", null, "OK");
                savedAlert.Show ();
            } else {
                UIAlertView promptNoText = new UIAlertView ("Talk Bot", "Please enter something \r\nfor me to save", null, "OK");
                promptNoText.Show ();
            }
        }

        void HandleOpenButtonClicked (object sender, EventArgs e)
        {
            if (_savedItemsController == null)
                _savedItemsController = new SpeechItemTableController ();
            
            this.PresentModalViewController (_savedItemsController, true);
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
    }
}


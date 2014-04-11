
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TalkBot
{
    public class Application
    {
        static void Main (string[] args)
        {
            UIApplication.Main (args);
        }
    }

    public partial class AppDelegate : UIApplicationDelegate
    {
        IPhoneViewController _iphoneVC;

        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            UIApplication.SharedApplication.SetStatusBarHidden (true, false);
            
            _iphoneVC = new IPhoneViewController ();
            
            window.AddSubview (_iphoneVC.View);
            
            window.MakeKeyAndVisible ();
            
            return true;
        }
        
        public override void DidEnterBackground (UIApplication application)
        {
            if(_iphoneVC.SavedItemsController != null)
                _iphoneVC.SavedItemsController.SpeechItems.SaveToDisk ();
        }
        
        public override void WillTerminate (UIApplication application)
        {
            if(_iphoneVC.SavedItemsController != null)
                _iphoneVC.SavedItemsController.SpeechItems.SaveToDisk ();
        }
    }
}

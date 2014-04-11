using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TalkBot
{
    public partial class SpeechItemTableController : UIViewController
    {
        UIToolbar toolbar;
        UITableView table;
        UIBarButtonItem editButton;
        UIBarButtonItem closeButton;

        public List<SpeechItem> SpeechItems {
            get;
            set;
        }

        public SpeechItem SelectedSpeechItem {
            get;
            set;
        }

        public SpeechItemTableController ()
        {
            SpeechItems = ReadFromDisk ();
            
            if (SpeechItems == null)
                SpeechItems = new List<SpeechItem> { new SpeechItem ("Please"), 
                                                      new SpeechItem ("Thank You"), 
                                                      new SpeechItem ("You're welcome"), 
                                                      new SpeechItem ("$123.45"),
                                                      new SpeechItem ("Hello, how are you?"),
                                                      new SpeechItem ("1+1=2"),
                                                      new SpeechItem ("10%") };
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            table = new UITableView (new RectangleF (0, 64, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 44), UITableViewStyle.Plain);
            View.AddSubview (table);

            toolbar = new UIToolbar ();
            toolbar.BarStyle = UIBarStyle.Default;
            toolbar.Frame = new RectangleF (0, 20, UIScreen.MainScreen.Bounds.Width, 44);
            View.AddSubview (toolbar);
            
            editButton = new UIBarButtonItem ();
            editButton.Style = UIBarButtonItemStyle.Bordered;
            editButton.Title = "Edit";
            editButton.Clicked += delegate {
                if (table.Editing) {
                    table.SetEditing (false, true);
                    editButton.Title = "Edit";
                } else {
                    table.SetEditing (true, true);
                    editButton.Title = "Done";
                }
            };
            
            closeButton = new UIBarButtonItem ();
            closeButton.Style = UIBarButtonItemStyle.Bordered;
            closeButton.Title = "Close";
            closeButton.Clicked += delegate {
                SelectedSpeechItem = null;
                DismissViewController (true, null);
            };
            
            toolbar.Items = new UIBarButtonItem[] { closeButton, new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace), editButton };
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            
            table.SetEditing (false, false);
            editButton.Title = "Edit";
            table.Source = new SpeechItemTableDataSource (this);
        }

        class SpeechItemTableDataSource : UITableViewSource
        {
            readonly string cellIdentitfier = "Cell";
            SpeechItemTableController controller;

            public SpeechItemTableDataSource (SpeechItemTableController controller) : base()
            {
                this.controller = controller;
            }

            public override int RowsInSection (UITableView tableView, int section)
            {
                return controller.SpeechItems.Count;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell (cellIdentitfier);
                
                if (cell == null)
                    cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentitfier);
                
                cell.TextLabel.Text = new NSString (controller.SpeechItems[indexPath.Row].Text);
                
                return cell;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                controller.SelectedSpeechItem = controller.SpeechItems[indexPath.Row];
                controller.table.DeselectRow (indexPath, false);
                controller.DismissViewController (true, null);
            }

            public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (editingStyle == UITableViewCellEditingStyle.Delete) {
                    controller.SpeechItems.RemoveAt (indexPath.Row);
                    controller.table.DeleteRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                }
            }
        }

        static List<SpeechItem> ReadFromDisk ()
        {
            List<SpeechItem> items = null;
            
            string path = Util.SpeechItemsPath ();
            
            if (File.Exists (path)) {
                using (Stream stream = File.Open (path, FileMode.Open)) {
                    BinaryFormatter bf = new BinaryFormatter ();
                    items = (List<SpeechItem>)bf.Deserialize (stream);
                }
            }
            return items;
        }
    }

    public static class Util
    {
        public static void SaveToDisk (this List<SpeechItem> speechItemList)
        {
            using (Stream s = File.Open (SpeechItemsPath (), FileMode.Create)) {
                BinaryFormatter bf = new BinaryFormatter ();
                bf.Serialize (s, speechItemList);
            }
        }

        public static string SpeechItemsPath ()
        {
            string documentsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
            string speechItemsPath = Path.Combine (documentsDir, "speechitems.bin");
            
            return speechItemsPath;
        }
    }
}

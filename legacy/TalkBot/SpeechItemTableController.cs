using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace TalkBot
{
    [Register("SpeechItemTableController")]
    public partial class SpeechItemTableController : UIViewController
    {
        List<SpeechItem> _speechItems;
        SpeechItem _selectedSpeechItem;
        UIToolbar _toolbar;
        UITableView _table;
        UIBarButtonItem _editButton;
        UIBarButtonItem _closeButton;

        public List<SpeechItem> SpeechItems {
            get { return this._speechItems; }
            set { _speechItems = value; }
        }

        public SpeechItem SelectedSpeechItem {
            get { return this._selectedSpeechItem; }
            set { _selectedSpeechItem = value; }
        }

        public SpeechItemTableController ()
        {
            _speechItems = ReadFromDisk ();
            
            if (_speechItems == null)
                _speechItems = new List<SpeechItem> { new SpeechItem ("Please"), 
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
            
            _table = new UITableView (new RectangleF (0, 44, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 44), UITableViewStyle.Plain);
            this.View.AddSubview (_table);
            
            _toolbar = new UIToolbar ();
            _toolbar.BarStyle = UIBarStyle.Black;
            _toolbar.Frame = new RectangleF (0, 0, UIScreen.MainScreen.Bounds.Width, 44);
            this.View.AddSubview (_toolbar);
            
            _editButton = new UIBarButtonItem ();
            _editButton.Style = UIBarButtonItemStyle.Bordered;
            _editButton.Title = "Edit";
            _editButton.Clicked += delegate {
                if (_table.Editing) {
                    _table.SetEditing (false, true);
                    _editButton.Title = "Edit";
                } else {
                    _table.SetEditing (true, true);
                    _editButton.Title = "Done";
                }
            };
            
            _closeButton = new UIBarButtonItem ();
            _closeButton.Style = UIBarButtonItemStyle.Bordered;
            _closeButton.Title = "Close";
            _closeButton.Clicked += delegate {
                SelectedSpeechItem = null;
                this.DismissModalViewControllerAnimated (true);
            };
            
            _toolbar.Items = new UIBarButtonItem[] { _closeButton, new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace), _editButton };
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            
            _table.SetEditing (false, false);
            _editButton.Title = "Edit";
            _table.Source = new SpeechItemTableDataSource (this);
        }

        class SpeechItemTableDataSource : UITableViewSource
        {
            string _cellIdentitfier = "Cell";
            SpeechItemTableController _controller;

            public SpeechItemTableDataSource (SpeechItemTableController controller) : base()
            {
                _controller = controller;
            }

            public override int RowsInSection (UITableView tableView, int section)
            {
                return _controller.SpeechItems.Count;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell (_cellIdentitfier);
                
                if (cell == null)
                    cell = new UITableViewCell (UITableViewCellStyle.Default, _cellIdentitfier);
                
                cell.TextLabel.Text = new NSString (_controller.SpeechItems[indexPath.Row].Text);
                
                return cell;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                _controller.SelectedSpeechItem = _controller.SpeechItems[indexPath.Row];
                _controller._table.DeselectRow (indexPath, false);
                _controller.DismissModalViewControllerAnimated (true);
            }

            public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (editingStyle == UITableViewCellEditingStyle.Delete) {
                    _controller.SpeechItems.RemoveAt (indexPath.Row);
                    _controller._table.DeleteRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
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

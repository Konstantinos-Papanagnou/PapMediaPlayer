using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace PapMediaPlayer.Activities_and_fragments.Settings
{
    [Activity(Label = "Search Configuration", Icon = "@drawable/Logo")]
    public class SearchConfiguration : Activity
    {
        Switch internalSw, externalSw;
        ListView lv;
        TextView displayDir;

        public static readonly string Root = "SearchConfigs";
        public static readonly string InternalKey = "InternalKey";
        public static readonly string ExternalKey = "ExternalKey";
        public static readonly string DirectorySelectedKey = "DirectoryKey";

        string currentDir;
        List<string> dirs;
        List<string> paths;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchConfigurations);
            FindViews();
            ISharedPreferences prefs = GetSharedPreferences(Root, FileCreationMode.Private);
            bool searchInternal = prefs.GetBoolean(InternalKey, true);
            bool searchExternal = prefs.GetBoolean(ExternalKey, true);
            string directory = prefs.GetString(DirectorySelectedKey, string.Empty);

            internalSw.Checked = searchInternal;
            lv.Enabled = !searchExternal;
            externalSw.Checked = searchExternal;
            if (!searchExternal)
            {
                displayDir.Text = "Selected Directory on SD: " + directory;
                displayDir.Visibility = ViewStates.Visible;
            }
            currentDir = "/storage/";
            File[] fileList = new File(currentDir).ListFiles();
            dirs = new List<string>();
            paths = new List<string>();
            dirs.Add("Back One Level");
            paths.Add("Invalid");
            foreach (File f in fileList)
            {
                dirs.Add(f.Name);
                paths.Add(f.AbsolutePath);
            }
            lv.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, dirs);

            internalSw.CheckedChange += InternalSw_CheckedChange;
            externalSw.CheckedChange += ExternalSw_CheckedChange;
            lv.ItemClick += Lv_ItemClick;
            lv.ItemLongClick += Lv_ItemLongClick;

            
        }

        private void Lv_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (e.Position == 0 && currentDir != "/storage")
            {
                string[] data = currentDir.Split('/');
                currentDir = string.Empty;
                for (int i = 1; i < data.Count() - 1; i++)
                {
                    currentDir += "/" + data[i];
                }
                File[] back = new File(currentDir).ListFiles();
                dirs.Clear();
                paths.Clear();
                dirs.Add("Back One Level");
                paths.Add("Invalid");
                foreach (File f in back)
                {
                    dirs.Add(f.Name);
                    paths.Add(f.AbsolutePath);
                }
                lv.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, dirs);
                return;
            }
            else if (e.Position == 0 && currentDir == "/storage")
                Toast.MakeText(this, "You are not allowed to go further back", ToastLength.Short).Show();

            File file = new File(paths[e.Position]);
            if (!file.IsDirectory)
                return;
            File[] fileList = new File(paths[e.Position] + "/").ListFiles();
            if(fileList == null)
            {
                Toast.MakeText(this, "Access Denied! Perhaps the folder does not exist or it can't be read.", ToastLength.Long).Show();
                return;
            }
            currentDir = paths[e.Position];
            dirs.Clear();
            paths.Clear();
            dirs.Add("Back One Level");
            paths.Add("Invalid");
            foreach (File f in fileList)
            {
                dirs.Add(f.Name);
                paths.Add(f.AbsolutePath);
            }
            lv.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, dirs);
        }

        private void Lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string item;
            if (e.Position == 0)
            {
                displayDir.Text = "Selected Directory on SD: " + string.Empty;
                item = string.Empty;
            }
            else
            {
                displayDir.Text = "Selected Directory on SD: " + dirs[e.Position];
                item = paths[e.Position];
            }
            File f = new File(paths[e.Position]);
            if (!f.IsDirectory)
                return;
            

            ISharedPreferencesEditor editor = GetSharedPreferences(Root, FileCreationMode.Append).Edit();
            editor.PutString(DirectorySelectedKey, item);
            editor.Commit();
        }

        private void ExternalSw_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ISharedPreferencesEditor editor = GetSharedPreferences(Root, FileCreationMode.Append).Edit();
            editor.PutBoolean(ExternalKey, e.IsChecked);
            editor.Commit();
            lv.Enabled = !e.IsChecked;
            if (e.IsChecked) displayDir.Visibility = ViewStates.Invisible;
            else displayDir.Visibility = ViewStates.Visible;
        }

        private void InternalSw_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ISharedPreferencesEditor editor = GetSharedPreferences(Root, FileCreationMode.Append).Edit();
            editor.PutBoolean(InternalKey, e.IsChecked);
            editor.Commit();
        }

        private void FindViews()
        {
            internalSw = FindViewById<Switch>(Resource.Id.SearchConfigurations_InternalStorageSwitch);
            externalSw = FindViewById<Switch>(Resource.Id.SearchConfigurations_ExternalStorageSwitch);
            lv = FindViewById<ListView>(Resource.Id.SearchConfigurations_ListView);
            displayDir = FindViewById<TextView>(Resource.Id.SearchConfigurations_SelectedDirTV);
        }
    }
}
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

namespace PapMediaPlayer.Activities_and_fragments.Settings
{
    [Activity(Label = "Startup")]
    public class Startup : Activity
    {
        TextView SelectedTextView;
        LinearLayout ll2;
        TextView playlistTV;
        public const string Prefs = "StartupSettings";
        public const string REMEMBER = "Remember";
        public const string VIEWMETHOD = "ViewMethod";
        public const string ALLTRACKS = "AllTracks";
        public const string CONTFOLDERS = "ContainingFolders";
        public const string PLAYLIST = "Playlist";
        public const string PLAYLISTNAME = "PlaylistName";

        private string playlistName = string.Empty;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CreateUI();
        }

        void CreateUI()
        {
            LinearLayout ll = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };
            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent))
                ll.LayoutParameters = lp;
            using (TextView tv = new TextView(this))
            {
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                    tv.LayoutParameters = lp;
                tv.Text = "Remember where you were before closing the application (Playlist and song in playlist)";
                tv.TextSize = 15f;
                tv.SetPadding(10, 5, 10, 0);
                ll.AddView(tv);
            }

            Switch sw = new Switch(this);
            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                sw.LayoutParameters = lp;
            sw.Text = "Remember";
            sw.TextSize = 15f;
            sw.SetPadding(10, 10, 10, 10);
            sw.Checked = GetSharedPreferences(Prefs, FileCreationMode.Private).GetBoolean(REMEMBER, true);
            sw.CheckedChange += Sw_CheckedChange;
            ll.AddView(sw);
            ll2 = new LinearLayout(this);
            if (sw.Checked) ll2.Visibility = ViewStates.Gone;
            else ll2.Visibility = ViewStates.Visible;
            ll2.Orientation = Orientation.Vertical;
            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent))
                ll2.LayoutParameters = lp;

            using (TextView tv = new TextView(this))
            {
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                    tv.LayoutParameters = lp;
                tv.Text = "What do you want to start with?";
                tv.TextSize = 15f;
                tv.SetPadding(10, 5, 10, 0);
                ll2.AddView(tv);
            }
            SelectedTextView = new TextView(this);
            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                SelectedTextView.LayoutParameters = lp;
            SelectedTextView.TextSize = 15f;
            SelectedTextView.SetPadding(10, 10, 10, 10);
            if (GetSharedPreferences(Prefs, FileCreationMode.Private).GetString(VIEWMETHOD, string.Empty) != PLAYLIST)
                SelectedTextView.Visibility = ViewStates.Gone;
            else
                SelectedTextView.Text = "Default Playlist: " + GetSharedPreferences(Prefs, FileCreationMode.Private).GetString(PLAYLISTNAME, string.Empty);
            ll2.AddView(SelectedTextView);

            playlistTV = new TextView(this);
           using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                playlistTV.LayoutParameters = lp;
            playlistTV.Text = "Default View Method: " + GetSharedPreferences(Prefs, FileCreationMode.Private).GetString(VIEWMETHOD, string.Empty);
            playlistTV.TextSize = 15f;
            playlistTV.SetPadding(10, 5, 10, 0);
            ll2.AddView(playlistTV);

            using (ListView lv = new ListView(this))
            {
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent))
                    lv.LayoutParameters = lp;
                lv.SetPadding(10, 5, 10, 0);
                lv.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, new string[] { "All Tracks", "Containing Folders", "Playlist" });
                lv.ItemClick += Lv_ItemClick;
                ll2.AddView(lv);
            }

            ll.AddView(ll2);
            SetContentView(ll);
            
        }

        private void Lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ISharedPreferencesEditor editor = GetSharedPreferences(Prefs, FileCreationMode.Append).Edit();
            switch (e.Position)
            {
                case 0:
                    editor.PutString(VIEWMETHOD, ALLTRACKS);
                    playlistTV.Text = "Default View Method: All Tracks";
                    SelectedTextView.Visibility = ViewStates.Gone;
                    break;
                case 1:
                    editor.PutString(VIEWMETHOD, CONTFOLDERS);
                    playlistTV.Text = "Default View Method: Containing Folders";
                    SelectedTextView.Visibility = ViewStates.Gone;
                    break;
                case 2:
                    editor.PutString(VIEWMETHOD, PLAYLIST);
                    playlistTV.Text = "Default View Method: Playlist";
                    Intent i = new Intent(this, typeof(PlaylistManagerActivity));
                    i.PutExtra("Action", "Select");
                    StartActivityForResult(i, 11);
                    break;
            }
            editor.Commit();
        }

        private void Sw_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ISharedPreferencesEditor pref = GetSharedPreferences(Prefs, FileCreationMode.Private).Edit();
            pref.PutBoolean(REMEMBER, e.IsChecked);
            if (e.IsChecked) ll2.Visibility = ViewStates.Gone;
            else ll2.Visibility = ViewStates.Visible;
            pref.Commit();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode != 11)
                return;
            if (resultCode == Result.Ok)
            {
                playlistName = data.GetStringExtra(PLAYLISTNAME);

                SelectedTextView.Text = "Selected Playlist: " + playlistName;
                ISharedPreferencesEditor editor = GetSharedPreferences(Prefs, FileCreationMode.Append).Edit();
                editor.PutString(PLAYLISTNAME, playlistName);
                SelectedTextView.Visibility = ViewStates.Visible;
                editor.Commit();
            }
            else if(resultCode == Result.Canceled)
            {
                ISharedPreferencesEditor editor = GetSharedPreferences(Prefs, FileCreationMode.Append).Edit();
                editor.PutString(VIEWMETHOD, CONTFOLDERS);
                editor.Commit();
                playlistTV.Text = "Default View Method: Containing Folders";
                SelectedTextView.Visibility = ViewStates.Gone;
            }
        }
    }
}
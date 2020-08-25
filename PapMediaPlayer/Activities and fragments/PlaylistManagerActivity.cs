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
using Android.Graphics.Drawables;
using Android.Graphics;
using PapMediaPlayer.Models;
using PapMediaPlayer.Adapters;
using PapMediaPlayer.Playlist_Manager;
using Newtonsoft.Json;

namespace PapMediaPlayer
{
    [Activity(Label = "Playlist Manager", Icon = "@drawable/Logo", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class PlaylistManagerActivity : Activity
    {
        LinearLayout trackSelectionLL, actionSelectionLL, playlistSelectionLL;
        Button previousbtn, continuebtn, removeAllPlaylists, addPlaylist;
        Spinner actionSpinner;
        ListView trackListView, playlistListView;
        TextView selected;
        int count = 0;

        const int NOTIFICATION_ID = 500;
        List<string> playlists;
        public static bool Finished { get { return true; } set { if(OnAdded != null && OnAdded.GetInvocationList().Any()) OnAdded(); } }
        public delegate void Added();
        public static event Added OnAdded;

        List<ForListViewMultiSelectWithColorChange.DataType> tracksToManipulate;
        private enum Action
        {
            Add,
            Remove,
            NotIdentified,
            Select
        }

        int page = 0;
        private Action _action { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PlaylistManager);
            FindViews();
            tracksToManipulate = new List<ForListViewMultiSelectWithColorChange.DataType>();
            string action = Intent.GetStringExtra("Action");
            bool ReadyData = Intent.GetBooleanExtra("DataExist", false);
           
            if (action == "Add")
            {
                _action = Action.Add;
                page = 2;
            }
            else if (action == "Remove")
            {
                _action = Action.Remove;
                page = 2;
            }
            else if(action == "Select")
            {
                _action = Action.Select;
                page = 2;
            }
            else
                _action = Action.NotIdentified;
            HandlePageTurn();
            ActionBar bar = ActionBar;
            bar.SetBackgroundDrawable(new ColorDrawable(Color.Olive));
            List<Paths> tracks = null;
            if(ReadyData)
                tracks = JsonConvert.DeserializeObject<List<Paths>>(Intent.GetStringExtra("Tracks"));
            // Set data
            SetData(ReadyData, tracks);

            //configure views based on action
            ConfigureView();
            trackListView.Adapter = new ForListViewMultiSelectWithColorChange(this, ref tracksToManipulate);
            //playlistListView.
            trackListView.ItemClick += TrackListView_ItemClick;
            //Subscribe to click events
            continuebtn.Click += Continuebtn_Click;
            previousbtn.Click += Previousbtn_Click;
            removeAllPlaylists.Click += RemoveAllPlaylists_Click;
            addPlaylist.Click += AddPlaylist_Click;
            OnAdded += PlaylistManagerActivity_OnAdded;
        }

        private void PlaylistManagerActivity_OnAdded()
        {
            playlists = PlaylistManager.GetAllPlaylists();
            playlistListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, playlists);
        }

        private void AddPlaylist_Click(object sender, EventArgs e)
        {
            FragmentTransaction trans = FragmentManager.BeginTransaction();
            AddPlaylistFragment frag = new AddPlaylistFragment();
            frag.Show(trans, "frag");
        }

        private void RemoveAllPlaylists_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder confirmation = new AlertDialog.Builder(this);
            confirmation.SetMessage("Are you sure you want to delete all the playlists?");
            confirmation.SetTitle("Confirmation");
            confirmation.SetIcon(Resource.Drawable.NotFound);
            confirmation.SetPositiveButton("Yes",
                delegate
                {
                    foreach (var playlist in playlists)
                    {
                        PlaylistManager.RemovePlaylist(playlist);
                    }
                    playlists = PlaylistManager.GetAllPlaylists();
                    playlistListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, playlists);
                });
            confirmation.SetNegativeButton("No", delegate { Toast.MakeText(this, "Removal process canceled by user", ToastLength.Long).Show(); });
            confirmation.Show();
        }

        private void Previousbtn_Click(object sender, EventArgs e)
        {
            if (--page < 0)
                page = 0;
            HandlePageTurn();
        }

        private void Continuebtn_Click(object sender, EventArgs e)
        {
            if (++page > 2)
                page = 2;
            HandlePageTurn();
        }

        private void TrackListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            tracksToManipulate[e.Position].selected = !tracksToManipulate[e.Position].selected;
            int position = trackListView.FirstVisiblePosition;
            trackListView.Adapter = new ForListViewMultiSelectWithColorChange(this, ref tracksToManipulate);
            
            trackListView.SetSelection(position);
            if (tracksToManipulate[e.Position].selected)
                selected.Text = "Selected: " + ++count;
            else selected.Text = "Selected: " + --count;
        }

        private void ConfigureView()
        {
            if (_action != Action.NotIdentified)
            {
                actionSelectionLL.Visibility = ViewStates.Gone;
                playlistSelectionLL.Visibility = ViewStates.Visible;
                trackSelectionLL.Visibility = ViewStates.Gone;
                previousbtn.Enabled = false;
                continuebtn.Enabled = false;
            }
            if (page == 0)
                previousbtn.Enabled = false;
        }

        private void SetData(bool ReadyData, List<Paths> tracks)
        {
            if (ReadyData)
            {
                foreach (var item in tracks)
                   tracksToManipulate.Add(new ForListViewMultiSelectWithColorChange.DataType { title = item.Title, selected = true , Path = item.Path });
                //Assume Selected
            }
            else
            {
                if (_action == Action.Select)
                    return;
                List<Track> all = XmlParser.JsonCacheParser.Read();
                foreach (var item in all)
                    tracksToManipulate.Add(new ForListViewMultiSelectWithColorChange.DataType { title = item.FullTitle, selected = false, Path = item.Path});
            }
        }

        public override void OnBackPressed()
        {
            Finish();
            OverridePendingTransition(Resource.Animation.SlidingLeftFromRightEdge, Resource.Animation.SlidingFromCenterToLeft);
        }

        private void FindViews()
        {
            trackSelectionLL = FindViewById<LinearLayout>(Resource.Id.PlaylistManager_ChooseTracks);
            actionSelectionLL = FindViewById<LinearLayout>(Resource.Id.PlaylistManager_SelectAction);
            playlistSelectionLL = FindViewById<LinearLayout>(Resource.Id.PlaylistManager_ChoosePlaylistToModify);
            previousbtn = FindViewById<Button>(Resource.Id.PlaylistManager_PreviousButton);
            continuebtn = FindViewById<Button>(Resource.Id.PlaylistManager_Continue);
            actionSpinner = FindViewById<Spinner>(Resource.Id.PlaylistManager_SelectionSpinner);
            trackListView = FindViewById<ListView>(Resource.Id.PlaylistManager_ChooseTracksListView);
            playlistListView = FindViewById<ListView>(Resource.Id.PlaylistManager_ChoosePlaylistListView);
            selected = FindViewById<TextView>(Resource.Id.PlaylistManager_Selected);
            addPlaylist = FindViewById<Button>(Resource.Id.PlaylistManager_AddAPlaylist);
            removeAllPlaylists = FindViewById<Button>(Resource.Id.PlaylistManager_RemoveAllPlaylists);
        }

        private void HandlePageTurn()
        {
            switch (page)
            {
                case 0:
                    trackSelectionLL.Visibility = ViewStates.Visible;
                    actionSelectionLL.Visibility = ViewStates.Gone;
                    playlistSelectionLL.Visibility = ViewStates.Gone;
                    previousbtn.Enabled = false;
                    continuebtn.Enabled = true;
                    break;
                case 1:
                    trackSelectionLL.Visibility = ViewStates.Gone;
                    actionSelectionLL.Visibility = ViewStates.Visible;
                    playlistSelectionLL.Visibility = ViewStates.Gone;
                    actionSpinner.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, new string[] { "Add", "Remove" });
                    actionSpinner.ItemSelected -= ActionSpinner_ItemSelected;
                    actionSpinner.ItemSelected += ActionSpinner_ItemSelected;
                    previousbtn.Enabled = true;
                    continuebtn.Enabled = true;
                    break;
                case 2:
                    trackSelectionLL.Visibility = ViewStates.Gone;
                    actionSelectionLL.Visibility = ViewStates.Gone;
                    playlistSelectionLL.Visibility = ViewStates.Visible;
                    previousbtn.Enabled = true;
                    continuebtn.Enabled = false;
                    SetupPlaylistPage();
                    break;
            }
        }

        private void SetupPlaylistPage()
        {
            playlists = PlaylistManager.GetAllPlaylists();
            playlistListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, playlists);
            if (playlists.Count == 0)
            {
                Notification.Builder notificationBuilder = new Notification.Builder(this)
.SetSmallIcon(Resource.Drawable.Warning)
.SetContentTitle("Playlist Manager says: " + "Warning")
.SetContentText("There are no playlists registered. Please create one.").SetVibrate(new long[] { 1000,100,800,100,500,100 });
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
            }
            playlistListView.ItemClick -= PlaylistListView_ItemClick;
            playlistListView.ItemClick += PlaylistListView_ItemClick;
            playlistListView.ItemLongClick -= PlaylistListView_ItemLongClick;
            playlistListView.ItemLongClick += PlaylistListView_ItemLongClick;
        }

        private void PlaylistListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            AlertDialog.Builder confirmation = new AlertDialog.Builder(this);
            confirmation.SetMessage("Are you sure you want to delete this playlist? " + playlists[e.Position]);
            confirmation.SetTitle("Confirmation");
            confirmation.SetIcon(Resource.Drawable.NotFound);
            confirmation.SetPositiveButton("Yes",
                delegate
                {
                    PlaylistManager.RemovePlaylist(playlists[e.Position]);
                    playlists = PlaylistManager.GetAllPlaylists();
                    playlistListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, playlists);
                });
            confirmation.SetNegativeButton("No", delegate { Toast.MakeText(this, "Removal process canceled by user", ToastLength.Long).Show(); });
            confirmation.Show();
        }

        private void PlaylistListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if(_action == Action.Select)
            {
                ActionSelect(e.Position);
                return;
            }
            string message;
            if (_action == Action.Add)
                message = "Are you sure you want to append to this playlist? ";
            else
                message = "Are you sure you want to remove from this playlist? ";

            AlertDialog.Builder confirmation = new AlertDialog.Builder(this);
            confirmation.SetMessage(message + playlists[e.Position]);
            confirmation.SetTitle("Confirmation");
            confirmation.SetIcon(Resource.Drawable.NotFound);
            confirmation.SetPositiveButton("Yes",
                delegate
                {
                    List<Paths> tracks = new List<Paths>();
                    foreach (var t in tracksToManipulate)
                        if (t.selected)
                            tracks.Add(new Paths { Title = t.title, Path = t.Path});
                    PlaylistManager manager = new PlaylistManager(playlists[e.Position]);
                    if (_action == Action.Add)
                        manager.AddTracksToPlaylist(tracks);
                    else if (_action == Action.Remove)
                        manager.RemoveTracksFromPlaylist(tracks);
                    Toast.MakeText(this, "Action Completed Successfully", ToastLength.Short).Show();

                });
            confirmation.SetNegativeButton("No", delegate { Toast.MakeText(this, "Process canceled by user", ToastLength.Long).Show(); });
            confirmation.Show();
        }

        void ActionSelect(int position)
        {
            PlaylistManager manager = new PlaylistManager(playlists[position]);

            Intent intent = new Intent();
            intent.PutExtra("PlaylistName", manager.PlaylistName);
            SetResult(Result.Ok, intent);
            OnBackPressed();
        }

        private void ActionSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (e.Position == 0) _action = Action.Add;
            else if (e.Position == 1) _action = Action.Remove;
        }
    }
}
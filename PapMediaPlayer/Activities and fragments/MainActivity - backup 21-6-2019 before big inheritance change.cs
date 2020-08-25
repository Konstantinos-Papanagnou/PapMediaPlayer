using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using sys = System;
using PapMediaPlayer.StorageHelper;
using Android.Content.Res;
using Android.Graphics;
using System.Collections.Generic;
using PapMediaPlayer.Models;
using PapMediaPlayer.Track_Finder;
using PapMediaPlayer.XmlParser;
using PapMediaPlayer.Adapters;
using Android.Content;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Views.InputMethods;
using System.Threading.Tasks;
using Android.Runtime;
using Java.Lang;
using System.Linq;
using PapMediaPlayer.Activities_and_fragments.Settings;

namespace PapMediaPlayer
{

    public class PositionEventArgs:sys.EventArgs
    {
        public int Position { get; private set; }
        public PositionEventArgs(int position)
        {
            Position = position;
        }
    }

    public class ShowAddRemoveEventArgs:sys.EventArgs
    {
        public bool Value { get; private set; }
        public ShowAddRemoveEventArgs(bool value)
        {
            Value = value;
        }
    }

    public class FlowInterruptedEventArgs: sys.EventArgs
    {
        public enum InterruptionType
        {
            pause,
            prevnext,
            play
        }
        public InterruptionType Type { get; private set; }
        public FlowInterruptedEventArgs(InterruptionType Type)
        {
            this.Type = Type;
        }
    }

    [Activity(Label = "Pap Media Player", Icon = "@drawable/Logo", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation| Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity, View.IOnTouchListener, GestureDetector.IOnGestureListener
    {
        #region Gesture Handling
        GestureDetector _gestureDetector;
        public bool OnDown(MotionEvent e)
        {
            CurrentlyPlaying_Click();
            return true;
        }
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            bool result = false;
            int SWIPE_THRESHOLD = 80;
            int SWIPE_VELOCITY_THRESHOLD = 80;
            try
            {
                float diffY = e2.GetY() - e1.GetY();
                float diffX = e2.GetX() - e1.GetX();
                if (sys.Math.Abs(diffX) <= sys.Math.Abs(diffY))
                {
                    if (sys.Math.Abs(diffY) > SWIPE_THRESHOLD && sys.Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
                    {
                        if (diffY > 0)
                        {
                            CurrentlyPlaying_Click();
                        }
                    }
                }
            }
            catch (sys.Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
            return result;
        }
        public void OnLongPress(MotionEvent e) { }
        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }
        public void OnShowPress(MotionEvent e) { }
        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            _gestureDetector.OnTouchEvent(e);
            return true;
        }
        #endregion
        private const int NOTIFICATION_ID = 10;
        private const int PlaylistManagerID = 55;
        private GridView grid;
        private static MediaPlayer player;
        private Button next, previous, playPause;
        private TextView CurrentlyPlaying, selectedCount; private int count = 0;
        private EditText searchBar;
        private LinearLayout addRemoveFPLL;
        private Button add, remove;

        /// <summary>
        /// To Bypass the position change on randomized first Start
        /// </summary>
        bool onStart = true;
        
        private List<int> matchesPos;
        List<string> folders;
        static ViewPlayManager manager;
        XmlCacheParser cache;
        public static bool DetailsInstanceExists = false;

        public delegate void PositionUpdateHandler(object sender, PositionEventArgs e);
        public static event PositionUpdateHandler OnUpdatePosition;

        public delegate void AddRemoveChange(ShowAddRemoveEventArgs e);
        public static event AddRemoveChange OnShowAddRemoveChanged;

        public delegate void FlowInterrupted(FlowInterruptedEventArgs e);
        public static event FlowInterrupted OnFlowInterrupted;

        public static MediaPlayer MainPlayer { get { return player; } }

        public static ViewPlayManager Manager { get { return manager; } set { manager = value; } }

        public static List<Track> Tracks { get { return tracks; } }

        public static Track CurrentlyPlayingTrack { get; private set; }
        private ViewMethods prevM;
        public static int TrackPosition { get { return position; } }
        //bool hasSD = true;
        private static List<Track> tracks;
        private static int position = 0;
        private int folder = -1;

        private string selectedPlaylistName;
        private bool AllowPlayerNotifications;
        
        private static bool showAddRemove = false;
        public static bool ShowAddRemove { get { return showAddRemove; } set { showAddRemove = value; OnShowAddRemoveChanged(new ShowAddRemoveEventArgs(value)); } }

        public static EventList<SelectableModel<Track>> selectedItems;
        protected override void OnCreate(Bundle bundle)
        {
            using (ISharedPreferences notprefs = GetSharedPreferences(Notification_Settings.Root, FileCreationMode.Private))
                AllowPlayerNotifications = notprefs.GetBoolean(Notification_Settings.PlayerNotKey, true);

            ViewMethods ViewMethod;
            RepeatMethod repMethod;
            if (GetSharedPreferences(Activities_and_fragments.Settings.Startup.Prefs, FileCreationMode.Private).GetBoolean("Remember", true))
            {
                using (ISharedPreferences prefs = GetSharedPreferences("Configurations", FileCreationMode.Private))
                {
                    selectedPlaylistName = prefs.GetString("CurrentPlaylist", "Folders");
                    position = prefs.GetInt("SongInPlaylist", 0);
                    ViewMethod = (ViewMethods)prefs.GetInt("ViewMethod", 0);
                }
            }
            else
            {
                using (ISharedPreferences prefs = GetSharedPreferences(Activities_and_fragments.Settings.Startup.Prefs, FileCreationMode.Private))
                {
                    selectedPlaylistName = prefs.GetString("PlaylistName", "Folders");
                    position = 0;
                    if (prefs.GetString("ViewMethod", string.Empty) == "AllTracks")
                        ViewMethod = ViewMethods.AllTracks;
                    else if (prefs.GetString("ViewMethod", string.Empty) == "ContainingFolders")
                        ViewMethod = ViewMethods.ContainingFolders;
                    else
                    {
                        ViewMethod = ViewMethods.Playlist;

                    }
                }
            }

            repMethod = (RepeatMethod)GetSharedPreferences("Configurations", FileCreationMode.Private).GetInt("RepeatMethod", 0);
            base.OnCreate(bundle);
            player = new MediaPlayer();
            player.SetAudioStreamType(Stream.Music);

            ActionBar bar = ActionBar;
            bar.SetBackgroundDrawable(new ColorDrawable(Color.Olive));

            //manager = new ViewPlayManager(ViewMethods.ContainingFolders, RepeatMethod.RepeatAllInOrder);
            manager = new ViewPlayManager(ViewMethod, repMethod);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FindViews();

            string[] filters; char[] specialChars;
            using(IXmlParsable parser = new XmlFilterParser(InternalStorageHelper.InternalXmlFileLocation + "/FiltersAndSpecialChars"))
            {
                XmlFilterParser.RVal data = (XmlFilterParser.RVal)parser.FetchItems();
                filters = data.filters;
                specialChars = data.specialChars;
            }
            List<Track> tracksFromSD = new List<Track>();
            bool hasSd = false;
            try
            {
                tracksFromSD = TrackFinder.GetListOfTracksFromSD(this, filters, specialChars);
                hasSd = true;
            }
            catch (NotMountedException e)
            {
                ISharedPreferences prefs = GetSharedPreferences(Notification_Settings.Root, FileCreationMode.Private);
                if (prefs.GetBoolean(Notification_Settings.ErrorNotKey, true))
                {
                    Android.Support.V4.App.NotificationCompat.Builder not = new Android.Support.V7.App.NotificationCompat.Builder(this)
                    .SetSmallIcon(Resource.Drawable.Logo).SetContentTitle("No SD card found!").SetContentText(e.Message)
                    .SetPriority((int)NotificationPriority.Min).SetStyle(new Android.Support.V7.App.NotificationCompat.BigTextStyle().BigText(e.Message))
                    .SetVibrate(new long[] { 1000, 1000, 1000, 1000 });

                    var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                    notificationManager.Notify(0, not.Build());
                }
            }
            try
            {
                tracks = TrackFinder.GetListOfTracksFromPhone(this);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
           // tracks = new List<Track>();
            if (hasSd)
                tracks = tracks.Union(tracksFromSD).ToList();
            cache = new XmlCacheParser(tracks);
            cache.Release();
            SetNotificationBar("Nothing is playing", null);
            InitializeUI(selectedPlaylistName, position);
            searchBar.AfterTextChanged += SearchBar_AfterTextChanged;
            searchBar.FocusChange += SearchBar_FocusChange;
            CurrentlyPlaying.SetOnTouchListener(this);
            _gestureDetector = new GestureDetector(this);
            manager.OnViewMethodChanged += Manager_OnViewMethodChanged;
            OnShowAddRemoveChanged += MainActivity_OnShowAddRemoveChanged;
            grid.ItemLongClick += Grid_ItemLongClick;
            selectedItems = new EventList<SelectableModel<Track>>();
            selectedItems.OnItemAdded += SelectedItems_OnItemAdded;
            selectedItems.OnItemRemoved += SelectedItems_OnItemRemoved;
            selectedItems.OnListCleared += SelectedItems_OnListCleared;
            add.Click += Add_Click;
            remove.Click += Remove_Click;
            OnFlowInterrupted += MainActivity_OnFlowInterrupted;
            manager.OnRepeatMethodChanged += Manager_OnRepeatMethodChanged;
        }

        private void WriteSharedPreferences(string CurrentPlaylist = null, int SongInPlaylist = -1, bool writeViewMethod = false, bool writeRepMethod = false, bool commit = true)
        {
            if (!GetSharedPreferences(Activities_and_fragments.Settings.Startup.Prefs, FileCreationMode.Private).GetBoolean("Remember", true) && !writeRepMethod)
                return;
            ISharedPreferencesEditor editor = GetSharedPreferences("Configurations", FileCreationMode.Private).Edit();
            if(CurrentPlaylist != null) editor.PutString("CurrentPlaylist", CurrentPlaylist);
            if (SongInPlaylist != -1) editor.PutInt("SongInPlaylist", SongInPlaylist);
            if (writeViewMethod) editor.PutInt("ViewMethod",(int)Manager.ViewMethod);
            if (writeRepMethod) editor.PutInt("RepeatMethod", (int)Manager.Repeat);
            editor.Apply();
            if (commit) editor.Commit();
        }

        private void Manager_OnRepeatMethodChanged(object sender, RepeatMethodEventArgs e)
        {
            SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
            WriteSharedPreferences(writeRepMethod: true);
        }

        protected override void OnResume()
        {
            ISharedPreferences notPrefs = GetSharedPreferences(Notification_Settings.Root, FileCreationMode.Private);
            AllowPlayerNotifications = notPrefs.GetBoolean(Notification_Settings.PlayerNotKey, true);
            base.OnResume();
        }

        private void SetNotificationBar(string message, Bitmap icon)
        {
            if (!AllowPlayerNotifications)
                return;
            Intent playPause = new Intent(this, typeof(Receivers.PauseReceiver));
            Intent previous = new Intent(this, typeof(Receivers.PreviousReceiver));
            Intent next = new Intent(this, typeof(Receivers.NextReceiver));
            Intent cancel = new Intent(this, typeof(Receivers.CancelReceiver));
            Intent loop = new Intent(this, typeof(Receivers.LoopBackReceiver));
            Intent details = new Intent(this, typeof(Receivers.DetailsReceiver));

            PendingIntent playPausePending = PendingIntent.GetBroadcast(this, 0, playPause, PendingIntentFlags.CancelCurrent);
            PendingIntent previousPending = PendingIntent.GetBroadcast(this, 1, previous, PendingIntentFlags.CancelCurrent);
            PendingIntent nextPending = PendingIntent.GetBroadcast(this, 2, next, PendingIntentFlags.CancelCurrent);
            PendingIntent cancelPending = PendingIntent.GetBroadcast(this, 3, cancel, PendingIntentFlags.CancelCurrent);
            PendingIntent loopPending = PendingIntent.GetBroadcast(this, 4, loop, PendingIntentFlags.CancelCurrent);
            PendingIntent OpenDetails = PendingIntent.GetBroadcast(this, 10, details, PendingIntentFlags.CancelCurrent);

            string artist = CurrentlyPlayingTrack.AuthorName;
            if (artist == "Unknown")
                artist = "Unknown Artist";
            Android.Support.V4.App.NotificationCompat.Builder not = new Android.Support.V7.App.NotificationCompat.Builder(this)
               .SetSmallIcon(Resource.Drawable.Logo).SetContentTitle(message).SetContentText(artist)
              .SetStyle(new Android.Support.V7.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(0, 1, 2, 4))
               .SetOngoing(player.IsPlaying).SetContentIntent(OpenDetails)
               .SetPriority((int)NotificationPriority.Max).AddAction(Resource.Drawable.Previous, "Previous", previousPending);


            if(icon != null) not.SetLargeIcon(icon);
            if (!player.IsPlaying) not.AddAction(Resource.Drawable.Play, "Play", playPausePending);
            else not.AddAction(Resource.Drawable.Pause, "Pause", playPausePending);
            not.AddAction(Resource.Drawable.Next, "Next", nextPending);
            switch (Manager.Repeat)
            {
                case RepeatMethod.NoRepeat:
                    not.AddAction(Resource.Drawable.NoLoopback24, "No Repeat", loopPending);
                    break;
                case RepeatMethod.Random:
                    not.AddAction(Resource.Drawable.Random24, "Random", loopPending);
                    break;
                case RepeatMethod.RepeatAllInOrder:
                    not.AddAction(Resource.Drawable.LoopInOrder24, "Loop In Order", loopPending);
                    break;
                case RepeatMethod.RepeatOnce:
                    not.AddAction(Resource.Drawable.RepeatOnce24, "Repeat Once", loopPending);
                    break;
                default:
                    not.AddAction(Resource.Drawable.RepeatOne24, "Repeat One", loopPending);
                    break;
            }

            not.AddAction(Resource.Drawable.ExitNotificationIcon, "Exit", cancelPending);

            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.Notify(NOTIFICATION_ID, not.Build());
        }

        private void MainActivity_OnFlowInterrupted(FlowInterruptedEventArgs e)
        {
            if (e.Type == FlowInterruptedEventArgs.InterruptionType.pause)
            {
                switch (manager.ViewMethod)
                {
                    case ViewMethods.AllTracks:
                        CurrentlyPlaying.Text = "Paused at: " + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    case ViewMethods.Playlist:
                        CurrentlyPlaying.Text = "Paused at: " + selectedPlaylistName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    default:
                        CurrentlyPlaying.Text = "Paused at: " + CurrentlyPlayingTrack.ContainingFolderName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                }
            }
            else
            {
                switch (manager.ViewMethod)
                {
                    case ViewMethods.AllTracks:
                        CurrentlyPlaying.Text = "Currently Playing: " + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    case ViewMethods.Playlist:
                        CurrentlyPlaying.Text = "Currently Playing: " + selectedPlaylistName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    default:
                        CurrentlyPlaying.Text = "Currently Playing: " + CurrentlyPlayingTrack.ContainingFolderName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                }
                if (e.Type == FlowInterruptedEventArgs.InterruptionType.prevnext)
                    WriteSharedPreferences(SongInPlaylist: position);
            }
        }

        private void Remove_Click(object sender, sys.EventArgs e)
        {
            Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
            intent.PutExtra("Action", "Remove");
            intent.PutExtra("DataExist", true);
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.SlidingRightAnim, Resource.Animation.SlidingLeftAnim);
            SetupBoxes();
        }

        private void Add_Click(object sender, sys.EventArgs e)
        {
            Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
            intent.PutExtra("Action", "Add");
            intent.PutExtra("DataExist", true);
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.SlidingRightAnim, Resource.Animation.SlidingLeftAnim);
            SetupBoxes();
        }

        private void SelectedItems_OnListCleared()
        {
            foreach(var item in selectedItems)
            {
                item.OnSelectionChanged -= OnSelectionChanged;
            }
        }

        private void SelectedItems_OnItemRemoved(PosArgs e)
        {
            selectedItems[e.Position].OnSelectionChanged -= OnSelectionChanged;
        }

        private void SelectedItems_OnItemAdded()
        {
            selectedItems[selectedItems.Count - 1].OnSelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectableEventArgs e)
        {
            if (e.Selected)
                selectedCount.Text = "Selected: " + ++count;
            else
                selectedCount.Text = "Selected: " + --count;
            if (count < 0) count = 0;
            if (count > 0 && !ShowAddRemove) ShowAddRemove = true;
            else if (count == 0 && ShowAddRemove) ShowAddRemove = false;
        }

        private void Grid_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (selectedItems.Count > 0)
                selectedItems.Clear();
            for(int i = 0; i < tracks.Count; i++)
                selectedItems.Add(new SelectableModel<Track>() { Data = tracks[i], Selected = false });
            prevM = manager.ViewMethod;
            manager.ViewMethod = ViewMethods.Selection;
            //grid.Adapter = new SongMultiSelectAdapter(this, ref selectedItems);
        }

        private void MainActivity_OnShowAddRemoveChanged(ShowAddRemoveEventArgs e)
        {
            if (e.Value)
            {
                addRemoveFPLL.Visibility = ViewStates.Visible;
            }
            else
            {
                addRemoveFPLL.Visibility = ViewStates.Gone;
            }
        }

        private void Manager_OnViewMethodChanged(object sender, ViewMethodEventArgs e)
        {
            //Task<List<Track>> ts;
            if (e.newVMethod == ViewMethods.AllTracks)
            {
                if (CurrentlyPlaying.Text == selectedPlaylistName + "/")
                    CurrentlyPlaying.Text = "Currently Playing: Nothing";
                grid.Adapter = null;
                // tracks = cache.GetCachedSongs(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                Task.Run(() => {
                    tracks.Clear();
                    tracks.Capacity = tracks.Count;
                    cache = new XmlCacheParser();
                    //tracks = cache.GetCachedSongs(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                    for(int i = 0; i < cache.GetMaxCountOfElements(); i++)
                    {
                        tracks.Add(cache.GetCachedSong(this, i));
                        RunOnUiThread(() => { if(tracks.Count != 0)manager.SetAdapter(this, ref grid, tracks, folders); });
                    }
                    cache.Release();                
                });
            }
            else if(e.newVMethod == ViewMethods.Playlist)
            {
                Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
                intent.PutExtra("Action", "Select");
                intent.PutExtra("DataExist", false);
                StartActivityForResult(intent, PlaylistManagerID);
            }
            else
            {
                //cache = new XmlCacheParser();
                //tracks = cache.GetCachedSongs(this);
               // cache.Release();
                if (e.newVMethod == ViewMethods.ContainingFolders)
                {
                    if (CurrentlyPlaying.Text == selectedPlaylistName + "/")
                        CurrentlyPlaying.Text = "Currently Playing: Nothing";
                }
                //if(tracks.Count > 0)
                    manager.SetAdapter(this, ref grid, tracks, folders);
            }
            if (e.newVMethod == ViewMethods.Selection || e.newVMethod == ViewMethods.ContainingFolders)
                return;
            WriteSharedPreferences(writeViewMethod: true);
        }

        private void SearchBar_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                if (matchesPos == null)
                    matchesPos = new List<int>();
                else { matchesPos.Clear(); matchesPos.Capacity = matchesPos.Count; }
                searchBar.Text = string.Empty;
                prevM = manager.ViewMethod;
                if(manager.ViewMethod == ViewMethods.ContainingFolders)
                {
                    manager.ViewMethod = ViewMethods.ContainingFolderSongs;
                }
            }
            if (!e.HasFocus)
            {
                matchesPos = null;
                // manager.ViewMethod = prevM;
                if (prevM != manager.ViewMethod)
                    manager.ViewMethod = prevM;
                manager.SetAdapter(this, ref grid, tracks, folders);
            }
        }

        private void SearchBar_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            matchesPos.Clear();
            matchesPos.Capacity = matchesPos.Count;
            List<Track> matches = new List<Track>();
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].FullTitle.ToLower().Contains(e.Editable.ToString().ToLower()) || tracks[i].AuthorName.ToLower().Contains(e.Editable.ToString().ToLower()))
                {
                    matchesPos.Add(i);
                    matches.Add(tracks[i]);
                }

            }
            //manager.SetAdapter(this, ref grid, matches, folders);    
            grid.Adapter = new SongAdapter(this, matches);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.ActionBar_Search)
            {
                if (manager.ViewMethod == ViewMethods.ContainingFolders)
                {
                    cache = new XmlCacheParser();
                    Task.Run(() => {
                        tracks.Clear();
                        tracks.Capacity = tracks.Count;
                        for (int i = 0; i < cache.GetMaxCountOfElements(); i++)
                            tracks.Add(cache.GetCachedSong(this, i));
                        cache.Release();
                    });
                }
                if(searchBar.Visibility == ViewStates.Gone)
                {
                    searchBar.Visibility = ViewStates.Visible;
                    searchBar.RequestFocus();
                    InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    imm.ShowSoftInput(searchBar, ShowFlags.Implicit);
                }
                else
                {
                    searchBar.Visibility = ViewStates.Gone;
                    InputMethodManager inputMethodManager = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    inputMethodManager.ToggleSoftInput(0, HideSoftInputFlags.ImplicitOnly);
                }

                return true;
            }
            else if(id == Resource.Id.ActionBar_Sort)
            {
                SetupBoxes();
                FragmentTransaction trans = FragmentManager.BeginTransaction();
                SortByFragment sortby = new SortByFragment();
                sortby.Show(trans, "Sort_By");
                return true;
            }
            else if(id == Resource.Id.ActionBar_PlaylistManager)
            {
                SetupBoxes();
                Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.SlidingRightAnim, Resource.Animation.SlidingLeftAnim);
                return true;
            }
            else if(id == Resource.Id.ActionBar_Advanced)
            {
                SetupBoxes();
                Intent intent = new Intent(this, typeof(AdvancedActivity));
                StartActivity(intent);
                return true;
            }
            else if(id == Resource.Id.ActionBar_Loopback)
            {
                FragmentTransaction trans = FragmentManager.BeginTransaction();
                LoopbackSettingsFragment loopback = new LoopbackSettingsFragment();
                loopback.Show(trans, "LoopBack");
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void SetupBoxes()
        {
            if (searchBar.Visibility == ViewStates.Visible)
                searchBar.Visibility = ViewStates.Gone;
            if (addRemoveFPLL.Visibility == ViewStates.Visible)
                addRemoveFPLL.Visibility = ViewStates.Gone;
        }

        private async void Grid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (manager.Repeat == RepeatMethod.Random && onStart)
                    onStart = false;

                if (manager.ViewMethod == ViewMethods.AllTracks || manager.ViewMethod == ViewMethods.ContainingFolderSongs || manager.ViewMethod == ViewMethods.Playlist)
                {
                    if (searchBar.Visibility == ViewStates.Gone || (searchBar.Visibility == ViewStates.Visible && matchesPos.Count == 0))
                    {
                        position = e.Position;
                        InitializePlayer();
                        if(manager.ViewMethod == ViewMethods.AllTracks)
                            WriteSharedPreferences(writeViewMethod:true, SongInPlaylist:e.Position);
                        WriteSharedPreferences(writeViewMethod: true, SongInPlaylist: e.Position);
                    }
                    else if (searchBar.Visibility == ViewStates.Visible && matchesPos.Count > 0)
                    {
                        position = matchesPos[e.Position];
                        InitializePlayer();
                    }
                    CurrentlyPlaying_Click();
                }
                else if (manager.ViewMethod == ViewMethods.ContainingFolders)
                {
                    // List<Track> tracksInFolder = new List<Track>();
                    tracks.Clear();
                    //tracks = new List<Track>();
                    tracks.Capacity = tracks.Count;
                    manager.ViewMethod = ViewMethods.ContainingFolderSongs;

                    // grid.Adapter = null;
                    int nPosition = e.Position;
                    folder = e.Position;
                    await Task.Run(() =>
                    {
                        cache = new XmlCacheParser();
                        //tracks = cache.GetCachedSongs(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        for (int i = 0; i < cache.GetMaxCountOfElements(); i++)
                        {
                            Track t = cache.GetCachedSong(this, i);
                            if (t.ContainingFolderName == folders[nPosition])
                            {
                                tracks.Add(t);
                                RunOnUiThread(() => { manager.SetAdapter(this, ref grid, tracks, folders); });
                            }
                        }
                        cache.Release();
                    });
                    WriteSharedPreferences(CurrentPlaylist:folders[folder]);
                }
            }
            catch (sys.OutOfMemoryException ex) { Toast.MakeText(this, ex.Message, ToastLength.Long).Show(); }
            catch (sys.Exception ex) { Toast.MakeText(this, ex.Message, ToastLength.Long).Show(); }
        }

        public override void OnLowMemory()
        {
            Toast.MakeText(this, "Low memory", ToastLength.Long).Show();
            base.OnLowMemory();
        }

        protected override void OnDestroy()
        {
            cache.Dispose();
            if (player != null)
            {
                player.Release();
                player.Dispose();
            }
            base.OnDestroy();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
           // SetContentView(Resource.Layout.Main);
           // InitializeUI();
        }

        private void InitializePlayer()
        {
            if (tracks.Count == 0) return;
            CurrentlyPlayingTrack = tracks[position];
            
            if (player.IsPlaying)
            {
                player.Stop();
            }
            player.Reset();
            player.SetDataSource(CurrentlyPlayingTrack.Path);
            player.Prepare();
            player.Start();
            switch (manager.ViewMethod)
            {
                case ViewMethods.AllTracks:
                    CurrentlyPlaying.Text = "Currently Playing: " + CurrentlyPlayingTrack.FullTitle;
                    SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                    break;
                case ViewMethods.Playlist:
                    CurrentlyPlaying.Text = "Currently Playing: " + selectedPlaylistName + "/" + CurrentlyPlayingTrack.FullTitle;
                    SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                    break;
                default:
                    CurrentlyPlaying.Text = "Currently Playing: " + CurrentlyPlayingTrack.ContainingFolderName + "/" + CurrentlyPlayingTrack.FullTitle;
                    SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                    break;
            }
        }

        private void InitializeUI(string CurrentPlaylist, int SongInPlaylist)
        {
            if (tracks.Count == 0)
                return;
            if (folders == null)
                folders = new List<string>();
            else folders.Clear();
            foreach (Track t in tracks)
            {
                if (!ExistInContFolders(folders, t.ContainingFolderName))
                    folders.Add(t.ContainingFolderName);
            }
            //grid.Adapter = new SongAdapter(this, tracks);
            if (manager.ViewMethod == ViewMethods.Playlist)
            {
                Playlist_Manager.PlaylistManager Pmanager = new Playlist_Manager.PlaylistManager(CurrentPlaylist);
                tracks = Pmanager.FetchTracksFromPlaylist();
                if (tracks.Count == 0)
                    return;
                CurrentlyPlayingTrack = tracks[SongInPlaylist];
                position = SongInPlaylist;
                CurrentlyPlaying.Text = "Paused at: " + CurrentPlaylist + "/" + CurrentlyPlayingTrack.FullTitle;
                SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
            }
            else if (manager.ViewMethod == ViewMethods.ContainingFolderSongs)
            {
                int folder = 0;
                for (int i = 0; i < folders.Count; i++)
                    if (folders[i] == CurrentPlaylist)
                        folder = i;
                var temp = tracks.ToList();
                foreach (var track in temp)
                {
                    if (track.ContainingFolderName != folders[folder])
                        tracks.Remove(track);
                }
                CurrentlyPlayingTrack = tracks[SongInPlaylist];
                position = SongInPlaylist;
                CurrentlyPlaying.Text = "Paused at: " + CurrentlyPlayingTrack.ContainingFolderName + "/" + CurrentlyPlayingTrack.FullTitle;
                SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
            }
            else if (manager.ViewMethod == ViewMethods.AllTracks)
            {
                CurrentlyPlayingTrack = tracks[SongInPlaylist];
                CurrentlyPlaying.Text = "Paused at: " + CurrentlyPlayingTrack.FullTitle;
                SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
            }
            else
            {
                CurrentlyPlaying.Text = "Currently Playing: Nothing";
            }
            manager.SetAdapter(this, ref grid, tracks, folders);

            grid.ItemClick += Grid_ItemClick;
            playPause.Click += PlayPause_Click;
            next.Click += Next_Click;
            player.Completion += Player_Completion;
            previous.Click += Previous_Click;
        }

        private bool ExistInContFolders(List<string> folders, string f)
        {
            foreach (string folder in folders)
                if (folder == f)
                    return true;
            return false;
        }

        private void CurrentlyPlaying_Click()
        {
            if (CurrentlyPlaying.Text == "Currently Playing: Nothing")
            {
                Toast.MakeText(this, "Not available", ToastLength.Short).Show();
                return;
            }
            if (DetailsInstanceExists)
                return;
            DetailsInstanceExists = true;
            Intent intent = new Intent(this, typeof(Details));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.slidingInAnim, Resource.Animation.SlidingOutAnim);
        }

        public void Previous_Click(object sender, sys.EventArgs e)
        {
            //if (position <= 0)
            //    position = tracks.Count; //because we will abstract one when we send it for initialization
            if (manager.ViewMethod == ViewMethods.ContainingFolders)
                return;
            position = manager.HandleUserActivity(tracks.Count, position, true);
            InitializePlayer();
            WriteSharedPreferences(SongInPlaylist: position);
        }

        public static void Previous()
        {
            if (manager.ViewMethod == ViewMethods.ContainingFolders)
                return;
            position = manager.HandleUserActivity(tracks.Count, position, true);

            CurrentlyPlayingTrack = tracks[position];

            if (player.IsPlaying)
            {
                player.Stop();
            }
            player.Reset();
            player.SetDataSource(CurrentlyPlayingTrack.Path);
            player.Prepare();
            player.Start();
            OnFlowInterrupted?.Invoke(new FlowInterruptedEventArgs(FlowInterruptedEventArgs.InterruptionType.prevnext));
        }

        public static void Next()
        {
                if (manager.ViewMethod == ViewMethods.ContainingFolders)
                    return;
                position = manager.HandleUserActivity(tracks.Count, position);

                CurrentlyPlayingTrack = tracks[position];

                if (player.IsPlaying)
                {
                    player.Stop();
                }
                player.Reset();
                player.SetDataSource(CurrentlyPlayingTrack.Path);
                player.Prepare();
                player.Start();
                OnFlowInterrupted?.Invoke(new FlowInterruptedEventArgs(FlowInterruptedEventArgs.InterruptionType.prevnext));
         }

        public static void PlayPause()
        {
            if (player.IsPlaying)
            {
                player.Pause();
                OnFlowInterrupted?.Invoke(new FlowInterruptedEventArgs(FlowInterruptedEventArgs.InterruptionType.pause));
            }
            else
            {
                player.Start();
                OnFlowInterrupted?.Invoke(new FlowInterruptedEventArgs(FlowInterruptedEventArgs.InterruptionType.play));
            }
        }

        private void Player_Completion(object sender, sys.EventArgs e)
        {
            //if (position >= tracks.Count-1)
            //    position = - 1; //because we will add one when we send it for initialization
            // if we are not on the first open of the app then and only then retreive the next position.
            if (!onStart) position = manager.HandleAutoRepeat(tracks.Count, position);
            else
            {
                onStart = false;
                if (position >= tracks.Count)
                    position = 0;
            }
            if (manager.Repeat == RepeatMethod.NoRepeat || (manager.Repeat == RepeatMethod.RepeatOnce && manager.reachedEnd))
                return;
            SetPosition(position);
            WriteSharedPreferences(SongInPlaylist: position);
        }

        public void Next_Click(object sender, sys.EventArgs e)
        {
            //if (position >= tracks.Count-1)
            //    position = - 1; //because we will add one when we send it for initialization
            try
            {
                if (manager.ViewMethod == ViewMethods.ContainingFolders)
                    return;
                position = manager.HandleUserActivity(tracks.Count, position);
                InitializePlayer();
                WriteSharedPreferences(SongInPlaylist: position);
            }catch(Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        public void PlayPause_Click(object sender, sys.EventArgs e)
        {
            if (player.IsPlaying)
            {
                player.Pause();
                switch (manager.ViewMethod)
                {
                    case ViewMethods.AllTracks:
                        CurrentlyPlaying.Text = "Paused at: " + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    case ViewMethods.Playlist:
                        CurrentlyPlaying.Text = "Paused at: " + selectedPlaylistName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    default:
                        CurrentlyPlaying.Text = "Paused at: " + CurrentlyPlayingTrack.ContainingFolderName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                }
            }
            else
            {
                player.Start();
                switch (manager.ViewMethod)
                {                
                    case ViewMethods.AllTracks:
                        CurrentlyPlaying.Text = "Currently Playing: " + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    case ViewMethods.Playlist:
                        CurrentlyPlaying.Text = "Currently Playing: " + selectedPlaylistName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                    default:
                        CurrentlyPlaying.Text = "Currently Playing: " + CurrentlyPlayingTrack.ContainingFolderName + "/" + CurrentlyPlayingTrack.FullTitle;
                        SetNotificationBar(CurrentlyPlayingTrack.FullTitle, CurrentlyPlayingTrack.Image != null ? CurrentlyPlayingTrack.Image : BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));
                        break;
                }
            }
            
        }
 
        private void FindViews()
        {
            grid = FindViewById<GridView>(Resource.Id.Main_gridView);
            playPause = FindViewById<Button>(Resource.Id.Main_PlayPauseBtn);
            next = FindViewById<Button>(Resource.Id.Main_NextButton);
            previous = FindViewById<Button>(Resource.Id.Main_PreviousButton);
            CurrentlyPlaying = FindViewById<TextView>(Resource.Id.Main_CurrentlyPlayingTextView);
            searchBar = FindViewById<EditText>(Resource.Id.Main_SearchSong);
            add = FindViewById<Button>(Resource.Id.Main_AddToPlaylist);
            remove = FindViewById<Button>(Resource.Id.Main_RemoveFromPlaylist);
            addRemoveFPLL = FindViewById<LinearLayout>(Resource.Id.Main_AddRemoveLayout);
            selectedCount = FindViewById<TextView>(Resource.Id.Main_Selected);
        }

        private void SetPosition(int newPosition)
        {
            InitializePlayer();
            //Check if there are any classes registered to the event
            if (OnUpdatePosition == null)
                return;
            // Rise a Update
            PositionEventArgs args = new PositionEventArgs(newPosition);
            OnUpdatePosition(this, args);
        }

        public override void OnBackPressed()
        {
            if (searchBar.Visibility == ViewStates.Visible)
            {
                searchBar.Visibility = ViewStates.Gone;
                return;
            }
            if (manager.ViewMethod == ViewMethods.ContainingFolderSongs)
            {
                if (CurrentlyPlaying.Text == selectedPlaylistName + "/")
                    CurrentlyPlaying.Text = "Currently Playing: Nothing";
                manager.ViewMethod = ViewMethods.ContainingFolders;
                // manager.SetAdapter(this, ref grid, null, folders);
            }
            else if (manager.ViewMethod == ViewMethods.Selection)
            {
                count = 0;
                ShowAddRemove = false;

                manager.ViewMethod = prevM;
            }
            else if (manager.ViewMethod == ViewMethods.AllTracks) manager.ViewMethod = ViewMethods.ContainingFolders;
            else base.OnBackPressed();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case PlaylistManagerID:
                    if(resultCode == Result.Ok)
                    {
                        selectedPlaylistName = data.GetStringExtra("PlaylistName");
                        Task.Run(() => { getTracksFromPlaylist(); });
                    }
                    break;
            }
        }

        private void getTracksFromPlaylist()
        {
            RunOnUiThread(() => { grid.Adapter = null; });
            Playlist_Manager.PlaylistManager manager = new Playlist_Manager.PlaylistManager(selectedPlaylistName);
            tracks.Clear();
            tracks.Capacity = tracks.Count;
            tracks = manager.FetchTracksFromPlaylist();
            cache = new XmlCacheParser();
            List<Track> cachedTracks = cache.GetCachedSongs(this);
            cache.Release();
            List<Track> tracksThatDontExist = new List<Track>();
            foreach(var item in tracks)
            {
                bool doesNotExist = false;
                foreach (var t in cachedTracks)
                {
                    if (t.Path == item.Path)
                    {
                        doesNotExist = true;
                    }
                }
                if (!doesNotExist)
                    tracksThatDontExist.Add(item);
            }
            if(tracksThatDontExist.Count > 0)
            {
                RunOnUiThread(() =>
                {
                    AlertDialog.Builder confirmation = new AlertDialog.Builder(this);
                    confirmation.SetMessage("There are some tracks in this playlist that either do not exist or you chose not to show them. You can either delete them or let them be. If you decide not to delete them the songs will appear normally but wont play.");
                    confirmation.SetTitle("Information");
                    confirmation.SetIcon(Resource.Drawable.NotFound);
                    confirmation.SetPositiveButton("Yes",
                        delegate
                        {
                            List<Paths> paths = new List<Paths>();
                            foreach (var track in tracksThatDontExist)
                                paths.Add(new Paths() { Title = track.FullTitle, Path = track.Path });
                            manager.RemoveTracksFromPlaylist(paths);
                            foreach (var t in tracksThatDontExist)
                                if (tracks.Contains(t))
                                    tracks.Remove(t);
                        });
                    confirmation.SetNegativeButton("No", delegate { Toast.MakeText(this, "Removal process canceled by user", ToastLength.Long).Show(); });
                    confirmation.Show();
                });
            }
            RunOnUiThread(() =>
            {
                player.Stop();
                player.Reset();
                CurrentlyPlaying.Text = selectedPlaylistName + "/";
                Manager.SetAdapter(this, ref grid, tracks, folders);
                WriteSharedPreferences(CurrentPlaylist: selectedPlaylistName);
            });
        }
    }
}
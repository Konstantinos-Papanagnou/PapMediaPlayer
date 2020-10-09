using Android.App;
using Android.Widget;
using Android.OS;
using sys = System;
using Android.Graphics;
using System.Collections.Generic;
using PapMediaPlayer.Models;
using PapMediaPlayer.Track_Finder;
using Android.Content;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Views.InputMethods;
using Android.Runtime;
using PapMediaPlayer.Activities_and_fragments.Settings;
using Android;
using Android.Content.PM;
using PapMediaPlayer.ViewManager;
using Newtonsoft.Json;
using System.Threading;

namespace PapMediaPlayer
{

    public class ShowAddRemoveEventArgs:sys.EventArgs
    {
        public bool Value { get; private set; }
        public ShowAddRemoveEventArgs(bool value)
        {
            Value = value;
        }
    }

    interface ICollectionListener
    {
        void Received(int count);
    }

    [Activity(Label = "Pap Media Player", Icon = "@drawable/Logo", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation| Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity, View.IOnTouchListener, GestureDetector.IOnGestureListener, Android.Support.V4.App.ActivityCompat.IOnRequestPermissionsResultCallback, ICollectionListener
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
        private const int PlaylistManagerID = 55;
        private GridView grid;
        private Button next, previous, playPause;
        private TextView CurrentlyPlaying, selectedCount;
        private EditText searchBar;
        private LinearLayout addRemoveFPLL;
        private Button add, remove;
        private LinearLayout Progress;
        private const int REQUEST_WRITEEXTERNALSTORAGE = 0;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == REQUEST_WRITEEXTERNALSTORAGE)
            {
                // Check if the only required permission has been granted
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                {
                    Toast.MakeText(this, "Permission Granted", ToastLength.Long).Show();
                    PapMediaPlayer.ViewManager.DataLoader.DataLoader dll = new ViewManager.DataLoader.DataLoader(this);
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        InitializeUI(dll);
                        RunOnUiThread(() => HideProgress());
                    });
                }
                else
                {
                    Toast.MakeText(this, "Permission Not Granted!", ToastLength.Long).Show();
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
        
        public static bool DetailsInstanceExists = false;

        private bool ShowAddRemove;
        private ViewManager.IViewManager ViewManager;

        private Receivers.SortMethodReceiver SortReceiver;
        private ServiceActivityReceiver ReceiveSongUpdates;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ActionBar bar = ActionBar;
            bar.SetBackgroundDrawable(new ColorDrawable(Color.Olive));
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FindViews();
            RequestPermissions();
            
            searchBar.AfterTextChanged += SearchBar_AfterTextChanged;
            searchBar.FocusChange += SearchBar_FocusChange;
            CurrentlyPlaying.SetOnTouchListener(this);
            _gestureDetector = new GestureDetector(this);

            grid.ItemLongClick += Grid_ItemLongClick;
            add.Click += Add_Click;
            remove.Click += Remove_Click;
            ReceiveSongUpdates = new ServiceActivityReceiver();
            ReceiveSongUpdates.Received += ReceiveSongUpdates_Received;
            RegisterReceiver(ReceiveSongUpdates, new IntentFilter("Activity." + ReceivedData.Actions.Next.ToString()));
        }

        private void ReceiveSongUpdates_Received(object sender, ReceivedData e)
        {
            if (e.Action != ReceivedData.Actions.Next)
                return;

            Paths t = Newtonsoft.Json.JsonConvert.DeserializeObject<Paths>(e.SerializedData);
            CurrentlyPlaying.Text = ViewManager.EnumeratePath() + t.Title;
        }

        private void RequestPermissions()
        {
            if (Android.Support.V4.App.ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                Android.Support.V4.App.ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, REQUEST_WRITEEXTERNALSTORAGE);
            }
            else {
                PapMediaPlayer.ViewManager.DataLoader.DataLoader dll = new ViewManager.DataLoader.DataLoader(this);
                ShowProgress();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    InitializeUI(dll);
                    RunOnUiThread(() => HideProgress());
                });
            }
        }

        private void Remove_Click(object sender, sys.EventArgs e)
        {
            Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
            intent.PutExtra("Action", "Remove");
            intent.PutExtra("DataExist", true);
            intent.PutExtra("Tracks", JsonConvert.SerializeObject(GetPathsFromTracks((SelectManager)ViewManager)));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.SlidingRightAnim, Resource.Animation.SlidingLeftAnim);
            SetupBoxes();
        }

        private void Add_Click(object sender, sys.EventArgs e)
        {
            Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
            intent.PutExtra("Action", "Add");
            intent.PutExtra("DataExist", true);
            intent.PutExtra("Tracks", JsonConvert.SerializeObject(GetPathsFromTracks((SelectManager)ViewManager)));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.SlidingRightAnim, Resource.Animation.SlidingLeftAnim);
            SetupBoxes();
        }

        private List<Paths> GetPathsFromTracks(SelectManager manager)
        {
            List<Paths> paths = new List<Paths>();
            var SelectedTracks = manager.GetTracks();
            foreach (var t in SelectedTracks)
                if (t.Selected)
                    paths.Add(new Paths() { Path = t.Data.Path, Title = t.Data.FullTitle });
            return paths;
        }

        #region Selection
        private void Grid_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if(ViewManager.GetType() == typeof(FoldersManager))
            {
                Toast.MakeText(this, "Cannot Initialize Selection Mode when browsing on folder view", ToastLength.Long).Show();
                return;
            }
            else if(ViewManager.GetType() == typeof(SelectManager))
            {
                Toast.MakeText(this, "You are already in selection mode!", ToastLength.Long).Show();
                return;
            }

            ShowProgress();
            ThreadPool.QueueUserWorkItem(o =>
            {
                ViewManager = new SelectManager(this, ((ITrackRetriever)ViewManager).GetTracks(), ViewManager);

                RunOnUiThread(() => grid.Adapter = ViewManager.GetAdapter());
                RunOnUiThread(() => HideProgress());
            });

        }

        public void Received(int count)
        {
            selectedCount.Text = "Selected: " + count;
            if (count > 0 && !ShowAddRemove) { ShowAddRemove = true; addRemoveFPLL.Visibility = ViewStates.Visible; }
            else if (count == 0 && ShowAddRemove) { ShowAddRemove = false; addRemoveFPLL.Visibility = ViewStates.Gone; }
        }

        #endregion

        private void SetupBoxes()
        {
            if (searchBar.Visibility == ViewStates.Visible)
                searchBar.Visibility = ViewStates.Gone;
            if (addRemoveFPLL.Visibility == ViewStates.Visible)
            {
                addRemoveFPLL.Visibility = ViewStates.Gone;
                ViewManager = ViewManager.OnBackPressed();
            }
        }

        #region Search

        private void SearchBar_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                searchBar.Text = string.Empty;
            if (!e.HasFocus)
            {
                ((PapMediaPlayer.ViewManager.Search.ISearchManager)ViewManager).SetSearch(false);
                grid.Adapter = ViewManager.GetAdapter();
            }
        }

        private void SearchBar_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        { 
            PapMediaPlayer.ViewManager.Search.ISearchManager SearchMan = (PapMediaPlayer.ViewManager.Search.ISearchManager)ViewManager;
            SearchMan.SetSearch(true);
            grid.Adapter = SearchMan.OnTextChanged(e.Editable.ToString());
        }
        #endregion
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
                if (searchBar.Visibility == ViewStates.Gone)
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
            else if (id == Resource.Id.ActionBar_Sort)
            {
                SetupBoxes();
                RegisterSortReceiver();
                FragmentTransaction trans = FragmentManager.BeginTransaction();

                SortByFragment sortby = new SortByFragment();
                sortby.Show(trans, "Sort_By");
                return true;
            }
            else if (id == Resource.Id.ActionBar_PlaylistManager)
            {
                SetupBoxes();
                Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.SlidingRightAnim, Resource.Animation.SlidingLeftAnim);
                return true;
            }
            else if (id == Resource.Id.ActionBar_Advanced)
            {
                SetupBoxes();
                Intent intent = new Intent(this, typeof(AdvancedActivity));
                StartActivity(intent);
                return true;
            }
            else if (id == Resource.Id.ActionBar_Loopback)
            {
                FragmentTransaction trans = FragmentManager.BeginTransaction();
                LoopbackSettingsFragment loopback = new LoopbackSettingsFragment();
                loopback.Show(trans, "LoopBack");
                return true;
            }
            else if (id == Resource.Id.ActionBar_Refresh)
            {
                ShowProgress();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    PapMediaPlayer.ViewManager.DataLoader.DataLoader loader = new ViewManager.DataLoader.DataLoader(this);
                    ViewManager = new AllTracksManager(this, loader.Load(true));
                    RunOnUiThread(() => grid.Adapter = ViewManager.GetAdapter());
                    RunOnUiThread(() => HideProgress());
                });

            }
            else if (id == Resource.Id.ActionBar_About)
            {
                using (Android.App.AlertDialog.Builder help = new Android.App.AlertDialog.Builder(this, Resource.Style.MyDialogTheme))
                {
                    help.SetTitle("About");
                    help.SetIcon(Resource.Drawable.About);
                    help.SetMessage("\n\nPap Media Player\n\nDeveloper Konstantinos Pap\n\nPowered by Pap Industries \n\nLoading Screens by Nikolaos Pothakis\n\nCopyrights © Konstantinos Pap 2018-2020.");
                    help.Show();
                }
            }
            return base.OnOptionsItemSelected(item);
        }

        private void RegisterSortReceiver()
        {
            if (SortReceiver != null)
                return;
            SortReceiver = new Receivers.SortMethodReceiver();
            SortReceiver.Received += R_Received;
            RegisterReceiver(SortReceiver, new IntentFilter("Action.Sort"));
        }

        private void UnRegisterReceiver()
        {
            if (SortReceiver == null) return;
            SortReceiver.Received -= R_Received;
            UnregisterReceiver(SortReceiver);
            SortReceiver = null;
        }

        private void R_Received(object sender, int e)
        {
            ShowProgress();
            ThreadPool.QueueUserWorkItem(o =>
            {
                switch ((ViewMethods)e)
                {
                    case ViewMethods.AllTracks:
                        ViewManager = new AllTracksManager(this);
                        break;
                    case ViewMethods.ContainingFolders:
                        ViewManager = new FoldersManager(this);
                        break;
                    case ViewMethods.Playlist:
                        Intent intent = new Intent(this, typeof(PlaylistManagerActivity));
                        intent.PutExtra("Action", "Select");
                        intent.PutExtra("DataExist", false);
                        RunOnUiThread(() => StartActivityForResult(intent, PlaylistManagerID));
                        break;
                }
                RunOnUiThread(() =>
                {
                    grid.Adapter = ViewManager.GetAdapter();

                    UnRegisterReceiver();
                    HideProgress();
                });
            });
        }



        private void Grid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                ShowProgress();
                ThreadPool.QueueUserWorkItem(o => {
                    
                    PapMediaPlayer.ViewManager.IViewManager temp = ViewManager;
                    ViewManager = ViewManager.OnClickHandler(e.Position);
                    if (temp != ViewManager)
                    {
                        RunOnUiThread(() => grid.Adapter = ViewManager.GetAdapter());
                    }
                    RunOnUiThread(() => HideProgress());

                });
            }
            catch (sys.OutOfMemoryException ex) { Toast.MakeText(this, ex.Message, ToastLength.Long).Show(); }
            catch (sys.Exception ex) { Toast.MakeText(this, ex.Message, ToastLength.Long).Show(); }
        }

        private void InitializeUI(PapMediaPlayer.ViewManager.DataLoader.DataLoader dll)
        {
            using (ISharedPreferences prefs = this.GetSharedPreferences(Startup.Prefs, FileCreationMode.Private))
            {
                try
                {
                    if (prefs.GetBoolean(Startup.REMEMBER, true))
                    {
                        ServiceSharedPref p = new ServiceSharedPref(this);
                        string pullfrom = p.PullDataFrom();
                        if (pullfrom == ServiceSharedPref.PULLDATAFROM_ALLTRACKS)
                            ViewManager = new AllTracksManager(this, dll.Load(false));
                        else if (pullfrom == ServiceSharedPref.PULLDATAFROM_FOLDER)
                            ViewManager = new ContainingFoldersManager(this, p.PullDataPath());
                        else
                            ViewManager = new PlaylistsManager(this, p.PullDataPath());

                        int songIndex = p.PullLastSong();
                        if (songIndex >= ViewManager.TrackCount())
                        {
                            songIndex = 0;
                            p.SaveLastSong(songIndex);
                        }

                        RunOnUiThread(() =>
                        {
                            if (ViewManager.TrackCount() != 0)
                                CurrentlyPlaying.Text = ViewManager.EnumeratePath() + ((PapMediaPlayer.ViewManager.ITrackRetriever)ViewManager).GetTracks()[songIndex].FullTitle;
                            else Toast.MakeText(this, "No Songs found.", ToastLength.Long).Show();
                        });
                    }
                    else
                    {
                        if (prefs.GetString(Startup.VIEWMETHOD, Startup.ALLTRACKS) == Startup.ALLTRACKS)
                            ViewManager = new AllTracksManager(this, dll.Load(false));
                        else if (prefs.GetString(Startup.VIEWMETHOD, Startup.ALLTRACKS) == Startup.CONTFOLDERS)
                            ViewManager = new FoldersManager(this, dll.Load(false));
                        else ViewManager = new PlaylistsManager(this, prefs.GetString(Startup.PLAYLISTNAME, string.Empty));
                    }
                }catch(TrackFinder.TrackNotFoundException ex)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, ex.Message + " Attempting to refetch data and restore cache", ToastLength.Long).Show();
                    });
                    ViewManager = new AllTracksManager(this, dll.Load(true, true));
                }
                catch(sys.Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, ex.Message + " Creating Cache.", ToastLength.Long).Show();
                    });
                    ViewManager = new AllTracksManager(this, dll.Load(true, true));
                }
            }
            RunOnUiThread(() =>
            {
                grid.Adapter = ViewManager.GetAdapter();
                grid.ItemClick += Grid_ItemClick;
                playPause.Click += PlayPause_Click;
                next.Click += Next_Click;
                previous.Click += Previous_Click;
            });
        }

        private void CurrentlyPlaying_Click()
        {
            if(ViewManager.TrackCount() == 0)
            {
                Toast.MakeText(this, "No Songs to play", ToastLength.Long).Show();
                return;
            }
            if (DetailsInstanceExists)
                return;
            DetailsInstanceExists = true;

            Intent intent = new Intent(this, typeof(Details));
            intent.PutExtra("PARAMS", string.Empty);
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.slidingInAnim, Resource.Animation.SlidingOutAnim);
        }

        public void Previous_Click(object sender, sys.EventArgs e)
        {
            ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.Previous);
        }

        public void Next_Click(object sender, sys.EventArgs e)
        {
            ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.Next);
        }
        public void PlayPause_Click(object sender, sys.EventArgs e)
        {
            ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.PlayPause);
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
            Progress = FindViewById<LinearLayout>(Resource.Id.LoadingLL);
        }

        public override void OnBackPressed()
        {
            if (searchBar.Visibility == ViewStates.Visible)
            {
                searchBar.Visibility = ViewStates.Gone;
                return;
            }
            ShowProgress();
            ThreadPool.QueueUserWorkItem(o =>
            {
                ViewManager.IViewManager temp = ViewManager;
                ViewManager = ViewManager.OnBackPressed();
                if (temp != ViewManager)
                {
                    RunOnUiThread(() => grid.Adapter = ViewManager.GetAdapter());
                }
                RunOnUiThread(() => HideProgress());
            });
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case PlaylistManagerID:
                    if(resultCode == Result.Ok)
                    {
                        ShowProgress();
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            ViewManager = new PlaylistsManager(this, data.GetStringExtra("PlaylistName"));
                            RunOnUiThread(() => { grid.Adapter = ViewManager.GetAdapter(); HideProgress(); });
                        });
                    }
                    break;
            }
        }

        private void HideProgress()
        {
            Progress.Visibility = ViewStates.Gone;
            grid.Visibility = ViewStates.Visible;
        }

        private void ShowProgress()
        {
            Progress.Visibility = ViewStates.Visible;
            grid.Visibility = ViewStates.Gone;
        }
    }
}
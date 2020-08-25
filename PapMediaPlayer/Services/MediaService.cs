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
using Android.Media;
using PapMediaPlayer.Models;
using PapMediaPlayer.StorageHelper;
using PapMediaPlayer.XmlParser;
using PapMediaPlayer.Manager;
using Android.Graphics;
using System.Reflection;
using Newtonsoft.Json;
using Android.Support.V4.Content;
using PapMediaPlayer.Activities_and_fragments.Settings;

namespace PapMediaPlayer.Services
{
    public enum ServiceCallAction
    {
        PlayPause,
        Next,
        Previous,
        PlayParam,
        LoopBackChangeParam,
        LoopBackChange,
        Stop,
        PlaylistUpdate,
        RequestLoopbackIcon,
        RequestSeekPosition,
        SeekTo,
        RequestCurrentSong
    }
    [Service(Name = "com.PapIndustries.PapMediaPlayerService")]
    public class MediaService : Service, AudioManager.IOnAudioFocusChangeListener
    {
        public static readonly string ACTION_KEY = "ACTION_KEY";
        public static readonly string PARAMS = "PARAMS";
        private Receiver receiver;
        private const int SERVICE_ID = 10;
        private MediaPlayer player;
        private List<Paths> tracks;
        private int song = 0;
        private IReplayManager manager;
        private Notification notify;
        private AudioManager audioManager;
        private ServiceSharedPref prefs;

        private bool FirstRun = true;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            InitList();
            InitPlayer();

            prefs = new ServiceSharedPref(this);
            RepeatSharedPrefs repPref = new RepeatSharedPrefs(this);        
            RepeatMethod repMethod = repPref.GetMethod();
            ChangeLoopback(repMethod.ToString());
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
                NotificationChannel chan = new NotificationChannel("com.PapIndustries.PapMediaPlayerService", "NotChannel", NotificationImportance.Max)
                {
                    LightColor = Color.Blue,
                    LockscreenVisibility = NotificationVisibility.Public
                };
                notificationManager.CreateNotificationChannel(chan);
            }
            SetNotificationBar();
            RegisterReceivers();
            StartForeground(SERVICE_ID, notify);
            base.OnCreate();
        }

        private void InitList()
        {
            ISharedPreferences p = this.GetSharedPreferences(Startup.Prefs, FileCreationMode.Private);
            if (p.GetBoolean(Startup.REMEMBER, true))
            {
                ServiceSharedPref prefs = new ServiceSharedPref(this);
                string pullfrom = prefs.PullDataFrom();
                if (pullfrom == ServiceSharedPref.PULLDATAFROM_ALLTRACKS)
                {
                    using (IXmlParsable parser = new JsonPlaylistParser(InternalStorageHelper.InternalPlaylistAllLocation, "All.json"))
                    {
                        tracks = (List<Paths>)parser.FetchItems();
                    }
                }
                else if (pullfrom == ServiceSharedPref.PULLDATAFROM_FOLDER)
                {
                    string folderName = prefs.PullDataPath();
                    tracks = Track_Finder.TrackFinder.GetSongsFromFolder(folderName);
                }
                else
                {
                    string playlistName = prefs.PullDataPath();
                    Playlist_Manager.PlaylistManager manager = new Playlist_Manager.PlaylistManager(playlistName);
                    tracks = Track_Finder.TrackFinder.ConvertToPaths(manager.FetchTracksFromPlaylist());
                }
                song = prefs.PullLastSong();
            }
            else
            {
                if (p.GetString(Startup.VIEWMETHOD, Startup.ALLTRACKS) == Startup.PLAYLIST)
                {
                    string playlistName = p.GetString(Startup.PLAYLISTNAME, string.Empty);
                    Playlist_Manager.PlaylistManager manager = new Playlist_Manager.PlaylistManager(playlistName);
                    tracks = Track_Finder.TrackFinder.ConvertToPaths(manager.FetchTracksFromPlaylist());
                }
                else
                    using (IXmlParsable parser = new JsonPlaylistParser(InternalStorageHelper.InternalPlaylistAllLocation, "All.json"))
                    {
                        tracks = (List<Paths>)parser.FetchItems();
                    }
            }
        }

        private void InitPlayer()
        {

            if (player == null) player = new MediaPlayer();
            player.Completion += Player_Completion;
            if (!RequestAudioFocus())
                StopSelf();
            player.Reset();
            player.SetDataSource(tracks[song].Path);
        }
        private void RegisterReceivers()
        {
            receiver = new Receiver();
            receiver.Received += Receiver_Received;
            RegisterReceiver(receiver, new IntentFilter("Pause"));
            RegisterReceiver(receiver, new IntentFilter("Previous"));
            RegisterReceiver(receiver, new IntentFilter("Next"));
            RegisterReceiver(receiver, new IntentFilter("Cancel"));
            RegisterReceiver(receiver, new IntentFilter("Details"));
            RegisterReceiver(receiver, new IntentFilter("LoopBack"));
        }

        public override void OnDestroy()
        {
            try
            {
                notify.Dispose();
                player.Stop();
                player.Release();
                player.Dispose();
                RemoveAudioFocus();
                UnregisterReceiver(receiver);
                StopForeground(true);
            }
            catch (Exception) { }
            finally
            {
                base.OnDestroy();
            }
        }
        private void Receiver_Received(object sender, ReceivedData e)
        {
            switch (e.Action)
            {
                case ReceivedData.Actions.Pause:
                    if (player.IsPlaying)
                        player.Pause();
                    else player.Start();
                    BroadcastPlayPause();
                    SetNotificationBar();
                    break;
                case ReceivedData.Actions.Previous:
                    PlayPrevious();
                    break;
                case ReceivedData.Actions.Next:
                    PlayNext();
                    break;
                case ReceivedData.Actions.Cancel:
                    StopSelf();
                    break;
                case ReceivedData.Actions.Details:
                    Intent intent = new Intent(this, typeof(Details));
                    Paths currentlyPlayingPaths = new Paths() { Path = tracks[song].Path, Title = tracks[song].Title };
                    intent.PutExtra("PARAMS", Newtonsoft.Json.JsonConvert.SerializeObject(currentlyPlayingPaths));
                    intent.AddFlags(ActivityFlags.NewTask);
                    StartActivity(intent);
                    break;
                case ReceivedData.Actions.LoopBack:
                    manager = manager.Next();
                    SaveRepMethod();
                    BroadcastLoopbackChange();
                    SetNotificationBar();
                    break;
                    
            }
        }
        private void PlayNext()
        {
            player.Reset();
            song = manager.HandleUserActivity(tracks.Count, song);
            player.SetDataSource(tracks[song].Path);
            player.Prepare();
            player.Start();
            BroadcastSongUpdate();
            SaveSong();
            SetNotificationBar();
        }
        private void PlayPrevious()
        {
            player.Reset();
            song = manager.HandleUserActivity(tracks.Count, song, true);
            player.SetDataSource(tracks[song].Path);
            player.Prepare();
            player.Start();
            BroadcastSongUpdate();
            SaveSong();
            SetNotificationBar();
        }

        private void BroadcastSongUpdate()
        {
            Intent intent = new Intent("Activity."+ReceivedData.Actions.Next.ToString());
            intent.PutExtra("PARAMS", JsonConvert.SerializeObject(tracks[song]));
            SendBroadcast(intent);
        }

        private void BroadcastPlayPause()
        {
            Intent intent = new Intent("Activity." + ReceivedData.Actions.Pause.ToString());
            intent.PutExtra("PARAMS", player.IsPlaying.ToString());
            SendBroadcast(intent);
        }

        private void BroadcastLoopbackChange()
        {
            Intent intent = new Intent("Activity." + ReceivedData.Actions.LoopBack.ToString());
            intent.PutExtra("PARAMS", manager.GetIcon().ToString());
            SendBroadcast(intent);
        }

        private void SaveSong()
        {
            if (GetSharedPreferences(Activities_and_fragments.Settings.Startup.Prefs, FileCreationMode.Private).GetBoolean("Remember", true))
            {
                prefs.SaveLastSong(song);
            }
        }

        private void Player_Completion(object sender, EventArgs e)
        {
            if(!FirstRun)
                song = manager.HandleAutoReplay(tracks.Count, song);
            else
                FirstRun = false;
            player.Reset();
            
            player.SetDataSource(tracks[song].Path);
            player.Prepare();
            player.Start();
            SaveSong();
            BroadcastSongUpdate();
            SetNotificationBar();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            ServiceCallAction action = (ServiceCallAction)intent.Extras.GetInt(ACTION_KEY);
            switch (action)
            {
                case ServiceCallAction.PlayPause:
                    if (!player.IsPlaying)
                        player.Start();
                    else player.Pause();
                    BroadcastPlayPause();
                    SetNotificationBar();
                    break;
                case ServiceCallAction.Previous:
                    PlayPrevious();
                    break;
                case ServiceCallAction.Next:
                    PlayNext();
                    break;
                case ServiceCallAction.LoopBackChange:
                    manager = manager.Next();
                    BroadcastLoopbackChange();
                    SetNotificationBar();
                    break;
                case ServiceCallAction.PlayParam:
                    song = intent.Extras.GetInt(PARAMS);
                    SaveSong();
                    Play();
                    BroadcastSongUpdate();
                    break;
                case ServiceCallAction.LoopBackChangeParam:
                    ChangeLoopback(intent.Extras.GetString(PARAMS));
                    break;
                case ServiceCallAction.Stop:
                    player.Stop();
                    player.Reset();
                    break;
                case ServiceCallAction.PlaylistUpdate:
                    HandlePlaylist(intent.Extras.GetString(PARAMS));
                    break;
                case ServiceCallAction.RequestLoopbackIcon:
                    BroadcastLoopbackChange();
                    break;
                case ServiceCallAction.RequestSeekPosition:
                    BroadcastSeekToPosition();
                    break;
                case ServiceCallAction.SeekTo:
                    player.SeekTo(intent.Extras.GetInt(PARAMS));
                    break;
                case ServiceCallAction.RequestCurrentSong:
                    BroadcastSongUpdate();
                    break;
            }
            return StartCommandResult.Sticky;
        }

        private void BroadcastSeekToPosition()
        {
            Intent intent = new Intent("Activity." + ReceivedData.Actions.SeekTo);
            intent.PutExtra(PARAMS, player.CurrentPosition.ToString());
            SendBroadcast(intent);
        }
        private void HandlePlaylist(string data)
        {
            List<Paths> deserializedTracks = JsonConvert.DeserializeObject<List<Paths>>(data);
            tracks = deserializedTracks;
        }

        private void ChangeLoopback(string Class)
        {
            //Reflection To decide what to initialize with no if and switches. Do not touch class names and assembly information.
            //In case of object not set to an instance of an object error check for the assembly information4
            //Dynamically loading the assembly
            string AssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Assembly assembly = Assembly.Load($"{AssemblyName}.dll");
            //Dynamically getting the type of the class we need
            Type classType = assembly.GetType($"{AssemblyName}.Manager.{Class}");
            //Creating an instance of the class we need (Specified by the intent came from LoopbackSettingsFragment).
            manager = (IReplayManager)Activator.CreateInstance(classType);
            //Notify the notification for the loopback changes.
            SaveRepMethod();
            //Inform GUI mostly for the change
            BroadcastLoopbackChange();
            SetNotificationBar();
        }

        private void SaveRepMethod()
        {
            RepeatSharedPrefs rep = new RepeatSharedPrefs(this);
            //Save
            rep.SetMethod(manager.Enumerate());
        }

        private void Play()
        {
            if (player.IsPlaying)
            {
                player.Stop();
            }
            player.Reset();
            player.SetDataSource(tracks[song].Path);
            player.Prepare();
            player.Start();
            BroadcastPlayPause();
            SetNotificationBar();
        }

        public override void OnLowMemory()
        {
            Toast.MakeText(this, "Low Memory", ToastLength.Long).Show();
            base.OnLowMemory();
        }

        private void SetNotificationBar()
        {
            Intent playPause = new Intent("Pause");
            Intent previous = new Intent("Previous");
            Intent next = new Intent("Next");
            Intent loop = new Intent("LoopBack");
            Intent details = new Intent("Details");
            Intent cancel = new Intent("Cancel");

            PendingIntent playPausePending = PendingIntent.GetBroadcast(this, 0, playPause, PendingIntentFlags.CancelCurrent);
            PendingIntent previousPending = PendingIntent.GetBroadcast(this, 1, previous, PendingIntentFlags.CancelCurrent);
            PendingIntent nextPending = PendingIntent.GetBroadcast(this, 2, next, PendingIntentFlags.CancelCurrent);
            PendingIntent cancelPending = PendingIntent.GetBroadcast(this, 3, cancel, PendingIntentFlags.CancelCurrent);
            PendingIntent loopPending = PendingIntent.GetBroadcast(this, 4, loop, PendingIntentFlags.CancelCurrent);
            PendingIntent OpenDetails = PendingIntent.GetBroadcast(this, 10, details, PendingIntentFlags.CancelCurrent);
            string artist = Track_Finder.TrackFinder.GetArtistOfTrack(tracks[song].Path);

            using (Notification.Builder not = new Notification.Builder(this, "com.PapIndustries.PapMediaPlayerService"))
            {
                using (Notification.MediaStyle mediaStyle = new Notification.MediaStyle())
                {
                    not.SetSmallIcon(Resource.Drawable.Logo).SetContentTitle(tracks[song].Title).SetContentText(artist)
                        .SetOngoing(player.IsPlaying).SetStyle(mediaStyle.SetShowActionsInCompactView(0, 1, 2, 3, 4))
                        .AddAction(Resource.Drawable.Previous, "Previous", previousPending).SetContentIntent(OpenDetails)
                        .SetPriority((int)NotificationPriority.Max).SetCategory(Notification.CategoryService);
                    Bitmap icon = Track_Finder.TrackFinder.GetImageOfTrack(tracks[song].Path);
                    not.SetLargeIcon(icon ?? BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note));

                    if (!player.IsPlaying) not.AddAction(Resource.Drawable.Play, "Play", playPausePending);
                    else not.AddAction(Resource.Drawable.Pause, "Pause", playPausePending);
                    not.AddAction(Resource.Drawable.Next, "Next", nextPending);
                    not.AddAction(manager.GetIcon(), manager.GetTitle(), loopPending);
                    not.AddAction(Resource.Drawable.ExitNotificationIcon, "Exit", cancelPending);
                    var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                    notify = not.Build();
                    notificationManager.Notify(SERVICE_ID, notify);
                }
            }
        }

        public void OnAudioFocusChange([GeneratedEnum] AudioFocus focusChange)
        {
            switch (focusChange)
            {
                case AudioFocus.Gain:
                    // resume playback
                    if (player == null) InitPlayer();
                    else if (!player.IsPlaying) player.Start();
                    player.SetVolume(1.0f, 1.0f);
                    break;
                case AudioFocus.Loss:
                    // Lost focus for an unbounded amount of time: stop playback and release media player
                    if (player.IsPlaying) player.Stop();
                    player.Release();
                    player = null;
                    break;
                case AudioFocus.LossTransient:
                    // Lost focus for a short time, but we have to stop
                    // playback. We don't release the media player because playback
                    // is likely to resume
                    if (player.IsPlaying) player.Pause();
                    break;
                case AudioFocus.LossTransientCanDuck:
                    // Lost focus for a short time, but it's ok to keep playing
                    // at an attenuated level
                    if (player.IsPlaying) player.SetVolume(0.1f, 0.1f);
                    break;
            }
        }
        private bool RequestAudioFocus()
        {
            audioManager = (AudioManager)GetSystemService(Context.AudioService);
            AudioFocusRequest result = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            if (result == AudioFocusRequest.Granted)
            {
                //Focus gained
                return true;
            }
            //Could not gain focus
            return false;
        }
        private bool RemoveAudioFocus()
        {
            return AudioFocusRequest.Granted ==
                    audioManager.AbandonAudioFocus(this);
        }
    }
}
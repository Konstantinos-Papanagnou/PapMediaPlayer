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
using Android.Graphics;
using System.Timers;
using Android.Content.Res;
using PapMediaPlayer.Models;

namespace PapMediaPlayer
{
    [Activity(Label = "Details", Icon = "@drawable/Logo", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Details : Activity, View.IOnTouchListener, GestureDetector.IOnGestureListener
    {
        #region Gesture Handeler
        GestureDetector _gestureDetector;
        public bool OnDown(MotionEvent e)
        {
            return false;
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
                if (Math.Abs(diffX) <= Math.Abs(diffY))
                {
                    if (Math.Abs(diffY) > SWIPE_THRESHOLD && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
                    {
                        if (diffY <= 0)
                        {
                            if (timer.Enabled)
                                timer.Stop();
                            MainActivity.DetailsInstanceExists = false;
                            Finish();
                            OverridePendingTransition(Resource.Animation.SlidingUpFromBottomAnim, Resource.Animation.SlidingUpFromTop);
                        }
                    }
                }
            }
            catch (Exception ex)
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

        private LinearLayout layout;
        private TextView TitleTV, ArtistTV, AlbumTV, GenreTV, CopyrightTV, CorruptedTV, LengthTV, YearTV, CorruptedReasonTV, CurrentPosTV;
        private ImageView imageView;
        private SeekBar seeker;
        private ImageButton previous, next, playPause, loop;
        private bool refresh = false;
        private bool inMins = false;
        private Timer timer;
        private int prevmin;
        private long lastClickRaised = 0;
        private int second = 0;
        private bool wasDouble = false;
        private Track CurrentTrack;
        private ServiceActivityReceiver receiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar bar = ActionBar;
            bar.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(Color.Olive));
            bar.SetHomeButtonEnabled(true);
            bar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.Details);
            FindViews();
            RegisterReceivers();
    
            // Create your application here
            if (Intent.GetStringExtra("PARAMS") != string.Empty)
            {
                List<Paths> paths = new List<Paths> { Newtonsoft.Json.JsonConvert.DeserializeObject<Paths>(Intent.GetStringExtra("PARAMS")) };
                CurrentTrack = Track_Finder.TrackFinder.ConvertPathsToTracks(paths)[0];
                PopulateViews();
            }
            else
            {
                ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.RequestCurrentSong);
            }
            seeker.ProgressChanged += Seeker_ProgressChanged;

            layout.SetOnTouchListener(this);
            _gestureDetector = new GestureDetector(this);


            timer = new Timer
            {
                Interval = 1000
            };
            timer.Elapsed += Timer_Elapsed;
            ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.RequestSeekPosition);
            next.Click += Next_Click;
            previous.Click += Previous_Click;
            playPause.Click += PlayPause_Click;
            loop.Click += Loop_Click;
            ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.RequestLoopbackIcon);
        }

        private void RegisterReceivers()
        {
            receiver = new ServiceActivityReceiver();
            receiver.Received += Receiver_Received;
            RegisterReceiver(receiver, new IntentFilter("Activity." + ReceivedData.Actions.Next.ToString()));
            RegisterReceiver(receiver, new IntentFilter("Activity." + ReceivedData.Actions.Pause.ToString()));
            RegisterReceiver(receiver, new IntentFilter("Activity." + ReceivedData.Actions.LoopBack.ToString()));
            RegisterReceiver(receiver, new IntentFilter("Activity." + ReceivedData.Actions.SeekTo.ToString()));
        }

        private void Receiver_Received(object sender, ReceivedData e)
        { 
            switch (e.Action)
            {
                case ReceivedData.Actions.Next:
                    Paths t = Newtonsoft.Json.JsonConvert.DeserializeObject<Paths>(e.SerializedData);
                    CurrentTrack = Track_Finder.TrackFinder.ConvertPathsToTracks(new List<Paths>() { t })[0];
                    PopulateViews();
                    break;
                case ReceivedData.Actions.Pause:
                    if (bool.Parse(e.SerializedData))
                    {
                        playPause.SetImageResource(Resource.Drawable.Pause);
                    }
                    else
                    {
                        playPause.SetImageResource(Resource.Drawable.Play);
                    }
                    break;
                case ReceivedData.Actions.LoopBack:
                    int icon = int.Parse(e.SerializedData);
                    loop.SetImageResource(icon);
                    break;
                case ReceivedData.Actions.SeekTo:
                    if (inMins)
                        seeker.Progress = int.Parse(e.SerializedData) / 60 / 1000; // reconverting to mins
                    else
                        seeker.Progress = int.Parse(e.SerializedData) / 1000; // reconverting to secs
                    break;
            }
        }
        private void Loop_Click(object sender, EventArgs e)
        {
            ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.LoopBackChange);
        }

        private void PlayPause_Click(object sender, EventArgs e)
        {
            ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.PlayPause);
        }

        private async void Previous_Click(object sender, EventArgs e)
        {
            long thistime = DateTime.Now.Millisecond;
            if (thistime - lastClickRaised < 250 && ((int)DateTime.Now.Second == second || (int)DateTime.Now.Second == second + 1))
            {
                RunOnUiThread(() => { if (seeker.Progress - 10 < 0) seeker.Progress = 0; else seeker.Progress -= 10; });
                lastClickRaised = 0;
                wasDouble = true;
            }
            else
            {
                second = DateTime.Now.Second;
                lastClickRaised = thistime;
                await System.Threading.Tasks.Task.Delay(500);
                if (wasDouble)
                    wasDouble = false;
                else
                    ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.Previous);    
            }
        }

        private async void Next_Click(object sender, EventArgs e)
        {
            
            long thistime = DateTime.Now.Millisecond;
            if (thistime - lastClickRaised < 250 && ((int)DateTime.Now.Second == second || (int) DateTime.Now.Second == second + 1))
            {
                RunOnUiThread(() => { seeker.Progress += 10; });
                lastClickRaised = 0;
                wasDouble = true;
            }
            else
            {
                second = DateTime.Now.Second;
                lastClickRaised = thistime;
                await System.Threading.Tasks.Task.Delay(500);
                if (wasDouble)
                    wasDouble = false;
                else
                    ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.Next);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int milli;
            if (inMins)
            {
                milli = seeker.Progress * 60 * 1000 + 1000; // convert to sec -> millis
                prevmin = seeker.Progress * 60; // we want the seconds to check
                seeker.Progress = milli / 60 / 1000; // reconverting to mins
            }
            else
            {
                prevmin = seeker.Progress; // already seconds
                milli = seeker.Progress * 1000 + 1000; // converting to millis
                seeker.Progress = milli / 1000; // reconverting to secs
            }


        }

        private void Seeker_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
                int Seconds = e.Progress % 60;
                string seconds = Seconds.ToString();
                if (seconds.Count() == 1)
                {
                    string temp = seconds;
                    seconds = "0" + temp;
                }
                CurrentPosTV.Text = inMins ? e.Progress / 3600 + ":" + seconds + ":" + e.Progress % 60 : e.Progress / 60 + ":" + seconds;
            if (!refresh)
            {
                // if we moving forward at least more than a second or backwars then seek to the position
                if (!inMins && (e.Progress * 1000 - prevmin * 1000 > 1100 || e.Progress * 1000 - prevmin * 1000 < 0))
                {
                    //MainActivity.MainPlayer.SeekTo(e.Progress * 1000);
                    ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.SeekTo, e.Progress * 1000);
                }
                // same as the above for minutes
                else if (inMins && (e.Progress * 60 * 1000 - prevmin * 1000 * 60 > 1100 || e.Progress * 60 * 1000 - prevmin * 1000 * 60 < 0))
                {
                    ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.SeekTo, e.Progress * 60 * 1000);
                    //MainActivity.MainPlayer.SeekTo(e.Progress * 60 * 1000);
                }
            }
            else refresh = false;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            if (timer.Enabled)
                timer.Stop();
            MainActivity.DetailsInstanceExists = false;
            Finish();
            OverridePendingTransition(Resource.Animation.SlidingUpFromBottomAnim, Resource.Animation.SlidingUpFromTop);
        }

        protected override void OnDestroy()
        {
            MainActivity.DetailsInstanceExists = false;
            if (timer.Enabled)
                timer.Stop();
            UnregisterReceiver(receiver);
            Finish();
            OverridePendingTransition(Resource.Animation.SlidingUpFromBottomAnim, Resource.Animation.SlidingUpFromTop);
            base.OnDestroy();
        }

        private void PopulateViews()
        {
            Bitmap image = CurrentTrack.Image;
            if (image == null)
            {
                using (BitmapFactory.Options opt = new BitmapFactory.Options())
                {
                    opt.OutHeight = 48;
                    opt.OutWidth = 48;
                    opt.InSampleSize = 2;
                    image = BitmapFactory.DecodeResource(Resources, Resource.Drawable.Note, opt);
                }
            }
            CorruptedReasonTV.Visibility = ViewStates.Gone;
            CorruptedTV.Visibility = ViewStates.Gone;
            TitleTV.Text = "Title: " + CurrentTrack.FullTitle;// MainActivity.Tracks[position].FullTitle;
            ArtistTV.Text = "Performer: " + CurrentTrack.AuthorName; //MainActivity.Tracks[position].AuthorName;
            AlbumTV.Text = "Album: " + CurrentTrack.Album;// MainActivity.Tracks[position].Album;
            GenreTV.Text = "Genre: " + CurrentTrack.Genre;// MainActivity.Tracks[position].Genre;
            CopyrightTV.Text = "Copyrights: " + CurrentTrack.Copyrights;// MainActivity.Tracks[position].Copyrights;
            LengthTV.Text = CurrentTrack.Length;// MainActivity.Tracks[position].Length;
            YearTV.Text = "Release Year: " + CurrentTrack.Year;// MainActivity.Tracks[position].Year.ToString();
            imageView.SetImageBitmap(image);
            if (CurrentTrack.isCorrupted)
            {
                CorruptedTV.Visibility = ViewStates.Visible;
                CorruptedTV.Text = CurrentTrack.isCorrupted ? "Corrupted: YES" : "Corrupted: NO";//MainActivity.Tracks[position].isCorrupted ? "Corrupted: YES" : "Corrupted: NO";
            }
            if (CurrentTrack.CorruptionReason != null)
            {
                CorruptedReasonTV.Visibility = ViewStates.Visible;
                CorruptedReasonTV.Text = "Possible Corruption Reasons: " + string.Join(",", CurrentTrack.CorruptionReason);
            }
            seeker.Progress = 0;
            if (CurrentTrack.DurationMins < 600)
            {
                inMins = false;
                seeker.Max = Convert.ToInt32(CurrentTrack.DurationMins * 60);
                CurrentPosTV.Text = "0:00";
            }
            else
            {
                inMins = true;
                seeker.Max = (int)CurrentTrack.DurationMins;
                CurrentPosTV.Text = "0:00:00";
            }
        }

        private void FindViews()
        {
            TitleTV = FindViewById<TextView>(Resource.Id.Details_TitleTV);
            ArtistTV = FindViewById<TextView>(Resource.Id.Details_ArtistTV);
            AlbumTV = FindViewById<TextView>(Resource.Id.Details_AlbumTV);
            GenreTV = FindViewById<TextView>(Resource.Id.Details_GenreTV);
            CopyrightTV = FindViewById<TextView>(Resource.Id.Details_CopyrightsTV);
            CorruptedTV = FindViewById<TextView>(Resource.Id.Details_CorruptedTV);
            LengthTV = FindViewById<TextView>(Resource.Id.Details_FullLengthTextView);
            CurrentPosTV = FindViewById<TextView>(Resource.Id.Details_CurrentPositionTextView);
            YearTV = FindViewById<TextView>(Resource.Id.Details_YearTV);
            imageView = FindViewById<ImageView>(Resource.Id.Details_ImageView);
            CorruptedReasonTV = FindViewById<TextView>(Resource.Id.Details_CorruptionReasonTextView);
            seeker = FindViewById<SeekBar>(Resource.Id.Details_SeekBar);
            layout = FindViewById<LinearLayout>(Resource.Id.Details_LinearForGesture);
            previous = FindViewById<ImageButton>(Resource.Id.Details_Previous);
            next = FindViewById<ImageButton>(Resource.Id.Details_Next);
            playPause = FindViewById<ImageButton>(Resource.Id.Details_PlayPause);
            loop = FindViewById<ImageButton>(Resource.Id.Details_Loop);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (!hasFocus)
                timer.Stop();
            else
            {
                refresh = true;

                timer.Start();
                ServiceStartHelper.StartHybridService(this, Services.ServiceCallAction.RequestSeekPosition);
                //if (inMins)
                //    seeker.Progress = MainActivity.MainPlayer.CurrentPosition / 60 / 1000; // reconverting to mins
                //else
                //    seeker.Progress = MainActivity.MainPlayer.CurrentPosition / 1000; // reconverting to secs
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem menuItem)
        {
          //  if (menuItem.ItemId != Android.Resource.Id.Home)
                //return base.OnOptionsItemSelected(menuItem);
            if (timer.Enabled)
                timer.Stop();
            Finish();
            OverridePendingTransition(Resource.Animation.SlidingUpFromBottomAnim, Resource.Animation.SlidingUpFromTop);
            return true;
        }
    }
}
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
using PapMediaPlayer.Activities_and_fragments.Settings;
using PapMediaPlayer.Models;
using PapMediaPlayer.StorageHelper;
using PapMediaPlayer.Track_Finder;
using PapMediaPlayer.XmlParser;
using System.IO;

namespace PapMediaPlayer.ViewManager.DataLoader
{
    public class DataLoader
    {
        private readonly Context Context;
        public DataLoader(Context context)
        {
            Context = context;
        }

        public List<Track> Load(bool append, bool restore = false)
        {
            string path = InternalStorageHelper.InternalPlaylistAllLocation;
            System.IO.Directory.CreateDirectory(path);
            using (IXmlParsable parser = new JsonPlaylistParser(path, "All.json"))
            {
                List<Track> tracks = null;
                if (!append)
                    tracks = JsonCacheParser.Read();
                if (tracks == null)
                {
                    List<Paths> songs = SearchForSongs();
                    if (restore)
                        if (System.IO.File.Exists(Path.Combine(path, "All.json")))
                            File.Delete(Path.Combine(path, "All.json"));
                    parser.AddItems(songs);
                    tracks = TrackFinder.ConvertPathsToTracks(songs);
                    CacheSongs(tracks);

                }

                return tracks;
            }
        }


        private List<Paths> SearchForSongs()
        {
            string[] filters; char[] specialChars;

            using (IXmlParsable parser = new XmlFilterParser(InternalStorageHelper.InternalXmlFileLocation + "/FiltersAndSpecialChars"))
            {
                XmlFilterParser.RVal data = (XmlFilterParser.RVal)parser.FetchItems();
                filters = data.filters;
                specialChars = data.specialChars;
            }
            List<Track> tracksFromSD = new List<Track>();
            bool hasSd = false;
            try
            {
                tracksFromSD = TrackFinder.GetListOfTracksFromSD(Context, filters, specialChars);
                hasSd = true;
            }
            catch (NotMountedException e)
            {
                ISharedPreferences prefs = Context.GetSharedPreferences(Notification_Settings.Root, FileCreationMode.Private);
                if (prefs.GetBoolean(Notification_Settings.ErrorNotKey, true))
                {
                    using (var style = new Android.Support.V7.App.NotificationCompat.BigTextStyle())
                    {
                        using (Android.Support.V7.App.NotificationCompat.Builder builder = new Android.Support.V7.App.NotificationCompat.Builder(Context))
                        {
                            using (Android.Support.V4.App.NotificationCompat.Builder not = builder
                            .SetSmallIcon(Resource.Drawable.Logo).SetContentTitle("No SD card found!").SetContentText(e.Message)
                            .SetPriority((int)NotificationPriority.Min).SetStyle(style.BigText(e.Message))
                            .SetVibrate(new long[] { 1000, 1000, 1000, 1000 }))
                            {
                                var notificationManager = (NotificationManager)Context.GetSystemService(Context.NotificationService);
                                notificationManager.Notify(0, not.Build());
                            }
                        }
                    }
                }
            }
            List<Track> tracks = new List<Track>();
            try
            {
                tracks = TrackFinder.GetListOfTracksFromPhone(Context);
            }
            catch (Exception ex)
            {
                ((Activity)Context).RunOnUiThread(() => { Toast.MakeText(Context, ex.Message, ToastLength.Long).Show(); });
            }
            if (hasSd)
                tracks = tracks.Union(tracksFromSD).ToList();
            List<Paths> paths = new List<Paths>();
            foreach (var track in tracks)
            {
                paths.Add(new Paths() { Path = track.Path, Title = track.FullTitle });
            }
            return paths;
        }

        private void CacheSongs(List<Track> tracks)
        {
            JsonCacheParser.Write(tracks);
        }
    }
}
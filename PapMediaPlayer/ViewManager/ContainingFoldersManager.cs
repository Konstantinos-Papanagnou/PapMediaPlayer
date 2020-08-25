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

using PapMediaPlayer.Models;
using PapMediaPlayer.XmlParser;
using PapMediaPlayer.Adapters;
namespace PapMediaPlayer.ViewManager
{
    public class ContainingFoldersManager : IViewManager, Search.ISearchManager, ITrackRetriever
    {
        private readonly Context Context;
        private readonly string Folder;
        private readonly List<Track> Tracks;
        private bool FirstRun = true;
        private bool Search = false;

        public ContainingFoldersManager(Context context, string folder)
        {
            Context = context;
            this.Folder = folder;
            Tracks = GetFolderTracks(folder);
            
        }

        private List<Track> GetTracks()
        {
            List<Track> tracks = JsonCacheParser.Read();
            return tracks;
        }

        private List<Track> GetFolderTracks(string folder)
        {
            List<Track> tracksInFolder = new List<Track>();
            List<Track> tracks = GetTracks();
            foreach(Track t in tracks)
            {
                if (t.ContainingFolderName == folder)
                    tracksInFolder.Add(t);
            }
            return tracksInFolder;
        }

        public int TrackCount()
        {
            return Tracks.Count;
        }

        public string EnumeratePath()
        {
            return Folder + "\\";
        }

        public BaseAdapter GetAdapter()
        {
            return new SongAdapter(Context, Tracks);
        }

        public IViewManager OnClickHandler(int position)
        {
            if (FirstRun) { SynchronizeServiceTracks(); SaveViewMethod(); }
            FirstRun = false;
            if (Search) SynchronizeServiceTracks();
            ServiceStartHelper.StartHybridService(Context, Services.ServiceCallAction.PlayParam, position);
            return this;
        }

        public void SynchronizeServiceTracks()
        {
            List<Track> t;
            if (Search && matches.Count > 0)
                t = matches;
            else t = Tracks;
            ServiceStartHelper.StartHybridService(Context, Services.ServiceCallAction.PlaylistUpdate, t);
        }

        public IViewManager OnBackPressed()
        {
            return new FoldersManager(Context);
        }
        List<Track> matches;
        public BaseAdapter OnTextChanged(string Compare)
        {
            matches = new List<Track>();
            for (int i = 0; i < Tracks.Count; i++)
            {
                if (Tracks[i].FullTitle.ToLower().Contains(Compare.ToLower()) || Tracks[i].AuthorName.ToLower().Contains(Compare.ToLower()))
                {
                    matches.Add(Tracks[i]);
                }
            }
            return new SongAdapter(Context, matches);
        }

        List<Track> ITrackRetriever.GetTracks()
        {
            return Tracks;
        }

        private void SaveViewMethod()
        {
            ServiceSharedPref prefs = new ServiceSharedPref(Context);
            prefs.SetPullDataFrom(ServiceSharedPref.PULLDATAFROM_FOLDER);
            prefs.SetDataPath(Folder);
        }

        public void SetSearch(bool searchActive)
        {
            Search = searchActive;
            if (!Search)
                SynchronizeServiceTracks();
        }
    }
}
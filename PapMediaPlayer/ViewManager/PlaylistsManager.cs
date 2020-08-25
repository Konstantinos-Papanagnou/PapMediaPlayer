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
using PapMediaPlayer.Adapters;

namespace PapMediaPlayer.ViewManager
{
    public class PlaylistsManager : IViewManager, Search.ISearchManager, ITrackRetriever
    {
        private readonly List<Track> Tracks;
        private readonly string CurrentPlaylist;
        private readonly Context Context;
        private bool FirstRun = true;
        private bool Search = false;
        public PlaylistsManager(Context Context, string CurrentPlaylist)
        {
            this.Context = Context;
            this.CurrentPlaylist = CurrentPlaylist;
            Playlist_Manager.PlaylistManager Pmanager = new Playlist_Manager.PlaylistManager(CurrentPlaylist);
            Tracks = Pmanager.FetchTracksFromPlaylist();
        }

        public string EnumeratePath()
        {
            return CurrentPlaylist + "\\";
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

        public List<Track> GetTracks()
        {
            return Tracks;
        }

        private void SaveViewMethod()
        {
            ServiceSharedPref prefs = new ServiceSharedPref(Context);
            prefs.SetPullDataFrom(ServiceSharedPref.PULLDATAFROM_PLAYLIST);
            prefs.SetDataPath(CurrentPlaylist);
        }

        public void SetSearch(bool searchActive)
        {
            Search = searchActive;
            if (!Search) SynchronizeServiceTracks();
        }

        public int TrackCount()
        {
            return Tracks.Count;
        }
    }
}
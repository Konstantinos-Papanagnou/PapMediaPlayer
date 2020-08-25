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
using System.Threading.Tasks;
using PapMediaPlayer.XmlParser;
using Android.Graphics;

namespace PapMediaPlayer.ViewManager
{
    public class AllTracksManager : IViewManager, Search.ISearchManager, ITrackRetriever
    {
        private List<Track> AllTracks;
        private readonly Context Context;
        private bool FirstRun = true;
        private bool Search = false;

        public AllTracksManager(Context Context, List<Track> tracks)
        {
            this.Context = Context;
            AllTracks = tracks;
        }

        public AllTracksManager(Context Context) 
        {
            this.Context = Context;
        }

        /// <summary>
        /// Retuns custom adapter from either the tracks given or the tracks from cache
        /// </summary>
        /// <returns></returns>
        public BaseAdapter GetAdapter()
        {
            if(AllTracks == null)
                AllTracks = JsonCacheParser.Read();

            return new SongAdapter(Context, AllTracks);
        }

        public string EnumeratePath()
        {
            return string.Empty;
        }

        public IViewManager OnClickHandler(int position)
        {
            if (FirstRun) { SynchronizeServiceTracks(); SaveViewMethod(); }
            FirstRun = false;
            if (Search) { SynchronizeServiceTracks(); }
            ServiceStartHelper.StartHybridService(Context, Services.ServiceCallAction.PlayParam, position);
            return this;
        }

        public void SynchronizeServiceTracks()
        {
            List<Track> t;
            if (Search && matches.Count > 0)
                t = matches;
            else
                t = AllTracks;
            ServiceStartHelper.StartHybridService(Context, Services.ServiceCallAction.PlaylistUpdate, t);
        }

        public IViewManager OnBackPressed()
        {
            return new FoldersManager(Context, AllTracks);
        }
        List<Track> matches;
        public BaseAdapter OnTextChanged(string Compare)
        {
            matches = new List<Track>();
            for (int i = 0; i < AllTracks.Count; i++)
            {
                if (AllTracks[i].FullTitle.ToLower().Contains(Compare.ToLower()) || AllTracks[i].AuthorName.ToLower().Contains(Compare.ToLower()))
                {
                    matches.Add(AllTracks[i]);
                }
            }
            return new SongAdapter(Context, matches);
        }

        public List<Track> GetTracks()
        {
            return AllTracks;
        }

        private void SaveViewMethod()
        {
            ServiceSharedPref prefs = new ServiceSharedPref(Context);
            prefs.SetPullDataFrom(ServiceSharedPref.PULLDATAFROM_ALLTRACKS);
        }

        public void SetSearch(bool searchActive)
        {
            Search = searchActive;
            if (!Search) SynchronizeServiceTracks();
        }

        public int TrackCount()
        {
            return AllTracks.Count;
        }
    }
}
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

using PapMediaPlayer.Adapters;
using PapMediaPlayer.Models;
using PapMediaPlayer.XmlParser;

namespace PapMediaPlayer.ViewManager
{
    public class FoldersManager:IViewManager, Search.ISearchManager
    {
        private List<string> folders;
        private readonly Context Context;
        private bool Search = false;
        public FoldersManager(Context context, List<Track> tracks)
        {
            Context = context;
            ConfigureFolders(tracks);
        }

        public FoldersManager(Context context, ref GridView grid, List<Track> tracks)
        {
            Context = context;
            ConfigureFolders(tracks);
            grid.Adapter = GetAdapter();
        }

        public FoldersManager(Context context)
        {
            Context = context;
            ConfigureFolders(GetAllTracks());
        }

        private List<Track> GetAllTracks()
        {
            return JsonCacheParser.Read();
        }

        public int TrackCount()
        {
            return folders.Count;
        }

        private void ConfigureFolders(List<Track> tracks)
        {
            folders = new List<string>();
            foreach (Track t in tracks)
            {
                if (!ExistInContFolders(t.ContainingFolderName))
                    folders.Add(t.ContainingFolderName);
            }
        }

        private bool ExistInContFolders(string f)
        {
            foreach (string folder in folders)
                if (folder == f)
                    return true;
            return false;
        }

        public string EnumeratePath()
        {
            return string.Empty;
        }

        public BaseAdapter GetAdapter()
        {
            return new ContainingFolderAdapter(Context, folders);
        }

        public IViewManager OnClickHandler(int position)
        {
            string folder;
            if (!Search)
                folder = folders[position];
            else
                folder = matches[position];
            return new ContainingFoldersManager(Context, folder);            
        }

        public void SynchronizeServiceTracks()
        {
            return; // We dont have a job for this one
        }

        public IViewManager OnBackPressed()
        {
            return this; //Default behavior on back pressed for folder manager;
        }
        List<string> matches;
        public BaseAdapter OnTextChanged(string Compare)
        {
            matches = new List<string>();
            for (int i = 0; i < folders.Count; i++)
            {
                if (folders[i].ToLower().Contains(Compare.ToLower()))
                {
                    matches.Add(folders[i]);
                }
            }
            return new ContainingFolderAdapter(Context, matches);
        }

        public void SetSearch(bool searchActive)
        {
            Search = searchActive;
        }
    }
}
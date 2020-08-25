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
using PapMediaPlayer.StorageHelper;
using Android.Content.Res;

namespace PapMediaPlayer.Playlist_Manager
{
    public class PlaylistManager
    {
        public string PlaylistName { get { return _playlistName; } }
        public string PlaylistPath { get; private set; }

        private string _playlistName;
        private const string extention = ".json";
        public PlaylistManager(string PlaylistName)
        {
            _playlistName = PlaylistName;
            PlaylistPath = System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "PlaylistSettings", "Playlists");
            System.IO.Directory.CreateDirectory(PlaylistPath);
        }

        public PlaylistManager(string PlaylistName, List<Paths> tracks)
        {
            this._playlistName = PlaylistName;
            System.IO.Directory.CreateDirectory(PlaylistPath);
            PlaylistPath = System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "PlaylistSettings", "Playlists");
            BuildXmlFile(tracks);
        }

        public List<Track> FetchTracksFromPlaylist()
        {
            using(IXmlParsable parser = new JsonPlaylistParser(PlaylistPath, _playlistName + extention))
            {
               return Track_Finder.TrackFinder.ConvertPathsToTracks((List<Paths>)parser.FetchItems());
            }
        }
        
        public void AddTracksToPlaylist(List<Paths> tracks)
        {
            using (IXmlParsable parser = new JsonPlaylistParser(PlaylistPath, _playlistName + extention))
            {
                parser.AddItems(tracks);
            }
        }

        public void AddTrackToPlaylist(Paths track)
        {
            using (IXmlParsable parser = new JsonPlaylistParser(PlaylistPath, _playlistName + extention))
            {
                parser.AddItem(track);
            }
        }

        public void RemoveTrackFromPlaylist(List<Paths> track)
        {
            using (IXmlParsable parser = new JsonPlaylistParser(PlaylistPath, _playlistName + extention))
            {
                parser.RemoveItem(track);
            }
        }

        public void RemoveTracksFromPlaylist(List<Paths> tracks)
        {
            using (IXmlParsable parser = new JsonPlaylistParser(PlaylistPath, _playlistName + extention))
            {
                parser.RemoveItems(tracks);
            }
        }

        private void BuildXmlFile(List<Paths> tracks) 
        {
            IXmlParsable parser = new JsonPlaylistParser(PlaylistPath, PlaylistName + extention, tracks);
            parser.Dispose();
        }

        public static List<string> GetAllPlaylists()
        {
            using (IXmlParsable parser = new JsonPlaylistsHolderParser(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "PlaylistSettings", "Playlists.json")))
            {
                List<JsonPlaylistsHolderParser.PlaylistHolder> playlists = (List<JsonPlaylistsHolderParser.PlaylistHolder>)parser.FetchItems();
                List<string> names = new List<string>();
                foreach (JsonPlaylistsHolderParser.PlaylistHolder data in playlists)
                    names.Add(data.PlaylistName);
                return names;
            }
        }

        public static void CreatePlaylist(string PlaylistName)
        {
            JsonPlaylistsHolderParser.PlaylistHolder data = new JsonPlaylistsHolderParser.PlaylistHolder { PlaylistName = PlaylistName, PlaylistPath = System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "PlaylistSettings", "Playlists", PlaylistName + extention) };
            using (IXmlParsable parser = new JsonPlaylistsHolderParser(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "PlaylistSettings", "Playlists.json")))
            {
                parser.AddItem(data);
            }
        }

        public static void RemovePlaylist(string PlaylistName)
        {
            JsonPlaylistsHolderParser.PlaylistHolder data = new JsonPlaylistsHolderParser.PlaylistHolder { PlaylistName = PlaylistName, PlaylistPath = System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "PlaylistSettings", "Playlists", PlaylistName + extention) };
            using (IXmlParsable parser = new JsonPlaylistsHolderParser(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "PlaylistSettings", "Playlists.json")))
            {
                System.IO.File.Delete(data.PlaylistPath);
                parser.RemoveItem(data);
            }
        }
    }

}
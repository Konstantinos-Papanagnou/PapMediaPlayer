using Android.Content;
using Android.Graphics;
using Java.IO;
using PapMediaPlayer.Activities_and_fragments.Settings;
using PapMediaPlayer.Models;
using PapMediaPlayer.StorageHelper;
using System;
using System.Collections.Generic;
using System.Security;
using TagFile = TagLib.File;

namespace PapMediaPlayer.Track_Finder
{
    public abstract class TrackFinder
    {

        private struct Data
        {
            public string Mp3file { get; set; }
            public string ContainingFolder { get; set; }
        }

        private static List<Data> Getmp3FilesFromSD(string path)
        {
            List<Data> mp3files = new List<Data>();
            File[] fileList = new File(path).ListFiles();
            foreach (File file in fileList)
            {
                if (file.IsFile && file.Name.Contains(".mp3"))
                {
                    string[] containingFolderPath = file.AbsolutePath.Split('/');
                    mp3files.Add(new Data { Mp3file = file.AbsolutePath, ContainingFolder = containingFolderPath[^2]});
                }
                else if (file.IsDirectory)
                {
                    List<Data> mp3folderFiles = Getmp3FilesFromSD(file.AbsolutePath);
                    foreach(Data folderfile in mp3folderFiles)
                        mp3files.Add(folderfile);
                }
            }
            return mp3files;
          
        }
        [SecuritySafeCritical]
        public static List<Track> GetListOfTracksFromSD(Context act, string[] filters, char[] specialChars)
        {

            List<Track> tracks = new List<Track>();
            ExternalStorage storage = new ExternalStorage(act, filters, specialChars);
            List<Data> paths = Getmp3FilesFromSD(storage.ExternalPath);
            foreach(Data path in paths)
            {
                tracks.Add(CreateTrack(path));
            }
            return tracks; 
        }

        private static List<Data> Getmp3FilesFromPhone(string path)
        {
            List<Data> mp3files = new List<Data>();
            try
            {
                File[] fileList = new File(path).ListFiles();
                foreach (File file in fileList)
                {
                    if (file.IsFile && file.Name.Contains(".mp3"))
                    {
                        string[] containingFolderPath = file.Path.Split('/');
                        mp3files.Add(new Data { Mp3file = file.AbsolutePath, ContainingFolder = containingFolderPath[^2] });
                    }
                    else if (file.IsDirectory)
                    {
                        List<Data> mp3folderFiles = Getmp3FilesFromPhone(file.AbsolutePath);
                        foreach (Data folderfile in mp3folderFiles)
                        {
                            mp3files.Add(folderfile);
                        }
                    }
                }
                return mp3files;
            }
            catch (Java.Lang.NullPointerException)
            {
                return mp3files;
            }
            catch (System.NullReferenceException)
            {
                return mp3files;
            }
            catch (Java.Lang.Reflect.InvocationTargetException)
            {
                return mp3files;
            }
        }
        [SecuritySafeCritical]
        public static List<Track> GetListOfTracksFromPhone(Context act)
        {
            ISharedPreferences prefs = act.GetSharedPreferences(SearchConfiguration.Root, FileCreationMode.Private);
            if (!prefs.GetBoolean(SearchConfiguration.InternalKey, true))
                return new List<Track>();
            List<Track> tracks = new List<Track>();
            List<Data> MusicFolder = Getmp3FilesFromPhone(InternalStorageHelper.InternalMusicFolderPath);
            List<Data> DownloadFolder = Getmp3FilesFromPhone(InternalStorageHelper.InternalDownloadsFolderPath);

            foreach (Data path in MusicFolder)
            {
                tracks.Add(CreateTrack(path));
            } 
            foreach (Data path in DownloadFolder)
            {
                tracks.Add(CreateTrack(path));
            }
            return tracks;
        }
        [SecuritySafeCritical]
        public static List<Track> ConvertPathsToTracks(List<Paths> paths)
        {
            try
            {
                List<Track> tracks = new List<Track>();
                foreach (Paths path in paths)
                {

                    tracks.Add(CreateTrack(path));

                }
                return tracks;
            }
            catch
            {
                throw new TrackNotFoundException();
            }

        }
        [SecurityCritical]
        public static Bitmap GetImageOfTrack(string path)
        {
            using(TagFile mp3file = TagFile.Create(path))
            {
                if (mp3file.Tag.Pictures.Length >= 1)
                {
                    var bin = (byte[])(mp3file.Tag.Pictures[0].Data.Data);
                    try
                    {
                        using BitmapFactory.Options opt = new BitmapFactory.Options
                        {
                            OutHeight = 48,
                            OutWidth = 48,
                            InSampleSize = 2
                        };
                        return BitmapFactory.DecodeByteArray(bin, 0, bin.Length, opt);
                    }
                    catch (Java.Lang.OutOfMemoryError)
                    {
                        throw new OutOfMemoryException("Unable to load image due to extensive memory usage. Please restart the application.");
                    }
                }
            }
            return null;
        }
        [SecurityCritical]
        public static string GetArtistOfTrack(string path)
        {
            using TagFile mp3file = TagFile.Create(path);
            return mp3file.Tag.FirstPerformer ?? "Unknown Artist";
        }

        public static List<Paths> GetSongsFromFolder(string folder)
        {
            List<Paths> paths;
            List<Paths> selected = new List<Paths>();
            using (XmlParser.IXmlParsable parser = new XmlParser.JsonPlaylistParser(InternalStorageHelper.InternalPlaylistAllLocation, "All.json"))
            {
                paths = (List<Paths>)parser.FetchItems();
                foreach (var path in paths)
                    if (path.Path.Contains(folder))
                        selected.Add(path);
            }
            return selected;
        }

        public static List<Paths> ConvertToPaths(List<Track> tracks)
        {
            List<Paths> paths = new List<Paths>();
            foreach(var track in tracks)
            {
                paths.Add(new Paths() { Path = track.Path, Title = track.FullTitle });
            }
            return paths;
        }


        private static Track CreateTrack(Data path)
        {

            bool hasImage = false;
            using TagFile mp3file = TagFile.Create(path.Mp3file);
            Bitmap image = null;
            if (mp3file.Tag.Pictures.Length >= 1)
            {
                hasImage = true;
                var bin = (byte[])(mp3file.Tag.Pictures[0].Data.Data);
                try
                {
                    using BitmapFactory.Options opt = new BitmapFactory.Options
                    {
                        OutHeight = 48,
                        OutWidth = 48,
                        InSampleSize = 2
                    };
                    image = BitmapFactory.DecodeByteArray(bin, 0, bin.Length, opt);
                }
                catch (Java.Lang.OutOfMemoryError)
                {
                    hasImage = false;
                    throw new OutOfMemoryException("Unable to load image due to extensive memory usage. Please restart the application.");
                }
            }

            string name;
            string fulltitle;
            if (mp3file.Tag.Title == null || mp3file.Tag.Title == string.Empty)
            {
                string filename = mp3file.Name;
                string[] items = filename.Split('/');
                name = items[^1].Remove(items[^1].Length - 4, 4);
                fulltitle = name;
                if (name.Length > 25)
                {
                    name = name.Remove(22);
                    name += "...";
                }
            }
            else
            {
                name = mp3file.Tag.Title.Length < 25 ? mp3file.Tag.Title : mp3file.Tag.Title.Remove(22) + "...";
                fulltitle = mp3file.Tag.Title;
            }
            using Android.Media.MediaMetadataRetriever retriever = new Android.Media.MediaMetadataRetriever();
            retriever.SetDataSource(path.Mp3file);
            TimeSpan duration = TimeSpan.FromMilliseconds(double.Parse(retriever.ExtractMetadata(Android.Media.MetadataKey.Duration)));
            var t = new Track
            {
                Name = name.ToLower(),
                FullTitle = fulltitle,
                ContainsImage = hasImage,
                AuthorName = mp3file.Tag.FirstPerformer ?? "Unknown",
                ContainingFolderName = path.ContainingFolder,
                Album = mp3file.Tag.Album ?? "Unknown",
                Path = path.Mp3file,
                Copyrights = mp3file.Tag.Copyright ?? "Unknown",
                isCorrupted = mp3file.PossiblyCorrupt,
                CorruptionReason = mp3file.CorruptionReasons,
                DurationMins = duration.TotalMinutes,
                Length = duration.TotalHours < 1 ? duration.ToString(@"mm\:ss") : duration.ToString(@"hh\:mm\:ss"),
                Genre = mp3file.Tag.FirstGenre ?? "Unknown",
                Year = mp3file.Tag.Year,
                Image = image//hasImage? image: DefaultImage
            };

            return t;
        }

        private static Track CreateTrack(Paths path)
        {

            bool hasImage = false;
            using TagFile mp3file = TagFile.Create(path.Path);

            Bitmap image = null;
            if (mp3file.Tag.Pictures.Length >= 1)
            {
                hasImage = true;
                var bin = (byte[])(mp3file.Tag.Pictures[0].Data.Data);
                try
                {
                    using BitmapFactory.Options opt = new BitmapFactory.Options
                    {
                        OutHeight = 48,
                        OutWidth = 48,
                        InSampleSize = 2
                    };
                    image = BitmapFactory.DecodeByteArray(bin, 0, bin.Length, opt);
                }
                catch (Java.Lang.OutOfMemoryError)
                {
                    hasImage = false;
                    throw new OutOfMemoryException("Unable to load image due to extensive memory usage. Please restart the application.");
                }
            }

            string name;
            string fulltitle;
            if (mp3file.Tag.Title == null || mp3file.Tag.Title == string.Empty)
            {
                string filename = mp3file.Name;
                string[] items = filename.Split('/');
                name = items[^1].Remove(items[^1].Length - 4, 4);
                fulltitle = name;
                if (name.Length > 25)
                {
                    name = name.Remove(22);
                    name += "...";
                }
            }
            else
            {
                name = mp3file.Tag.Title.Length < 25 ? mp3file.Tag.Title : mp3file.Tag.Title.Remove(22) + "...";
                fulltitle = mp3file.Tag.Title;
            }

            string folderName = string.Empty;
            string[] array = mp3file.Name.Split('/');
            string folder = array[^2];
            using Android.Media.MediaMetadataRetriever retriever = new Android.Media.MediaMetadataRetriever();
            retriever.SetDataSource(path.Path);
            TimeSpan duration = TimeSpan.FromMilliseconds(double.Parse(retriever.ExtractMetadata(Android.Media.MetadataKey.Duration)));
            Track t = new Track
            {
                FullTitle = fulltitle,
                ContainsImage = hasImage,
                Name = name.ToLower(),
                ContainingFolderName = folder,
                AuthorName = mp3file.Tag.FirstPerformer ?? "Unknown",
                Album = mp3file.Tag.Album ?? "Unknown",
                Path = path.Path,
                Copyrights = mp3file.Tag.Copyright ?? "Unknown",
                isCorrupted = mp3file.PossiblyCorrupt,
                CorruptionReason = mp3file.CorruptionReasons,
                DurationMins = duration.TotalMinutes,
                Length = duration.TotalHours < 1 ? duration.ToString(@"mm\:ss") : duration.ToString(@"hh\:mm\:ss"),
                Genre = mp3file.Tag.FirstGenre ?? "Unknown",
                Year = mp3file.Tag.Year,
                Image = image//hasImage ? image : DefaultImage
            };

            return t;
        }


        [Serializable]
        public class TrackNotFoundException : Exception
        {
            public TrackNotFoundException() { }
            public TrackNotFoundException(string message) : base(message) { }
            public TrackNotFoundException(string message, Exception inner) : base(message, inner) { }
            protected TrackNotFoundException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }

}
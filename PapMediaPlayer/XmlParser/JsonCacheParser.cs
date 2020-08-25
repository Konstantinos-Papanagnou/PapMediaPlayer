using System.Collections.Generic;
using PapMediaPlayer.Models;
using System.IO;
using PapMediaPlayer.StorageHelper;
using Newtonsoft.Json;
using TagFile = TagLib.File;
using Android.Graphics;

namespace PapMediaPlayer.XmlParser
{        
    public sealed class JsonCacheParser
    {
        static readonly string Path = System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.json");
        public static void Write(List<Track> tracks)
        {
            foreach (var track in tracks)
            {
                track.Image = null;
            }
            File.WriteAllText(Path, JsonConvert.SerializeObject(tracks));
        }

        public static List<Track> Read()
        {
            if (!File.Exists(Path))
                throw new System.Exception("Cache Does Not Exist");
            List<Track> tracks = JsonConvert.DeserializeObject<List<Track>>(File.ReadAllText(Path));

            foreach (var track in tracks)
            {
                if (track.ContainsImage)
                {
                    var mp3file = TagFile.Create(track.Path);
                    Bitmap image = null;
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
                        throw new System.OutOfMemoryException("Unable to load image due to extensive memory usage. Please restart the application.");
                    }
                    track.Image = image;
                }
            }
            return tracks;
        }
    }
}
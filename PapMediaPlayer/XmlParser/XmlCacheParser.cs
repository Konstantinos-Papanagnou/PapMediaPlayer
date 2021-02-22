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
using System.Xml.Linq;
using PapMediaPlayer.Models;
using Android.Graphics;
using System.IO;
using PapMediaPlayer.StorageHelper;
using TagFile = TagLib.File;
using System.Threading.Tasks;
using System.Security;

namespace PapMediaPlayer.XmlParser
{        
    public sealed class XmlCacheParser: IDisposable
    {
        XDocument doc;

        public XmlCacheParser()
        {
            if (File.Exists(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.xml")))
                doc = XDocument.Load(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.xml"));
        }
        public XmlCacheParser(List<Track> tracks)
        {
            Directory.CreateDirectory(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache"));
            if (File.Exists(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.xml")))
                File.Delete(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.xml"));
            doc = new XDocument(new XElement("Root"));
            foreach(Track t in tracks)
            {
                string filePath;
                if (t.ContainsImage)
                {
                    filePath = t.Path;
                }
                else filePath = "Default";
                

                XElement element = new XElement("Track", filePath);

                element.Add(new XAttribute("Title", t.FullTitle));
                element.Add(new XAttribute("Name", t.Name));
                element.Add(new XAttribute("Year", t.Year));
                element.Add(new XAttribute("Artist", t.AuthorName));
                element.Add(new XAttribute("Album", t.Album));
                element.Add(new XAttribute("Path", t.Path));
                element.Add(new XAttribute("ContainingFolder", t.ContainingFolderName));
                element.Add(new XAttribute("Copyrights", t.Copyrights));
                element.Add(new XAttribute("Length", t.Length));
                element.Add(new XAttribute("DurationMins", t.DurationMins));
                element.Add(new XAttribute("IsCorrupted", t.isCorrupted));
                element.Add(new XAttribute("CorruptionReason", t.CorruptionReason == null? "": t.CorruptionReason.ToString()));
                element.Add(new XAttribute("Genre", t.Genre));
                doc.Root.Add(element);
            }
            doc.Save(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.xml"));
        }

        public static XDocument GetDoc()
        {
            return XDocument.Load(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.xml"));
        }

        public static List<Track> GetCachedSongs(XDocument doc)
        {
            List<Track> tracks = new List<Track>();

            foreach (XElement element in doc.Root.Elements())
            {
                tracks.Add(new Models.Track
                {
                    FullTitle = element.Attribute("Title").Value,
                    Name = element.Attribute("Name").Value,
                    AuthorName = element.Attribute("Artist").Value,
                    Album = element.Attribute("Album").Value,
                    Path = element.Attribute("Path").Value,
                    ContainingFolderName = element.Attribute("ContainingFolder").Value,
                    Copyrights = element.Attribute("Copyrights").Value,
                    Length = element.Attribute("Length").Value,
                    DurationMins = CastToDouble(element.Attribute("DurationMins").Value),
                    isCorrupted = Convert.ToBoolean(element.Attribute("IsCorrupted").Value),
                    Genre = element.Attribute("Genre").Value,
                    Year = Convert.ToUInt32(element.Attribute("Year").Value)
                });
            }
            return tracks;
        }
        [SecuritySafeCritical]
        public List<Track> GetCachedSongs(Context c)
        {
            List<Track> tracks = new List<Track>();

            foreach(XElement element in doc.Root.Elements())
            {
                bool hasImage = false;
                Bitmap image = null;
                if (element.Value != "Default")
                {
                    hasImage = true;
                    using (TagFile mp3file = TagFile.Create(element.Value))
                    {
                        var bin = (byte[])(mp3file.Tag.Pictures[0].Data.Data);
                        try
                        {
                            using (BitmapFactory.Options opt = new BitmapFactory.Options())
                            {
                                opt.OutHeight = 48;
                                opt.OutWidth = 48;
                                opt.InSampleSize = 2;
                                image = BitmapFactory.DecodeByteArray(bin, 0, bin.Length, opt);
                            }
                        }
                        catch (Java.Lang.OutOfMemoryError)
                        {
                            hasImage = false;
                            throw new OutOfMemoryException("Unable to load image due to extensive memory usage. Please restart the application");
                        }
                    }
            }
                tracks.Add(new Models.Track
                {
                    FullTitle = element.Attribute("Title").Value,
                    Name = element.Attribute("Name").Value,
                    AuthorName = element.Attribute("Artist").Value,
                    Album = element.Attribute("Album").Value,
                    Path = element.Attribute("Path").Value,
                    ContainingFolderName = element.Attribute("ContainingFolder").Value,
                    Copyrights = element.Attribute("Copyrights").Value,
                    Length = element.Attribute("Length").Value,
                    DurationMins = CastToDouble(element.Attribute("DurationMins").Value),
                    isCorrupted = Convert.ToBoolean(element.Attribute("IsCorrupted").Value),
                    Genre = element.Attribute("Genre").Value,
                    Year = Convert.ToUInt32(element.Attribute("Year").Value),
                    Image = image,
                    ContainsImage = hasImage
                });
            }
            return tracks;
        }
        public int GetMaxCountOfElements()
        {
            return doc.Root.Elements().Count();
        }
        [SecuritySafeCritical]
        public Track GetCachedSong(Context c, int position)
        {
           
                bool hasImage = false;
                Bitmap image = null;

            XElement element = doc.Root.Elements().ElementAt(position);
                if (element.Value != "Default")
                {
                    hasImage = true;
                using (TagFile mp3file = TagFile.Create(element.Value))
                {
                    var bin = (byte[])(mp3file.Tag.Pictures[0].Data.Data);
                    try
                    {
                        using (BitmapFactory.Options opt = new BitmapFactory.Options())
                        {
                            opt.OutHeight = 48;
                            opt.OutWidth = 48;
                            opt.InSampleSize = 2;
                            image = BitmapFactory.DecodeByteArray(bin, 0, bin.Length, opt);
                        }
                    }
                    catch (Java.Lang.OutOfMemoryError)
                    {
                        hasImage = false;
                        throw new OutOfMemoryException("Unable to load image due to extensive memory usage. Please restart the application.");
                    }
                }
            }

                return new Track
                {
                    FullTitle = element.Attribute("Title").Value,
                    Name = element.Attribute("Name").Value,
                    AuthorName = element.Attribute("Artist").Value,
                    Album = element.Attribute("Album").Value,
                    Path = element.Attribute("Path").Value,
                    ContainingFolderName = element.Attribute("ContainingFolder").Value,
                    Copyrights = element.Attribute("Copyrights").Value,
                    Length = element.Attribute("Length").Value,
                    DurationMins = CastToDouble(element.Attribute("DurationMins").Value),
                    isCorrupted = Convert.ToBoolean(element.Attribute("IsCorrupted").Value),
                    Genre = element.Attribute("Genre").Value,
                    Year = Convert.ToUInt32(element.Attribute("Year").Value),
                    Image = image,
                    ContainsImage = hasImage
                };
        }

        private static double CastToDouble(string stringToSplit)
        {
            string[] array = stringToSplit.Split('.');
            string Final;
            if (array.Length == 2)
            {
                Final = array[0] + "." + array[1];
                string ToCompare = array[0] + array[1];
                if (double.Parse(Final) == double.Parse(ToCompare))
                {
                    Final = array[0] + "," + array[1];
                    return double.Parse(Final);
                }
                else
                {
                    return double.Parse(Final);
                }
            }
            else
            {
                Final = array[0];
                return double.Parse(Final);
            }
        }

        //Release associated resources and delete cache
        public void Dispose()
        {
            doc = null;
            File.Delete(System.IO.Path.Combine(InternalStorageHelper.InternalXmlFileLocation, "Cache", "Cache.xml"));
            GC.SuppressFinalize(this);
        }

        //Release associated resources.
        public void Release()
        {
            GC.SuppressFinalize(doc);
            doc = null;
        }
    }
}
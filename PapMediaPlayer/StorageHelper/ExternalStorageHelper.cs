using Java.IO;
using Android.Content.Res;
using System;
using Android.App;
using Android.Content;
using PapMediaPlayer.Activities_and_fragments.Settings;

namespace PapMediaPlayer.StorageHelper
{
    /// <summary>
    /// Abstract class. Only used to return the External path of the sd card.
    /// </summary>
    /// <exception cref="NotMountedException">Hits a NotMountedException when it cannot find sdcard. SD card path varies from device to device but should be correct.</exception>
    public class ExternalStorage
    {
        string[] filters;
        char[] specialChars;
        Context act;
        public ExternalStorage(Context act, string[] filters, char[] specialChars)
        {
            this.filters = filters;
            this.specialChars = specialChars;
            this.act = act;
        }

        public string ExternalPath { 
			get
            {
                ISharedPreferences prefs = act.GetSharedPreferences(SearchConfiguration.Root, FileCreationMode.Private);
                if (prefs.GetBoolean(SearchConfiguration.ExternalKey, true))
                    return CalculateExternalPath();
                else
                {
                    string dir = prefs.GetString(SearchConfiguration.DirectorySelectedKey, string.Empty);
                    if (dir == string.Empty)
                        throw new NotMountedException("SD card not Mounted or have restricted access! If you have an SD card mounted go to advanced settings and configure the Search Options and filters and keys for more advanced users");
                    return dir;
                }
            }
            private set
            {
                ExternalPath = value;
            }
        }

        private string CalculateExternalPath()
        {
            File[] fileList = new File("/storage/").ListFiles();
            string path = string.Empty;
            foreach (File file in fileList)
            {
                if (file.IsDirectory)
                {
                    bool found = false;
                    foreach(string filter in filters)
                    {
                        if (filter.StartsWith("$"))
                        {
                            if (filter.ContainsAny(specialChars))
                            {
                                string noCode = filter.Remove(0, 1);
                                string left = noCode.Replace("#", string.Empty);
                                if (file.Name.Length == noCode.Length & file.Name.Contains(left))
                                    found = true;
                            }

                        }
                        else if (file.Name.Contains(filter))
                            found = true;
                    }
                    if (found)
                    {
                        path = file.AbsolutePath;
                        break;
                    }
                }
            }
            File f = new File(path);
            //if (!f.CanRead() || !f.CanWrite())
                //throw new NotMountedException("SD card not Mounted or have restricted access! If you have an SD card mounted go to advanced settings and configure the Search Options and filters and keys for more advanced users");
            if (path == string.Empty)
                throw new NotMountedException("SD card not Mounted or have restricted access! If you have an SD card mounted go to advanced settings and configure the Search Options and filters and keys for more advanced users");
            return path;
        }

    }

    public static class StringExtentions
    {
        public static bool ContainsAny(this string text, char[] chars)
        {
            foreach (char c in chars)
            {
                if (text.Contains(c.ToString()))
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    internal class NotMountedException: System.Exception
    {
        public NotMountedException() : base()
        {

        }

        public NotMountedException(string message):base(message)
        {

        }
    }
}
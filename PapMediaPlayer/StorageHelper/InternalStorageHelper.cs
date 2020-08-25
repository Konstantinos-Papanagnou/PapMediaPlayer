using Android.OS;

namespace PapMediaPlayer.StorageHelper
{
    public abstract class InternalStorageHelper
    {
        public static string InternalMusicFolderPath {
            get
            {
                return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryMusic).AbsolutePath;
               
            }
        }

        public static string InternalDownloadsFolderPath
        {
            get { return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath; }
        }

        public static string InternalXmlFileLocation
        {
            get { return System.IO.Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, "PapMediaPlayer"); }
        }

        public static string InternalPlaylistAllLocation
        {
            get { return System.IO.Path.Combine(InternalXmlFileLocation, "PlaylistSettings", "Playlists", "All"); }
        }

        public static string InternalCacheLocation
        {
            get { return System.IO.Path.Combine(InternalXmlFileLocation, "Cache", "Cache.xml"); }
        }
    }
}
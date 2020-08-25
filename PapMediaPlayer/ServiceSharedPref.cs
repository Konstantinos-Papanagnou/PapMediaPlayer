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
using PapMediaPlayer.Activities_and_fragments.Settings;

namespace PapMediaPlayer
{
    public class ServiceSharedPref
    {
        public const string SERVICE_DATA_PULL_CONFIG = "SERVICE_DATA_PULL_CONFIG";
        public const string PULLDATAFROM_KEY = "PULLDATAFROM_KEY";
        public const string PULLDATAFROM_ALLTRACKS = "PULLDATAFROM_ALLTRACKS";
        public const string PULLDATAFROM_FOLDER = "PULLDATAFROM_FOLDER";
        public const string PULLDATAFROM_PLAYLIST = "PULLDATAFROM_PLAYLIST";
        public const string PULLERPATH_KEY = "PULLERPATH_KEY";
        public const string PULLLASTSONGINDEX_KEY = "PULLLASTSONG_KEY";

        private readonly Context context;
        public ServiceSharedPref(Context context)
        {
            this.context = context;
        }

        public string PullDataFrom()
        {
            using (ISharedPreferences pref = context.GetSharedPreferences(SERVICE_DATA_PULL_CONFIG, FileCreationMode.Private))
            {
                return pref.GetString(PULLDATAFROM_KEY, PULLDATAFROM_ALLTRACKS);
            }
        }

        public string PullDataPath()
        {
            using(ISharedPreferences pref = context.GetSharedPreferences(SERVICE_DATA_PULL_CONFIG, FileCreationMode.Private))
            {
                return pref.GetString(PULLERPATH_KEY, string.Empty);
            }
        }

        public void SetPullDataFrom(string data)
        {
            using (ISharedPreferencesEditor pref = context.GetSharedPreferences(SERVICE_DATA_PULL_CONFIG, FileCreationMode.Private).Edit())
            {
                pref.PutString(PULLDATAFROM_KEY, data);
                pref.Apply();
                pref.Commit();
            }
        }

        public void SetDataPath(string data)
        {
            using (ISharedPreferencesEditor pref = context.GetSharedPreferences(SERVICE_DATA_PULL_CONFIG, FileCreationMode.Private).Edit())
            {
                pref.PutString(PULLERPATH_KEY, data);
                pref.Apply();
                pref.Commit();
            }
        }

        public int PullLastSong()
        {
            using(ISharedPreferences pref = context.GetSharedPreferences(SERVICE_DATA_PULL_CONFIG, FileCreationMode.Private))
            {
                return pref.GetInt(PULLLASTSONGINDEX_KEY, 0);
            }
        }

        public void SaveLastSong(int index)
        {
            using(ISharedPreferencesEditor pref = context.GetSharedPreferences(SERVICE_DATA_PULL_CONFIG, FileCreationMode.Private).Edit())
            {
                pref.PutInt(PULLLASTSONGINDEX_KEY, index);
                pref.Apply();
                pref.Commit();
            }
        }
    }
}
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

namespace PapMediaPlayer
{
    public class RepeatSharedPrefs
    {
        public const string RepeatConf = "RepConf";
        public const string RepeatAttr = "RepAttr";
        private readonly Context @Context;
        public RepeatSharedPrefs(Context context)
        {
            @Context = context;
        }

        public RepeatMethod GetMethod()
        {
            using (ISharedPreferences prefs = @Context.GetSharedPreferences(RepeatConf, FileCreationMode.Private))
            {
                return (RepeatMethod)prefs.GetInt(RepeatAttr, 0);
            }
        }

        public void SetMethod(RepeatMethod repeat)
        {
            using (ISharedPreferencesEditor editor = @Context.GetSharedPreferences(RepeatConf, FileCreationMode.Private).Edit())
            {
                editor.PutInt(RepeatAttr, (int)repeat);
                editor.Apply();
                editor.Commit();
            }
        }
    }
}
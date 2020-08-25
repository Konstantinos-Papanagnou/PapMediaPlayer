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

namespace PapMediaPlayer
{
    /// <summary>
    /// Use this class to start the service with an action across activities
    /// </summary>
    public abstract class ServiceStartHelper
    {
        /// <summary>
        /// Use this method to fire up MediaPlayerService with a specific action
        /// </summary>
        /// <param name="context">Can be an Activity or Service</param>
        /// <param name="action">Action enumeration found on MediaPlayerService</param>
        /// <param name="param">Data associated with the action</param>
        public static void StartHybridService(Context context, Services.ServiceCallAction action, int param)
        {
            Intent intent = new Intent(context, typeof(PapMediaPlayer.Services.MediaService));
            intent.PutExtra(Services.MediaService.ACTION_KEY, (int)action);
            intent.PutExtra(Services.MediaService.PARAMS, param);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else context.StartService(intent);
        }

        /// <summary>
        /// Use this method to fire up MediaPlayerService with a specific action
        /// </summary>
        /// <param name="context">Can be an Activity or Service</param>
        /// <param name="action">Action enumeration found on MediaPlayerService</param>
        /// <param name="param">Data associated with the action</param>
        public static void StartHybridService(Context context, Services.ServiceCallAction action, string param = "")
        {
            Intent intent = new Intent(context, typeof(PapMediaPlayer.Services.MediaService));
            intent.PutExtra(Services.MediaService.ACTION_KEY, (int)action);
            intent.PutExtra(Services.MediaService.PARAMS, param);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else context.StartService(intent);
        }

        /// <summary>
        /// Use this method to fire up MediaPlayerService with a specific action
        /// </summary>
        /// <param name="context">Can be an Activity or Service</param>
        /// <param name="action">Action enumeration found on MediaPlayerService</param>
        /// <param name="param">Data associated with the action</param>
        public static void StartHybridService(Context context, Services.ServiceCallAction action, List<Track> param)
        {
            List<Paths> paths = new List<Paths>();
            foreach (var item in param)
            {
                paths.Add(new Paths() { Path = item.Path, Title = item.FullTitle });
            }
            Intent intent = new Intent(context, typeof(PapMediaPlayer.Services.MediaService));
            intent.PutExtra(Services.MediaService.ACTION_KEY, (int)action);
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(paths);
            intent.PutExtra(Services.MediaService.PARAMS, output);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else context.StartService(intent);
        }
    }
}
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

namespace PapMediaPlayer.Receivers
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "Action.Sort" })]
    public class SortMethodReceiver : BroadcastReceiver
    {
        public const string METHOD = "Method";
        public event OnReceived Received;
        public delegate void OnReceived(object sender, int e);
        public override void OnReceive(Context context, Intent intent)
        {
            Receive(intent.GetIntExtra(METHOD, 0));
        }

        public virtual void Receive(int e)
        {
            Received?.Invoke(this, e);
        }
    }
}
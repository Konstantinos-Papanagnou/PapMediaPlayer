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
    [IntentFilter(new[] { "Action.SelectChanged" })]
    public class SelectChangeReceiver : BroadcastReceiver
    {
        public const string EXTRA = "Count";
        public event OnReceived Received;
        public delegate void OnReceived(object sender, int count);
        public override void OnReceive(Context context, Intent intent)
        {
            Receive(intent.GetIntExtra(EXTRA, 0));
        }

        public virtual void Receive(int count)
        {
            Received?.Invoke(this, count);
        }
    }
}
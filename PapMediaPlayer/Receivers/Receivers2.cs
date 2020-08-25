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
    public class ReceivedData : EventArgs
    {
        public string SerializedData;
        public Actions Action;
        public enum Actions
        {
            Pause,
            Next,
            Previous,
            Cancel,
            Details,
            LoopBack,
            SeekTo
        }
        public ReceivedData(Actions Action)
        {
            this.Action = Action;
        }

        public ReceivedData(Actions Action, string SerializedData)
        {
            this.Action = Action;
            this.SerializedData = SerializedData;
        }
    }
    [BroadcastReceiver]
    [IntentFilter(new[] { "Pause", "Next", "Previous", "Cancel", "Details", "LoopBack" })]
    public class Receiver: BroadcastReceiver
    {
        public event OnReceived Received;
        public delegate void OnReceived(object sender, ReceivedData e);
        public override void OnReceive(Context context, Intent intent)
        {
            Receive(new ReceivedData((ReceivedData.Actions)Enum.Parse(typeof(ReceivedData.Actions),intent.Action)));
        }

        public virtual void Receive(ReceivedData e)
        {
            Received?.Invoke(this, e);
        }
    }

    [BroadcastReceiver]
    [IntentFilter(new[] { "Activity.Pause", "Activity.Next", "Activity.LoopBack", "Activity.SeekTo"})]
    public class ServiceActivityReceiver : BroadcastReceiver
    {
        public event OnReceived Received;
        public delegate void OnReceived(object sender, ReceivedData e);
        public override void OnReceive(Context context, Intent intent)
        {                                                                                           // Removing the Activity. part
            Receive(new ReceivedData((ReceivedData.Actions)Enum.Parse(typeof(ReceivedData.Actions), intent.Action.Split('.')[1]), intent.GetStringExtra("PARAMS")));
        }

        public virtual void Receive(ReceivedData e)
        {
            Received?.Invoke(this, e);
        }
    }
}
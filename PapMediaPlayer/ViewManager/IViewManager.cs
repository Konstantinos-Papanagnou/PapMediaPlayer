using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PapMediaPlayer.ViewManager
{
    public interface IViewManager
    {
        BaseAdapter GetAdapter();

        string EnumeratePath();

        IViewManager OnClickHandler(int position);

        void SynchronizeServiceTracks();

        IViewManager OnBackPressed();

        int TrackCount();
    }
}
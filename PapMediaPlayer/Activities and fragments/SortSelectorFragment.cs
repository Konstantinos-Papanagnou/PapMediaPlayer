using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using PapMediaPlayer.Receivers;
namespace PapMediaPlayer
{
    public class SortByFragment : DialogFragment
    {
        private ListView lv;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            Dialog.SetTitle("View:");

            View view = inflater.Inflate(Resource.Layout.SortSelectorFragment, container, false);
            lv = view.FindViewById<ListView>(Resource.Id.SortSelectorFragment_ListView);
            return view;
        }

        private void Lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = new Intent("Action.Sort");
            intent.PutExtra(SortMethodReceiver.METHOD, e.Position);
            Activity.SendBroadcast(intent);
            this.Dismiss();
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            string[] SortingMethods = { "All Tracks", "Containing Folders", "Playlist" };
            lv.Adapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItemSingleChoice, SortingMethods);
            lv.ItemClick += Lv_ItemClick;
        }
    }
}
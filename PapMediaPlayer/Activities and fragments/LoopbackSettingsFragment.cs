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
using PapMediaPlayer.Adapters;
using PapMediaPlayer.Manager;

namespace PapMediaPlayer
{
    public class LoopbackSettingsFragment : DialogFragment
    {
        private ListView lv;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            Dialog.SetTitle("Repeat Options:");

            View view = inflater.Inflate(Resource.Layout.LoopbackSettingsFragment, container, false);
            lv = view.FindViewById<ListView>(Resource.Id.LoopbackSettingsFragment_ListView);
            return view;
        }

        private void Lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = new Intent(Activity, typeof(Services.MediaService));
            intent.PutExtra(Services.MediaService.ACTION_KEY, (int)Services.ServiceCallAction.LoopBackChangeParam);
            switch (e.Position)
            {
                case 0:
                    intent.PutExtra(Services.MediaService.PARAMS, RepeatMethod.RepeatAllInOrder.ToString());
                   // MainActivity.Manager.Repeat = RepeatMethod.RepeatAllInOrder;
                    break;
                case 1:
                    intent.PutExtra(Services.MediaService.PARAMS, RepeatMethod.RepeatOnce.ToString());
                   // MainActivity.Manager.Repeat = RepeatMethod.RepeatOnce;
                    break;
                case 2:
                    intent.PutExtra(Services.MediaService.PARAMS, RepeatMethod.RepeatOne.ToString());
                  //  MainActivity.Manager.Repeat = RepeatMethod.RepeatOne;
                    break;
                case 3:
                    intent.PutExtra(Services.MediaService.PARAMS, RepeatMethod.NoRepeat.ToString());
                   // MainActivity.Manager.Repeat = RepeatMethod.NoRepeat;
                    break;
                case 4:
                    intent.PutExtra(Services.MediaService.PARAMS, RepeatMethod.RandomReplay.ToString());
                  //  MainActivity.Manager.Repeat = RepeatMethod.Random;
                    break;
            }
            Activity.StartService(intent);
            this.Dismiss();
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        { 
            base.OnActivityCreated(savedInstanceState);
            List<RepeatSelectionAdapter.Input> input = new List<RepeatSelectionAdapter.Input>();
            input.Add(new RepeatSelectionAdapter.Input() { Text = "Repeat All In Order", Icon = Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.LoopInOrder) });
            input.Add(new RepeatSelectionAdapter.Input() { Text = "Repeat Once", Icon = Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.RepeatOnce) });
            input.Add(new RepeatSelectionAdapter.Input() { Text = "Repeat One", Icon = Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.RepeatOne) });
            input.Add(new RepeatSelectionAdapter.Input() { Text = "No Repeat", Icon = Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.NoLoopback) });
            input.Add(new RepeatSelectionAdapter.Input() { Text = "Random Repeat", Icon = Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.Random) });


            // string[] SortingMethods = { "Repeat All In Order", "Repeat Once", "Repeat One", "No Repeat", "Random" };
            lv.Adapter = new RepeatSelectionAdapter(Activity, input);
            lv.ItemClick += Lv_ItemClick;
        }
    }
}
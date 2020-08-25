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
    [Activity(Label = "Settings", Icon = "@drawable/Logo")]
    public class AdvancedActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.Advanced);

            string[] options = { "Startup", "Hidden Songs", "Notification Settings", "Delete Songs"};
            string[] options2 = { "Search Configuration", "Filters and Keys" };
            ListView Alv = FindViewById<ListView>(Resource.Id.Advanced_SelectionList);
            ListView Glv = FindViewById<ListView>(Resource.Id.General_SelectionList);
            Alv.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, options2);
            Glv.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, options);
            Glv.ItemClick += Lv_ItemClick;
            Alv.ItemClick += Alv_ItemClick;
        }

        private void Alv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = null;
            switch (e.Position)
            {
                case 0:
                    // TODO Search Configuration
                    intent = new Intent(this, typeof(SearchConfiguration));
                    break;
                case 1:
                    // TODO Filters and Keys
                    intent = new Intent(this, typeof(Filters_and_Keys));
                    break;
            }
            if(intent != null)
                StartActivity(intent);
        }

        private void Lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = null;
            switch (e.Position)
            {
                case 0:
                    // TODO Startup
                    intent = new Intent(this, typeof(Startup));
                    break;
                case 1:
                    // TODO Hidden Songs
                    intent = new Intent(this, typeof(Hidden_Songs));
                    break;
                case 2:
                    // TODO Notification Settings
                    intent = new Intent(this, typeof(Notification_Settings));
                    break;
                case 3:
                    // TODO Delete Songs
                    intent = new Intent(this, typeof(Delete_Songs));
                    break;
            }
            if(intent != null)
                StartActivity(intent);
        }
    }
}
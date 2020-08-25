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

namespace PapMediaPlayer.Activities_and_fragments.Settings
{
    [Activity(Label = "Notification Settings", Icon = "@drawable/Logo")]
    public class Notification_Settings : Activity
    {
        public static readonly string Root = "NotificationSettings";
        public static readonly string ErrorNotKey = "ErrorNotificationsActivated";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            CreateUI();
        }

        void CreateUI()
        {
            LinearLayout ll = new LinearLayout(this);
            ll.Orientation = Orientation.Vertical;
            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent))
                ll.LayoutParameters = lp;

            using (TextView tv = new TextView(this))
            {
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent))
                    tv.LayoutParameters = lp;
                tv.Text = "This will only affect the error notifications such as SD card not mounted notifications. Other notification such as warnings will not be affected";
                tv.TextSize = 15f;
                tv.SetPadding(10, 10, 0, 10);
                ll.AddView(tv);
            }
            using (Switch sw2 = new Switch(this))
            {
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                    sw2.LayoutParameters = lp;
                sw2.Text = "Error Notifications";
                sw2.SetPadding(20, 5, 0, 0);
                sw2.TextSize = 20f;
                ISharedPreferences data = GetSharedPreferences(Root, FileCreationMode.Private);
                sw2.Checked = data.GetBoolean(ErrorNotKey, true);
                sw2.CheckedChange += Sw2_CheckedChange;
                ll.AddView(sw2);
            }


            SetContentView(ll);
        }

        private void Sw2_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ISharedPreferencesEditor editor = GetSharedPreferences(Root, FileCreationMode.Append).Edit();
            editor.PutBoolean(ErrorNotKey, e.IsChecked);
            editor.Commit();
        }
    }
}
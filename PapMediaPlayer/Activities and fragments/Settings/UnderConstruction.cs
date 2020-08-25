using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PapMediaPlayer.Activities_and_fragments.Settings
{
    public abstract class UnderConstruction
    {
        public static LinearLayout GetLayout(Context Context)
        {
            LinearLayout ll = new LinearLayout(Context);
            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent))
                ll.LayoutParameters = lp;

            using (TextView tv = new TextView(Context))
            {
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent))
                    tv.LayoutParameters = lp;
                tv.Text = "This Screen is Under Construction ...";
                tv.SetPadding(0, 100, 0, 100);
                tv.TextSize = 20f;
                tv.Gravity = GravityFlags.CenterHorizontal;
                ll.AddView(tv);
            }

            using (ImageView iv = new ImageView(Context))
            {
                //using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                //     iv.LayoutParameters = lp;
                iv.LayoutParameters = new Android.Views.ViewGroup.LayoutParams(200, 200);
                iv.SetMaxHeight(200);
                iv.SetMaxWidth(200);
                iv.SetImageBitmap(BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.UnderConstruction));
                //iv.SetForegroundGravity(GravityFlags.CenterHorizontal);
                ll.AddView(iv);
            }
            return ll;
        }
    }
}
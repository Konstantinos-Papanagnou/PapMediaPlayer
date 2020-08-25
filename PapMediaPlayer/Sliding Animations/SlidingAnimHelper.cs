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

namespace PapMediaPlayer.Sliding_Animations
{
    public abstract class SlidingAnimHelper
    {

        /// <summary>
        /// Animates a view so that it slides in from the bottom of it's container.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="view"></param>
        public static void slideIn(Context context, View view)
        {   
            runSimpleAnimation(context, view, Resource.Animation.slidingInAnim);
        }
        /// <summary>
        /// Animates a view so that it slides out to the bottom of it's container.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="view"></param>
        public static void slideOut(Context context, View view)
        {
            runSimpleAnimation(context, view, Resource.Animation.SlidingOutAnim);
        }

        private static void runSimpleAnimation(Context context, View view, int animationId)
        {
            view.StartAnimation(Android.Views.Animations.AnimationUtils.LoadAnimation(
                    context, animationId
            ));
        }
    }
}
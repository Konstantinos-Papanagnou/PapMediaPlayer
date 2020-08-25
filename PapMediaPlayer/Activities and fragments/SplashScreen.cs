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
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using System.Threading;

namespace PapMediaPlayer.Activities_and_fragments
{
    [Activity(Label = "Pap Media Player", Icon = "@drawable/Logo", MainLauncher = false, NoHistory = true, Theme = "@style/splashscreen2")]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 1500, // 3 sec.
                AutoReset = false // Do not reset the timer after it's elapsed
            };
            
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                StartActivity(typeof(MainActivity));
            };
            timer.Start();
            
        }
    }

    [Activity(Label = "Pap Media Player", MainLauncher = false, NoHistory = true, Theme = "@style/splashscreen")]
    public class SplashScreen2 : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 1500, // 3 sec.
                AutoReset = false // Do not reset the timer after it's elapsed
            };
            
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                StartActivity(typeof(MainActivity));
            };
            timer.Start();
            
        }
    }

    [Activity(Label = "Pap Media Player", MainLauncher = false, NoHistory = true, Theme = "@style/splashscreen3")]
    public class SplashScreen3 : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 1500, // 3 sec.
                AutoReset = false // Do not reset the timer after it's elapsed
            };
            
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                StartActivity(typeof(MainActivity));
            };
            timer.Start();
         
        }
    }

    [Activity(Label = "Pap Media Player", MainLauncher = false, NoHistory = true, Theme = "@style/splashscreen4")]
    public class SplashScreen4 : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 1500, // 3 sec.
                AutoReset = false // Do not reset the timer after it's elapsed
            };
            
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
               StartActivity(typeof(MainActivity));
            };
            timer.Start();
            
        }
    }

    [Activity(Label = "Pap Media Player", MainLauncher = false, NoHistory = true, Theme = "@style/splashscreen5")]
    public class SplashScreen5 : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 1500, // 3 sec.
                AutoReset = false // Do not reset the timer after it's elapsed
            };
            
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                StartActivity(typeof(MainActivity));
            };
            timer.Start();
            
        }
    }


    [Activity(Label = "Pap Media Player", MainLauncher = false, NoHistory = true, Theme = "@style/splashscreen6")]
    public class SplashScreen6 : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 1500, // 3 sec.
                AutoReset = false // Do not reset the timer after it's elapsed
            };
            
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                StartActivity(typeof(MainActivity));
            };
            timer.Start();
            
        }
    }

    [Activity(Label = "Pap Media Player", MainLauncher = true, NoHistory = true, Theme = "@style/splashscreen2")]
    public class Selector : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Random rand = new Random();
            int screen = rand.Next(6);
            switch (screen)
            {
                case 1:
                    StartActivity(typeof(SplashScreen));
                    break;
                case 2:
                    StartActivity(typeof(SplashScreen2));
                    break;
                case 3:
                    StartActivity(typeof(SplashScreen3));
                    break;
                case 4:
                    StartActivity(typeof(SplashScreen4));
                    break;
                case 5:
                    StartActivity(typeof(SplashScreen5));
                    break;
                default:
                    StartActivity(typeof(SplashScreen6));
                    break;
            }   
        }
    }

}
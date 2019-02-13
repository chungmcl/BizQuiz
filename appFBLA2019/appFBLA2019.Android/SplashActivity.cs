//BizQuiz App 2019

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using System;
using System.Collections.Generic;

namespace appFBLA2019.Droid
{
    [Activity(Label = "BizQuiz", Icon = "@drawable/icon", Theme = "@style/splashscreen", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}
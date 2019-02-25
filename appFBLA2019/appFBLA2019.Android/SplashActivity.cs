//BizQuiz App 2019

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using System;
using System.Collections.Generic;

namespace appFBLA2019.Droid
{
    /// <summary>
    /// The splash activity shows a logo while the main activity initializes
    /// </summary>
    [Activity(Label = "BizQuiz", Icon = "@drawable/icon", Theme = "@style/splashscreen", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        /// <summary>
        /// when the Splash activity is created
        /// </summary>
        /// <param name="savedInstanceState"></param>
        /// <param name="persistentState"></param>
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        /// <summary>
        /// When the Splash activity is switched to
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            this.StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}
using System;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Preferences;

namespace FallDetector.Sources
{
    [Activity(Label = "FallDetector", MainLauncher = true, Icon = "@drawable/FallingIcon")]
    public class MainActivity : Activity
    {
        private FallBroadcastReceiver receiver;
        private ISharedPreferences prefs;

        /*private Button accButton;
        private Button orientButton;
        private Button inclinButton;
        private TextView countTextView;*/

        private ImageButton settingsButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Console.WriteLine("MainActivity OnCreate");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MainLayout);
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            var fallDetectorIntent = new Intent(this, typeof(FallDetectorService));
            fallDetectorIntent.PutExtra("FallServiceStarted", "FallService");
            StartService(fallDetectorIntent);

            settingsButton = FindViewById<ImageButton>(Resource.Id.SettingsButton);
            settingsButton.Click += delegate
            {
                this.onClick(settingsButton);
            };

            /*accButton = FindViewById<Button>(Resource.Id.accelerometerButton);
            orientButton = FindViewById<Button>(Resource.Id.orientationButton);
            inclinButton = FindViewById<Button>(Resource.Id.inclinationButton);
            countTextView = FindViewById<TextView>(Resource.Id.fallCountTextView);

            this.updateUI();

            accButton.Click += delegate
            {
                this.onClick(accButton);
            };
            orientButton.Click += delegate
            {
                this.onClick(orientButton);
            };
            inclinButton.Click += delegate
            {
                this.onClick(inclinButton);
            };*/
        }

        protected override void OnResume()
        {
            base.OnResume();

            var intentMsg = new Intent();
            intentMsg.SetAction("FallBroadcastReceiver");
            intentMsg.PutExtra(TAG.enableFallReportTAG, false);
            SendBroadcast(intentMsg);
        }

        protected override void OnPause()
        {
            base.OnPause();

            var intentMsg = new Intent();
            intentMsg.SetAction("FallBroadcastReceiver");
            intentMsg.PutExtra(TAG.enableFallReportTAG, true);
            SendBroadcast(intentMsg);

        }

        private void onClick(View v)
        {
            /*if (v == accButton)
            {
                Intent plotActivityIntent = new Intent(this, typeof(PlotActivity));
                plotActivityIntent.PutExtra("PlotAccelerometer", true);
                StartActivity(plotActivityIntent);
            }
            else if (v == orientButton)
            {
                Intent plotActivityIntent = new Intent(this, typeof(PlotActivity));
                plotActivityIntent.PutExtra("PlotOrientation", true);
                StartActivity(plotActivityIntent);
            }
            else if (v == inclinButton)
            {
                Intent plotActivityIntent = new Intent(this, typeof(PlotActivity));
                plotActivityIntent.PutExtra("PlotInclination", true);
                StartActivity(plotActivityIntent);
            }*/

            if (v == settingsButton)
            {
                Intent settingsActivityIntent = new Intent(this, typeof(SettingsActivity));
                StartActivity(settingsActivityIntent);
            }
        }

        public void updateUI()
        {
            RunOnUiThread(() =>
            {
                int count = prefs.GetInt("FALL_COUNT", 0);
                //countTextView.Text = "Count " + count.ToString();

            });
        }
    }
}
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

        private Button accButton;
        private Button orientButton;
        private Button inclinButton;
        private TextView countTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Console.WriteLine("MainActivity OnCreate");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            accButton = FindViewById<Button>(Resource.Id.accelerometerButton);
            orientButton = FindViewById<Button>(Resource.Id.orientationButton);
            inclinButton = FindViewById<Button>(Resource.Id.inclinationButton);
            countTextView = FindViewById<TextView>(Resource.Id.fallCountTextView);

            this.updateUI();

            var fallDetectorIntent = new Intent(this, typeof(FallDetectorService));
            fallDetectorIntent.PutExtra("FallServiceStarted", "FallService");
            StartService(fallDetectorIntent);

            receiver = new FallBroadcastReceiver();
            var intentFilter = new IntentFilter();
            intentFilter.AddAction("FallBroadcastReceiver");

            RegisterReceiver(receiver, intentFilter);

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
            };
        }

        protected override void OnResume()
        {
            base.OnResume(); // Always call the superclass first.
        }

        private void onClick(View v)
        {
            if (v == accButton)
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
            }

        }

        public void updateUI()
        {

            RunOnUiThread(() =>
            {
                int count = prefs.GetInt("FALL_COUNT", 0);
                countTextView.Text = "Count " + count.ToString();

            });

        }

    }
}



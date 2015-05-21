
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
        private ISharedPreferences pref;
        private ISharedPreferencesEditor prefEditor;

        private float maxTh = 2.5f; //Upper threshold
        private float minTh = 0.7f; //lower threshold

        private Button accButton;
        private Button orientButton;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            pref = PreferenceManager.GetDefaultSharedPreferences(this);
            prefEditor = pref.Edit();

            prefEditor.PutFloat("maxTh", maxTh);
            prefEditor.PutFloat("minTh", minTh);

            prefEditor.Commit();

            accButton = FindViewById<Button>(Resource.Id.accelerometerButton);
            orientButton = FindViewById<Button>(Resource.Id.orientationButton);

            var fallDetectorIntent = new Intent(this, typeof(FallDetectorService));
            fallDetectorIntent.PutExtra("FallServiceStarted", "FallService");
            StartService(fallDetectorIntent);

            accButton.Click += delegate
            {
                this.onClick(accButton);
            };
            orientButton.Click += delegate
            {
                this.onClick(orientButton);
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
           
        }

    }
}



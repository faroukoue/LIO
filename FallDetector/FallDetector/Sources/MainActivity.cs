
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

        private Button next;


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

            next = FindViewById<Button>(Resource.Id.button);

            var fallDetectorIntent = new Intent(this, typeof(FallDetectorService));
            fallDetectorIntent.PutExtra("FallServiceStarted", "FallService");
            StartService(fallDetectorIntent);

            next.Click += delegate
            {
                this.onClick(next);
            };

        }

        protected override void OnResume()
        {
            base.OnResume(); // Always call the superclass first.
        }

        private void onClick(View v)
        {
            if (v == next)
            {
                //Toast.MakeText (this, "Button clicked", ToastLength.Short).Show ();
                var plotActivityIntent = new Intent(this, typeof(PlotActivity));
                StartActivity(plotActivityIntent);

            }
           
        }

    }
}



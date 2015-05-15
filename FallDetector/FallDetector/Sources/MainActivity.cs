
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace FallDetector.Sources
{
    [Activity(Label = "FallDetector", MainLauncher = true, Icon = "@drawable/FallingIcon")]
    public class MainActivity : Activity
    {
        private Button next;

        private ImageButton plusThMax;
        private ImageButton minusthMax;

        private FallBroadcastReceiver receiver;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            next = FindViewById<Button>(Resource.Id.button);

            plusThMax = FindViewById<ImageButton>(Resource.Id.plusTHMaxButton);
            minusthMax = FindViewById<ImageButton>(Resource.Id.minusTHMaxButton);

            var fallDetectorIntent = new Intent(this, typeof(FallDetectorService));
            fallDetectorIntent.PutExtra("FallServiceStarted", "FallService");
            StartService(fallDetectorIntent);

            next.Click += delegate
            {
                this.onClick(next);
            };
            plusThMax.Click += delegate
            {
                this.onClick(plusThMax);
            };
            minusthMax.Click += delegate
            {
                this.onClick(minusthMax);
            };
        }

        protected override void OnResume()
        {
            base.OnResume(); // Always call the superclass first.

            receiver = new FallBroadcastReceiver();
            var intentFilter = new IntentFilter();
            intentFilter.AddAction("FallBroadcastReceiver");
            //intentFilter.addAction("FallDetectorService");

            //RegisterReceiver(receiver, intentFilter);

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



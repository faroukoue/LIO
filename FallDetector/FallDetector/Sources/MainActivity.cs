
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

        private ImageButton plusThMax;
        private ImageButton minusThMax;
        private ImageButton plusThMin;
        private ImageButton minusThMin;

        private TextView thMaxTextView;
        private TextView thMinTextView;


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

            plusThMax = FindViewById<ImageButton>(Resource.Id.plusTHMaxButton);
            minusThMax = FindViewById<ImageButton>(Resource.Id.minusTHMaxButton);
            plusThMin = FindViewById<ImageButton>(Resource.Id.plusTHMinButton);
            minusThMin = FindViewById<ImageButton>(Resource.Id.minusTHMinButton);

            thMaxTextView = FindViewById<TextView>(Resource.Id.THMaxTextView);
            thMinTextView = FindViewById<TextView>(Resource.Id.THMinTextView);

            this.thMaxTextView.Text = maxTh.ToString();
            this.thMinTextView.Text = minTh.ToString();

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
            minusThMax.Click += delegate
            {
                this.onClick(minusThMax);
            };
            plusThMin.Click += delegate
            {
                this.onClick(plusThMin);
            };
            minusThMin.Click += delegate
            {
                this.onClick(minusThMin);
            };
        }

        protected override void OnResume()
        {
            base.OnResume(); // Always call the superclass first.
        }

        private void onClick(View v)
        {
            var intentFall = new Intent(this, typeof(FallBroadcastReceiver));

            if (v == next)
            {
                //Toast.MakeText (this, "Button clicked", ToastLength.Short).Show ();
                var plotActivityIntent = new Intent(this, typeof(PlotActivity));
                StartActivity(plotActivityIntent);

            }
            else if (v == plusThMax)
            {
                maxTh += 0.1f;
                intentFall.PutExtra("Update_threshold", "MaxTh");
            }
            else if (v == minusThMax)
            {
                maxTh -= 0.1f;
                intentFall.PutExtra("Update_threshold", "MaxTh");
            }
            else if (v == plusThMin)
            {
                minTh += 0.1f;
                intentFall.PutExtra("Update_threshold", "MinTh");
            }
            else if (v == minusThMin)
            {
                minTh -= 0.1f;
                intentFall.PutExtra("Update_threshold", "MinTh");
            }
            this.updateUI();
            SendBroadcast(intentFall);
        }

        private void updateUI()
        {

            RunOnUiThread(() =>
                {
                    this.thMaxTextView.Text = maxTh.ToString();
                    this.thMinTextView.Text = minTh.ToString();
                });
        }

    }
}



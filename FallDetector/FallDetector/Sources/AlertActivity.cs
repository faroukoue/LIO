using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using Android.Views;
using Android.Telephony;

namespace FallDetector.Sources
{

    [Activity(Label = "AlertActivity")]
    public class AlertActivity : Activity
    {
        private const String fallMessage = "Fall detected! Are you OK?";
        private const int notificationId = 0;

        private ISharedPreferences prefs;

        private Button okButton;
        private Button reportButton;
        private TextView countTextView;

        private Timer timer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.AlertLayout);

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            okButton = FindViewById<Button>(Resource.Id.IMOKButton);
            reportButton = FindViewById<Button>(Resource.Id.ReportButton);
            countTextView = FindViewById<TextView>(Resource.Id.CountTextView);

            timer = new Timer(15 * 1000, 1000, this);
            timer.Start();


            okButton.Click += delegate
            {
                this.onClick(okButton);
            };

            reportButton.Click += delegate
            {
                this.onClick(reportButton);
            };

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

            if (v == okButton)
            {
                this.Finish();
            }
            else if (v == reportButton)
            {
                this.reportFall();
            }

        }

        public void reportFall()
        {
            this.timer.Cancel();

            this.Finish();

        }

        public void updateTextView(String content)
        {
            RunOnUiThread(() =>
            {
                this.countTextView.Text = content;
            });
        }

        private class Timer : CountDownTimer
        {
            private AlertActivity alertAct;
            private int count = 15;

            public Timer(long millisInFuture, long countDownInterval, AlertActivity act)
                : base(millisInFuture, countDownInterval)
            {
                alertAct = act;
            }

            public override void OnFinish()
            {
                alertAct.reportFall();
            }

            public override void OnTick(long millisUntilFinished)
            {
                count--;
                alertAct.updateTextView(count.ToString());
            }
        }
    }
}
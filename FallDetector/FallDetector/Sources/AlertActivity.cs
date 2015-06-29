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

            //this.sendNotification();
            //this.sendEmail();
            this.sendSMS();

            this.Finish();

        }

        private void sendNotification()
        {
            Boolean checkNotification = prefs.GetBoolean(TAG.notificationTAG, false);

            if (!checkNotification)
                return;

            Console.WriteLine("sendNotification");
            Intent intent = new Intent(this, typeof(MainActivity));

            const int pendingIntentId = 0; 
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(this, pendingIntentId, intent, PendingIntentFlags.OneShot);

            Notification.Builder notificationBuilder = new Notification.Builder(this)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("Fall")
                .SetContentText(fallMessage)
                .SetDefaults(NotificationDefaults.All)
                .SetSmallIcon(Resource.Drawable.Warning);

            Notification notification = notificationBuilder.Build();
            notification.Flags = NotificationFlags.AutoCancel;

            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(notificationId, notification);
        }

        private void sendEmail()
        {
            Boolean checkEmail = prefs.GetBoolean(TAG.emailTAG, false);
            String emailAdress = prefs.GetString(TAG.emailAdressTAG, null);

            if (!checkEmail || emailAdress == null)
                return;

            var emailIntent = new Intent(Android.Content.Intent.ActionSendto);

            emailIntent.SetType("message/rfc822");

            //emailIntent.PutExtra(Android.Content.Intent.ExtraEmail, new String[]{emailAdress});
            emailIntent.SetData(Android.Net.Uri.Parse("mailto:" + emailAdress));
            emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Fall detected!");
            emailIntent.PutExtra(Android.Content.Intent.ExtraText, fallMessage);

            //emailIntent.AddFlags(ActivityFlags.NewTask);

            try
            {
                StartActivity(Intent.CreateChooser(emailIntent, "Send Mail ..."));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            


        }

        private void sendSMS()
        {
            Boolean checkSms = prefs.GetBoolean(TAG.smsTAG, false);
            String phoneNumber = prefs.GetString(TAG.phoneNumberTAG, null);

            if (!checkSms || phoneNumber == null)
                return;

            try
            {
                SmsManager.Default.SendTextMessage(phoneNumber, null, fallMessage, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

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
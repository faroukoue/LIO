using System;
using System.Net.Mail;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Telephony;
using Android.Preferences;


namespace FallDetector.Sources
{
    [Service]
    [IntentFilter(new String[] { "AlertService" })]
    class AlertService : Service
    {
        private const String fallMessage = "Fall detected! Are you OK?";
        private const int notificationId = 0;

        private ISharedPreferences prefs;
        private Timer timer;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [Obsolete]
        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            timer = new Timer(15 * 1000, 1000, this);
            timer.Start();

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);

            Log.Debug(TAG.fallDetectorTAG, "OnTaskRemoved");
            this.StopSelf();

        }
        public void reportFall()
        {
            this.timer.Cancel();

            this.sendNotification();
            this.sendEmail();
            this.sendSMS();

            this.StopSelf();

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

            // Command line argument must the the SMTP host.
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("user@gmail.com", "password");

            MailMessage mm = new MailMessage("donotreply@domain.com", emailAdress, "Fall detected!", fallMessage);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
            mm.Dispose();


            var emailIntent = new Intent(Android.Content.Intent.ActionSendto);

            emailIntent.SetType("message/rfc822");

            //emailIntent.PutExtra(Android.Content.Intent.ExtraEmail, new String[]{emailAdress});
            emailIntent.SetData(Android.Net.Uri.Parse("mailto:" + emailAdress));
            emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Fall detected!");
            emailIntent.PutExtra(Android.Content.Intent.ExtraText, fallMessage);

            //emailIntent.AddFlags(ActivityFlags.NewTask);

            /*try
            {
                StartActivity(Intent.CreateChooser(emailIntent, "Send Mail ..."));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }*/

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

        private class Timer : CountDownTimer
        {
            private AlertService alertSer;

            public Timer(long millisInFuture, long countDownInterval, AlertService ser)
                : base(millisInFuture, countDownInterval)
            {
                alertSer = ser;
            }

            public override void OnFinish()
            {
                alertSer.reportFall();
            }

            public override void OnTick(long millisUntilFinished)
            {

            }
        }
    }
}
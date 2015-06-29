using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Telephony;

namespace FallDetector.Sources
{
    class AlertReport
    {
        private const String fallMessage = "Fall detected! Are you OK?";
        private const int notificationId = 0;

        private Context context_ = null;
        private ISharedPreferences prefs;

        public AlertReport(Context context)
        {
            context_ = context;
        }

        public void reportFall()
        {

            this.sendNotification();
            //this.sendEmail();
            this.sendSMS();

        }

        private void sendNotification()
        {
            Boolean checkNotification = prefs.GetBoolean(TAG.notificationTAG, false);

            if (!checkNotification)
                return;

            Console.WriteLine("sendNotification");
            Intent intent = new Intent(context_, typeof(MainActivity));

            const int pendingIntentId = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(context_, pendingIntentId, intent, PendingIntentFlags.OneShot);

            Notification.Builder notificationBuilder = new Notification.Builder(context_)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("Fall")
                .SetContentText(fallMessage)
                .SetDefaults(NotificationDefaults.All)
                .SetSmallIcon(Resource.Drawable.Warning);

            Notification notification = notificationBuilder.Build();
            notification.Flags = NotificationFlags.AutoCancel;

            NotificationManager notificationManager =
                context_.GetSystemService(Context.NotificationService) as NotificationManager;

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
                context_.StartActivity(Intent.CreateChooser(emailIntent, "Send Mail ..."));
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



    }
}
using System;

using Android.App;
using Android.Content;
using Android.Widget;

namespace FallDetector.Sources
{
    [BroadcastReceiver(Enabled = true)]
    public class FallBroadcastReceiver : BroadcastReceiver
    {
        private Service service;

        public FallBroadcastReceiver()
        {
            this.service = null;
        }
        public FallBroadcastReceiver(Service ser)
        {
            this.service = ser;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String temp = intent.GetStringExtra("Update_threshold");
            if (service != null && temp != "")
            {
                Console.WriteLine("Update_threshold");
                ((FallDetectorService)service).updateThreshold();
            }
            //Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();




        }
    }
}


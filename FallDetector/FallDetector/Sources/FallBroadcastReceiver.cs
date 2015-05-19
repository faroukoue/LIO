using System;

using Android.App;
using Android.Content;
using Android.Widget;

namespace FallDetector.Sources
{
    [BroadcastReceiver(Enabled = true)]
    public class FallBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            String temp = intent.GetStringExtra("Update_threshold");
            if (temp != "")
            {
                Console.WriteLine("Update_threshold");
                //((FallDetectorService)context).updateThreshold();
            }
            //Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();




        }
    }
}


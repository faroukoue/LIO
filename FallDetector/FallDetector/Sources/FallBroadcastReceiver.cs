
using System;
using Android.Content;
using Android.Widget;

namespace FallDetector.Sources
{
    [BroadcastReceiver(Enabled = true)]
    public class FallBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            String temp = intent.GetStringExtra("FallDetected");
            //Console.WriteLine(temp);
            Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();


        }
    }
}


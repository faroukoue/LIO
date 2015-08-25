using System;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.Util;

namespace FallDetector.Sources
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "FallBroadcastReceiver" })]
    public class FallBroadcastReceiver : BroadcastReceiver
    {
        private const String TAG = "FallBroadcastReceiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Boolean enable = intent.GetBooleanExtra("enableFallReportTAG", false);

            try
            {
                Log.Debug(TAG, "FallDetected");
            }
            catch (Exception excep)
            {
                Log.Error(TAG, excep.ToString());
            }

        }
    }
}


using System;

using Android.App;
using Android.Content;
using Android.OS;

namespace FallDetector.Sources
{
    public class FallDetectorServiceConnection : Java.Lang.Object, IServiceConnection
    {
        protected FallDetectorServiceBinder binder;
        protected Activity activity;

        public FallDetectorServiceBinder Binder
        {
            get { return this.binder; }
            set { this.binder = value; }
        }

        public FallDetectorServiceConnection(PlotActivity pActivity)
        {
            if (pActivity != null)
                this.activity = pActivity;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Console.WriteLine("OnServiceConnected");
            FallDetectorServiceBinder fallBinder = service as FallDetectorServiceBinder;
            if (fallBinder != null)
            {
                Console.WriteLine("OnServiceConnected Succeeded");

                this.binder = fallBinder;
                this.binder.activity = this.activity;
                ((PlotActivity)this.activity).isBound = true;
                ((PlotActivity)this.activity).fallServiceBinder = fallBinder;

            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Console.WriteLine("OnServiceDisconnected");
            ((PlotActivity)this.activity).isBound = false;
            this.binder.activity = null;
            ((PlotActivity)this.activity).fallServiceBinder = null;
        }


    }
}
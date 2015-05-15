using System;

using Android.OS;
using Android.App;


namespace FallDetector.Sources
{
    public class FallDetectorServiceBinder : Binder
    {
        protected FallDetectorService service;
        protected Activity pActivity;
        public FallDetectorService Service
        {
            get { return this.Service; }
        }

        public Activity activity
        {
            get { return this.pActivity; }
            set { this.pActivity = value; }
        }


        public FallDetectorServiceBinder(FallDetectorService fallService)
        {
            this.service = fallService;
        }
    }
}
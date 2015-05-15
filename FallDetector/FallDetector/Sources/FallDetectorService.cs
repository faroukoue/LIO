using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Hardware;
using Android.Util;
using Android;


namespace FallDetector.Sources
{
    [Service]
    [IntentFilter(new String[] { "FallDetectorService" })]
    public class FallDetectorService : Service, ISensorEventListener
    {
        private static readonly object _syncLock = new object();
        private SensorManager mSensorManager;
        private Sensor mAccelerometer;
        private CustomCountDownTimer timer;

        private Boolean freeFallDetected_ = false;

        public Boolean FreeFallDetected
        {
            get { return freeFallDetected_; }
            set { freeFallDetected_ = value; }
        }
        private Boolean impactDetected_ = false;

        public Boolean ImpactDetected
        {
            get { return impactDetected_; }
            set { impactDetected_ = value; }
        }

        public Boolean isBound = false;
        public FallDetectorServiceBinder binder;

        private const int notificationId = 0;
        private const float maxTh = 1.8f; //Upper threshold
        private const float minTh = 0.7f; //lower threshold
        private const long timeFallingWindow = 3; //time of the fall (in seconds)



        public FallDetectorService()
            : base()
        {

        }

        public override IBinder OnBind(Intent intent)
        {
            Console.WriteLine("OnBind");

            binder = new FallDetectorServiceBinder(this);
            this.isBound = true;

            return binder;
        }

        [Obsolete]
        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            mSensorManager = (SensorManager)GetSystemService(SensorService);
            mAccelerometer = mSensorManager.GetDefaultSensor(SensorType.Accelerometer);
            mSensorManager.RegisterListener(this, mAccelerometer, SensorDelay.Normal);

            timer = new CustomCountDownTimer(timeFallingWindow * 1000, 1000, this);

            String temp = intent.GetStringExtra("FallServiceStarted");
            Console.WriteLine(temp);

            /*var intentFall = new Intent(this, typeof(FallBroadcastReceiver));
            intentFall.PutExtra("FallDetected", "Fall");
            SendBroadcast(intentFall);*/

            Log.Debug("FallDetectorService", "StartCommandResult");

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            mSensorManager.UnregisterListener(this);
            Console.WriteLine("OnDestroy");
            Log.Debug("FallDetectorService", "OnDestroy");
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            Console.WriteLine("OnTaskRemoved");

            this.StopSelf();

        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            lock (_syncLock)
            {
                float ax = e.Values[0] / SensorManager.GravityEarth;
                float ay = e.Values[1] / SensorManager.GravityEarth;
                float az = e.Values[2] / SensorManager.GravityEarth;

                double accT = Math.Sqrt(ax * ax + ay * ay + az * az);

                if (this.binder != null && this.binder.activity != null)
                    ((PlotActivity)this.binder.activity).updatePlot(e.Timestamp / 1e9, accT);

                if (accT < minTh && !this.FreeFallDetected)
                {
                    this.FreeFallDetected = true;
                    this.timer.Start();
                    Console.WriteLine("Timer Start");
                }

                if (accT > maxTh && !this.ImpactDetected)
                {
                    this.ImpactDetected = true;
                }

                //Console.WriteLine(temp);
                //Log.Debug("OnSensorChanged", temp);

            }
        }

        public void triggersFallDetected()
        {
            this.sendNotification();

            Console.WriteLine("Fall Detected");

        }

        public void resetFallDetection()
        {
            this.FreeFallDetected = false;
            this.ImpactDetected = false;

            //temporary
            //this.sendNotification();
        }

        public void sendNotification()
        {

            Console.WriteLine("sendNotification");

            Intent intent = new Intent(this, typeof(MainActivity));

            const int pendingIntentId = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(this, pendingIntentId, intent, PendingIntentFlags.OneShot);

            Notification.Builder notificationBuilder = new Notification.Builder(this)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("Fall")
                .SetContentText("Fall Detected! Are you OK ?")
                .SetDefaults(NotificationDefaults.All)
                .SetSmallIcon(Resource.Drawable.Warning);

            Notification notification = notificationBuilder.Build();
            notification.Flags = NotificationFlags.AutoCancel;

            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(notificationId, notification);
        }

    }


}


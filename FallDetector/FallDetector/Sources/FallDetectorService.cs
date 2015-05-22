using System;

using Android.App;
using Android.OS;
using Android.Content;
using Android.Hardware;
using Android.Util;


namespace FallDetector.Sources
{
    [Service]
    [IntentFilter(new String[] { "FallDetectorService" })]
    public class FallDetectorService : Service, ISensorEventListener
    {
        private FallBroadcastReceiver receiver;

        private static readonly object _syncLock = new object();
        private SensorManager sensorManager;
        private Sensor accelerometerSensor;
        private Sensor magnetometerSensor;
        private Sensor gyroscopeSensor;

        private CustomCountDownTimer timer;

        private float[] lastAccelerometer;
        private float[] lastMagnetometer;
        private Boolean lastAccelerometerSet = false;
        private Boolean lastMagnetometerSet = false;
        private float[] rotMatrix;
        private float[] orientationValues;

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
        private const float maxTh = 2.2f; //Upper threshold
        private const float minTh = 0.7f; //lower threshold
        private const long timeFallingWindow = 2; //time of the fall (in seconds)


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

        public void init()
        {

            lastAccelerometer = new float[3];
            lastMagnetometer = new float[3];
            rotMatrix = new float[9];
            orientationValues = new float[3];

            sensorManager = (SensorManager)GetSystemService(SensorService);

            accelerometerSensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            sensorManager.RegisterListener(this, accelerometerSensor, SensorDelay.Normal);

            gyroscopeSensor = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
            //sensorManager.RegisterListener(this, gyroscopeSensor, SensorDelay.Normal);

            magnetometerSensor = sensorManager.GetDefaultSensor(SensorType.MagneticField);
            sensorManager.RegisterListener(this, magnetometerSensor, SensorDelay.Normal);

            timer = new CustomCountDownTimer(timeFallingWindow * 1000, 1000, this);

            receiver = new FallBroadcastReceiver();
            var intentFilter = new IntentFilter();
            intentFilter.AddAction("FallBroadcastReceiver");

            RegisterReceiver(receiver, intentFilter);
        }

        [Obsolete]
        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            this.init();

            String temp = intent.GetStringExtra("FallServiceStarted");
            Console.WriteLine(temp);

            Log.Debug("FallDetectorService", "StartCommandResult");

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            sensorManager.UnregisterListener(this);
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
            //Console.WriteLine(e.Sensor.StringType + " / " + Sensor.StringTypeAccelerometer);

            switch (e.Sensor.StringType)
            {
                case Sensor.StringTypeAccelerometer:
                    lock (_syncLock)
                    {
                        float ax = e.Values[0];
                        float ay = e.Values[1];
                        float az = e.Values[2];

                        e.Values.CopyTo(lastAccelerometer, 0);
                        lastAccelerometerSet = true;

                        double accT = (Math.Sqrt(ax * ax + ay * ay + az * az)) / SensorManager.GravityEarth;

                        if (this.binder != null && this.binder.activity != null && ((PlotActivity)this.binder.activity).PlotAccelerometer)
                            ((PlotActivity)this.binder.activity).updateAccPlot(e.Timestamp / 1e9, accT);

                        if (accT < minTh && !this.FreeFallDetected)
                        {
                            this.FreeFallDetected = true;
                            this.timer.Start();
                            Console.WriteLine("Timer Start");
                        }

                        if (accT > maxTh && !this.ImpactDetected && this.FreeFallDetected)
                        {
                            this.ImpactDetected = true;
                        }

                    }
                    break;

                case Sensor.StringTypeMagneticField:
                    lock (_syncLock)
                    {
                        e.Values.CopyTo(lastMagnetometer, 0);
                        lastMagnetometerSet = true;
                    }

                    break;

                case Sensor.StringTypeGyroscope:
                    lock (_syncLock)
                    {
                        
                    }
                    break;

            }

            if (lastMagnetometerSet && lastAccelerometerSet)
            {
                try
                {
                    SensorManager.GetRotationMatrix(rotMatrix, null, lastAccelerometer, lastMagnetometer);
                    SensorManager.GetOrientation(rotMatrix, orientationValues);

                    for (int i = 0; i < 3; ++i)
                    {
                        orientationValues[i] = (float)(orientationValues[i] * (180.0 / Math.PI));
                    }

                    double azimuth = orientationValues[0];
                    double pitch = orientationValues[1];
                    double roll = orientationValues[2];

                    if (this.binder != null && this.binder.activity != null && ((PlotActivity)this.binder.activity).PlotOrientation)
                        ((PlotActivity)this.binder.activity).updateOrientPlot(e.Timestamp / 1e9, azimuth, pitch, roll);
                }
                catch (Exception excep)
                {
                    Log.Error("FallDetectorService", excep.ToString());
                }


                //Console.WriteLine("Azimuth = " + orientationValues[0].ToString() + " Pitch = " + orientationValues[1].ToString() + " Roll = " + orientationValues[2].ToString());


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
        }

        public void updateThreshold()
        {
            Console.WriteLine("updateThreshold" + maxTh.ToString() + minTh.ToString());
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


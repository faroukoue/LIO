using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Content;
using Android.Hardware;
using Android.Util;
using Android.Preferences;


namespace FallDetector.Sources
{
    [Service]
    [IntentFilter(new String[] { "FallDetectorService" })]
    public class FallDetectorService : Service, ISensorEventListener
    {
        private const String TAG = "FallDetectorService";
        private const String PrefTAG = "FALL_COUNT";

        private ISharedPreferences prefs;
        private ISharedPreferencesEditor prefsEditor;

        private static readonly object _syncLock = new object();
        private SensorManager sensorManager;
        private Sensor accelerometerSensor;
        private Sensor magnetometerSensor;
        private Sensor gyroscopeSensor;
        private Sensor rotationSensor;

        private CustomCountDownTimer timer;

        private float[] lastAccelerometer;
        private float[] lastMagnetometer;
        private Boolean lastAccelerometerSet = false;
        private Boolean lastMagnetometerSet = false;
        private float[] rotMatrix;
        private float[] orientationValues;
        private double accT = 0;
        private double inclination = 0;
        List<double> inclinationList;
        List<double> pitchList;

        private float azimuth = 0;
        private float pitch = 0;
        private float roll = 0;
        private float pitchFreeFall = 0;
        private float pitchImpact = 0;
        private float rollFreeFall = 0;
        private float rollImpact = 0;

        private Boolean freeFallDetected = false;
        private Boolean impactDetected = false;
        private Boolean orientationChanged = false;

        private int fallCount = -1;

        public Boolean OrientationChanged
        {
            get { return orientationChanged; }
            set { orientationChanged = value; }
        }

        public Boolean FreeFallDetected
        {
            get { return freeFallDetected; }
            set { freeFallDetected = value; }
        }

        public Boolean ImpactDetected
        {
            get { return impactDetected; }
            set { impactDetected = value; }
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
            Log.Debug(TAG, "OnBind");

            binder = new FallDetectorServiceBinder(this);
            this.isBound = true;

            return binder;
        }

        public void init()
        {

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            prefsEditor = prefs.Edit();

            fallCount = prefs.GetInt(PrefTAG, 0);

            prefsEditor.PutInt(PrefTAG, fallCount);


            lastAccelerometer = new float[3];
            lastMagnetometer = new float[3];
            rotMatrix = new float[16];
            orientationValues = new float[3];

            inclinationList = new List<double>();
            pitchList = new List<double>();

            sensorManager = (SensorManager)GetSystemService(SensorService);

            accelerometerSensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            sensorManager.RegisterListener(this, accelerometerSensor, SensorDelay.Normal);

            gyroscopeSensor = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
            //sensorManager.RegisterListener(this, gyroscopeSensor, SensorDelay.Normal);

            magnetometerSensor = sensorManager.GetDefaultSensor(SensorType.MagneticField);
            //sensorManager.RegisterListener(this, magnetometerSensor, SensorDelay.Normal);

            rotationSensor = sensorManager.GetDefaultSensor(SensorType.RotationVector);
            sensorManager.RegisterListener(this, rotationSensor, SensorDelay.Normal);

            timer = new CustomCountDownTimer(timeFallingWindow * 1000, 1000, this);
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

            Log.Debug(TAG, "OnTaskRemoved");
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

                        accT = (Math.Sqrt(ax * ax + ay * ay + az * az)) / SensorManager.GravityEarth;

                        double acosAz = Math.Acos(az / (accT * SensorManager.GravityEarth));
                        double acosAzToDegree = acosAz * (180 / Math.PI);
                        inclination = Math.Round(acosAzToDegree);


                        if (this.binder != null && this.binder.activity != null)
                        {

                            if (((PlotActivity)this.binder.activity).PlotAccelerometer)
                                ((PlotActivity)this.binder.activity).updateAccPlot(e.Timestamp / 1e9, accT);
                            else if (((PlotActivity)this.binder.activity).PlotInclination)
                                ((PlotActivity)this.binder.activity).updateIncliPlot(e.Timestamp / 1e9, inclination);

                        }

                        if (this.freeFallDetected)
                        {
                            inclinationList.Add(inclination);
                            pitchList.Add(pitch);
                        }


                        if (accT < minTh && !this.FreeFallDetected)
                        {
                            this.FreeFallDetected = true;
                            this.pitchFreeFall = pitch;
                            this.rollFreeFall = roll;
                            this.timer.Start();

                            Console.WriteLine("Timer Start");

                            Log.Debug(TAG, "Timer Start");
                            //Log.Debug(TAG, "pitchFreeFall : " + pitchFreeFall);
                            //Log.Debug(TAG, "rollFreeFall : " + rollFreeFall);
                        }

                        /*if (accT > maxTh && !this.ImpactDetected && this.FreeFallDetected)
                        {
                            this.ImpactDetected = true;
                            this.pitchImpact = pitch;
                            this.rollImpact = roll;

                            float tempPitch = Math.Abs(pitchFreeFall) + Math.Abs(pitchImpact);
                            float tempRoll = Math.Abs(rollFreeFall) + Math.Abs(rollImpact);

                            if (tempPitch >= 90 || tempRoll >= 90)
                                this.orientationChanged = true;

                            Log.Debug(TAG, "pitchImpact : " + pitchImpact);
                            Log.Debug(TAG, "rollImpact : " + rollImpact);
                        }*/

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

                case Sensor.StringTypeRotationVector:
                    lock (_syncLock)
                    {
                        float[] tempValue = new float[3];
                        e.Values.CopyTo(tempValue, 0);

                        SensorManager.GetRotationMatrixFromVector(rotMatrix, tempValue);
                        SensorManager.GetOrientation(rotMatrix, orientationValues);

                        for (int i = 0; i < 3; ++i)
                        {
                            orientationValues[i] = (float)(orientationValues[i] * (180.0 / Math.PI));
                        }

                        azimuth = orientationValues[0];
                        pitch = orientationValues[1];
                        roll = orientationValues[2];

                        if (this.binder != null && this.binder.activity != null && ((PlotActivity)this.binder.activity).PlotOrientation)
                            ((PlotActivity)this.binder.activity).updateOrientPlot(e.Timestamp / 1e9, azimuth, pitch, roll);
                    }
                    break;

            }

            /*if (lastMagnetometerSet && lastAccelerometerSet)
            {
                try
                {
                    SensorManager.GetRotationMatrix(rotMatrix, null, lastAccelerometer, lastMagnetometer);
                    SensorManager.GetOrientation(rotMatrix, orientationValues);

                    for (int i = 0; i < 3; ++i)
                    {
                        orientationValues[i] = (float)(orientationValues[i] * (180.0 / Math.PI));
                    }

                    azimuth = orientationValues[0];
                    pitch = orientationValues[1];
                    roll = orientationValues[2];

                    if (this.binder != null && this.binder.activity != null && ((PlotActivity)this.binder.activity).PlotOrientation)
                        ((PlotActivity)this.binder.activity).updateOrientPlot(e.Timestamp / 1e9, azimuth, pitch, roll);
                }
                catch (Exception excep)
                {
                    Log.Error("FallDetectorService", excep.ToString());
                }

                //Console.WriteLine("Azimuth = " + orientationValues[0].ToString() + " Pitch = " + orientationValues[1].ToString() + " Roll = " + orientationValues[2].ToString());

            }*/

        }

        public void triggersFallDetected()
        {
            this.sendNotification();
            this.fallCount++;

            prefsEditor.PutInt(PrefTAG, this.fallCount);
            prefsEditor.Apply();

            var intentMsg = new Intent();
            intentMsg.SetAction("FallBroadcastReceiver");
            intentMsg.PutExtra("FallDetected", true);
            SendBroadcast(intentMsg);
            Log.Debug(TAG, "Fall Detected");

        }

        public void resetFallDetection()
        {


            double maxIncl = inclinationList.Max();
            double minIncl = inclinationList.Min();

            Log.Debug(TAG, "MaxIncli : " + maxIncl.ToString() + " MinIncli : " + minIncl.ToString());

            double maxPitch = pitchList.Max();
            double minPitch = pitchList.Min();

            double deltaPitch = Math.Abs(maxPitch) + Math.Abs(minPitch);
            if (deltaPitch >= 90)
                this.orientationChanged = true;

            Log.Debug(TAG, "maxPitch : " + maxPitch.ToString() + " minPitch : " + minPitch.ToString());

            if (this.impactDetected && this.freeFallDetected && this.orientationChanged)
                this.triggersFallDetected();

            pitchList.Clear();
            inclinationList.Clear();

            this.FreeFallDetected = false;
            this.ImpactDetected = false;
            this.orientationChanged = false;


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


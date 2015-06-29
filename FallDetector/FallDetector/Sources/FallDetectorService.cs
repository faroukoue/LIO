using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Content;
using Android.Hardware;
using Android.Util;
using Android.Preferences;
using Android.Telephony;


namespace FallDetector.Sources
{
    [Service]
    [IntentFilter(new String[] { "FallDetectorService" })]
    public class FallDetectorService : Service, ISensorEventListener
    {
        private const int notificationId = 0;
        private const float maxAccTh = 2.2f; //Upper threshold
        private const float minAccTh = 0.7f; //Lower threshold
        private const float omegaTh = 10f; //Omega threshold
        private const long timeFallingWindow = 2; //time of the fall (in seconds)
        private const String fallMessage = "Fall detected! Are you OK?";

        private ISharedPreferences prefs;
        private ISharedPreferencesEditor prefsEditor;
        private FallBroadcastReceiver receiver;

        private static readonly object _syncLock = new object();
        private SensorManager sensorManager;
        private Sensor accelerometerSensor;
        private Sensor gyroscopeSensor;
        private Sensor rotationSensor;

        private CustomCountDownTimer timer;

        private float[] rotMatrix;
        private float[] orientationValues;
        private float[] rateOfRotation;

        private double accT = 0;
        private double inclination = 0;
        private double omegaAmpl = 0;
        private List<double> inclinationList;
        private List<double> pitchList;
        private List<double> omegaPitch;
        private List<double> rollList;
        private List<double> omegaRoll;

        private float azimuth = 0;
        private float pitch = 0;
        private float roll = 0;

        private Boolean freeFallDetected = false;
        private Boolean impactDetected = false;
        private Boolean orientationChanged = false;
        private Boolean omegaAmplitudeChanged = false;

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

        private AlertReport alert;

        private Boolean enableFallReport = false;

        public Boolean EnableFallReport
        {
            get { return enableFallReport; }
            set { enableFallReport = value; }
        }


        public FallDetectorService()
            : base()
        {
        }

        public override IBinder OnBind(Intent intent)
        {
            Log.Debug(TAG.fallDetectorTAG, "OnBind");

            binder = new FallDetectorServiceBinder(this);
            this.isBound = true;

            return binder;
        }

        public void init()
        {
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            prefsEditor = prefs.Edit();

            fallCount = prefs.GetInt(TAG.fallDetectorTAG, 0);

            prefsEditor.PutInt(TAG.PrefTAG, fallCount);

            rotMatrix = new float[16];
            orientationValues = new float[3];
            rateOfRotation = new float[3];

            inclinationList = new List<double>();
            pitchList = new List<double>();
            rollList = new List<double>();
            omegaPitch = new List<double>();
            omegaRoll = new List<double>();

            sensorManager = (SensorManager)GetSystemService(SensorService);

            accelerometerSensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            sensorManager.RegisterListener(this, accelerometerSensor, SensorDelay.Normal);

            gyroscopeSensor = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
            sensorManager.RegisterListener(this, gyroscopeSensor, SensorDelay.Normal);

            rotationSensor = sensorManager.GetDefaultSensor(SensorType.RotationVector);
            sensorManager.RegisterListener(this, rotationSensor, SensorDelay.Normal);

            timer = new CustomCountDownTimer(timeFallingWindow * 1000, 1000, this);

            alert = new AlertReport(this);

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

            Log.Debug(TAG.fallDetectorTAG, "OnTaskRemoved");
            this.StopSelf();

        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            //Log.Debug(TAG, e.Sensor.StringType);

            if (e.Sensor == accelerometerSensor)
            {
                this.updateAccelerometer(e);

            }
            Log.Debug(TAG.fallDetectorTAG, e.Sensor.ToString());

            if (e.Sensor == rotationSensor)
            {
                this.updateRotation(e);
            }
            if (e.Sensor == gyroscopeSensor)
            {
                this.updateGyroscope(e);
            }

            if (this.freeFallDetected)
            {
                inclinationList.Add(inclination);
                pitchList.Add(pitch);
                omegaPitch.Add(rateOfRotation[0]);
                rollList.Add(roll);
                omegaRoll.Add(rateOfRotation[1]);
            }

        }

        public void triggersFallDetected()
        {
            this.startAlertActivity();

            this.fallCount++;

            prefsEditor.PutInt(TAG.PrefTAG, this.fallCount);
            prefsEditor.Apply();

            Log.Debug(TAG.fallDetectorTAG, "Fall Detected");

        }

        private void updateAccelerometer(SensorEvent e)
        {
            Log.Debug(TAG.fallDetectorTAG, "accelerometerSensor");
            lock (_syncLock)
            {
                float ax = e.Values[0];
                float ay = e.Values[1];
                float az = e.Values[2];

                accT = (Math.Sqrt(ax * ax + ay * ay + az * az)) / SensorManager.GravityEarth;

                double acosAz = Math.Acos(az / (accT * SensorManager.GravityEarth));
                double acosAzToDegree = acosAz * (180 / Math.PI);
                inclination = Math.Round(acosAzToDegree);

                if (this.binder != null && this.binder.activity != null)
                {
                    if (((PlotActivity)this.binder.activity).PlotAccelerometer)
                    {
                        ((PlotActivity)this.binder.activity).updateAccPlot(e.Timestamp / 1e9, accT);
                    }

                    /*else if (((PlotActivity)this.binder.activity).PlotInclination)
                        ((PlotActivity)this.binder.activity).updateIncliPlot(e.Timestamp / 1e9, inclination);*/
                }

                if (accT < minAccTh && !this.FreeFallDetected)
                {
                    this.FreeFallDetected = true;
                    this.timer.Start();

                    Log.Debug(TAG.fallDetectorTAG, "Timer Start");
                }

                if (accT > maxAccTh && !this.ImpactDetected && this.FreeFallDetected)
                {
                    this.ImpactDetected = true;
                }

            }
        }

        private void updateRotation(SensorEvent e)
        {
            lock (_syncLock)
            {
                Log.Debug(TAG.fallDetectorTAG, "RotationSensor");
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
        }

        private void updateGyroscope(SensorEvent e)
        {
            lock (_syncLock)
            {
                Log.Debug(TAG.fallDetectorTAG, "GyroscopeSensor");
                e.Values.CopyTo(rateOfRotation, 0);

                float ax = rateOfRotation[0]; //Pitch
                float ay = rateOfRotation[1]; //Roll
                float az = rateOfRotation[2]; //Azimuth

                omegaAmpl = (Math.Sqrt(ax * ax + ay * ay + az * az));

                if (omegaAmpl > omegaTh && this.freeFallDetected && !this.omegaAmplitudeChanged)
                    this.omegaAmplitudeChanged = true;

                if (this.binder != null && this.binder.activity != null && ((PlotActivity)this.binder.activity).PlotInclination)
                    ((PlotActivity)this.binder.activity).updateGyroscopePlot(e.Timestamp / 1e9, omegaAmpl);

            }
        }


        public void notifyFallDetection()
        {
            double maxIncl = inclinationList.Max();
            double minIncl = inclinationList.Min();

            double maxPitch = pitchList.Max();
            double minPitch = pitchList.Min();
            double deltaPitch = Math.Abs(maxPitch) + Math.Abs(minPitch);

            double maxRoll = rollList.Max();
            double minRoll = rollList.Min();
            double deltaRoll = Math.Abs(maxRoll) + Math.Abs(minRoll);

            double maxOmegaPitch = Math.Abs(omegaPitch.Max());
            double minOmegaPitch = Math.Abs(omegaPitch.Min());

            double maxOmegaRoll = Math.Abs(omegaRoll.Max());
            double minOmegaRoll = Math.Abs(omegaRoll.Min());

            bool omegaPitchTh = (maxOmegaPitch > omegaTh) || (minOmegaPitch > omegaTh);
            bool omegaRollTh = (maxOmegaRoll > omegaTh) || (minOmegaRoll > omegaTh);

            if ((deltaPitch >= 60) || (deltaRoll >= 60))
                this.orientationChanged = true;

            //Log.Debug(TAG, "omegaRollTh : " + omegaRollTh.ToString());
            Log.Debug(TAG.fallDetectorTAG, "omegaAmpl : " + omegaAmpl.ToString());
            Log.Debug(TAG.fallDetectorTAG, "MaxIncli : " + maxIncl.ToString() + " MinIncli : " + minIncl.ToString());
            Log.Debug(TAG.fallDetectorTAG, "maxPitch : " + maxPitch.ToString() + " minPitch : " + minPitch.ToString());
            Log.Debug(TAG.fallDetectorTAG, "maxRoll : " + maxRoll.ToString() + " minRoll : " + minRoll.ToString());


            if (this.impactDetected && this.freeFallDetected && this.orientationChanged && this.omegaAmplitudeChanged)
                this.triggersFallDetected();

            this.reset();

        }

        private void reset()
        {
            pitchList.Clear();
            inclinationList.Clear();
            rollList.Clear();
            omegaPitch.Clear();
            omegaRoll.Clear();

            this.FreeFallDetected = false;
            this.ImpactDetected = false;
            this.orientationChanged = false;
            this.omegaAmplitudeChanged = false;
        }

        public void startAlertActivity()
        {

            Intent alertActivityIntent = new Intent(this, typeof(AlertActivity));
            alertActivityIntent.SetFlags(ActivityFlags.NewTask);

            StartActivity(alertActivityIntent);
        }

    }
}


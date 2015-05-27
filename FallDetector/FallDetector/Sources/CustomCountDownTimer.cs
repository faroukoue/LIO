using System;

using Android.App;
using Android.OS;

namespace FallDetector.Sources
{

    public class CustomCountDownTimer : CountDownTimer
    {
        private Service service;
        private int count = 0;
        public CustomCountDownTimer(long millisInFuture, long countDownInterval, Service ser)
            : base(millisInFuture, countDownInterval)
        {
            this.service = ser;
        }

        public override void OnFinish()
        {
            Console.WriteLine("OnFinish Timer" + count.ToString());
            
            this.count = 0;
            /*if (((FallDetectorService)service).FreeFallDetected && ((FallDetectorService)service).ImpactDetected && ((FallDetectorService)service).OrientationChanged)
                ((FallDetectorService)service).triggersFallDetected();*/

            ((FallDetectorService)service).resetFallDetection();

        }

        public override void OnTick(long millisUntilFinished)
        {
            this.count++;
        }
    }
}
using System;

namespace FallDetector.Sources
{
    public class TAG
    {
        private static TAG instance;

        public const String notificationTAG = "NOTIFCATION";
        public const String emailTAG = "EMAIL";
        public const String emailAdressTAG = "EMAIL_ADRESS";
        public const String smsTAG = "SMS";
        public const String phoneNumberTAG = "PHONE_NUMBER";
        public const String fallDetectorTAG = "FallDetectorService";
        public const String PrefTAG = "FALL_COUNT";
        public const String onPauseTAG = "ONPAUSE";
        public const String enableFallReportTAG = "ENABLE_FALL_REPORT_SERVICE";

        private TAG() { }

        public static TAG Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TAG();
                }
                return instance;
            }
        }

    }
}
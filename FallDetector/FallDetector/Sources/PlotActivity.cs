using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace FallDetector.Sources
{
    [Activity(Label = "PlotActivity")]
    public class PlotActivity : Activity
    {

        private Button stopPlotButton;

        private LineSeries accelerometerSeries;
        private LineSeries inclinationSeries;
        private LineSeries azimuthOrientationSeries;
        private LineSeries pitchOrientationSeries;
        private LineSeries rollOrientationSeries;
        private LineSeries gyroscopeSeries;

        private PlotView plotView;
        private double startTimestamp = -1;
        public Boolean isBound = false;

        private Boolean plotAccelerometer = false;
        private Boolean plotOrientation = false;
        private Boolean plotInclination = false;

        public Boolean PlotInclination
        {
            get { return plotInclination; }
            set { plotInclination = value; }
        }

        public Boolean PlotAccelerometer
        {
            get { return plotAccelerometer; }
            set { plotAccelerometer = value; }
        }
        

        public Boolean PlotOrientation
        {
            get { return plotOrientation; }
            set { plotOrientation = value; }
        }


        public FallDetectorServiceBinder fallServiceBinder;
        private FallDetectorServiceConnection fallServiceConnection;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.fallServiceConnection = new FallDetectorServiceConnection(this);

            var intent = this.Intent;
            this.plotAccelerometer = intent.GetBooleanExtra("PlotAccelerometer", false);
            this.plotOrientation = intent.GetBooleanExtra("PlotOrientation", false);
            this.plotInclination = intent.GetBooleanExtra("PlotInclination", false);


            // Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.Plot_Layout);

            plotView = new PlotView(this);
            plotView.Clickable = true;


            stopPlotButton = new Button(this);
            stopPlotButton.Text = "Start";


            this.AddContentView(plotView,
                new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            this.AddContentView(stopPlotButton,
                new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            stopPlotButton.Click += delegate
            {
                this.onClick(stopPlotButton);
            };

            //var plotView = this.FindViewById<PlotView>(Resource.Id.plotview);

        }

        private void onClick(View v)
        {
            if (v == stopPlotButton)
            {
                if (isBound)
                {
                    isBound = false;
                    this.fallServiceBinder.activity = null;
                    this.fallServiceBinder = null;
                    //plotView.InvalidatePlot(true);
                    UnbindService(this.fallServiceConnection);

                    stopPlotButton.Text = "Start";
                }
                else
                {
                    accelerometerSeries.Points.Clear();
                    azimuthOrientationSeries.Points.Clear();
                    pitchOrientationSeries.Points.Clear();
                    rollOrientationSeries.Points.Clear();
                    inclinationSeries.Points.Clear();
                    gyroscopeSeries.Points.Clear();

                    plotView.Model.ResetAllAxes();
                    plotView.InvalidatePlot(false);
                    startTimestamp = -1;
                    var fallDetectorIntent = new Intent(this, typeof(FallDetectorService));
                    BindService(fallDetectorIntent, this.fallServiceConnection, Bind.AutoCreate);

                    stopPlotButton.Text = "Stop";
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            

            plotView.Model = null;
            plotView.Model = CreatePlotModel();
            plotView.InvalidatePlot(true);

        }
        protected override void OnPause()
        {
            base.OnPause();
            if (isBound)
            {
                isBound = false;
                UnbindService(this.fallServiceConnection);
            }
                
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private PlotModel CreatePlotModel()
        {

            var plotModel = new PlotModel {};

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "TimeStamp" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, /*Maximum = 5, Minimum = 0,*/});

            accelerometerSeries = new LineSeries
            {
                MarkerType = MarkerType.Square,
                MarkerSize = 4,
                MarkerStrokeThickness = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Black,
                Color = OxyColors.Red,
                Smooth = true,
                Title = "Accelerometer"
            };

            inclinationSeries = new LineSeries
            {
                MarkerType = MarkerType.Square,
                MarkerSize = 4,
                MarkerStrokeThickness = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Black,
                Color = OxyColors.Blue,
                Smooth = true,
                Title = "Inclination"
            };

            azimuthOrientationSeries = new LineSeries
            {
                MarkerType = MarkerType.Square,
                MarkerSize = 4,
                MarkerStrokeThickness = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Black,
                Color = OxyColors.Green,
                Smooth = true,
            };

            pitchOrientationSeries = new LineSeries
            {
                MarkerType = MarkerType.Square,
                MarkerSize = 4,
                MarkerStrokeThickness = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Black,
                Color = OxyColors.Blue,
                Smooth = true,
            };

            rollOrientationSeries = new LineSeries
            {
                MarkerType = MarkerType.Square,
                MarkerSize = 4,
                MarkerStrokeThickness = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Black,
                Color = OxyColors.Red,
                Smooth = true,
            };

            gyroscopeSeries = new LineSeries
            {
                MarkerType = MarkerType.Square,
                MarkerSize = 4,
                MarkerStrokeThickness = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Black,
                Color = OxyColors.Gold,
                Smooth = true,
            };

            plotModel.Series.Add(accelerometerSeries);
            plotModel.Series.Add(azimuthOrientationSeries);
            plotModel.Series.Add(pitchOrientationSeries);
            plotModel.Series.Add(rollOrientationSeries);
            plotModel.Series.Add(inclinationSeries);
            plotModel.Series.Add(gyroscopeSeries);

            return plotModel;
        }

        public void updateAccPlot(double xValue, double yValue)
        {
            if (startTimestamp == -1)
                startTimestamp = xValue;

            RunOnUiThread(() =>
            {
                double tempTimeStamp = xValue - startTimestamp;
                accelerometerSeries.Points.Add(new DataPoint(tempTimeStamp, yValue));
                plotView.Model.InvalidatePlot(true);
            });
        }

        public void updateOrientPlot(double timeStamp, double azimuth, double pitch, double roll)
        {
            if (startTimestamp == -1)
                startTimestamp = timeStamp;

            RunOnUiThread(() =>
            {
                double tempTimeStamp = timeStamp - startTimestamp;
                //azimuthOrientationSeries.Points.Add(new DataPoint(tempTimeStamp, azimuth));
                pitchOrientationSeries.Points.Add(new DataPoint(tempTimeStamp, pitch));
                //rollOrientationSeries.Points.Add(new DataPoint(tempTimeStamp, roll));

                plotView.Model.InvalidatePlot(true);
            });
        }

        public void updateIncliPlot(double xValue, double yValue)
        {
            if (startTimestamp == -1)
                startTimestamp = xValue;

            RunOnUiThread(() =>
            {
                double tempTimeStamp = xValue - startTimestamp;
                inclinationSeries.Points.Add(new DataPoint(tempTimeStamp, yValue));
                plotView.Model.InvalidatePlot(true);
            });
        }

        public void updateGyroscopePlot(double timeStamp, double omegaAmpl)
        {
            if (startTimestamp == -1)
                startTimestamp = timeStamp;

            RunOnUiThread(() =>
            {
                double tempTimeStamp = timeStamp - startTimestamp;
                //gyroscopeSeries.Points.Add(new DataPoint(tempTimeStamp, azimuth));
                gyroscopeSeries.Points.Add(new DataPoint(tempTimeStamp, omegaAmpl));
                //gyroscopeSeries.Points.Add(new DataPoint(tempTimeStamp, roll));

                plotView.Model.InvalidatePlot(true);
            });
        }
    }
}
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
        private LineSeries series1;
        private PlotView plotView;
        private double startTimestamp = -1;
        public Boolean isBound = false;

        public FallDetectorServiceBinder fallServiceBinder;
        private FallDetectorServiceConnection fallServiceConnection;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.Plot_Layout);

            plotView = new PlotView(this);

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
                    plotView.InvalidatePlot(true);
                    UnbindService(this.fallServiceConnection);

                    stopPlotButton.Text = "Start";
                }
                else
                {
                    series1.Points.Clear();
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
            //mSensorManager.RegisterListener(this, mAccelerometer, SensorDelay.Normal);
            this.fallServiceConnection = new FallDetectorServiceConnection(this);

            plotView.Model = null;
            plotView.Model = CreatePlotModel();
            plotView.InvalidatePlot(true);

        }
        protected override void OnPause()
        {
            base.OnPause();
            if(isBound)
                UnbindService(this.fallServiceConnection);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private PlotModel CreatePlotModel()
        {

            var plotModel = new PlotModel { Title = "Accelerometer" };

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Time"});
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 5, Minimum = 0, Title = "Acc" });

            series1 = new LineSeries
            {
                MarkerType = MarkerType.Square,
                MarkerSize = 4,
                MarkerStrokeThickness = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Black,
                Color = OxyColors.Red,
                Smooth = true,
            };

            plotModel.Series.Add(series1);

            return plotModel;
        }

       public void updatePlot(double xValue, double yValue)
        {
            if (startTimestamp == -1)
                startTimestamp = xValue;

            RunOnUiThread(() =>
            {
                double tempTimeStamp = xValue - startTimestamp;
                series1.Points.Add(new DataPoint(tempTimeStamp, yValue));
                plotView.Model.InvalidatePlot(true);
            });
        }
    }
}
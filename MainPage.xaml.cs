using SkiaSharp;
using SkiaSharp.Views.Maui;
using Klepsydra.Resources.Scripts;

namespace Klepsydra
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            StartAccelerometer();
            SetupTimer();
            hourglass = new Hourglass();
        }

        #region Skia UI

        private Hourglass hourglass;

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            hourglass.Draw(canvas, e.Info);
        }

        private void OnTimePassed()
        {
            hourglass.TimePassed();
            canvasView.InvalidateSurface();
        }

        #endregion

        #region Rotation

        private double _lastAngle = 0;
        private double _rotationAngle = 180;
        private double _angleThreshold = 5;

        private bool _startedByButton = false;

        private void StartAccelerometer()
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        }

        private void RotationTimer(AccelerometerChangedEventArgs e)
        {

        } 

        private void ButtonTimer(AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;

            // When Z is close to ±1, phone is lying flat — ignore these cases
            if (Math.Abs(data.Acceleration.Z) > 0.85)
            {
                if (timer.IsRunning)
                    StopTimer(null, null);
                return;
            }
            else if (!timer.IsRunning)
            {
                StopTimer(null, null);
            }

            // Compute angle in degrees from X and Y
            double angle = -Math.Atan2(data.Acceleration.Y, data.Acceleration.X) * (180 / Math.PI) + 90;

            // Smooth and reduce twitching: only update if angle changes significantly
            if (Math.Abs(angle - _lastAngle) < _angleThreshold) // 5 degrees threshold
                return;

            _lastAngle = angle;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                canvasView.Rotation = angle;
            });
        }

        private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            if (!_startedByButton) RotationTimer(e); 
            else ButtonTimer(e);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            }
        }

        #endregion

        #region Timer

        IDispatcherTimer timer;
        double seconds = 10;

        private void SetupTimer()
        {
            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerChange;

            DisableButton(stopButton);
            DisableButton(resetButton);

            timeLeftPanel.IsVisible = false;
        }

        private void ContentPageSizeChanged(object sender, EventArgs e)
        {
            MainGrid.WidthRequest = this.Width;
            MainGrid.HeightRequest = this.Height;
        }

        private void TimeChanged(object sender, TextChangedEventArgs e)
        {
            if(e.NewTextValue.Where(x => !char.IsDigit(x)).Count() > 0)
            {
                inputView.Text = e.OldTextValue;
            }
        }

        private void StartTimer(object sender, EventArgs e)
        {
            EnableButton(resetButton);
            EnableButton(stopButton);
            DisableButton(startButton);

            timeLeftPanel.IsVisible = true;
            SavedTimers.IsVisible = false;

            seconds = int.Parse(inputView.Text);
            timeLeftLabel.Text = $"{seconds} seconds left";
            timer.Start();

            //Skia Part

        }

        private void StopTimer(object ?sender, EventArgs ?e)
        {

            if(timer.IsRunning)
            {
                timer.Stop();
                stopButton.Text = "Resume";
                return;
            }

            timer.Start();
            stopButton.Text = "Stop";

        }

        private void ResetTimer(object sender, EventArgs e)
        {
            EnableButton(startButton);
            DisableButton(resetButton);
            DisableButton(stopButton);

            timeLeftPanel.IsVisible = false;
            SavedTimers.IsVisible = true;

            timer.Stop();
        }

        private void DisableButton(Button b)
        {
            b.IsEnabled = false;
            b.FontFamily = "DotoMedium";
            b.IsVisible = false;
        }

        private void EnableButton(Button b)
        {
            b.IsEnabled = true;
            b.FontFamily = "DotoExtraBold";
            b.IsVisible = true;
        }

        private void TimerChange(object? sender, EventArgs e)
        {
            seconds -= timer.Interval.TotalSeconds;
            if (seconds <= 0)
            {
                DisableButton(stopButton);
                DisableButton(resetButton);
                EnableButton(startButton);

                timeLeftLabel.Text = $"0 seconds left";
                StopTimer(stopButton, new EventArgs());
            }
            else
            {
                timeLeftLabel.Text = $"{Math.Ceiling(seconds)} seconds left";
            }
        }

        #endregion
    }
}

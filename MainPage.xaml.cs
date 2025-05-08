using SkiaSharp;
using SkiaSharp.Views.Maui;
using Klepsydra.Resources.Scripts;
using Plugin.Maui.Audio;

namespace Klepsydra
{
    public partial class MainPage : ContentPage
    {
        Hourglass hourglass;
        IDispatcherTimer timer;
        double seconds = 10;
        double waitingTime = 10;
        private IAudioPlayer player;

        public MainPage()
        {
            InitializeComponent();
            hourglass = new Hourglass();
            SetupTimer();

            InitializeAudio();
            StartAccelerometer();
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var info = e.Info;
            float width = info.Width;
            float height = info.Height;

            // Save the canvas state
            canvas.Save();

            // Center the canvas
            canvas.Translate(width / 2f, height / 2f);

            // Optional: apply rotation if needed (e.g., to simulate hourglass flip)
            // canvas.RotateDegrees(180);

            // Scale hourglass to fit in view, preserving aspect ratio
            float scale = Math.Min(width / 200f, height / 400f); // 200x400 is hourglass base size
            canvas.Scale(scale);

            // Move origin back to top-left of where hourglass should draw
            canvas.Translate(-100, -200); // Half of base hourglass width/height

            // Draw hourglass
            hourglass.Draw(canvas, info);

            canvas.Restore();
        }


        private async void InitializeAudio()
        {
            var audio = AudioManager.Current;
            player = audio.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("Sound.wav"));
        }

        #region Accelerometer

        private double _lastAngle = 0;
        private double _rotation = 0;
        private double _angleThreshold = 5;

        bool _startedByButton = false;

        private void StartAccelerometer()
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        }

        private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            _rotation = e.Reading.Acceleration.Y;

            if (!_startedByButton) RotationTimer(e);
            else ButtonTimer(e);
        }

        private void RotationTimer(AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;

            if (data.Acceleration.Y < -0.8)
            {
                if (timer.IsRunning) return;

                timer.Start();
                cancelButton.IsVisible = false;
                inputHolder.IsVisible = false;
                MainGrid.RotateTo(180, 5, Easing.Default);
            }
            else if (data.Acceleration.Y > 0.8)
            {
                if (player.IsPlaying)
                {
                    ResetTimer(null, null);
                    return;
                }

                if (!timer.IsRunning) return;

                timer.Stop();
                cancelButton.IsVisible = true;
                MainGrid.RotateTo(0, 5, Easing.Default);
            }
        }

        private void ButtonTimer(AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;

            //// When Z is close to ±1, phone is lying flat — ignore these cases
            //if (Math.Abs(data.Acceleration.Z) > 0.97)
            //{
            //    if (timer.IsRunning)
            //        StopTimer(null, null);
            //    return;
            //}
            //else if (!timer.IsRunning)
            //{
            //    StopTimer(null, null);
            //}

            // Compute angle in degrees from X and Y
            double angle = -Math.Atan2(data.Acceleration.Y, data.Acceleration.X) * (180 / Math.PI) + 90;

            // Smooth and reduce twitching: only update if angle changes significantly
            if (Math.Abs(angle - _lastAngle) < _angleThreshold) // 5 degrees threshold
                return;

            _lastAngle = angle;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RotatingClock.Rotation = angle;
            });
        }

        #endregion

        #region Timer

        private void SetupTimer()
        {
            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimeChanged;

            DisableButton(stopButton);
            DisableButton(resetButton);

            timeLeftPanel.IsVisible = false;
            endButton.IsVisible = false;
        }

        private void TimeChanged(object? sender, EventArgs e)
        {
            seconds -= timer.Interval.TotalSeconds;

            // Sand drops each tick
            hourglass.TimePassed((waitingTime - seconds) / waitingTime);

            RotatingClock.InvalidateSurface();
            StartingClock.InvalidateSurface();

            if (seconds <= 0)
            {
                TimesUp();
                timer.Stop();
            }
            else
                timeLeftLabel.Text = $"{Math.Ceiling(seconds)} seconds left";
        }

        private void TimesUp()
        {
            if (player.IsPlaying) return;
            timer.Stop();

            Vibration.Vibrate();


            player.Loop = true;
            player.Play();

            endButton.IsVisible = true;
            timeLeftLabel.Text = "Time's up!";

            if (!_startedByButton)
            {
                endButton.IsEnabled = false;
                endButton.Text = "Rotate device to end";
                return;
            }

            endButton.IsEnabled = true;
            endButton.Text = "Finished!";
        }

        private void StartTimer(object? sender, EventArgs? e)
        {
            _startedByButton = true;

            EnableButton(resetButton);
            EnableButton(stopButton);
            DisableButton(startButton);
            inputView.IsEnabled = false;

            timeLeftPanel.IsVisible = true;
            StartingClock.IsVisible = false;
            endButton.IsVisible = false;

            if (int.TryParse(inputView.Text, out int val))
                seconds = val;
            else
                seconds = 10;

            waitingTime = seconds;
            timeLeftLabel.Text = $"{seconds} seconds left";
            timer.Start();
        }

        private void StopTimer(object? sender, EventArgs? e)
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                stopButton.Text = "Resume";
                return;
            }

            timer.Start();
            stopButton.Text = "Stop";
        }

        private void ResetTimer(object? sender, EventArgs? e)
        {
            if (_rotation < 190 && _rotation > 170 && !_startedByButton)
                return;
            else if (!_startedByButton)
            {
                inputHolder.IsVisible = true;
                MainGrid.RotateTo(0, 5, Easing.Default);
            }

            cancelButton.IsVisible = false;
            Vibration.Cancel();
            player.Stop();
            _startedByButton = false;

            EnableButton(startButton);
            DisableButton(resetButton);
            DisableButton(stopButton);
            inputView.IsEnabled = true;

            timeLeftPanel.IsVisible = false;
            StartingClock.IsVisible = true;
            RotatingClock.InvalidateSurface();

            timer.Stop();
            hourglass = new Hourglass();
            StartingClock.InvalidateSurface();

            endButton.IsVisible = false;
            stopButton.Text = "Stop";
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

        private void ContentPageSizeChanged(object sender, EventArgs e)
        {
            MainGrid.WidthRequest = this.Width;
            MainGrid.HeightRequest = this.Height;
        }

        private void InputChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.Where(x => !char.IsDigit(x)).Any())
            {
                inputView.Text = e.OldTextValue;
            }
        }

        #endregion
    }
}

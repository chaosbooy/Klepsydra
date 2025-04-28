using SkiaSharp;

namespace Klepsydra.Resources.Scripts
{
    public class Hourglass
    {
        private List<Particle> particles = new();
        private int fallenCount = 0;
        private readonly int totalParticles;
        private readonly int particleSize;
        private readonly float hourglassWidth;
        private readonly float topY = 100;
        private readonly float bottomY = 700;
        private readonly float centerY = 400;

        public Hourglass(int particleCount = 100, int particleSize = 5, float hourglassWidth = 160)
        {
            this.totalParticles = particleCount;
            this.particleSize = particleSize;
            this.hourglassWidth = hourglassWidth;
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            var rnd = new Random();
            particles.Clear();

            for (int i = 0; i < totalParticles; i++)
            {
                float y = topY + (float)(rnd.NextDouble() * (centerY - topY));
                float progress = (y - topY) / (centerY - topY);
                float xRange = (1 - progress) * (hourglassWidth / 2);
                float x = 200 + (float)(rnd.NextDouble() * xRange * 2 - xRange);

                particles.Add(new Particle { X = x, Y = y, IsFallen = false });
            }
        }

        public void TimePassed()
        {
            if (fallenCount < totalParticles)
            {
                particles[fallenCount].IsFallen = true;
                fallenCount++;
            }
        }

        public void Draw(SKCanvas canvas, SKImageInfo info)
        {
            float centerX = info.Width / 2;

            // Draw outline
            using var outlinePaint = new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 4,
                IsStroke = true
            };

            canvas.DrawLine(centerX - hourglassWidth / 2, topY, centerX + hourglassWidth / 2, topY, outlinePaint);
            canvas.DrawLine(centerX - hourglassWidth / 2, topY, centerX, centerY, outlinePaint);
            canvas.DrawLine(centerX + hourglassWidth / 2, topY, centerX, centerY, outlinePaint);
            canvas.DrawLine(centerX - hourglassWidth / 2, bottomY, centerX + hourglassWidth / 2, bottomY, outlinePaint);
            canvas.DrawLine(centerX - hourglassWidth / 2, bottomY, centerX, centerY, outlinePaint);
            canvas.DrawLine(centerX + hourglassWidth / 2, bottomY, centerX, centerY, outlinePaint);

            // Draw particles
            using var particlePaint = new SKPaint
            {
                Color = SKColors.Goldenrod,
                IsAntialias = true
            };

            int stacked = 0;
            foreach (var p in particles)
            {
                float x = p.X;
                float y = p.Y;

                if (p.IsFallen)
                {
                    int row = stacked / 10;
                    int col = stacked % 10;
                    float spacing = particleSize + 1;
                    x = centerX - 45 + col * spacing;
                    y = bottomY - spacing - row * spacing;
                    stacked++;
                }

                canvas.DrawRect(x - particleSize / 2, y - particleSize / 2, particleSize, particleSize, particlePaint);
            }
        }

        private class Particle
        {
            public float X { get; set; }
            public float Y { get; set; }
            public bool IsFallen { get; set; }
        }
    }
}

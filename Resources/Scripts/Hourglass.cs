using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Klepsydra.Resources.Scripts
{
    public class Hourglass
    {
        private List<Particle> particles = new();
        private int fallenCount = 0;
        private readonly int totalParticles;
        private readonly int particleSize;
        private readonly float hourglassWidth;
        private readonly float hourglassHeight;
        private readonly float topOffset;
        private readonly float leftOffset;

        private SKPoint[] hourglassBase;
        private readonly SKPaint[] sandPaints =
        {
            new SKPaint { Color = SKColors.Blue, IsAntialias = true },
            new SKPaint { Color = SKColors.Turquoise, IsAntialias = true },
            new SKPaint { Color = SKColors.Purple, IsAntialias = true },
            new SKPaint { Color = SKColors.LightBlue, IsAntialias = true }
        };

        public Hourglass(int particleCount = 300, int particleSize = 6, float hourglassWidth = 160, float hourglassHeight = 320)
        {
            this.totalParticles = particleCount;
            this.particleSize = particleSize;
            this.hourglassWidth = hourglassWidth;
            this.hourglassHeight = hourglassHeight;
            topOffset = 5;
            leftOffset = 5;

            InitializeParticles();
            InitializeHourglass();
        }

        private void InitializeHourglass()
        {
            hourglassBase = new SKPoint[]
            {
                new SKPoint(leftOffset, topOffset),
                new SKPoint(leftOffset + hourglassWidth, topOffset),
                new SKPoint(leftOffset + hourglassWidth, topOffset + hourglassHeight),
                new SKPoint(leftOffset, topOffset + hourglassHeight),
                new SKPoint(leftOffset, topOffset)
            };
        }

        private void InitializeParticles()
        {
            var rnd = new Random();
            particles.Clear();

            for (int i = 0; i < totalParticles; i++)
            {
                float y = topOffset + (float)rnd.Next((int)(hourglassHeight / 2));
                float progress = (y - topOffset) / (hourglassHeight / 2);
                float xRange = (1 - progress) * (hourglassWidth / 2);
                float x = leftOffset + progress * hourglassWidth / 2 + (float)rnd.Next((int)(xRange * 2));

                particles.Add(new Particle { X = x, Y = y, IsFalling = false });
            }
        }

        public void TimePassed(double percentageFallen)
        {
            int targetFallen = (int)(percentageFallen * totalParticles);

            if (targetFallen > fallenCount)
            {
                // Forward in time: make more particles fall
                while (fallenCount < targetFallen && fallenCount < totalParticles)
                {
                    particles[fallenCount].IsFalling = true;
                    fallenCount++;
                }
            }
            else if (targetFallen < fallenCount)
            {
                // Rewind time: restore particles back to top
                while (fallenCount > targetFallen && fallenCount > 0)
                {
                    fallenCount--;
                    particles[fallenCount].IsFalling = false;
                    ResetParticleToTop(particles[fallenCount]);
                }
            }
        }

        private void ResetParticleToTop(Particle p)
        {
            var rnd = new Random();
            float y = topOffset + (float)rnd.Next((int)(hourglassHeight / 2));
            float progress = (y - topOffset) / (hourglassHeight / 2);
            float xRange = (1 - progress) * (hourglassWidth / 2);
            float x = leftOffset + progress * hourglassWidth / 2 + (float)rnd.Next((int)(xRange * 2));

            p.X = x;
            p.Y = y;
            p.IsFalling = false;
        }



        public void Draw(SKCanvas canvas, SKImageInfo info)
        {
            canvas.Clear(SKColors.Transparent);

            using var outlinePaint = new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 4,
                IsStroke = true
            };

            using var woodPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 10,
                IsStroke = true
            };

            Random rnd = new Random();
            float halfHeight = hourglassHeight / 2;
            float gravity = 2.0f; // Stronger = denser pile at the bottom

            for (int i = 0; i < particles.Count; i++)
            {
                var p = particles[i];

                if (p.IsFalling)
                {
                    float progress = i / (float)totalParticles;
                    float yOffset = 1f - MathF.Pow((float)rnd.NextDouble(), gravity);
                    p.Y = halfHeight + topOffset + yOffset * halfHeight;


                    float pileProgress = (p.Y - topOffset - halfHeight) / halfHeight;
                    float xRange = pileProgress * (hourglassWidth / 2);
                    p.X = leftOffset + (1 - pileProgress) * hourglassWidth / 2 + (float)rnd.Next((int)(xRange * 2));

                    p.IsFalling = false;
                }

                canvas.DrawRect(
                    p.X - particleSize / 2,
                    p.Y - particleSize / 2,
                    particleSize - particleSize / 2 + (float)rnd.NextDouble() * particleSize,
                    particleSize - particleSize / 2 + (float)rnd.NextDouble() * particleSize,
                    sandPaints[rnd.Next(sandPaints.Length)]
                );
            }

            canvas.DrawPoints(SKPointMode.Polygon, hourglassBase, outlinePaint);
            canvas.DrawLine(new SKPoint(0, topOffset), new SKPoint(hourglassWidth + 2 * leftOffset, topOffset), woodPaint);
            canvas.DrawLine(new SKPoint(0, topOffset + hourglassHeight), new SKPoint(hourglassWidth + 2 * leftOffset, topOffset + hourglassHeight), woodPaint);
        }

        private class Particle
        {
            public float X { get; set; }
            public float Y { get; set; }
            public bool IsFalling { get; set; }
        }
    }
}

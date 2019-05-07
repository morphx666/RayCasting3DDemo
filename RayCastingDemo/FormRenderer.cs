using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayCastingDemo {
    public partial class FormRenderer : Form {
        private readonly Particle camera;
        private readonly List<Vector> walls;
        private readonly List<Particle> lights;

        public FormRenderer(Particle viewer, List<Vector> walls, List<Particle> lights) {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);

            this.camera = viewer;
            this.walls = walls;
            this.lights = lights;

            this.Paint += RenderScene;
        }

        private void RenderScene(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.Clear(Color.Black);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            lock(camera) {
                if(camera.Rays.Count == 0) return;

                Rectangle r = this.DisplayRectangle;
                double x;
                double y;
                double p;
                int alpha;
                double rw = Math.Ceiling((double)r.Width / camera.Rays.Count);
                double maxM = Vector.Distance(r.X, r.Y, r.Width, r.Height);

                for(int i = 0; i < camera.Rays.Count; i++) {
                    //p = rays[i].Magnitude * Math.Cos((rays[i].Angle - viewer.Angle) * Vector.ToRad);
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * Math.Cos(camera.Angle * Vector.ToRad) + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * Math.Sin(camera.Angle * Vector.ToRad);
                    //p = rays[i].Magnitude;

                    x = ((double)i * r.Width) / camera.Rays.Count;
                    y = Math.Min(100.0 * camera.ViewDistance / p, r.Height);

                    //double ad = 255.0 * (y * 0.8) / r.Height; // 'y' factor is ambient light.
                    double ad = 40000.0 / p;
                    foreach(Particle l in lights) {
                        foreach(Vector lr in l.Rays) {
                            if(Vector.Distance(lr.Destination, camera.Rays[i].Destination) < 3.0 * rw) {
                                ad += 58.0 * camera.ViewDistance / lr.Magnitude;
                                break;
                            }
                        }
                    }
                    alpha = Math.Max(Math.Min((int)ad, 255), 0);

                    using(SolidBrush b = new SolidBrush(Color.FromArgb(alpha, camera.Rays[i].Color))) {
                        g.FillRectangle(b, x, (r.Height - y) / 2.0, rw * camera.ViewDistance / p, y);
                    }
                }
            }
        }
    }
}

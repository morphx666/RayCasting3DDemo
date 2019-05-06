using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayCastingDemo {
    public partial class FormRenderer : Form {
        private readonly Viewer viewer;
        private readonly List<Vector> walls;
        private readonly List<Vector> rays;

        public FormRenderer(Viewer viewer, List<Vector> walls, List<Vector> rays) {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);

            this.viewer = viewer;
            this.walls = walls;
            this.rays = rays;

            this.Paint += RenderScene;
        }

        private void RenderScene(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.Clear(Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            lock(viewer) {
                if(rays.Count == 0) return;

                Rectangle r = this.DisplayRectangle;
                double x;
                double y;
                double p;
                int alpha;
                double rw = Math.Ceiling((double)r.Width / rays.Count);

                //double minA = rays.Min((ray) => ray.Angle);
                //double maxA = rays.Max((ray) => ray.Angle);
                //double minM = 0;
                double maxM = Vector.Distance(r.X, r.Y, r.Width, r.Height);

                for(int i = 0; i < rays.Count; i++) {
                    //p = rays[i].Magnitude * Math.Cos((rays[i].Angle - viewer.Angle) * Vector.ToRad);
                    p = (rays[i].X2 - rays[i].X1) * Math.Cos(viewer.Angle * Vector.ToRad) + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (rays[i].Y2 - rays[i].Y1) * Math.Sin(viewer.Angle * Vector.ToRad);
                    //p = rays[i].Magnitude;

                    x = ((double)i * r.Width) / rays.Count;
                    y = Math.Min(100.0 * viewer.ViewDistance / p, r.Height);

                    //alpha = (int)Scale(p, 0, maxM, 255, 0);
                    //alpha = Math.Min(Math.Max(0, alpha), 255);
                    alpha = Math.Min((int)((255.0 * (y * 1.75)) / r.Height), 255); // 'y' factor is brightness.

                    using(SolidBrush b = new SolidBrush(Color.FromArgb(alpha, rays[i].Color))) {
                        g.FillRectangle(b, x, (r.Height - y) / 2.0, rw, y);
                    }
                }
            }
        }

        private double Scale(double v, double vMin, double vMax, double rMin, double rMax) {
            return ((v - vMin) / (vMax - vMin)) * (rMax - rMin) + rMin;
        }
    }
}

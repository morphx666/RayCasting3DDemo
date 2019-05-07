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
    public partial class Form3DRenderer : Form {
        public enum RenderModes {
            Shaded,
            Textured
        }

        private readonly Particle camera;
        private readonly List<Vector> walls;
        private readonly List<Particle> lights;
        private Size wbSize = new Size(Properties.Resources.WallBmp.Width, Properties.Resources.WallBmp.Height);

        public RenderModes RenderMode { get; set; } = RenderModes.Shaded;

        public Form3DRenderer(Particle viewer, List<Vector> walls, List<Particle> lights) {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.camera = viewer;
            this.walls = walls;
            this.lights = lights;


            this.Paint += (object sender, PaintEventArgs e) => {
                switch(RenderMode) {
                    case RenderModes.Shaded: RenderScene(e); break;
                    case RenderModes.Textured: RenderSceneTextured(e); break;
                }
            };
        }

        private void RenderScene(PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.Clear(Color.Black);

            lock(camera) {
                if(camera.Rays.Count == 0) return;

                Rectangle r = this.DisplayRectangle;
                double x;
                double y;
                double p;
                double rw = (double)r.Width / camera.Rays.Count;

                for(int i = 0; i < camera.Rays.Count; i++) {
                    //p = camera.Rays[i].Magnitude * Math.Cos(camera.Rays[i].AngleRad - camera.AngleRad);
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * camera.AngleRadCos + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * camera.AngleRadSin;
                    //p = camera.Rays[i].Magnitude; // Fish eye effect

                    x = ((double)i * r.Width) / camera.Rays.Count;
                    y = Math.Min((r.Height / 32.0) * camera.ViewDistance / p, r.Height);

                    double ad = 4000.0 / p;
                    foreach(Particle l in lights) {
                        foreach(Vector lr in l.Rays) {
                            if(Vector.Distance(lr.Destination, camera.Rays[i].Destination) < 3.0 * rw) {
                                ad += 58.0 * camera.ViewDistance / lr.Magnitude;
                                break;
                            }
                        }
                    }

                    using(SolidBrush b = new SolidBrush(Color.FromArgb(Math.Max(Math.Min((int)ad, 255), 0), camera.Rays[i].Color))) {
                        g.FillRectangle(b, x, (r.Height - y) / 2.0, rw, y);
                    }
                }
            }
        }

        private void RenderSceneTextured(PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.Clear(Color.Black);

            lock(camera) {
                if(camera.Rays.Count == 0) return;

                RectangleF src;
                RectangleF trg;

                Rectangle r = this.DisplayRectangle;
                double x;
                double y;
                double p;
                double rw = (double)r.Width / camera.Rays.Count;
                Vector w;
                double bmpOffset = 0;

                for(int i = 0; i < camera.Rays.Count; i++) {
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * camera.AngleRadCos + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * camera.AngleRadSin;

                    x = ((double)i * r.Width) / camera.Rays.Count;
                    y = Math.Min((r.Height / 32.0) * camera.ViewDistance / p, r.Height);

                    w = (Vector)camera.Rays[i].Tag;
                    bmpOffset = (double)w.Tag;

                    src = new RectangleF((float)bmpOffset, 0, (float)rw, wbSize.Height);
                    trg = new RectangleF((float)x, (float)((r.Height - y) / 2.0), (float)rw, (float)y);
                    g.DrawImage(Properties.Resources.WallBmp, trg, src, GraphicsUnit.Pixel);
                    w.Tag = (bmpOffset + rw) % wbSize.Width;
                }
            }
        }
    }
}

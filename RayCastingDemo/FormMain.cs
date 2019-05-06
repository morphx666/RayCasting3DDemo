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
    public partial class FormMain : Form {
        private readonly List<Vector> walls = new List<Vector>();
        private readonly List<Vector> rays = new List<Vector>();
        bool isDragging;
        Point mouseDownOrigin;
        Viewer viewer;
        FormRenderer renderer;

        public enum Movements {
            None = 0b00000000,
            Forward = 0b00000001,
            Back = 0b00000010,
            Left = 0b00000100,
            Right = 0b00001000,
            LookLeft = 0b00010000,
            LookRight = 0b00100000
        }
        private Movements movement = Movements.None;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);

            viewer = new Viewer(this.DisplayRectangle, 200, 520) {
                FOV = 70,
                Magnitude = 200,
                Angle = 295.0
            };

            walls.CreateRectangle(Color.Gray, 0, 0, this.DisplayRectangle.Width, this.DisplayRectangle.Height);
            walls.CreateRectangle(Color.Blue, 600, 130, 30, 200);
            walls.CreateRectangle(Color.Red, 80, 80, 200, 150);
            walls.CreateRectangle(Color.Green, 300, 200, 100, 80);

            renderer = new FormRenderer(viewer, walls, rays);
            renderer.Size = this.Size;
            renderer.Show();
            if(Screen.AllScreens.Count() > 1) {
                Rectangle r = Screen.AllScreens[1].Bounds;
                renderer.Location = new Point(r.X + (r.Width - renderer.Width) / 2,
                                              r.Y + (r.Height - renderer.Height) / 2);
            }

            Task.Run(() => {
                while(true) {
                    Thread.Sleep(60);
                    if(movement != Movements.None) ProcessKeys();
                }
            });

            UpdateRays();

            this.Paint += RenderScene;
            this.MouseDown += (object s, MouseEventArgs e) => {
                if(e.Button == MouseButtons.Left) {
                    isDragging = true;
                    viewer.X1 = e.Location.X;
                    viewer.Y1 = e.Location.Y;
                    UpdateRays();
                    mouseDownOrigin = e.Location;
                }
            };
            this.MouseUp += (object s, MouseEventArgs e) => isDragging = (e.Button == MouseButtons.Left);
            this.MouseMove += (object s, MouseEventArgs e) => {
                if(isDragging) {
                    viewer.X1 += e.Location.X - mouseDownOrigin.X;
                    viewer.Y1 += e.Location.Y - mouseDownOrigin.Y;
                    UpdateRays();
                    mouseDownOrigin = e.Location;
                }
            };

            this.KeyDown += (object s, KeyEventArgs e) => HandleKeyDown(e);
            renderer.KeyDown += (object s, KeyEventArgs e) => HandleKeyDown(e);
            this.KeyUp += (object s, KeyEventArgs e) => HandleKeyUp(e);
            renderer.KeyUp += (object s, KeyEventArgs e) => HandleKeyUp(e);
        }

        private void HandleKeyDown(KeyEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Up: movement |= Movements.Forward; break;
                case Keys.Down: movement |= Movements.Back; break;
                case Keys.Left:
                    if((e.Modifiers & Keys.Control) == Keys.Control) {
                        movement |= Movements.LookLeft;
                    } else {
                        movement |= Movements.Left;
                    }
                    break;
                case Keys.Right:
                    if((e.Modifiers & Keys.Control) == Keys.Control) {
                        movement |= Movements.LookRight;
                    } else {
                        movement |= Movements.Right;
                    }
                    break;
            }
        }

        private void HandleKeyUp(KeyEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Up: movement ^= Movements.Forward; break;
                case Keys.Down: movement ^= Movements.Back; break;
                case Keys.Left:
                    if((e.Modifiers & Keys.Control) == Keys.Control) {
                        movement ^= Movements.LookLeft;
                    } else {
                        movement ^= Movements.Left;
                    }
                    break;
                case Keys.Right:
                    if((e.Modifiers & Keys.Control) == Keys.Control) {
                        movement ^= Movements.LookRight;
                    } else {
                        movement ^= Movements.Right;
                    }
                    break;
            }
        }

        private void ProcessKeys() {
            const double s = 4.0;
            if((movement & Movements.Forward) == Movements.Forward) viewer.Move(s);
            if((movement & Movements.Back) == Movements.Back) viewer.Move(-s);
            if((movement & Movements.Left) == Movements.Left) viewer.Angle -= s;
            if((movement & Movements.Right) == Movements.Right) viewer.Angle += s;
            if((movement & Movements.LookLeft) == Movements.LookLeft) {
                viewer.Angle -= 90;
                viewer.Move(s);
                viewer.Angle += 90;
            }
            if((movement & Movements.LookRight) == Movements.LookRight) {
                viewer.Angle += 90;
                viewer.Move(s);
                viewer.Angle -= 90;
            }
            UpdateRays();
        }

        private void UpdateRays() {
            Vector ray;
            Vector minV;
            double minD;
            double d;

            lock(viewer) {
                rays.Clear();

                double a1 = viewer.Angle - viewer.FOV / 2;
                double a2 = viewer.Angle + viewer.FOV / 2;
                double s = 0.25 * Math.Sign(a2 - a1);

                for(double a = a1; a < a2; a += s) {
                    ray = new Vector(1.0, a, viewer.Origin);

                    minV = new Vector();
                    minD = double.PositiveInfinity;

                    foreach(Vector w in walls) {
                        PointF? pi = w.Intersects(ray);
                        if(pi.HasValue) {
                            d = Vector.Distance(ray.Origin, pi.Value);
                            if(d < minD) {
                                minD = d;
                                minV = ray;
                                minV.Color = w.Color;
                            }
                        }
                    }

                    if(minD != double.PositiveInfinity) {
                        minV.Magnitude = minD;
                        rays.Add(minV);
                    }
                }
            }
            if(this.WindowState == FormWindowState.Minimized) {
                renderer.Invalidate();
            } else {
                this.Invalidate();
            }
        }

        private void RenderScene(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            lock(viewer) {
                g.Clear(Color.Black);

                foreach(Vector r in rays) r.Paint(g, Gradient(r.Color, Color.Black, r.Origin, r.Destination));
                foreach(Vector w in walls) w.Paint(g, w.Color);

                g.FillEllipse(Brushes.White, viewer.X1 - 8, viewer.Y1 - 8, 16, 16);

                viewer.Paint(g, Color.Magenta);

                renderer.Invalidate();
            }
        }

        private Pen Gradient(Color c1, Color c2, PointF p1, PointF p2) {
            Brush b = new LinearGradientBrush(p1, p2, c1, c2);
            return new Pen(b);
        }
    }
}
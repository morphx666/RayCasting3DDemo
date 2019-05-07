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
        private readonly List<Particle> lights = new List<Particle>();

        private bool isDragging;
        private Point mouseDownOrigin;
        private const double moveMentSpeed = 3.0;

        private readonly Particle camera;
        private readonly FormRenderer renderer;
        private bool lightsOn = true;

        private readonly Color lightRayColor = Color.FromArgb(128, Color.White);

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

            camera = new Particle(this.DisplayRectangle, 200, 520) {
                FOV = 75,
                Magnitude = 200.0,
                Angle = 295.0
            };

            lights.Add(new Particle(this.DisplayRectangle, 20, 500) {
                FOV = 90,
                Magnitude = 1200.0,
                Angle = 340.0
            });

            walls.CreateRectangle(Color.Gray, 0, 0, this.DisplayRectangle.Width, this.DisplayRectangle.Height);
            walls.CreateRectangle(Color.Blue, 600, 130, 30, 200);
            walls.CreateRectangle(Color.Red, 80, 80, 200, 150);
            walls.CreateRectangle(Color.Green, 300, 200, 100, 80);

            renderer = new FormRenderer(camera, walls, lights) { Size = this.Size };
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

            UpdateObjects();

            this.Paint += RenderScene;
            this.MouseDown += (object s, MouseEventArgs e) => {
                if(e.Button == MouseButtons.Left) {
                    isDragging = true;
                    camera.X1 = e.Location.X;
                    camera.Y1 = e.Location.Y;
                    UpdateObjects();
                    mouseDownOrigin = e.Location;
                }
            };
            this.MouseUp += (object s, MouseEventArgs e) => isDragging = !(e.Button == MouseButtons.Left);
            this.MouseMove += (object s, MouseEventArgs e) => {
                if(isDragging) {
                    camera.X1 += e.Location.X - mouseDownOrigin.X;
                    camera.Y1 += e.Location.Y - mouseDownOrigin.Y;
                    UpdateObjects();
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
                case Keys.L:
                    lightsOn = !lightsOn;
                    UpdateObjects();
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
            if((movement & Movements.Forward) == Movements.Forward) camera.Move(moveMentSpeed);
            if((movement & Movements.Back) == Movements.Back) camera.Move(-moveMentSpeed);
            if((movement & Movements.Left) == Movements.Left) camera.Angle -= moveMentSpeed;
            if((movement & Movements.Right) == Movements.Right) camera.Angle += moveMentSpeed;
            if((movement & Movements.LookLeft) == Movements.LookLeft) {
                camera.Angle -= 90;
                camera.Move(moveMentSpeed);
                camera.Angle += 90;
            }
            if((movement & Movements.LookRight) == Movements.LookRight) {
                camera.Angle += 90;
                camera.Move(moveMentSpeed);
                camera.Angle -= 90;
            }
            UpdateObjects();
        }

        private void UpdateObjects() {
            lock(camera) {
                camera.UpdateRays(walls);
                if(lightsOn) {
                    lights.ForEach((l) => l.UpdateRays(walls));
                } else {
                    lights.ForEach((l) => l.Rays.Clear());
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

            g.Clear(Color.Black);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            lock(camera) {
                foreach(Vector l in lights[0].Rays) l.Paint(g, Gradient(lightRayColor, Color.Black, l));
                foreach(Vector r in camera.Rays) r.Paint(g, Gradient(r.Color, Color.Black, r));
                foreach(Vector w in walls) w.Paint(g, w.Color);

                g.FillEllipse(Brushes.White, camera.X1 - 8, camera.Y1 - 8, 16, 16);

                camera.Paint(g, Color.Magenta);
                if(lightsOn) lights.ForEach((l) => l.Paint(g, Color.White));

                renderer.Invalidate();
            }
        }

        private Pen Gradient(Color c1, Color c2, Vector v) {
            Vector v1 = new Vector(v);
            v1.Magnitude = Vector.Distance(0, 0, this.DisplayRectangle.Width, this.DisplayRectangle.Height);
            Brush b = new LinearGradientBrush(v.Origin, v1.Destination, c1, c2);
            return new Pen(b);
        }
    }
}
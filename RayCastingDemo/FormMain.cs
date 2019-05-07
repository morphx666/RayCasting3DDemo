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

        private readonly int[][] map = { // 50x50 grid
            new int[] {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            new int[] {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,1,1,1,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[] {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };

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

            camera = new Particle(this.DisplayRectangle, this.DisplayRectangle.Width / 2, this.DisplayRectangle.Height + 16) {
                FOV = 80,
                Magnitude = 200.0,
                Angle = 270.0
            };

            //lights.Add(new Particle(this.DisplayRectangle, 20, 500) {
            //    FOV = 90,
            //    Magnitude = 1200.0,
            //    Angle = 340.0
            //});

            BuildMap();

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
            if((movement & Movements.Forward) == Movements.Forward) MoveCheckCollision(moveMentSpeed);
            if((movement & Movements.Back) == Movements.Back) MoveCheckCollision(-moveMentSpeed);
            if((movement & Movements.Left) == Movements.Left) camera.Angle -= moveMentSpeed;
            if((movement & Movements.Right) == Movements.Right) camera.Angle += moveMentSpeed;
            if((movement & Movements.LookLeft) == Movements.LookLeft) {
                camera.Angle -= 90;
                MoveCheckCollision(moveMentSpeed);
                camera.Angle += 90;
            }
            if((movement & Movements.LookRight) == Movements.LookRight) {
                camera.Angle += 90;
                MoveCheckCollision(moveMentSpeed);
                camera.Angle -= 90;
            }
            UpdateObjects();
        }

        private bool MoveCheckCollision(double amt) {
            camera.Move(amt);
            for(int i = 0; i < walls.Count; i += 4) {
                if(IsInsideRect(
                    walls[i + 0].X1, walls[i + 0].Y1,
                    walls[i + 2].X1, walls[i + 1].Y1,
                    walls[i + 2].X1, walls[i + 3].Y1,
                    walls[i + 3].X1, walls[i + 3].Y1,
                    camera.X1, camera.Y1)) {
                    camera.Move(-amt);
                    return false;
                }
            }
            return true;
        }

        private bool IsInsideRect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, double x, double y) {
            // Calculate area of rectangle ABCD  
            double A = Area(x1, y1, x2, y2, x3, y3) +
                       Area(x1, y1, x4, y4, x3, y3);

            // Calculate Area of triangle PAB  
            double A1 = Area(x, y, x1, y1, x2, y2);

            // Calculate Area of triangle PBC  
            double A2 = Area(x, y, x2, y2, x3, y3);

            // Calculate Area of triangle PCD  
            double A3 = Area(x, y, x3, y3, x4, y4);

            // Calculate Area of triangle PAD 
            double A4 = Area(x, y, x1, y1, x4, y4);

            // Check if sum of A1, A2, A3   
            // and A4 is the same as A  
            return (A == A1 + A2 + A3 + A4);
        }

        private double Area(double x1, double y1, double x2, double y2, double x3, double y3) {
            return Math.Abs((x1 * (y2 - y3) +
                             x2 * (y3 - y1) +
                             x3 * (y1 - y2)) / 2.0);
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
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            lock(camera) {
                foreach(Particle l in lights) foreach(Vector r in l.Rays) r.Paint(g, Gradient(lightRayColor, Color.Black, r));
                foreach(Vector r in camera.Rays) r.Paint(g, Gradient(r.Color, Color.Black, r));
                foreach(Vector w in walls) w.Paint(g, w.Color, 1);

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

        private void BuildMap() {
            Size cellSize = new Size(16, 16);

            for(int y = 0; y < map.Length; y++) {
                int fx = 0;
                int xn = 0;
                for(int x = 0; x < map.Length; x++) {
                    if(map[y][x] != 0) {
                        if(xn == 0) fx = 0;
                        xn += 1;
                    } else {
                        bool isOnBorder = !((x > 0) && (x < map.Length) && (y > 0) && (y < map.Length));
                        bool hasNeiborgh = !isOnBorder && ((map[y - 1][x - 1] != 0) || (map[y + 1][x - 1] != 0) || (map[y][x - 2] != 0) || (map[y][x] != 0));
                        if((xn > 1) || ((xn == 1) && !hasNeiborgh)) walls.CreateRectangle((x - xn) * cellSize.Width, y * cellSize.Height, xn * cellSize.Width, cellSize.Height);
                        xn = 0;
                    }
                }
                if(xn > 1) walls.CreateRectangle(fx * cellSize.Width, y * cellSize.Height, xn * cellSize.Width, cellSize.Height);
            }

            for(int x = 0; x < map.Length; x++) {
                int fy = 0;
                int yn = 0;
                for(int y = 0; y < map.Length; y++) {
                    if(map[y][x] != 0) {
                        if(yn == 0) fy = y;
                        yn += 1;
                    } else {
                        if(yn > 1) walls.CreateRectangle(x * cellSize.Width, (y - yn) * cellSize.Height, cellSize.Width, yn * cellSize.Height);
                        yn = 0;
                    }
                }
                if(yn > 1) walls.CreateRectangle(x * cellSize.Width, fy * cellSize.Height, cellSize.Width, yn * cellSize.Height);
            }

            int xs = 1 + this.Size.Width - this.DisplayRectangle.Width;
            int ys = 1 + this.Size.Height - this.DisplayRectangle.Height;
            this.Size = new Size(cellSize.Width * map.Length + xs, cellSize.Height * map[0].Length + ys);
        }
    }
}
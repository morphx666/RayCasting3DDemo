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
    public partial class Form2DMain : Form {
        private readonly List<Vector> walls = new List<Vector>();
        private readonly List<Particle> lights = new List<Particle>();

        private bool isDragging;
        private Point mouseDownOrigin;
        private const double moveMentSpeed = 3.0;

        private readonly Particle camera;
        private readonly Form3DRenderer renderer;
        private bool lightsOn = true;

        private readonly Color lightRayColor = Color.FromArgb(128, Color.White);

        private Color[] palette = new Color[10];

        private readonly int[][] map = { // 50x50 grid
            new int[] {2,3,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,3,0,0,0,2},
            new int[] {2,0,0,2,0,0,2,6,6,6,6,6,6,6,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,0,0,0,0,0,2,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,3,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,5,5,5,5,5,5,5,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,3,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,5,0,0,0,0,0,0,0,0,0,5,0,0,0,3,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,5,0,0,0,0,0,0,0,5,0,0,0,3,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,5,0,0,0,3,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,0,3,3,0,0,0,0,0,5,0,0,3,0,0,0,0,0,3,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,3,0,0,0,0,5,0,0,0,3,0,0,0,0,0,3,0,0,2},
            new int[] {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0,0,3,0,0,0,5,0,0,0,0,3,0,0,0,0,0,3,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,3,0,0,0,5,0,0,0,0,0,3,0,0,0,0,0,3,2},
            new int[] {2,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,3,2},
            new int[] {2,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,2,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,3,0,0,0,9,0,9,0,0,0,0,0,3,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,2,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,3,0,0,0,0,9,0,0,0,0,0,0,3,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,2,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,3,0,0,0,9,0,9,0,0,0,0,0,3,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,2,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,2,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,2,2,2,2,2,0,0,0,0,0,0,0,2,0,0,0,0,2,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,2,0,0,0,3,0,0,0,3,3,3,3,3,3,3,3,3,3,3,0,0,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            new int[] {2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            new int[] {2,0,0,2,2,2,2,2,2,2,2,2,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7,7,7,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,2,0,0,0,3,0,0,0,0,0,0,0,0,0,2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,6,6,0,0,0,0,2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,2,0,0,0,0,0,0,0,6,6,0,0,0,0,2,0,0,2,0,0,0,2,2,2,2,2,2,2,2,2,0,0,0,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,2,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,2,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,2,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,2,0,0,0,0,0,0,0,0,2,2,2,2,2,2,0,0,2,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,2,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,2,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            new int[] {2,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            new int[] {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2}
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

        public Form2DMain() {
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

            renderer = new Form3DRenderer(camera, walls, lights) {
                Size = this.Size,
                RenderMode = Form3DRenderer.RenderModes.Shaded
            };
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
                MoveCheckCollision(moveMentSpeed * 1.2);
                camera.Angle += 90;
            }
            if((movement & Movements.LookRight) == Movements.LookRight) {
                camera.Angle += 90;
                MoveCheckCollision(moveMentSpeed * 1.2);
                camera.Angle -= 90;
            }
            UpdateObjects();
        }

        private bool MoveCheckCollision(double amt) {
            camera.Move(amt);
            for(int i = 0; i < walls.Count; i += 4) {
                if(ExtensionMethods.IsInsideRect(
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
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            lock(camera) {
                foreach(Particle l in lights) foreach(Vector r in l.Rays) r.Paint(g, Gradient(lightRayColor, Color.Black, r));
                foreach(Vector r in camera.Rays) r.Paint(g, Gradient(r.Color, Color.Black, r));
                //foreach(Vector w in walls) w.Paint(g, w.Color, 1);
                for(int i = 0; i < walls.Count; i += 4) {
                    using(Brush b = new SolidBrush(walls[i].Color)) {
                        g.FillRectangle(b, walls[i + 0].X1, walls[i + 0].Y1,
                                           walls[i + 2].X1 - walls[i + 0].X1, walls[i + 3].Y1 - walls[i + 0].Y1);
                    }
                }

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

        private void BuildMap() { // FIXME: This code is broken!
            palette[0] = Color.Black;
            palette[1] = Color.Gray;
            palette[2] = Color.White;
            palette[3] = Color.Red;
            palette[4] = Color.MistyRose;
            palette[5] = Color.Brown;
            palette[6] = Color.SandyBrown;
            palette[7] = Color.Orange;
            palette[8] = Color.Yellow;
            palette[9] = Color.Green;

            Size cellSize = new Size(16, 16);

            int palIdx = 0;
            for(int y = 0; y < map.Length; y++) {
                int fx = 0;
                int xn = 0;
                for(int x = 0; x < map.Length; x++) {
                    if(map[y][x] != 0) {
                        if(xn == 0) {
                            fx = 0;
                            palIdx = map[y][x];
                        }
                        xn += 1;
                    } else {
                        bool isOnBorder = !((x > 0) && (x < map.Length) && (y > 0) && (y < map.Length));
                        bool hasNeiborgh = !isOnBorder && ((map[y - 1][x - 1] != 0) || (map[y + 1][x - 1] != 0) || (map[y][x - 2] != 0) || (map[y][x] != 0));
                        if((xn > 1) || ((xn == 1) && !hasNeiborgh)) walls.CreateRectangle(palette[palIdx], (x - xn) * cellSize.Width, y * cellSize.Height, xn * cellSize.Width, cellSize.Height);
                        xn = 0;
                    }
                }
                if(xn > 1) walls.CreateRectangle(palette[palIdx], fx * cellSize.Width, y * cellSize.Height, xn * cellSize.Width, cellSize.Height);
            }

            for(int x = 0; x < map.Length; x++) {
                int fy = 0;
                int yn = 0;
                for(int y = 0; y < map.Length; y++) {
                    if(map[y][x] != 0) {
                        if(yn == 0) {
                            fy = y;
                            palIdx = map[y][x];
                        }
                        yn += 1;
                    } else {
                        if(yn > 1) walls.CreateRectangle(palette[palIdx], x * cellSize.Width, (y - yn) * cellSize.Height, cellSize.Width, yn * cellSize.Height);
                        yn = 0;
                    }
                }
                if(yn > 1) walls.CreateRectangle(palette[palIdx], x * cellSize.Width, fy * cellSize.Height, cellSize.Width, yn * cellSize.Height);
            }

            int xs = 1 + this.Size.Width - this.DisplayRectangle.Width;
            int ys = 1 + this.Size.Height - this.DisplayRectangle.Height;
            this.Size = new Size(cellSize.Width * map.Length + xs, cellSize.Height * map[0].Length + ys);
        }
    }
}
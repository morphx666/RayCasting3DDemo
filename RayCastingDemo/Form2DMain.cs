using RayCasting;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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

        private Renderer renderer;
        private readonly Particle camera;
        private readonly Form3DRenderer frm3D;
        private bool lightsOn = true;

        private readonly Color lightRayColor = Color.FromArgb(128, Color.White);

        private readonly Color[] palette = new Color[10];

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

            lights.Add(new Particle(this.DisplayRectangle, this.DisplayRectangle.Width - 30, 700) {
                FOV = 90,
                Magnitude = 1200.0,
                Angle = 260.0
            });

            renderer = new Renderer(camera, walls, lights);

            BuildMap();

            frm3D = new Form3DRenderer(camera, walls, lights, renderer) {
                Size = this.Size,
                RenderMode = Form3DRenderer.RenderModes.Shaded
            };
            frm3D.Show();
            if(Screen.AllScreens.Count() > 1) {
                Rectangle r = Screen.AllScreens[1].Bounds;
                frm3D.Location = new Point(r.X + (r.Width - frm3D.Width) / 2,
                                           r.Y + (r.Height - frm3D.Height) / 2);
            }

            Task.Run(() => { // Keyboard monitor thread
                while(true) {
                    Thread.Sleep(45);
                    if(movement != Movements.None) ProcessKeys();
                }
            });

            Task.Run(() => { // Renderer thread
                bool lastLightsOn = lightsOn;
                while(true) {
                    Thread.Sleep(33);
                    if((movement != Movements.None) || isDragging || (lastLightsOn != lightsOn)) {
                        if(this.WindowState == FormWindowState.Minimized) {
                            frm3D.Invalidate();
                        } else {
                            this.Invalidate();
                        }
                        lastLightsOn = lightsOn;
                    }
                }
            });

            UpdateObjects();

            this.Paint += (object s, PaintEventArgs e) => {
                renderer.Render2DMap(e.Graphics, this.DisplayRectangle, lightsOn, lightRayColor);
                frm3D.Invalidate();
            };

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
            frm3D.KeyDown += (object s, KeyEventArgs e) => HandleKeyDown(e);
            this.KeyUp += (object s, KeyEventArgs e) => HandleKeyUp(e);
            frm3D.KeyUp += (object s, KeyEventArgs e) => HandleKeyUp(e);
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
                    if(!lightsOn) lights.ForEach((l) => l.Rays.Clear());
                    UpdateObjects();
                    frm3D.UpdateTitleBarText();
                    break;
                case Keys.T:
                    switch(frm3D.RenderMode) {
                        case Form3DRenderer.RenderModes.Shaded:
                            frm3D.RenderMode = Form3DRenderer.RenderModes.Textured;
                            break;
                        case Form3DRenderer.RenderModes.Textured:
                            frm3D.RenderMode = Form3DRenderer.RenderModes.TexturedAndColorized;
                            break;
                        case Form3DRenderer.RenderModes.TexturedAndColorized:
                            frm3D.RenderMode = Form3DRenderer.RenderModes.Shaded;
                            break;
                    }
                    frm3D.UpdateTitleBarText();
                    this.Invalidate();
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
                camera.UpdateRays(walls, 0.25);
                if(lightsOn) lights.ForEach((l) => l.UpdateRays(walls));
            }
        }

        private void BuildMap() { // FIXME: This code is broken! 😒
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

            //walls.Clear();
            //walls.CreateRectangle(40, 300, 400, 100);
            //walls.CreateRectangle(0, 0, 10, this.DisplayRectangle.Height);
        }
    }
}
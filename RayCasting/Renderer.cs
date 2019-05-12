using OpenSimplexNoiseSample;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace RayCasting {
    public class Renderer {
        private readonly Particle camera;
        private readonly List<Vector> walls;
        private readonly List<Particle> lights;

        public Renderer(Particle camera, List<Vector> walls, List<Particle> lights) {
            this.camera = camera;
            this.walls = walls;
            this.lights = lights;
        }

        public void Render2DMap(Graphics g, Rectangle r, bool showLights, Color lightRayColor) {
            g.Clear(Color.Black);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            lock(camera) {
                lights.ForEach((light) => light.Rays.ForEach((ray) => ray.Paint(g, Gradient(lightRayColor, Color.Black, ray, r))));
                camera.Rays.ForEach((ray) => ray.Paint(g, Gradient(ray.Color, Color.Black, ray, r)));
                //walls.ForEach((w) => w.Paint(g, w.Color, 1)); // Outline
                for(int i = 0; i < walls.Count; i += 4) { // Filled
                    using(Brush b = new SolidBrush(walls[i].Color)) {
                        g.FillRectangle(b, walls[i + 0].X1, walls[i + 0].Y1,
                                           walls[i + 2].X1 - walls[i + 0].X1, walls[i + 3].Y1 - walls[i + 0].Y1);
                    }
                }

                g.FillEllipse(Brushes.White, camera.X1 - 8, camera.Y1 - 8, 16, 16);

                camera.Paint(g, Color.Magenta);
                if(showLights) lights.ForEach((l) => l.Paint(g, Color.White));
            }
        }

        public void Render3DMapShaded(Graphics g, Rectangle r) {
            g.Clear(Color.Black);

            lock(camera) {
                if(camera.Rays.Count == 0) return;

                double x;
                double y;
                double p;
                double rw = (double)r.Width / camera.Rays.Count;
                int a;

                for(int i = 0; i < camera.Rays.Count; i++) {
                    //p = camera.Rays[i].Magnitude * Math.Cos(camera.Rays[i].AngleRad - camera.AngleRad);
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * camera.AngleRadCos + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * camera.AngleRadSin;
                    //p = camera.Rays[i].Magnitude; // Fish eye effect

                    x = i * rw;
                    y = Math.Min((r.Height / 28.0) * camera.ViewDistance / p, r.Height);
                    a = Math.Max(Math.Min((int)CalculateAlpha(p, rw, i), 255), 0);

                    using(SolidBrush b = new SolidBrush(Color.FromArgb(a, camera.Rays[i].Color))) {
                        g.FillRectangle(b, x, (r.Height - y) / 2.0, rw, y);
                    }
                }
            }
        }

        public void Render3DMapTextured(Graphics g, Rectangle r, Bitmap texture) {
            g.Clear(Color.Black);

            lock(camera) {
                if(camera.Rays.Count == 0) return;

                RectangleF src;
                RectangleF trg;

                double x;
                double y;
                double p;
                double rw = (double)r.Width / camera.Rays.Count;
                int a;

                int wallIndex = 0;
                int wallSideIndex;
                int sideIndex;
                int lastWallIndex = -1;
                double bmpOffset = 0;
                double lastBmpOffset = 0;

                for(int i = 0; i < camera.Rays.Count; i++) {
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * camera.AngleRadCos + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * camera.AngleRadSin;

                    x = i * rw;
                    y = Math.Min((r.Height / 28.0) * camera.ViewDistance / p, r.Height);

                    wallSideIndex = (int)camera.Rays[i].Tag;
                    sideIndex = wallSideIndex % 4;
                    wallIndex = wallSideIndex - sideIndex;
                    switch(sideIndex) {
                        case 0: // Top
                            bmpOffset = camera.Rays[i].X2;
                            break;
                        case 3: // Bottom
                            bmpOffset = camera.Rays[i].X2;
                            break;
                        case 1: // Left
                            bmpOffset = camera.Rays[i].Y2;
                            break;
                        case 2: // Right
                            bmpOffset = camera.Rays[i].Y2;
                            break;
                    }
                    while(bmpOffset <= 0) bmpOffset += texture.Width;
                    bmpOffset %= (texture.Width - rw);

                    if(wallIndex != lastWallIndex) {
                        lastWallIndex = wallIndex;
                        lastBmpOffset = 0;
                    }

                    src = new RectangleF((float)bmpOffset, 0, (float)Math.Abs(lastBmpOffset - bmpOffset), texture.Height);
                    trg = new RectangleF((float)x, (float)((r.Height - y) / 2.0), (float)rw, (float)y);
                    a = Math.Max(Math.Min((int)CalculateAlpha(p, rw, i), 255), 0);

                    g.DrawImage(texture, trg, src, GraphicsUnit.Pixel);

                    lastBmpOffset = bmpOffset;
                }
            }
        }

        public void Render3DMapTextured(Graphics g, Rectangle r, Bitmap texture, double colorize) {
            g.Clear(Color.Black);

            lock(camera) {
                if(camera.Rays.Count == 0) return;

                RectangleF src;
                RectangleF trg;
                DirectBitmap ctext = new DirectBitmap(texture);
                DirectBitmap scene = new DirectBitmap(r.Width, r.Height);
                Graphics sg = Graphics.FromImage(scene.Bitmap);
                sg.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 0, 0)), r);

                double x;
                double y;
                double p;
                double rw = (double)r.Width / camera.Rays.Count;
                int a;

                int wallIndex = 0;
                int wallSideIndex;
                int sideIndex;
                int lastWallIndex = -1;
                double bmpOffset = 0;
                double lastBmpOffset = 0;

                for(int i = 0; i < camera.Rays.Count; i++) {
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * camera.AngleRadCos + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * camera.AngleRadSin;

                    x = i * rw;
                    y = Math.Min((r.Height / 28.0) * camera.ViewDistance / p, r.Height);

                    wallSideIndex = (int)camera.Rays[i].Tag;
                    sideIndex = wallSideIndex % 4;
                    wallIndex = wallSideIndex - sideIndex;
                    switch(sideIndex) {
                        case 0: // Top
                            bmpOffset = camera.Rays[i].X2;
                            break;
                        case 3: // Bottom
                            bmpOffset = camera.Rays[i].X2;
                            break;
                        case 1: // Left
                            bmpOffset = camera.Rays[i].Y2;
                            break;
                        case 2: // Right
                            bmpOffset = camera.Rays[i].Y2;
                            break;
                    }
                    while(bmpOffset <= 0) bmpOffset += texture.Width;
                    bmpOffset %= (texture.Width - rw);

                    if(wallIndex != lastWallIndex) {
                        lastWallIndex = wallIndex;
                        lastBmpOffset = 0;
                    }

                    src = new RectangleF((float)bmpOffset, 0, (float)Math.Abs(lastBmpOffset - bmpOffset), texture.Height);
                    trg = new RectangleF((float)x, (float)((r.Height - y) / 2.0), (float)rw, (float)y);
                    a = Math.Max(Math.Min((int)CalculateAlpha(p, rw, i), 255), 0);

                    sg.DrawImage(ctext.Bitmap, trg, src, GraphicsUnit.Pixel);
                    ColorizeBitmap(scene, Color.FromArgb(a, camera.Rays[i].Color), trg, colorize);

                    lastBmpOffset = bmpOffset;
                }

                g.DrawImageUnscaled(scene.Bitmap, 0, 0);
                ctext.Dispose();
                sg.Dispose();
                scene.Dispose();
            }
        }

        private double CalculateAlpha(double p, double rw, int i) {
            double ad = 6000.0 / p;
            foreach(Particle l in lights) {
                foreach(Vector lr in l.Rays) {
                    if(Vector.Distance(lr.Destination, camera.Rays[i].Destination) < 6.0 * rw) {
                        ad += 58.0 * camera.ViewDistance / lr.Magnitude;
                        break;
                    }
                }
            }

            return ad;
        }

        private void ColorizeBitmap(DirectBitmap scene, Color color, RectangleF r, double p) {
            Color c;
            double p1 = 1.0 - p;
            for(int y = (int)r.Y; y < r.Bottom; y++) {
                for(int x = (int)r.X; x < r.Right; x++) {
                    c = scene.GetPixel(x, y);
                    scene.SetPixel(x, y, Color.FromArgb(
                        (int)(p * (c.R * color.A / 255) + color.R * p1),
                        (int)(p * (c.G * color.A / 255) + color.G * p1),
                        (int)(p * (c.B * color.A / 255) + color.B * p1)));

                    //scene.SetPixel(x, y, Color.FromArgb(
                    //    (255 - (255 - c.A) * (255 - color.A) / 255),
                    //    (c.R * (255 - color.A) + color.R * color.A) / 255,
                    //    (c.G * (255 - color.A) + color.G * color.A) / 255,
                    //    (c.B * (255 - color.A) + color.B * color.A) / 255));

                    //scene.SetPixel(x, y,
                    //    Color.FromArgb(
                    //        (int)(c.A * p + color.A * p1),
                    //        (int)(c.R * p + color.R * p1),
                    //        (int)(c.G * p + color.G * p1),
                    //        (int)(c.B * p + color.B * p1)));
                }
            }
        }

        private Pen Gradient(Color c1, Color c2, Vector v, Rectangle r) {
            Vector v1 = new Vector(v) {
                Magnitude = Vector.Distance(0, 0, r.Width, r.Height)
            };
            return new Pen(new LinearGradientBrush(v.Origin, v1.Destination, c1, c2));
        }
    }
}

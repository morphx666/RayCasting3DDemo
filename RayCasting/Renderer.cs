using System;
using System.Collections.Generic;
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

                for(int i = 0; i < camera.Rays.Count; i++) {
                    //p = camera.Rays[i].Magnitude * Math.Cos(camera.Rays[i].AngleRad - camera.AngleRad);
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * camera.AngleRadCos + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * camera.AngleRadSin;
                    //p = camera.Rays[i].Magnitude; // Fish eye effect

                    x = ((double)i * r.Width) / camera.Rays.Count;
                    y = Math.Min((r.Height / 28.0) * camera.ViewDistance / p, r.Height);

                    double ad = 6000.0 / p;
                    foreach(Particle l in lights) {
                        foreach(Vector lr in l.Rays) {
                            if(Vector.Distance(lr.Destination, camera.Rays[i].Destination) < 6.0 * rw) {
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

                int wallIndex = 0;
                int wallSideIndex;
                int sideIndex;
                double bmpOffset = 0;

                for(int i = 0; i < camera.Rays.Count; i++) {
                    p = (camera.Rays[i].X2 - camera.Rays[i].X1) * camera.AngleRadCos + // https://youtu.be/eOCQfxRQ2pY?t=606
                        (camera.Rays[i].Y2 - camera.Rays[i].Y1) * camera.AngleRadSin;

                    x = ((double)i * r.Width) / camera.Rays.Count;
                    y = Math.Min((r.Height / 28.0) * camera.ViewDistance / p, r.Height);

                    wallSideIndex = (int)camera.Rays[i].Tag;
                    sideIndex = wallSideIndex % 4;
                    wallIndex = wallSideIndex - sideIndex;

                    // TODO: Detect Start and finish of the all and draw the bitmap accordingly, instead of drawing it in 'rw' steps

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

                    src = new RectangleF((float)bmpOffset, 0, (float)(rw), texture.Height);
                    trg = new RectangleF((float)x, (float)((r.Height - y) / 2.0), (float)rw, (float)y);
                    g.DrawImage(texture, trg, src, GraphicsUnit.Pixel);
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

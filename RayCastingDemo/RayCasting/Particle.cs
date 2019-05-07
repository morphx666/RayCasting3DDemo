using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCastingDemo {
    public class Particle : Vector {
        private readonly List<Vector> mRays = new List<Vector>();
        private double mFOV = 170.0;
        private double mViewDistance = 0;

        private RectangleF area;

        public List<Vector> Rays { get { return mRays; } }

        public double FOV {
            get { return mFOV; }
            set {
                mFOV = value;
                UpdateViewDistance();
            }
        }

        public double ViewDistance {
            get { return mViewDistance; }
        }

        public Particle(RectangleF area, double x, double y) : base(1.0, 0.0, x, y) {
            this.area = area;
            UpdateViewDistance();
        }

        private void UpdateViewDistance() {
            mViewDistance = (area.Width / 2.0) / Math.Tan((mFOV * Vector.ToRad) / 2.0);
        }

        public void UpdateRays(List<Vector> walls) {
            Vector ray;
            Vector minV;
            double minD;
            double d;

            mRays.Clear();

            double a1 = Angle - FOV / 2;
            double a2 = Angle + FOV / 2;
            double s = 0.25 * Math.Sign(a2 - a1);

            for(double a = a1; a < a2; a += s) {
                ray = new Vector(1.0, a, Origin);

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
                            w.Tag = 0.0; // bmpOffset
                            minV.Tag = w;
                        }
                    }
                }

                if(minD != double.PositiveInfinity) {
                    minV.Magnitude = minD;
                    mRays.Add(minV);

                }
            }
        }

        public override void Paint(Graphics g, Color c, int w = 2) {
            double a = Angle;
            using(Pen p = new Pen(c, w)) {
                g.DrawLine(p, X1, Y1, X2, Y2);

                Angle = a - FOV / 2;
                g.DrawLine(p, X1, Y1, X2, Y2);

                Angle = a + FOV / 2;
                g.DrawLine(p, X1, Y1, X2, Y2);

                Angle = a;
            }
        }
    }
}

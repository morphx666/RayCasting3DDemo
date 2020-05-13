using MorphxLibs;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RayCasting {
    public class Particle : Vector {
        private readonly List<Vector> mRays = new List<Vector>();
        private double mFOV = 170.0 * Constants.ToRad;
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
            mViewDistance = (area.Width / 2.0) / Math.Tan(mFOV / 2.0);
        }

        public void UpdateRays(List<Vector> walls, double precission = Constants.PI90 / 240) {
            Vector ray;
            Vector minV;
            double minD;
            double d;

            mRays.Clear();

            double a1 = Angle - FOV / 2.0;
            double a2 = Angle + FOV / 2.0;
            double s = precission * Math.Sign(a2 - a1);

            for(double a = a1; a < a2; a += s) {
                ray = new Vector(1.0, a, Origin);

                minV = new Vector();
                minD = double.PositiveInfinity;

                for(int i = 0; i < walls.Count; i++) {
                    Vector w = walls[i];
                    PointF? pi = w.Intersects(ray);
                    if(pi.HasValue) {
                        d = Vector.Distance(ray.Origin, pi.Value);
                        if(d < minD) {
                            minD = d;
                            minV = ray;
                            minV.Color = w.Color;
                            minV.Tag = i; // Wall ID
                        }
                    }
                }

                if(minD != double.PositiveInfinity) {
                    minV.Magnitude = minD;
                    mRays.Add(minV);
                }
            }
        }

        public override void Paint(Graphics g, Color c, float w = 2, double scale = 1.0) {
            double a = Angle;
            using(Pen p = new Pen(c, (float)w)) {
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
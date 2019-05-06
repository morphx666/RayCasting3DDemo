using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCastingDemo {
    public class Viewer : Vector {
        private double mFOV = 170.0;
        private double mViewDistance = 0;

        private RectangleF area;

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

        public Viewer(RectangleF area, double x, double y) : base(1.0, 0.0, x, y) {
            this.area = area;
            UpdateViewDistance();
        }

        private void UpdateViewDistance() {
            mViewDistance = (area.Width / 2.0) / Math.Tan((mFOV * Vector.ToRad) / 2.0);
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

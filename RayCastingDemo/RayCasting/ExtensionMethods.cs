using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCastingDemo {
    public static class ExtensionMethods {
        public static void DrawLine(this Graphics g, Pen p, double x1, double y1, double x2, double y2) {
            g.DrawLine(p, (float)x1, (float)y1, (float)x2, (float)y2);
        }

        public static void FillRectangle(this Graphics g, Brush b, double x, double y, double width, double height) {
            g.FillRectangle(b, (float)x, (float)y, (float)width, (float)height);
        }

        public static void DrawEllipse(this Graphics g, Pen p, double x, double y, double width, double height) {
            g.DrawEllipse(p, (float)x, (float)y, (float)width, (float)height);
        }

        public static void FillEllipse(this Graphics g, Brush b, double x, double y, double width, double height) {
            g.FillEllipse(b, (float)x, (float)y, (float)width, (float)height);
        }

        public static void CreateRectangle(this List<Vector> v, double x, double y, double width, double height) {
            v.Add(Vector.VectorFromPoints(x, y, x + width, y)); // Top
            v.Add(Vector.VectorFromPoints(x, y, x, y + height)); // Left

            v.Add(Vector.VectorFromPoints(x + width, y, x + width, y + height)); // Right
            v.Add(Vector.VectorFromPoints(x, y + height, x + width, y + height)); // Bottom
        }

        public static void CreateRectangle(this List<Vector> v, Color c, double x, double y, double width, double height) {
            v.Add(Vector.VectorFromPoints(x, y, x + width, y, c)); // 0:Top
            v.Add(Vector.VectorFromPoints(x, y, x, y + height, c)); // 1:Left

            v.Add(Vector.VectorFromPoints(x + width, y, x + width, y + height, c)); // 2:Right
            v.Add(Vector.VectorFromPoints(x, y + height, x + width, y + height, c)); // 3:Bottom
        }
    }
}

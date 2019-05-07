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

        public static void DrawImage(this Graphics g, Bitmap bmp, double x, double y, double width, double height) {
            g.DrawImage(Properties.Resources.WallBmp, (float)x, (float)y, (float)width, (float)height);
        }

        public static void CreateRectangle(this List<Vector> v, double x, double y, double width, double height) {
            v.Add(Vector.VectorFromPoints(x, y, x + width, y)); // 0:Top
            v.Add(Vector.VectorFromPoints(x, y, x, y + height)); // 1:Left

            v.Add(Vector.VectorFromPoints(x + width, y, x + width, y + height)); // 2:Right
            v.Add(Vector.VectorFromPoints(x, y + height, x + width, y + height)); // 3:Bottom
        }

        public static void CreateRectangle(this List<Vector> v, Color c, double x, double y, double width, double height) {
            v.Add(Vector.VectorFromPoints(x, y, x + width, y, c)); // 0:Top
            v.Add(Vector.VectorFromPoints(x, y, x, y + height, c)); // 1:Left

            v.Add(Vector.VectorFromPoints(x + width, y, x + width, y + height, c)); // 2:Right
            v.Add(Vector.VectorFromPoints(x, y + height, x + width, y + height, c)); // 3:Bottom
        }

        // https://www.geeksforgeeks.org/check-whether-given-point-lies-inside-rectangle-not/
        public static bool IsInsideRect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, double x, double y) {
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

        private static double Area(double x1, double y1, double x2, double y2, double x3, double y3) {
            return Math.Abs((x1 * (y2 - y3) +
                             x2 * (y3 - y1) +
                             x3 * (y1 - y2)) / 2.0);
        }
    }
}

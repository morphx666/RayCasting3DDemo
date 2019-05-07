using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayCastingDemo {
    public class Vector {
        public const double ToRad = Math.PI / 180.0;
        public const double ToDeg = 180.0 / Math.PI;

        private double mMagnitude;
        private double mAngle;
        private PointF mOrigin;
        private double angleCos = 1.0;
        private double angleSin = 0.0;
        private Color mColor = Color.White;

        private double angleRad;
        private readonly int hc = Guid.NewGuid().GetHashCode();

        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;

        public object Tag { get; set; }

        public Vector() {
            mAngle = 0.0;
            mMagnitude = 1.0;
            mOrigin = new PointF(0, 0);
        }

        public Vector(double magnitude, double angle, PointF origin) {
            mMagnitude = Math.Abs(magnitude);
            Angle = angle;
            if(magnitude < 0) Angle += 180.0;
            mOrigin = origin;
        }

        public Vector(PointF origin, PointF destination) {
            ResetVectorFromPoints(origin.X, origin.Y, destination.X, destination.Y);
        }

        public Vector(PointF origin, PointF destination, Color color) : this(origin, destination) {
            mColor = color;
        }

        public Vector(Vector vector) : this(vector.Magnitude, vector.Angle, vector.Origin) {
            mColor = vector.Color;
        }

        public Vector(double magnitude, double angle, double x, double y) : this(magnitude, angle, new PointF((float)x, (float)y)) {
        }

        public double Magnitude {
            get { return mMagnitude; }
            set {
                mMagnitude = Math.Abs(value);
                if(value < 0) Angle += 180.0;
            }
        }

        public double Angle {
            get { return mAngle; }
            set {
                if(value != mAngle) {
                    if(value < 0) value += 360.0;
                    mAngle = value % 360.0;

                    angleRad = mAngle * ToRad;
                    angleCos = Math.Cos(angleRad);
                    angleSin = Math.Sin(angleRad);
                }
            }
        }

        public double AngleRad { get { return angleRad; } }

        public double AngleRadCos { get { return angleCos; } }
        public double AngleRadSin { get { return angleSin; } }

        public PointF Origin {
            get { return mOrigin; }
            set {
                if(mOrigin != value) {
                    mOrigin = value;
                    OnChanged();
                }
            }
        }

        public PointF Destination {
            get { return new PointF((float)X2, (float)Y2); }
            set { ResetVectorFromPoints(mOrigin.X, mOrigin.Y, value.X, value.Y); }
        }

        public double X1 {
            get { return mOrigin.X; }
            set { ResetVectorFromPoints(value, mOrigin.Y, X2, Y2); }
        }

        public double Y1 {
            get { return mOrigin.Y; }
            set { ResetVectorFromPoints(mOrigin.X, value, X2, Y2); }
        }

        public double X2 {
            get { return mOrigin.X + mMagnitude * angleCos; }
            set { ResetVectorFromPoints(mOrigin.X, mOrigin.Y, value, Y2); }
        }

        public double Y2 {
            get { return mOrigin.Y + mMagnitude * angleSin; }
            set { ResetVectorFromPoints(mOrigin.X, mOrigin.Y, X2, value); }
        }

        public double Slope {
            get { return (Y2 - Y1) / (X2 - X1); }
        }

        public Color Color {
            get { return mColor; }
            set { mColor = value; }
        }

        public void Move(double offset) {
            Vector ov = new Vector(offset, mAngle, mOrigin);
            Origin = ov.Destination;
        }

        public void Translate(double x, double y) {
            PointF dp = Destination;
            mOrigin = new PointF((float)(mOrigin.X + x), (float)(mOrigin.Y + y));
            Destination = new PointF((float)(dp.X + x), (float)(dp.Y + y));
        }

        public void Transform(double angle, PointF p) {
            angle *= ToRad;
            double dx = X1 - p.X;
            double dy = Y1 - p.Y;
            double d = Vector.Distance(dx, dy);
            double a = Math.Atan2(dy, dx) + angle;
            double xp1 = p.X + d * Math.Cos(a);
            double yp1 = p.Y + d * Math.Sin(a);
            dx = X2 - p.X;
            dy = Y2 - p.Y;
            d = Vector.Distance(dx, dy);
            a = Math.Atan2(dy, dx) + angle;
            ResetVectorFromPoints(xp1, yp1, p.X + d * Math.Cos(a), p.Y + d * Math.Sin(a));
        }

        public PointF? Intersects(Vector v) {
            double d = (X1 - X2) * (v.Y1 - v.Y2) - (Y1 - Y2) * (v.X1 - v.X2);
            double t = ((X1 - v.X1) * (v.Y1 - v.Y2) - (Y1 - v.Y2) * (v.X1 - v.X2)) / d;
            double u = -((X1 - X2) * (Y1 - v.Y1) - (Y1 - Y2) * (X1 - v.X1)) / d;

            if((t >= 0.0) && (t <= 1.0) && (u >= 0)) {
                PointF p = new PointF((float)(X1 + t * (X2 - X1)), (float)(Y1 + t * (Y2 - Y1)));
                return p;
            } else {
                return null;
            }
        }

        private void ResetVectorFromPoints(double px1, double py1, double px2, double py2) {
            Vector v = Vector.VectorFromPoints(px1, py1, px2, py2);
            mMagnitude = v.Magnitude;
            mOrigin = v.Origin;
            Angle = v.Angle;
        }

        public static Vector VectorFromPoints(double px1, double py1, double px2, double py2) {
            Vector v = new Vector();
            double dx = px2 - px1;
            double dy = py2 - py1;

            v.Angle = Math.Atan2(dy, dx) * ToDeg;
            v.Magnitude = Vector.Distance(dx, dy);
            v.Origin = new PointF((float)px1, (float)py1);

            return v;
        }

        public static Vector VectorFromPoints(double px1, double py1, double px2, double py2, Color c) {
            Vector v = Vector.VectorFromPoints(px1, py1, px2, py2);
            v.Color = c;
            return v;
        }

        public static Vector Normalize(PointF p1, PointF p2) {
            Vector v = Vector.VectorFromPoints(p1.X, p1.Y, p2.X, p2.Y);
            v.Magnitude = 1.0;
            return v;
        }

        public static bool operator ==(Vector v1, Vector v2) {
            return (v1.Angle == v2.Angle) && (v1.Magnitude == v2.Magnitude);
        }

        public static bool operator !=(Vector v1, Vector v2) {
            return !(v1 == v2);
        }

        public static Vector operator +(Vector v1, Vector v2) {
            Vector v3 = new Vector(v2) { Origin = v1.Destination };
            return new Vector(v1.Origin, v3.Destination);
        }

        public static Vector operator -(Vector v1, Vector v2) {
            Vector v3 = new Vector(v2) { Origin = v1.Origin };
            return new Vector(v3.Destination, v1.Destination);
        }

        public static Vector operator *(Vector v1, double s) {
            return new Vector(v1.Magnitude * s, v1.Angle, v1.Origin);
        }

        public static Vector operator *(double s, Vector v1) {
            return v1 * s;
        }

        public static Vector operator /(Vector v1, double s) {
            return v1 / s;
        }

        public static double Pow(Vector v1, double power) {
            if(power == 2.0) return Vector.Dot(v1, v1);
            return Math.Pow(v1.Magnitude, power);
        }

        public static double Dot(Vector v1, Vector v2) {
            double a = Math.Abs(v1.Angle - v2.Angle);
            if(a > 180.0) a = 360.0 - a;
            return v1.Magnitude * v2.Magnitude * Math.Cos(a * ToRad);
        }

        public static double Cross(Vector v1, Vector v2) {
            double rx = v1.X2 - v1.X1;
            double ry = v1.Y2 - v1.Y1;
            double tx = v2.X2 - v2.X1;
            double ty = v2.Y2 - v2.Y1;
            return rx * ty - ry * tx;
        }

        public static Vector Cross(Vector v1, double s) {
            return new Vector(v1.Origin, new PointF((float)(v1.X1 + -s * (v1.Y2 - v1.Y1)), (float)(v1.Y1 + s * (v1.X2 - v1.X1))));
        }

        public static Vector Cross(double s, Vector v1) {
            return Cross(v1, s);
        }

        public static double Distance(double x1, double y1, double x2, double y2) {
            return Distance(x2 - x1, y2 - y1);
        }

        public static double Distance(double dx, double dy) {
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double Distance(PointF p1, PointF p2) {
            return Distance(p1.X, p1.Y, p2.X, p2.Y);
        }

        public virtual void Paint(Graphics g, Color c, int w = 2) {
            using(Pen p = new Pen(c, w)) {
                Paint(g, p);
            }
        }

        public virtual void Paint(Graphics g, Pen p) {
            if(mMagnitude == 0.0) return;

            g.DrawLine(p, X1, Y1, X2, Y2);
        }

        private void OnChanged() {
            Changed?.Invoke(this, new EventArgs());
        }

        public override string ToString() {
            return string.Format("Magnitude: {0:F2}{8}Angle: {1:F2}{8}({2:F2}, {3:F2})-({4:F2}, {5:F2}){8}y = {6:F2}x + {7:F2}",
                                mMagnitude,
                                mAngle,
                                X1, Y1, X2, Y2,
                                Slope, X1,
                                " " + Environment.NewLine
                            );
        }

        public override bool Equals(object obj) {
            return this == (Vector)obj;
        }

        public override int GetHashCode() {
            return hc;
        }
    }
}

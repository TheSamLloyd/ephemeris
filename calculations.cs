using System;
namespace ephemeris
{
    public struct Angle
    {
        double measure;
        string units;
        public Angle(double measure, string units)
        {
            this.measure = measure;
            this.units = units;
            this.normalize();
        }
        public Boolean isDegrees() => (this.units.ToLower() == "degrees");

        public void normalize()
        {
            if (this.isDegrees())
            {
                this.measure = this.measure % 360;
                if (this.measure > 180)
                {
                    this.measure = this.measure - 360;
                }
            }
            else
            {
                this.measure = this.measure % (2 * Math.PI);
                if (this.measure > Math.PI)
                {
                    this.measure = this.measure - (Math.PI * 2);
                }
            }
        }
        public static Angle difference(Angle ang1, Angle ang2)
        {
            Boolean outputDeg = ang1.isDegrees();
            ang1.ToRadians();
            ang2.ToRadians();
            double outputMeasure = Math.Min(Math.Abs(ang1.measure - ang2.measure), Math.Abs(2 * Math.PI - (ang1.measure - ang2.measure)));
            Angle output = new Angle(outputMeasure, "radians");
            output.normalize();
            if (outputDeg)
            {
                output.ToDegrees();
            }
            return output;
        }
        public static double ToRadians(double deg) => deg / 180 * Math.PI;
        public static double ToDegrees(double rad) => rad / Math.PI * 180;
        public void ToDegrees()
        {
            if (!this.isDegrees())
            {
                this.units = "degrees";
                this.measure = ToDegrees(this.measure);
            }
        }
        public void ToRadians()
        {
            if (this.isDegrees())
            {
                this.units = "radians";
                this.measure = ToRadians(this.measure);
            }
        }
        public double Measure()
        {
            if (this.isDegrees())
            {
                return Angle.ToRadians(this.measure);
            }
            else
            {
                return this.measure;
            }
        }
    }
    public struct Body
    {
        string Name;

        public string getName() => this.Name;
        public Body(string Name)
        {
            this.Name = Name;
        }
        public Coordinate getSphericalVel(DateTime time, Ephemeris eph) => eph.SphericalVel(this, time);

        public Coordinate getSpherical(DateTime time, Ephemeris eph) => eph.geoSpherical(this, time);
    }
    public class Coordinate
    {
        public double x, y, z;

        public double rho;
        public Angle theta;
        public Angle phi;

        public double[] Triple;
        string type;
        public Boolean rectangular()
        {
            return (this.type.ToLower() == "rectangular");
        }

        public Coordinate(double x, double y, double z)
        {

            this.type = "rectangular";
            this.x = x;
            this.y = y;
            this.z = z;
            this.Triple = new Double[3] { this.x, this.y, this.z };

        }
        public Coordinate(double rho, Angle theta, Angle phi)
        {

            this.type = "spherical";
            this.rho = rho;
            this.theta = theta;
            this.phi = phi;
            this.Triple = new Double[3] { this.rho, this.theta.Measure(), this.phi.Measure() };

        }
        public Coordinate(double[] xyz, string type = "rectangular")
        {

            this.type = type;
            this.x = xyz[0];
            this.x = xyz[1];
            this.x = xyz[2];
            this.Triple = xyz;

        }

        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            if (a.rectangular() == b.rectangular())
            {
                double[] k = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    k[i] = a.Triple[i] + b.Triple[i];
                }
                return new Coordinate(k, a.type);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public static Coordinate operator *(Coordinate a, Coordinate b)
        {
            if (a.rectangular() == b.rectangular())
            {
                double[] k = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    k[i] = a.Triple[i] / b.Triple[i];
                }
                return new Coordinate(k, a.type);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public static Coordinate operator *(Coordinate a, double b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] * b;
            }
            return new Coordinate(k, a.type);
        }
        public static Coordinate operator /(Coordinate a, Coordinate b)
        {
            if (a.rectangular() == b.rectangular())
            {
                double[] k = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    k[i] = a.Triple[i] / b.Triple[i];
                }
                return new Coordinate(k, a.type);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public static Coordinate operator /(Coordinate a, double b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] / b;
            }
            return new Coordinate(k, a.type);
        }

        public static Coordinate operator -(Coordinate a, Coordinate b)
        {

            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] - b.Triple[i];
            }
            return new Coordinate(k, a.type);

        }
        public Coordinate toSpherical()
        {
            double rho = Math.Sqrt(Math.Pow(this.x, 2) + Math.Pow(this.y, 2) + Math.Pow(this.z, 2));
            double theta = (Math.Atan2(this.y, this.x) + 2 * Math.PI) % (Math.PI * 2);
            double phi = (Math.PI / 2 - Math.Acos(this.z / rho));
            return new Coordinate(rho, theta, phi);
        }
    }
    public static class Chebyshev
    {
        private static double N(int n, double x)
        {
            if (Math.Abs(x) <= 1)
            {
                return Math.Cos(n * Math.Acos(x));
            }
            else
            {
                Console.WriteLine(x);
                throw new ArgumentOutOfRangeException("x");
            }
        }

        private static double deriv(int n, double x)
        {
            if (Math.Abs(x) <= 1)
            {
                return n * Math.Sin(n * Math.Acos(x)) / Math.Sqrt(1 - Math.Pow(x, 2));
            }
            else
            {
                Console.WriteLine(x);
                throw new ArgumentOutOfRangeException("x");
            }
        }
        public static double[] sequence(double[] coefs, double x, int sections = 1)
        {
            int l = coefs.GetLength(0) / sections;
            double[] output = new double[sections];
            for (int k = 0; k < sections; k++)
            {
                double sum = 0;
                for (int i = 0; i < l; i++)
                {
                    sum += coefs[k * l + i] * N(i, x);
                }
                output[k] = sum;
            }
            return output;
        }
        public static double[] Dsequence(double[] coefs, double x, int sections = 1)
        {
            int l = coefs.GetLength(0) / sections;
            double[] output = new double[sections];
            for (int k = 0; k < sections; k++)
            {
                double sum = 0;
                for (int i = 0; i < l; i++)
                {
                    sum += coefs[k * l + i] * deriv(i, x);
                }
                output[k] = sum;
            }
            return output;
        }
    }
}
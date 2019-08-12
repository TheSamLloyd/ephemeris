using System;
using System.IO;

namespace ephemeris
{
    public struct Body
    {
        string Name;

        public string getName() => this.Name;
        public Body(string Name)
        {
            this.Name = Name;
        }
        public Coordinate[] getAbsolute(string time, Ephemeris eph) => eph.calculatePos(this, eph.toJD(DateTime.Parse(time)));

        public Coordinate getSphericalVel(string time, Ephemeris eph) => eph.SphericalVel(this, time);
    }
    public class Coordinate
    {
        public double x => Triple[0];
        public double y => Triple[1];
        public double z => Triple[2];

        public double[] Triple = new double[3];

        public Coordinate(double x, double y, double z, string type = "Rectangular")
        {
            this.Triple = new double[3] { x, y, z };
        }
        public Coordinate(double[] triple)
        {
            this.Triple = triple;
        }

        double[] get() => this.Triple;

        public Coordinate toSpherical(){
            double rho = Math.Sqrt(Math.Pow(this.x,2)+Math.Pow(this.y,2)+Math.Pow(this.z,2));
            double theta = (Math.Atan2(this.y, this.x) + 2 * Math.PI) % (Math.PI * 2);
            double phi = (Math.PI / 2 - Math.Acos(this.z / rho));
            return new Coordinate(rho, theta, phi, "Spherical");
        }
        public Coordinate toRectangular(){
            double x = this.x*Math.Sin(this.y)*Math.Cos(this.z);
            double y = this.x*Math.Sin(this.y)*Math.Sin(this.z);
            double z = this.x*Math.Cos(this.y);
            return new Coordinate(x, y, z);
        }
        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] + b.Triple[i];
            }
            return new Coordinate(k);
        }
        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] - b.Triple[i];
            }
            return new Coordinate(k);
        }
        public static Coordinate operator *(Coordinate a, Coordinate b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] * b.Triple[i];
            }
            return new Coordinate(k);
        }
        public static Coordinate operator *(Coordinate a, double b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] * b;
            }
            return new Coordinate(k);
        }
        public static Coordinate operator /(Coordinate a, Coordinate b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] / b.Triple[i];
            }
            return new Coordinate(k);
        }
        public static Coordinate operator /(Coordinate a, double b)
        {
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = a.Triple[i] / b;
            }
            return new Coordinate(k);
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
    class Program
    {
        static void Main()
        {
            Ephemeris JPL430 = new Ephemeris(
                new Object[11][]
                    {
                        new Object[] {"Mercury", 3, 14, 4},
                        new Object[] {"Venus", 171,10,2},
                        new Object[] {"Earth-Moon Barycenter", 231, 13, 2},
                        new Object[] {"Mars", 309, 11, 1},
                        new Object[] {"Jupiter", 342, 8, 1},
                        new Object[] {"Saturn", 366, 7, 1},
                        new Object[] {"Uranus", 387, 6, 1},
                        new Object[] {"Neptune", 405, 6, 1},
                        new Object[] {"Pluto", 423, 6,1},
                        new Object[] {"Moon", 441, 13, 8},
                        new Object[] {"Sun", 753, 11, 2},


                    },
                new Object[][]
                    {
                        new Object[] {"ascp1950.430.txt", 2433264.5},
                        new Object[] {"ascp2050.430.txt", 2469776.5},
                        new Object[] {"ascp2150.430.txt", 2506320.5}
                    }
            );
            Body[] bodies = new Body[] {
            new Body("Sun"),
            new Body("Venus"),
            new Body("Mars"),
            new Body("Pluto"),
            new Body("Moon"),
            new Body("Mercury"),
            new Body("Saturn"),
            new Body("Uranus"),
            new Body("Neptune"),
            new Body("Jupiter")
            };
            Transits transits = new Transits(2,true);
            string Natal = DateTime.Parse("1993-11-30T21:30:00Z").ToString();
            DateTime Now = DateTime.UtcNow;
            string now = Now.ToString();
            System.Console.WriteLine(Now.ToString("u").Replace(" ", "T"));
            Console.WriteLine(JPL430.toJD(DateTime.Parse(now)));
            foreach (Body body in bodies)
            {
                Coordinate Co = JPL430.geoSpherical(body, now);
                foreach (Body natalBody in bodies)
                {
                    Coordinate NatalCo = JPL430.geoSpherical(natalBody, Natal);
                    Transits.Transit transit = transits.classify(Math.Abs(AngleDifference(ToDegrees(Co.y), ToDegrees(NatalCo.y))));
                    if (transit.getName() != "")
                    {
                        Console.WriteLine(body.getName()+ "\t"+transit.getName()+"\t" + natalBody.getName()+"\t"+transit.getOrb());
                    }
                }
            }
        }
        public static double ToRadians(double deg) => deg / 180 * Math.PI;
        public static double ToDegrees(double deg) => deg / Math.PI * 180;

        public static double AngleDifference(double angle1, double angle2, Boolean degrees=true){
            if (degrees){
                return Math.Min(angle1 - angle2, 360 - (angle1 - angle2));
            }
            else{
                return Math.Min(angle1 - angle2, 2*Math.PI - (angle1 - angle2));
            }
        }
    }
}

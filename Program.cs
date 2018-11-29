﻿using System;
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
        public Coordinate getAbsolute(string time, Ephemeris eph)
        {
            return eph.calculatePos(this, eph.toJD(DateTime.Parse(time)));
        }
    }
    public class Coordinate
    {
        public double x => Triple[0];
        public double y => Triple[1];
        public double z => Triple[2];

        public double[] Triple = new double[3];

        public Coordinate(double x, double y, double z, string type="Rectangular")
        {
            this.Triple = new double[3] { x, y, z };
        }
        public Coordinate(double[] triple)
        {
            this.Triple = triple;
        }

        double[] get()
        {
            return this.Triple;
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
                        new Object[] {"ascp1950.430.txt", 2433264.5}
                    }
            );
            Body Mars = new Body("Mars");
            Body Sun = new Body("Sun");
            Body Venus = new Body("Venus");
            DateTime Now = DateTime.UtcNow;
            string now = Now.ToString();
            System.Console.WriteLine(Now.ToString("u").Replace(" ","T"));
            Coordinate Co = JPL430.geoSpherical(Sun, now);
            for (int i = 0; i < 3; i++)
            {
                if (i<1){
                    Console.WriteLine(Co.Triple[i]);
                }
                else
                {
                    Console.WriteLine(ToDegrees(Co.Triple[i]));
                }
            }
        }
        public static double ToRadians(double deg){
            return deg/180*Math.PI;
        }
        public static double ToDegrees(double deg)
        {
            return deg/Math.PI*180;
        }
    }
}

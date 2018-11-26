using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ephemeris
{
    class Program
    {
        static void Main(string[] args){
            System.Console.WriteLine(Ephemeris.ephFile.toJD());
        }
    }
    class Ephemeris
    {
        public static double parser(string toParse)
        {
            string[] array = toParse.Split("D");
            double output;
            double mantissa;
            short exponent;
            if (array.Length != 2)
            {
                if (Double.TryParse(toParse, out output))
                {
                    return output;
                }
                else
                {
                    throw new Exception("array not right length....");
                }
            }
            else if (Double.TryParse(array[0], out mantissa) && Int16.TryParse(array[1], out exponent))
            {
                return mantissa * Math.Pow(10.0, exponent);
            }
            else
            {
                throw new Exception("Couldn't parse input....");
            }
        }
        class Header
        {
            class BodyData
            {
                Body body;
                int StartPoint;
                int nCoefs;
                int nSets;
                public BodyData(Body body, int StartPoint, int nCoefs, int nSets = 1)
                {
                    this.body = body;
                    this.StartPoint = StartPoint;
                    this.nCoefs = nCoefs;
                    this.nSets = nSets;
                }
            }
            
            static Body SolarSystemBarycenter = new Body("Solar System Barycenter");
            static Body Earth = new Body("Earth", SolarSystemBarycenter);

            BodyData[] bodies = new BodyData[10] {
                new BodyData(new Body("Mercury", SolarSystemBarycenter), 3, 14, 4),
                new BodyData(new Body("Venus", SolarSystemBarycenter), 171, 10, 2),
                new BodyData(new Body("Earth-Moon Barycenter", SolarSystemBarycenter), 231, 13, 2),
                new BodyData(new Body("Mars", SolarSystemBarycenter), 309, 11),
                new BodyData(new Body("Jupiter", SolarSystemBarycenter), 342, 8),
                new BodyData(new Body("Saturn", SolarSystemBarycenter), 366, 7),
                new BodyData(new Body("Uranus", SolarSystemBarycenter), 387, 6),
                new BodyData(new Body("Neptune", SolarSystemBarycenter), 405, 6),
                new BodyData(new Body("Pluto", SolarSystemBarycenter), 423, 6),
                new BodyData(new Body("Moon", Earth), 441, 13, 8),

            };
        }
        public class ephFile
        {
            int startYear;
            int endYear;

            //some magic numbers here
            int linesPerBlock = 341;
            int numbersPerLine = 3;
            int blocksPerFile = 1142;

            public static double toJD(DateTime dt = new DateTime()){
                if (dt.Equals(new DateTime())){
                    dt = DateTime.UtcNow;
                }
                double unixEpoch = 2440587.5;
                double jd = unixEpoch + (dt.Ticks - DateTime.UnixEpoch.Ticks)/(10_000_000.0) / (60 * 60 * 24.0);
                return jd;
            }

            class Block
            {
                Block(double[] data)
                {

                }
                class Coefficients
                {
                    Coefficients(double[] coefs)
                    {

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
                                throw new ArgumentOutOfRangeException("x");
                            }
                        }
                    }
                }
            }
        }
        class Body
        {
            string name;
            Body relativeTo;
            class Coordinate
            {
                double[] coordinate = new double[3];
                double x => this.coordinate[0];
                double y => this.coordinate[1];
                double z => this.coordinate[2];
            }
            Coordinate position;
            Coordinate velocity;

            public Body(string name, Body relativeTo = null)
            {
                this.name = name;
                this.relativeTo = relativeTo;
            }
        }
    }

    static class fileReader
    {
        static IEnumerable<String> getLines(string path, int lineStart = 0, int lineEnd = -1)
        {

            return System.IO.File.ReadLines(path).TakeWhile((line, index) => index >= lineStart & (index <= lineEnd | lineEnd == -1));

        }
    }
}
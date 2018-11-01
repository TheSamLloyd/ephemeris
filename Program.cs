using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ephemeris
{
    class Program
    {
        static void Main(string[] args) => System.Console.WriteLine("Nothing implemented yet.");
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
                output = mantissa * Math.Pow(10.0, exponent);
                return output;
            }
            else
            {
                throw new Exception("Couldn't parse input....");
            }
        }
        class Header
        {

        }
        class ephFile
        {
            int startYear;
            int endYear;
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
                            if (-1 <= x & 1 >= x)
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
            public class Position
            {
                double x;
                double y;
                double z;
                Body relativeTo;
            }
            public class Velocity
            {
                double x;
                double y;
                double z;
                Body relativeTo;
            }
        }
    }
    static class fileReader
    {
        static IEnumerable<String> getLines(string path, int lineStart = 0, int lineEnd = -1)
        {
            if (lineEnd == -1)
            {
                return System.IO.File.ReadLines(path).TakeWhile((line, index) => index >= lineStart);
            }
            else
            {
                return System.IO.File.ReadLines(path).TakeWhile((line, index) => index >= lineStart & index <= lineEnd);
            }
        }

    }
}
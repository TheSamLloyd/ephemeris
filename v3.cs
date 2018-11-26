using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace planetary
{
    class Body
    {
        string Name { get; }
        Body(string Name)
        {
            this.Name = Name;
        }
        Coordinate getAbsolute(double time)
        {

        }
    }
    class Coordinate
    {
        double x => Triple[0];
        double y => Triple[1];
        double z => Triple[2];

        public double[] Triple = new double[3];

        Coordinate(double x, double y, double z)
        {
            this.Triple = new double[3] { x, y, z };
        }
        Coordinate(double[] triple)
        {
            this.Triple = triple;
        }

        double[] get()
        {
            return this.Triple;
        }

    }
    class Ephemeris
    {
        public static double toJD(DateTime dt = new DateTime())
        {
            if (dt.Equals(new DateTime()))
            {
                dt = DateTime.UtcNow;
            }
            double unixEpoch = 2440587.5;
            double jd = unixEpoch + (dt.Ticks - DateTime.UnixEpoch.Ticks) / (10_000_000.0) / (60 * 60 * 24.0);
            return jd;
        }

        public static double scaler(double x, double rangeStart, double rangeEnd)
        {
            return (x-rangeStart)/(rangeEnd-rangeStart)*2-1;
        }
        public static double parser(string toParse)
        {
            string[] array = toParse.Split("D");
            double output, mantissa;
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
        public static class fileReader
        {
            public static IEnumerable<String> getLines(string path, int lineStart = 0, int count = 0)
            {

                return System.IO.File.ReadLines(path).Skip(lineStart).Take(count);

            }
        }
        class Header
        {
            Object[][] data = new Object[10][]
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
            new Object[] {"Moon", 441, 13, 8}

        };
            public Object[] getInfo(int index)
            {
                return data[index];
            }
        }

        class File
        {
            string filename;
            public double startTime;
            File(string filename, double startTime)
            {
                this.filename = filename;
                this.startTime = startTime;
            }

            //some magic numbers here
            int linesPerBlock = 341;
            int numbersPerLine = 3;
            int blocksPerFile = 1142;
            int dimension = 3;
            Block getBlock(double jDate)
            {
                int blockNumber = (int)Math.Ceiling((jDate-this.startTime)/32);
                double[] blockData = new double[linesPerBlock * numbersPerLine];
                IEnumerable<string> rawData = fileReader.getLines(filename, blockNumber * linesPerBlock, linesPerBlock);
                int i = 0;
                foreach (string line in rawData)
                {
                    foreach (string entry in line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        blockData[i] = Ephemeris.parser(entry);
                        i++;
                    }
                }
                return new Block(blockData);
            }
            class Block
            {
                double[] data;
                double startTime;
                double endTime;
                public Block(double[] data)
                {
                    this.data = data;
                    this.startTime = data[0];
                    this.endTime = data[1];
                }

                public double position(int x)
                {
                    return this.data[x];
                }

            }
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
                throw new ArgumentOutOfRangeException("x");
            }
        }
    }
}

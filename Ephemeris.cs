using System;
using System.Collections.Generic;
using System.Linq;

namespace ephemeris
{
    public class Ephemeris
    {
        public Header header;
        public Ephemeris(Object[][] bodies, Object[][] files)
        {
            this.header = new Header(bodies, files);
        }

        public double toJD(DateTime dt = new DateTime())
        {
            if (dt.Equals(new DateTime()))
            {
                dt = DateTime.UtcNow;
            }
            double L = 1.550505 * Math.Pow(10, -8);
            DateTime TAIEpoch = new DateTime(1977, 1, 1, 0, 0, 0);
            double jd = ((dt.Ticks - TAIEpoch.Ticks) / (10_000_000.0) + 37) / (60 * 60 * 24.0);
            jd = jd * (1 + L);
            jd += 2443144.5003725;
            return jd;
        }

        public Coordinate[] calculatePos(Body body, double jd)
        {
            File ephFile = selectFile(jd);
            File.Block ephBlock = ephFile.getBlock(jd);
            Object[] bodyInfo = selectBody(body.getName());
            //this function gets the start and end times of the relevant block
            double[] tRange = ephBlock.position(new int[2] { 0, 1 });
            //then it scales our given time to a range the Chebyshev polynomials can accept and tells us which set to take
            double[] scaledTime = scaler(jd, tRange[0], tRange[1], (int)bodyInfo[3]);
            //so the range we need to take is: [startpos-1+numCoefs*dimension*set, numCoefs*dimension]
            double[] coefs = ephBlock.position((int)bodyInfo[1] - 1 + (int)bodyInfo[2] * (int)ephFile.dimension * (int)scaledTime[0], (int)bodyInfo[2] * ephFile.dimension);
            double[] position = Chebyshev.sequence(coefs, scaledTime[1], ephFile.dimension);
            double[] velocity = Chebyshev.Dsequence(coefs, scaledTime[1], ephFile.dimension);
            return new Coordinate[2] { new Coordinate(position), new Coordinate(velocity) };
        }

        public Coordinate[] getEarth(string JD)
        {
            Body barycenter = new Body("Earth-Moon Barycenter");
            Body moon = new Body("Moon");
            double jd = toJD(DateTime.Parse(JD));
            Coordinate[] barycenterC = calculatePos(barycenter, jd);
            Coordinate[] moonC = calculatePos(moon, jd);
            double emRatio = 81.300569074190620;
            double[] earthCoord = new double[3];
            double[] earthVel = new double[3];
            for (int i = 0; i < moonC[0].Triple.Length; i++)
            {
                earthCoord[i] = barycenterC[0].Triple[i] - moonC[0].Triple[i] / (emRatio + 1);
                earthVel[i] = barycenterC[1].Triple[i] - moonC[1].Triple[i] / (emRatio + 1);
                Console.WriteLine(moonC[1].Triple[i]);
            }
            return new Coordinate[2] { new Coordinate(earthCoord), new Coordinate(earthVel) };
        }

        public Coordinate geoSpherical(Body body, string JD)
        {
            Coordinate eCo = this.getEarth(JD)[0];
            double jd = toJD(DateTime.Parse(JD));
            Coordinate bCo = this.calculatePos(body, jd)[0];
            double[] bRelCo = new double[eCo.Triple.Length];
            for (int i = 0; i < bRelCo.Length; i++)
            {
                bRelCo[i] = bCo.Triple[i] - eCo.Triple[i];
            }
            double rho(double[] co)
            {
                double d = 0;
                foreach (double item in co)
                {
                    d += Math.Pow(item, 2);
                }
                return Math.Sqrt(d);
            }
            double theta(double[] co)
            {
                return (Math.Atan2(co[1], co[0]) + 2 * Math.PI) % (Math.PI * 2);
            }
            double phi(double[] co)
            {
                return Math.PI / 2 - Math.Acos(co[2] / rho(co));
            }
            return new Coordinate(rho(bRelCo), theta(bRelCo), phi(bRelCo), "Spherical");
        }

        public Coordinate SphericalVel(Body body, string JD)
        {
            Coordinate[] earth = this.getEarth(JD);
            double jd = toJD(DateTime.Parse(JD));
            Coordinate[] absolute = calculatePos(body, jd);
            Coordinate Pos = new Coordinate(absolute[0].x - earth[0].x, absolute[0].y - earth[0].y, absolute[0].z - earth[0].z);
            Coordinate Vel = new Coordinate(absolute[1].x - earth[1].x, absolute[1].y - earth[1].y, absolute[1].z - earth[1].z);
            double rhoD = (Pos.x * Vel.x + Pos.y * Vel.y + Pos.z * Vel.z) / Math.Sqrt(Math.Pow(Pos.x, 2) + Math.Pow(Pos.y, 2) + Math.Pow(Pos.z, 2));
            double phiD = (-Pos.y * Vel.x + Pos.x * Vel.y) / (Math.Pow(Pos.x, 2) + Math.Pow(Pos.y, 2));
            double thetaD = (Pos.z * (Pos.x * Vel.x + Pos.y * Vel.y) - (Pos.x * Pos.x + Pos.y * Pos.y) * Vel.z) / (Math.Sqrt(Pos.x * Pos.x + Pos.y * Pos.y) * (Pos.x * Pos.x + Pos.y * Pos.y + Pos.z * Pos.z));
            return new Coordinate(rhoD, thetaD, phiD, "Spherical");
        }
        File selectFile(double jd)
        {
            int whichTake = -1;
            for (int i = 0; i < this.header.numFiles; i++)
            {
                Object[] file = this.header.getFileInfo(i);
                string fileName = (string)file[0];
                double fileStart = (double)file[1];
                if (jd > fileStart)
                {
                    whichTake = i;
                }
                else
                {
                    break;
                }
            }
            if (whichTake == -1)
            {
                throw new ArgumentOutOfRangeException("No information for the date provided.");
            }
            else
            {
                Object[] outFile = this.header.getFileInfo(whichTake);
                return new File((string)outFile[0], (double)outFile[1]);
            }
        }
        Object[] selectBody(string name)
        {
            for (int i = 0; i < this.header.numBodies; i++)
            {
                Object[] body = this.header.getBodyInfo(i);
                if (name.Equals((string)body[0]))
                {
                    return this.header.getBodyInfo(i);
                }
            }
            throw new KeyNotFoundException("No body by the name " + name);
        }
        public static double[] scaler(double x, double rangeStart, double rangeEnd, int numSets = 1)
        {
            double toParse = (x - rangeStart) / (rangeEnd - rangeStart) * numSets;
            int nSet = (int)Math.Floor(toParse);
            double tau = (toParse - nSet) * 2 - 1;
            return new double[] { nSet, tau };
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
        public struct Header
        {
            public Header(Object[][] bodies, Object[][] files)
            {
                this.data = bodies;
                this.ephFiles = files;

            }
            Object[][] data, ephFiles;
            public Object[] getBodyInfo(int index)
            {
                return data[index];
            }
            public Object[] getFileInfo(int index)
            {
                return ephFiles[index];
            }
            public int numFiles => ephFiles.GetLength(0);
            public int numBodies => data.GetLength(0);
        }

        public class File
        {
            string filename;
            public double startTime;
            public File(string filename, double startTime)
            {
                this.filename = filename;
                this.startTime = startTime;
            }

            //some magic numbers here
            public int linesPerBlock = 341;
            public int numbersPerLine = 3;
            public int blocksPerFile = 1142;
            public int dimension = 3;
            public int daysPerBlock = 32;
            public Block getBlock(double jDate)
            {
                int blockNumber = (int)Math.Ceiling((jDate - this.startTime) / daysPerBlock);
                double[] blockData = new double[linesPerBlock * numbersPerLine];
                IEnumerable<string> rawData = fileReader.getLines(filename, (blockNumber - 1) * linesPerBlock + 1, linesPerBlock);
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
            public struct Block
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

                public double[] position(int[] x)
                {
                    double[] output = new double[x.GetLength(0)];
                    for (int i = 0; i < output.GetLength(0); i++)
                    {
                        output[i] = this.data[x[i]];
                    }
                    return output;
                }
                public double[] position(int x, int length)
                {
                    double[] output = this.data.Skip(x).Take(length).ToArray<double>();
                    return output;
                }

            }
        }
    }
}

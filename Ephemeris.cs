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
            DateTime TAIEpoch = new DateTime(1976, 12, 31, 23, 59, 23);
            double jd = (dt.Ticks - TAIEpoch.Ticks) / (10_000_000.0) / (60 * 60 * 24.0);
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
            Coordinate earthCoord = barycenterC[0] - moonC[0] / (emRatio + 1);
            Coordinate earthVel = barycenterC[1] - moonC[1] / (emRatio + 1);
            return new Coordinate[2] { earthCoord, earthVel };
        }

        public Coordinate geoSpherical(Body body, string JD)
        {
            Coordinate eCo = this.getEarth(JD)[0];
            double jd = toJD(DateTime.Parse(JD));
            Coordinate bCo = this.calculatePos(body, jd)[0];
            Coordinate bRelCo = bCo - eCo;
            if (body.getName()=="Moon") bRelCo=bCo;
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
            return new Coordinate(rho(bRelCo.Triple), theta(bRelCo.Triple), phi(bRelCo.Triple), "Spherical");
        }

        public Coordinate SphericalVel(Body body, string JD)
        {
            Coordinate[] earth = this.getEarth(JD);
            double jd = toJD(DateTime.Parse(JD));
            Coordinate[] absolute = calculatePos(body, jd);
            Coordinate Pos = absolute[0] - earth[0];
            Coordinate Vel = (absolute[1] - earth[1]) / 86400;
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
                return new File("./ephFiles/" + (string)outFile[0], (double)outFile[1]);
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
            return Double.Parse(toParse.Replace("D", "E"), System.Globalization.NumberStyles.Float);
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
            public Object[] getBodyInfo(int index) => data[index];
            public Object[] getFileInfo(int index) => ephFiles[index];
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

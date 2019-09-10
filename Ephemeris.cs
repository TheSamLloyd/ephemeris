using System;
using System.Collections.Generic;
using System.Linq;

namespace ephemeris
{
    public class Ephemeris
    {
        public static Ephemeris JPL430 = new Ephemeris(
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
        public Header header;
        public Ephemeris(Object[][] bodies, Object[][] files)
        {
            this.header = new Header(bodies, files);
        }

        public Body[] getBodies()
        {
            Body[] output = new Body[this.header.numBodies];
            for (int i = 0; i < this.header.numBodies; i++)
            {
                output.SetValue(new Body(this.header.getBodyName(i)), i);
            }
            return output;
        }
        public static double toJD(DateTime dt = new DateTime(), Boolean rtv = true)
        {
            if (dt.Equals(new DateTime()))
            {
                dt = DateTime.UtcNow;
            }
            double jd;
            if (rtv)
            {
                //All the weird shit here is just to approximate BDT, which done properly would require relativistic correction, which... I don't know how to do!
                double L = 1.550505 * Math.Pow(10, -8);
                DateTime TAIEpoch = new DateTime(1976, 12, 31, 23, 59, 23);
                jd = (dt.Ticks - TAIEpoch.Ticks) / (10_000_000.0) / (60 * 60 * 24.0);
                jd = jd * (1 + L);
                jd += 2443144.5003725;
            }
            else
            {
                DateTime TAIEpoch = new DateTime(1977, 01, 01, 00, 00, 00);
                jd = (dt.Ticks - TAIEpoch.Ticks) / (10_000_000.0) / (60 * 60 * 24.0);
                jd += 2443144.5;
            }
            return jd;
        }

        public Coordinate[] calculatePos(Body body, double jd)
        {
            File ephFile = selectFile(jd);
            File.Block ephBlock = ephFile.getBlock(jd);
            Header.HeaderData bodyInfo = selectBody(body.getName());
            //this function gets the start and end times of the relevant block
            double[] tRange = ephBlock.position(new int[2] { 0, 1 });
            //then it scales our given time to a range the Chebyshev polynomials can accept and tells us which set to take
            double[] scaledTime = scaler(jd, tRange[0], tRange[1], bodyInfo.getNumCoordinates());
            //so the range we need to take is: [startpos-1+numCoefs*dimension*set, numCoefs*dimension]
            double[] coefs = ephBlock.position(bodyInfo.getStart() - 1 + bodyInfo.getNumCoefficients() * ephFile.dimension * (int)scaledTime[0], bodyInfo.getNumCoefficients() * ephFile.dimension);
            double[] position = Chebyshev.sequence(coefs, scaledTime[1], ephFile.dimension);
            double[] velocity = Chebyshev.Dsequence(coefs, scaledTime[1], ephFile.dimension);
            return new Coordinate[2] { new Coordinate(position), new Coordinate(velocity) };
        }

        public Coordinate[] getEarth(double jd)
        {
            Body barycenter = new Body("Earth-Moon Barycenter");
            Body moon = new Body("Moon");
            Coordinate[] barycenterC = calculatePos(barycenter, jd);
            Coordinate[] moonC = calculatePos(moon, jd);
            //magic number here is the mass ratio between the earth and the moon that allows us to find the barycenter
            double emRatio = 81.300569074190620;
            Coordinate earthCoord = barycenterC[0] - moonC[0] / (emRatio + 1);
            Coordinate earthVel = barycenterC[1] - moonC[1] / (emRatio + 1);
            return new Coordinate[2] { (Coordinate)earthCoord, (Coordinate)earthVel };
        }

        public Coordinate geoSpherical(Body body, DateTime jd)
        {
            double JD = toJD(jd, true);
            Coordinate eCo = this.getEarth(JD)[0];
            Coordinate bCo = this.calculatePos(body, JD)[0];
            Coordinate bRelCo = bCo - eCo;
            if (body.getName() == "Moon") bRelCo = bCo;
            double rho(double[] co)
            {
                double d = 0;
                foreach (double item in co)
                {
                    d += Math.Pow(item, 2);
                }
                return Math.Sqrt(d);
            }
            Angle theta(double[] co)
            {
                return new Angle((Math.Atan2(co[1], co[0]) + 2 * Math.PI) % (Math.PI * 2), "radians");
            }
            Angle phi(double[] co)
            {
                return new Angle(Math.PI / 2 - Math.Acos(co[2] / rho(co)), "radians");
            }
            return new Coordinate(rho(bRelCo.Triple), theta(bRelCo.Triple), phi(bRelCo.Triple));
        }

        public Coordinate SphericalVel(Body body, DateTime JD)
        {
            double jd = toJD(JD, true);
            Coordinate[] earth = this.getEarth(jd);
            Coordinate[] absolute = calculatePos(body, jd);
            Coordinate Pos = absolute[0] - earth[0];
            Coordinate Vel = (absolute[1] - earth[1]) / 86400;
            double rhoD = (Pos.x * Vel.x + Pos.y * Vel.y + Pos.z * Vel.z) / Math.Sqrt(Math.Pow(Pos.x, 2) + Math.Pow(Pos.y, 2) + Math.Pow(Pos.z, 2));
            Angle phiD = new Angle((-Pos.y * Vel.x + Pos.x * Vel.y) / (Math.Pow(Pos.x, 2) + Math.Pow(Pos.y, 2)), "radians");
            Angle thetaD = new Angle((Pos.z * (Pos.x * Vel.x + Pos.y * Vel.y) - (Pos.x * Pos.x + Pos.y * Pos.y) * Vel.z) / (Math.Sqrt(Pos.x * Pos.x + Pos.y * Pos.y) * (Pos.x * Pos.x + Pos.y * Pos.y + Pos.z * Pos.z)), "radians");
            return new Coordinate(rhoD, thetaD, phiD);
        }
        File selectFile(double jd)
        {
            int whichTake = -1;
            for (int i = 0; i < this.header.numFiles; i++)
            {
                File file = this.header.getFileInfo(i);
                string fileName = (string)file.getFileName();
                double fileStart = file.getStartTime();
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
                File outFile = this.header.getFileInfo(whichTake);
                return new File("./ephFiles/" + outFile.getFileName(), outFile.getStartTime());
            }
        }
        Header.HeaderData selectBody(string name)
        {
            for (int i = 0; i < this.header.numBodies; i++)
            {
                Header.HeaderData body = this.header.getBodyInfo(i);
                if (name.Equals(body.getName()))
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
                this.data = new HeaderData[bodies.Length];
                for (int i = 0; i < this.data.Length; i++)
                {
                    this.data.SetValue(new HeaderData(bodies[i]), i);
                }
                this.ephFiles = new File[files.Length];
                for (int i = 0; i < this.ephFiles.Length; i++)
                {
                    this.ephFiles.SetValue(new File(files[i][0].ToString(), (double)files[i][1]), i);
                }
            }
            HeaderData[] data;
            File[] ephFiles;
            public HeaderData getBodyInfo(int index) => this.data[index];
            public string getBodyName(int index) => this.data[index].getName();
            public File getFileInfo(int index) => this.ephFiles[index];
            public int numFiles => ephFiles.GetLength(0);
            public int numBodies => data.GetLength(0);

            public struct HeaderData
            {
                string name;
                int start, numCoefficients, numCoordinates;
                //for backwards compatibility
                public HeaderData(Object[] info)
                {
                    this.name = info[0].ToString();
                    this.start = (int)info[1];
                    this.numCoefficients = (int)info[2];
                    this.numCoordinates = (int)info[3];
                }
                public string getName() => this.name;
                public int getStart() => this.start;
                public int getNumCoefficients() => this.numCoefficients;
                public int getNumCoordinates() => this.numCoordinates;

            }
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
            public string getFileName() => this.filename;
            public double getStartTime() => this.startTime;
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

// using System;
// using System.IO;
// using System.Collections.Generic;
// using System.Linq;

// namespace XephemerisDraft
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             Ephemeris.ephFile eph = new Ephemeris.ephFile("ascp1950.430.txt", 2433264.5, 2469808.5);
//             eph.getBlock(2);
//         }
//     }
//     class Ephemeris
//     {
//         public static double toJD(DateTime dt = new DateTime())
//         {
//             if (dt.Equals(new DateTime()))
//             {
//                 dt = DateTime.UtcNow;
//             }
//             double unixEpoch = 2440587.5;
//             double jd = unixEpoch + (dt.Ticks - DateTime.UnixEpoch.Ticks) / (10_000_000.0) / (60 * 60 * 24.0);
//             return jd;
//         }
//         public static double parser(string toParse)
//         {
//             string[] array = toParse.Split("D");
//             double output, mantissa;
//             short exponent;
//             if (array.Length != 2)
//             {
//                 if (Double.TryParse(toParse, out output))
//                 {
//                     return output;
//                 }
//                 else
//                 {
//                     throw new Exception("array not right length....");
//                 }
//             }
//             else if (Double.TryParse(array[0], out mantissa) && Int16.TryParse(array[1], out exponent))
//             {
//                 return mantissa * Math.Pow(10.0, exponent);
//             }
//             else
//             {
//                 throw new Exception("Couldn't parse input....");
//             }
//         }

//         Body[] bodies = new Body[10] {
//                 new Body("Mercury", 3, 14, 4),
//                 new Body("Venus", 171, 10, 2),
//                 new Body("Earth-Moon Barycenter", 231, 13, 2),
//                 new Body("Mars", 309, 11),
//                 new Body("Jupiter", 342, 8),
//                 new Body("Saturn", 366, 7),
//                 new Body("Uranus", 387, 6),
//                 new Body("Neptune", 405, 6),
//                 new Body("Pluto", 423, 6),
//                 new Body("Moon", 441, 13, 8),

//             };
//         class Header
//         {
//             //need to figure out: which file to use, which block to take. We can... do that.

//         }
//         public class ephFile
//         {
//             double startTime, endTime;
//             string filename;
//             //some magic numbers here
//             int linesPerBlock = 341;
//             int numbersPerLine = 3;
//             int blocksPerFile = 1142;
//             int dimension = 3;

//             public ephFile(string filename, double startTime, double endTime)
//             {
//                 this.filename = filename;
//                 this.startTime = startTime;
//                 this.endTime = endTime;
//             }

//             public Block getBlock(int blockNumber)
//             {
//                 double[] blockData = new double[341 * 3];
//                 IEnumerable<string> rawData = fileReader.getLines(filename, blockNumber * linesPerBlock, linesPerBlock);
//                 int i = 0;
//                 foreach (string line in rawData)
//                 {
//                     foreach (string entry in line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries))
//                     {
//                         blockData[i] = Ephemeris.parser(entry);
//                         i++;
//                     }
//                 }
//                 return new Block(blockData);
//             }
//             public class Block
//             {
//                 double[] data;
//                 double startTime;
//                 double endTime;
//                 public Block(double[] data)
//                 {
//                     this.data = data;
//                     this.startTime = data[0];
//                     this.endTime = data[1];
//                 }
//                 class Coefficients
//                 {
//                     Coefficients(double[] coefs)
//                     {

//                     }
//                     public static class Chebyshev
//                     {
//                         private static double N(int n, double x)
//                         {
//                             if (Math.Abs(x) <= 1)
//                             {
//                                 return Math.Cos(n * Math.Acos(x));
//                             }
//                             else
//                             {
//                                 throw new ArgumentOutOfRangeException("x");
//                             }
//                         }
//                     }
//                 }
//             }
//         }
//         class Body
//         {
//             string name;
//             Body relativeTo;
//             public class Coordinate
//             {
//                 double[] coordinate = new double[3];
//                 public double x => this.coordinate[0];
//                 public double y => this.coordinate[1];
//                 public double z => this.coordinate[2];
//                 public Coordinate(double x=0, double y=0, double z=0){
//                     this.coordinate = new double[3] {x,y,z};
//                 }
//                 public Coordinate(double[] coords){
//                     this.coordinate = coords;
//                 }
//             }
//             Coordinate position;

//             public Coordinate getPosition(){
//                 return this.position;
//             }
//             Coordinate velocity;
//             public Coordinate getVelocity(){
//                 return this.velocity;
//             }
            
//             public Body(string name, int StartPoint, int nCoefs, int nSets = 1)
//             {
//                 this.name = name;

//             }
//             Coordinate correctRelative(Body satellite, Body primary)
//             {
//                 double x,y,z;
//                 x=satellite.getPosition().x-primary.getPosition().x;
//                 y=satellite.getPosition().y-primary.getPosition().y;
//                 z=satellite.getPosition().z-primary.getPosition().z;

//                 return new Coordinate(x,y,z);
//             }
//         }
//     }

//     public static class fileReader
//     {
//         public static IEnumerable<String> getLines(string path, int lineStart = 0, int count = 0)
//         {

//             return System.IO.File.ReadLines(path).Skip(lineStart).Take(count);

//         }
//     }
// }
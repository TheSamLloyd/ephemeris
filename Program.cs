using System;
using System.IO;

namespace ephemeris
{
  class Program
  {
    static void Main(string[] args) => System.Console.WriteLine("Nothing implemented yet.");
  }
  class Ephemeris
  {
    class Header
    {

    }
    class ephFile
    {
      int startYear;
      int endYear;
      class Block
      {
        class Coefficients
        {
          class Chebyshev
          {
              private double N(int n, double x){
                  if (-1<=x & 1>=x){
                      return Math.Cos(n*Math.Acos(x));
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

}
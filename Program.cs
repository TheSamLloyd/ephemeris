using System;
using System.IO;

namespace ephemeris
{
  class Program
  {
    static void Main(string[] args)
    {
      Block now = Block.readFile("0.2458400084722D+7");
        foreach (var item in now.getRange(2,14))
        {
            System.Console.WriteLine(item);
        }
        System.Console.WriteLine(now.tau(parser("0.2458400084722D+7")));
      System.Console.ReadLine();
    }
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
      if (Double.TryParse(array[0], out mantissa) && Int16.TryParse(array[1], out exponent))
      {
        output = mantissa * Math.Pow(10.0, exponent);
        return output;
      }
      else
      {
        throw new Exception("Couldn't parse input....");
      }
    }
  }
  class Block
  {
    public double[] selfData;
    double begin;
    double end;
    public Block(double[,] table)
    {
      begin = table[0, 0];
      end = table[0, 1];
      selfData = badFlatten(table);
    }

    private double[] badFlatten(double[,] table)
    {
      int kIndex = 0;
      int size = table.GetLength(0) * table.GetLength(1);
      double[] k = new double[size + 2];
      for (var i = 0; i < table.GetLength(0); i++)
      {
        for (var j = 0; j < table.GetLength(1); j++)
        {
          k[kIndex++] = table[i, j];
        }
      }
      k[0] = k[1] = 0;
      return k;
    }

    public double[] getRange(int origin, int count)
    {
      double[] k = new double[count];
      for (var i = 0; i < count; i++)
      {
        k[i] = this.selfData[origin + i];
      }
      return k;
    }

    public double tau(double t)
    {
      return 2*(t - this.begin) / (this.end - this.begin)-1;
    }
    public double[] tau(double t, int n)
    {
        double[] k= new double[2];
        k[0] = Math.Floor(n*tau(t));
        k[1] = 2*(n*(1+tau(t))/2.0-k[0])-1;
        return k;
    }
    public static Block readFile(string t, string filename = @"ascp1950.430.txt")
    {
      int counter = 0;
      int subcounter = -1;
      string[] split;
      string line;
      System.IO.StreamReader file =
          new System.IO.StreamReader(filename);
      double e00;
      int block = -1;
      double tt = Program.parser(t);
      double[,] table = new double[341, 3];
      while ((line = file.ReadLine()) != null)
      {
        split = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (counter == 1)
        {
          e00 = Program.parser(split[0]);
          block = (int)Math.Ceiling((tt - e00) / 32);
        }
        else if (counter == 341 * (block - 1) + 1)
        {
          subcounter++;
        }
        else if (counter > 341 * (block) && block >= 0)
        {
          break;
        }
        if (subcounter >= 0)
        {
          for (var i = 0; i < split.Length; i++)
          {
            table[subcounter, i] = Program.parser(split[i]);
          }
          subcounter++;
        }
        counter++;
      }
      file.Close();
      return new Block(table);
    }
  }
}

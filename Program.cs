using System;
using System.IO;

namespace ephemeris
{
    class Program
    {
        static void Main()
        {
            Ephemeris ephemeris = Ephemeris.JPL430;
            DateTime Natal = DateTime.Parse("1993-11-30T21:30:00Z").ToUniversalTime();
            System.Console.WriteLine(Natal.ToString("u").Replace(" ", "T"));
            Console.WriteLine(Ephemeris.toJD());
            foreach (Body body in ephemeris.getBodies())
            {
                Console.WriteLine(
                body.getName() + ": " + Math.Round(Angle.ToDegrees(body.getSpherical(Natal, ephemeris).theta.Measure()), 3));
            }
        }
    }
}

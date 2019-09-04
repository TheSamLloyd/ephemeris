using System;
using System.IO;

namespace ephemeris
{
    class Program
    {
        static void Main()
        {
            Ephemeris ephemeris = Ephemeris.JPL430;
            DateTime Natal = DateTime.Parse("1993-11-30T21:30:00Z");
            DateTime Now = DateTime.UtcNow;
            System.Console.WriteLine(Now.ToString("u").Replace(" ", "T"));
            Console.WriteLine(ephemeris.toJD(Now));
            Body[] bodies = ephemeris.getBodies();
            foreach (Body body in bodies)
            {
                Console.WriteLine(
                body.getName() + ": " + Angle.ToDegrees(ephemeris.geoSpherical(body, Natal).theta.Measure()));
            }
        }
    }
}

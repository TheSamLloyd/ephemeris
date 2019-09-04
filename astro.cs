using System;
namespace ephemeris
{
    public class Transits
    {
        double orb;
        Ephemeris eph;
        public Transits(Ephemeris eph, double orb, Boolean degrees = true)
        {
            this.eph = eph;
            if (degrees)
            {
                this.orb = orb;
            }
            else
            {
                this.orb = Angle.ToDegrees(orb);
            }
        }
        public class Transit
        {
            double angle;
            double orb;
            string name;
            Boolean degrees;
            double duration;
            public Transit(string name, double angle, double orb, Boolean degrees = true, double duration = 0)
            {
                this.angle = angle;
                this.orb = orb;
                this.name = name;
                this.degrees = degrees;
                this.duration = duration;
            }
            public string getName()
            {
                return this.name;
            }
            public double getAngle()
            {
                return this.angle;
            }
            public double getOrb()
            {
                return this.orb;
            }
        }
        Transit[] types = {
            new Transit("Conjunct",0,2),
            new Transit("Sextil",60,2),
            new Transit("Square",90,2),
            new Transit("Trine",120,2),
            new Transit("Oppose",180,2),
        };

        // public Transit classify(Body current, Body natal, Boolean duration = false)
        // {
        //     double difference = eph.geoSpherical(current) - natal.y;
        //     foreach (Transit type in types)
        //     {

        //     }
        // }

    };

}
using System;
namespace ephemeris
{
    public class Transits
    {
        double orb;
        public Transits(double orb, Boolean degrees = true)
        {
            if (degrees)
            {
                this.orb = orb;
            }
            else
            {
                this.orb = Program.ToDegrees(orb);
            }
        }
        public class Transit
        {
            double angle;
            double orb;
            string name;
            Boolean degrees;
            public Transit(string name, double angle, double orb, Boolean degrees = true)
            {
                this.angle = angle;
                this.orb = orb;
                this.name = name;
                this.degrees = degrees;
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

        public Transit classify(double angle, Boolean degrees = true)
        {
            foreach (Transit type in types)
            {
                if (angle >= Math.Abs(type.getAngle() - type.getOrb()) && angle <= type.getAngle() + type.getOrb())
                {
                    return new Transit(type.getName(), angle, Math.Abs(angle - type.getAngle()));
                }
            }
            return new Transit("", 0, 0);
        }

    };

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;

namespace WpfApplication4
{
    class Sight
    {

        Organism source;

        double range;
        double legWidth;

        Point[] boundaries;

        bool hit;


        public Sight(Organism source, double range, double legWidth)
        {
            this.source = source;

            this.range = range;

            this.legWidth = legWidth;

            boundaries = new Point[3];
            for (int i = 0; i < 3; i++)
            {
                boundaries[i] = new Point();
            }

            calculateBoundaries();

        }


        public void calculateBoundaries()
        {
            //Start from the origin
            boundaries[0] = source.getPosition();

            //Construct direction vector from angle, Extend by range units from original position
            Point midLeg = new Point(source.getPosition().X + Math.Sin(source.getAngle()) * range, source.getPosition().Y + Math.Cos(source.getAngle()) * range);

            //Now we will find two new points that are half of the leg width apart from this midpoint, perpindicular to the last line.
            double tempAngle = source.getAngle() + 90;

            boundaries[1] = new Point(midLeg.X + Math.Sin(tempAngle) * (legWidth / 2), midLeg.Y + Math.Cos(tempAngle) * (legWidth / 2));

            //Repeat with the opposite direction
            tempAngle = source.getAngle() - 90;

            boundaries[2] = new Point(midLeg.X + Math.Sin(tempAngle) * (legWidth / 2), midLeg.Y + Math.Cos(tempAngle) * (legWidth / 2));
        }

        public Point[] getBoundaries()
        {
            return boundaries;
        }

        public bool isPointWithin(Point point)
        {
            bool result = false;
            int j = boundaries.Count() - 1;
            for (int i = 0; i < boundaries.Count(); i++)
            {
                if (boundaries[i].Y < point.Y & boundaries[j].Y >= point.Y || boundaries[j].Y < point.Y && boundaries[i].Y >= point.Y)
                {
                    if (boundaries[i].X + (point.Y - boundaries[i].Y) / (boundaries[j].Y - boundaries[i].Y) * (boundaries[j].X - boundaries[i].X) < point.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }

            hit = result;
            return result;
        }

        public bool canSee()
        {
            return hit;
        }

        public void setCanSee(bool hit)
        {
            this.hit = hit;
        }

        public double getRange()
        {
            return range;
        }
    }
}

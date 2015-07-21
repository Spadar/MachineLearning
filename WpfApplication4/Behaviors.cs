using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfApplication4
{
    class Behaviors
    {
        Dictionary<int, string> BehaviorList;

        World world;

        List<string> blacklistMethods;

        Random ranGen;

        double moveMod;
        double turnMod;
        double energyMod;

        public Behaviors(World world)
        {
            this.world = world;

            BehaviorList = new Dictionary<int, string>();

            ranGen = new Random();

            moveMod = 1;

            turnMod = 0.04;

            energyMod = 0.5;

            blacklistMethods = new List<String>();
            createMethodBlacklist();

            int index = 0;
            //Constructing the dictionary of possible actions
            foreach (var method in GetType().GetMethods())
            {
                //Ignore operations that aren't related to an organism.
                if (!blacklistMethods.Contains(method.Name))
                {
                    BehaviorList.Add(index, method.Name);
                    index++;
                }
            }
        }

        public string getBehavior(int index)
        {
            return BehaviorList[index];
        }

        public string getRandomBehavior()
        {
            return BehaviorList[ranGen.Next(BehaviorList.Count - 1)];
        }

        public int moveForward(Organism target, int index)
        {
            double cost = 0.25 * energyMod;

            Point proposedPosition = new Point();

            proposedPosition.X = target.getPosition().X + (Math.Sin(target.getAngle()) * target.getSpeed() * moveMod);
            proposedPosition.Y = target.getPosition().Y + (Math.Cos(target.getAngle()) * target.getSpeed() * moveMod);

            if(proposedPosition.X < world.getWorldSize().X & proposedPosition.X > 0)
            {
                if(proposedPosition.Y < world.getWorldSize().Y & proposedPosition.Y > 0)
                {
                    target.modifyPosition(proposedPosition);

                    target.drainEnergy(cost * calculateSpeedCost(target.getSpeed()));
                }
            }

            target.drainEnergy(cost * calculateSpeedCost(target.getSpeed()) + 0.25);

            return index + 1;
        }

        public int moveBackward(Organism target, int index)
        {
            double cost = 0.25 * energyMod;

            Point proposedPosition = new Point();

            proposedPosition.X = target.getPosition().X - (Math.Sin(target.getAngle()) * target.getSpeed() * moveMod);
            proposedPosition.Y = target.getPosition().Y - (Math.Cos(target.getAngle()) * target.getSpeed() * moveMod);

            if (proposedPosition.X < world.getWorldSize().X & proposedPosition.X > 0)
            {
                if (proposedPosition.Y < world.getWorldSize().Y & proposedPosition.Y > 0)
                {
                    target.modifyPosition(proposedPosition);

                    target.drainEnergy(cost * calculateSpeedCost(target.getSpeed()));
                }
            }

            target.drainEnergy(cost * calculateSpeedCost(target.getSpeed()) + 0.25);

            return index + 1;
        }

        public int turnRight(Organism target, int index)
        {
            double cost = 0.25 * energyMod;

            target.setAngle(target.getAngle() - turnMod);

            target.drainEnergy(cost);

            return index + 1;
        }

        public int turnLeft(Organism target, int index)
        {
            double cost = 0.25 * energyMod;

            target.setAngle(target.getAngle() + turnMod);

            target.drainEnergy(cost);

            return index + 1;
        }

        public int ifSightHit(Organism target, int index)
        {
            double cost = 0.25 * energyMod;
            target.drainEnergy(cost);

            if(target.getSight().canSee())
            {
                return index + 1;
            }
            else
            {
                return index + 2;
            }
        }

        public int ifSightNotHit(Organism target, int index)
        {
            double cost = 0.25 * energyMod;
            target.drainEnergy(cost);

            if (!target.getSight().canSee())
            {
                return index + 1;
            }
            else
            {
                return index + 2;
            }
        }

        private double calculateSpeedCost(double speed)
        {
            return speed * 1.5;
        }

        public void createMethodBlacklist()
        {
            blacklistMethods.Add("createMethodBlacklist");
            blacklistMethods.Add("getBehaviorsLength");
            blacklistMethods.Add("ToString");
            blacklistMethods.Add("Equals");
            blacklistMethods.Add("GetHashCode");
            blacklistMethods.Add("GetType");
            blacklistMethods.Add("getBehavior");
            blacklistMethods.Add("getRandomBehavior");
            blacklistMethods.Add("calculateSpeedCost");
        }

        public int getBehaviorsLength()
        {
            return BehaviorList.Count;
        }
    }
}

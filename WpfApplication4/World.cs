using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace WpfApplication4
{
    class World
    {
        double worldWidth;
        double worldHeight;

        Timer worldClock;

        Behaviors behaviors;

        OrganismManager organismManager;

        FoodManager foodManager;

        long worldLiveTime;

        Random ranGen;

        public World()
        {
            ranGen = new Random();

            worldWidth = 750;
            worldHeight = 750;

            worldLiveTime = 0;

            worldClock = new Timer();

            worldClock.Interval = 1;

            worldClock.Elapsed += worldClock_Elapsed;

            behaviors = new Behaviors(this);

            organismManager = new OrganismManager(behaviors, this, ranGen);

            foodManager = new FoodManager(this, ranGen);

            worldClock.Start();
        }



        void worldClock_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (worldLiveTime > 100)
            {
                List<Organism> activeOrganisms = organismManager.getActiveOrganisms();

                try
                {
                    foreach (Organism organism in activeOrganisms)
                    {
                        organism.getSight().calculateBoundaries();
                        List<Food> nearbyFood = foodManager.checkFoodAroundPoint(organism.getPosition(), organism.getSight().getRange());

                        List<bool> sightHits = new List<bool>();
                        foreach (Food food in nearbyFood)
                        {
                            sightHits.Add(organism.getSight().isPointWithin(new Point(food.getPosition().X - (Food.getSize() / 2), food.getPosition().Y - (Food.getSize() / 2))));

                            if (checkDistance(food.getPosition(), organism.getPosition()) < Food.getSize() & !food.isEaten())
                            {
                                food.setEaten(true);
                                organism.restoreEnergy(200);
                            }
                        }

                        bool hasSight = false;
                        foreach(bool hit in sightHits)
                        {
                            if(hit)
                            {
                                hasSight = true;
                            }
                        }

                        if(hasSight)
                        {
                            organism.getSight().setCanSee(hasSight);
                        }

                        if (!organism.isExecuting())
                        {
                            organism.setExecuting(true);
                            organism.executeLine();
                        }
                    }
                }
                catch(InvalidOperationException)
                {

                }

            }

            if(worldLiveTime%30 == 0 & worldLiveTime >= 30)
            {
                foodManager.attemptFoodSpawn();
            }

            if(worldLiveTime%1000 == 0 & worldLiveTime >= 1000)
            { 
                save(); 
            }

            worldLiveTime++;

        }

        public Vector getWorldSize()
        {
            return new Vector(worldWidth, worldHeight);
        }

        public Point randomPointInWorld()
        {
            return new Point(ranGen.NextDouble() * getWorldSize().X, ranGen.NextDouble() * getWorldSize().Y);
        }

        private double checkDistance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow((point2.X - point1.X),2) + Math.Pow((point2.Y - point1.Y),2));
        }

        public void save()
        {
            organismManager.saveOrganisms();
        }

        public void pause()
        {
            worldClock.Stop();
        }

        public void resume()
        {
            worldClock.Start();
        }

        public OrganismManager getOrganismManager()
        {
            return organismManager;
        }

        public FoodManager getFoodManager()
        {
            return foodManager;
        }

    }
}

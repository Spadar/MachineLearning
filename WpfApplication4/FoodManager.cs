using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApplication4
{
    class FoodManager
    {
        public delegate void FoodAdditionEventHandler(Food source, EventArgs e);
        public event FoodAdditionEventHandler OnFoodAddition;

        public delegate void FoodDeletionEventHandler(Food source, EventArgs e);
        public event FoodDeletionEventHandler OnFoodDeletion;


        //3d food array
        //The purpose of this implementation is to limit the amount of searching that has to be done to locate which Food are near an organism.
        Food[,,] foodGrid;

        List<Food> activeFood;

        World world;

        Random ranGen;

        int arrayDepth;
        int gridSize;

        int maxFood;

        public FoodManager(World world, Random ranGen)
        {
            //The maximum amount of food that can exist within any particular cell
            arrayDepth = 10;

            //The span of the grid
            gridSize = 50;

            this.ranGen = ranGen;

            foodGrid = new Food[gridSize, gridSize, arrayDepth];

            activeFood = new List<Food>();

            this.world = world;

            ranGen = new Random();

            maxFood = 100;

        }

        public List<Food> getActiveFood()
        {
            return activeFood;
        }

        protected virtual void OnFoodAdditionEvent(Food source, EventArgs e)
        {
            if (OnFoodAddition != null)
            {
                OnFoodAddition(source, e);
            }
        }
        protected virtual void OnFoodDeletionEvent(Food source, EventArgs e)
        {
            if (OnFoodDeletion != null)
            {
                OnFoodDeletion(source, e);
            }
        }

        private int[] findGridCell(Point point)
        {
            int[] position = new int[2];

            position[0] = (int)Math.Ceiling(point.X / (world.getWorldSize().X / gridSize)) - 1;
            position[1] = (int)Math.Ceiling(point.Y / (world.getWorldSize().Y / gridSize)) - 1;

            return position;
        }

        private void addFood()
        {
            retry:
            Point position = new Point(ranGen.NextDouble() * world.getWorldSize().X, ranGen.NextDouble() * world.getWorldSize().Y);

            int[] arrayPosition = new int[3];

            int[] xyPos = findGridCell(position);

            int zPos = 0;

            //Find an empty slot in the foodGrid
            Food arrayTest = null;
            while((arrayTest = foodGrid[xyPos[0],xyPos[1],zPos]) != null)
            {
                zPos++;
                if(zPos > arrayDepth)
                {
                    goto retry;
                }
            }

            arrayPosition[0] = xyPos[0];
            arrayPosition[1] = xyPos[1];
            arrayPosition[2] = zPos;

            Food food = new Food(position, arrayPosition);

            foodGrid[arrayPosition[0], arrayPosition[1], arrayPosition[2]] = food;
            activeFood.Add(food);

            food.OnFoodEaten += food_OnFoodEaten;

            OnFoodAdditionEvent(food, EventArgs.Empty);
        }

        void food_OnFoodEaten(Food source, EventArgs e)
        {
            //When a food is eaten. Remove it and replace it with a new one.
            removeFood(source);
            //addFood();
        }

        private void removeFood(Food food)
        {
            int[] arrayLocation = food.getArrayLocation();
            foodGrid[arrayLocation[0],arrayLocation[1],arrayLocation[2]] = null;
            activeFood.Remove(food);

            OnFoodDeletionEvent(food, EventArgs.Empty);
        }

        public List<Food> checkFoodAroundPoint(Point point, double distance)
        {

            //Currently searching the entire area around the point. This could be further optimized by including a direction vector. 

            //Find how many cells our search should include
            int searchBreadth = (int)Math.Ceiling(distance / (world.getWorldSize().X / gridSize));

            List<Food> foundFood = new List<Food>();

            int[] xyPos = findGridCell(point);

            //Search the grid surrounding the central point for Food. Add any found to the list
            for(int x = -searchBreadth; x < searchBreadth; x++)
            {
                for(int y = -searchBreadth; y < searchBreadth; y++)
                {
                    for(int z = 0; z < arrayDepth; z++)
                    {
                        if (xyPos[0] + x >= 0 & xyPos[1] + y >= 0 & xyPos[0] + x <= 49 & xyPos[1] + y <= 49)
                        {
                            Food testFood = foodGrid[xyPos[0] + x, xyPos[1] + y, z];

                            if (testFood != null)
                            {
                                foundFood.Add(testFood);
                            }
                        }
                    }
                }
            }

            return foundFood;
        }

        public void createInitialFood()
        {
            while (activeFood.Count < maxFood)
            {
                addFood();
            }
        }

        public void attemptFoodSpawn()
        {
            if(activeFood.Count < maxFood)
            {
                addFood();
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApplication4
{
    class Food
    {
        bool eaten;
        static int size = 5;

        int[] arrayLocation;

        Point position;


        public delegate void FoodEatenEventHandler(Food source, EventArgs e);
        public event FoodEatenEventHandler OnFoodEaten;

        public Food(Point position, int[] arrayLocation)
        {
            this.eaten = false;
            this.position = position;
            this.arrayLocation = arrayLocation;

        }
        protected virtual void OnFoodEatenEvent(Food source, EventArgs e)
        {
            if (OnFoodEaten != null)
            {
                OnFoodEaten(source, e);
            }
        }

        public bool isEaten()
        {
            return eaten;
        }

        public static int getSize()
        {
            return size;
        }

        public void setEaten(bool eaten)
        {
            this.eaten = eaten;
            if(eaten)
            {
                OnFoodEatenEvent(this, EventArgs.Empty);
            }
        }

        public Point getPosition()
        {
            return position;
        }

        public int[] getArrayLocation()
        {
            return arrayLocation;
        }
    }
}

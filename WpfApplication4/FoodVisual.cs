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
using System.Timers;

namespace WpfApplication4
{
    class FoodVisual
    {
        Ellipse food;
        Food source;

        public delegate void SourceEatenEventHandler(FoodVisual source, EventArgs e);
        public event SourceEatenEventHandler OnSourceEaten;

        public FoodVisual(Food source, Ellipse food)
        {
            this.source = source;

            this.food = food;

            food.Dispatcher.Invoke(new Action(() => food.Height = Food.getSize()));
            food.Dispatcher.Invoke(new Action(() => food.Width = Food.getSize()));

            food.Dispatcher.Invoke(new Action(() => food.Fill = new SolidColorBrush(Colors.LightGreen)));

            source.OnFoodEaten += source_OnFoodEaten;
        }

        void source_OnFoodEaten(Food source, EventArgs e)
        {
            OnSourceEatenEvent(EventArgs.Empty);
        }

        protected virtual void OnSourceEatenEvent(EventArgs e)
        {
            if (OnSourceEaten != null)
            {
                OnSourceEaten(this, e);
            }
        }
        
        public void update()
        {
            food.Dispatcher.Invoke(new Action(() => Canvas.SetLeft(food, source.getPosition().X - (Food.getSize() / 2))));
            food.Dispatcher.Invoke(new Action(() => Canvas.SetTop(food, source.getPosition().Y - (Food.getSize() / 2))));
        }

        public UIElement getVisual()
        {
            return food;
        }
    }
}

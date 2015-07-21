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
    class OrganismVisual
    {
        Ellipse organism;
        Polygon sight;
        Organism source;
        TextBlock energy;

        PointCollection boundaries;

        public delegate void SourceDeathEventHandler(OrganismVisual source, EventArgs e);
        public event SourceDeathEventHandler OnSourceDeath;

        public OrganismVisual(Organism source, Ellipse organism, Polygon sight, PointCollection boundaries, TextBlock energy)
        {
            this.source = source;

            source.OnDeath += source_OnDeath;

            this.organism = organism;

            this.boundaries = boundaries;

            this.energy = energy;


            organism.Dispatcher.Invoke(new Action(() => organism.Fill = new SolidColorBrush(Colors.Red)));
            organism.Dispatcher.Invoke(new Action(() => organism.Height = source.getSize()));
            organism.Dispatcher.Invoke(new Action(() => organism.Width = source.getSize()));

            this.sight = sight;
            sight.Dispatcher.Invoke(new Action(() => sight.Fill = new SolidColorBrush(Color.FromArgb(50, 200, 200, 200))));
            sight.Dispatcher.Invoke(new Action(() => sight.Points = boundaries));

            energy.Dispatcher.Invoke(new Action(() => energy.Foreground = new SolidColorBrush(Colors.White)));
            energy.Dispatcher.Invoke(new Action(() => energy.HorizontalAlignment = HorizontalAlignment.Center));
            energy.Dispatcher.Invoke(new Action(() => energy.VerticalAlignment = VerticalAlignment.Center));
            energy.Dispatcher.Invoke(new Action(() => energy.TextAlignment = TextAlignment.Center));
            energy.Dispatcher.Invoke(new Action(() => energy.Height = 15));
            energy.Dispatcher.Invoke(new Action(() => energy.Width = 40));

            constructPointCollection();

        }

        void source_OnDeath(object sender, EventArgs e)
        {
            OnSourceDeathEvent(EventArgs.Empty);
        }

        protected virtual void OnSourceDeathEvent(EventArgs e)
        {
            if (OnSourceDeath != null)
            {
                OnSourceDeath(this, e);
            }
        }

        public void update()
        {
            double posX = source.getPosition().X;
            double posY = source.getPosition().Y;

            organism.Dispatcher.Invoke(new Action(() => Canvas.SetLeft(organism, posX - source.getSize()/2)));
            organism.Dispatcher.Invoke(new Action(() => Canvas.SetTop(organism, posY - source.getSize()/2)));

            boundaries.Dispatcher.Invoke(new Action(() => updatePointCollection()));

            if(source.getSight().canSee())
            {
                sight.Dispatcher.Invoke(new Action(() => sight.Fill = new SolidColorBrush(Color.FromArgb(50, 255, 200, 200))));
            }
            else
            {
                sight.Dispatcher.Invoke(new Action(() => sight.Fill = new SolidColorBrush(Color.FromArgb(50, 200, 200, 200))));
            }

            energy.Dispatcher.Invoke(new Action(() => Canvas.SetLeft(energy, (posX) - energy.Width/2)));
            energy.Dispatcher.Invoke(new Action(() => Canvas.SetTop(energy, (posY - source.getSize() / 2) - 15)));
            energy.Dispatcher.Invoke(new Action(() => energy.Text = ((int)source.getEnergy()).ToString()));
        }

        private void constructPointCollection()
        {
            foreach (Point point in source.getSight().getBoundaries())
            {
                boundaries.Dispatcher.Invoke(new Action(() => boundaries.Add(point)));
            }
        }

        private void updatePointCollection()
        {
            for(int i = 0; i < 3; i++)
            {
                boundaries.Dispatcher.Invoke(new Action(() => boundaries[i] = source.getSight().getBoundaries()[i]));
            }
        }

        public UIElement[] getVisuals()
        {
            return new UIElement[] {sight, organism, energy};
        }
    }
}

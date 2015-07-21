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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Timer mainClock;

        Canvas canvas;

        double windowWidth;
        double windowHeight;

        World world;

        List<FoodVisual> foodVisuals;
        List<OrganismVisual> organismVisuals;

        public MainWindow()
        {
            //Decide Window Height/Width
            windowHeight = 760;
            windowWidth = 760;

            InitializeComponent();

            //Initializing the main clock
            mainClock = new Timer();
            mainClock.Interval = 1;

            //Force the window to match the size of the content
            this.SizeToContent = SizeToContent.WidthAndHeight;
            //Initialize the canvas
            canvas = new Canvas();
            //Set the canvas size to the desired window width and height.
            canvas.Width = windowWidth;
            canvas.Height = windowHeight;
            canvas.Background = new SolidColorBrush(Colors.Black);
            //Display the canvas on the window
            this.Content = canvas;

            //As the last thing after inital setup, start the main clock.
            mainClock.Elapsed += mainClock_Elapsed;

            foodVisuals = new List<FoodVisual>();
            organismVisuals = new List<OrganismVisual>();

            world = new World();

            world.getOrganismManager().OnOrganismAddition += MainWindow_OnOrganismAddition;

            world.getFoodManager().OnFoodAddition += MainWindow_OnFoodAddition;
            
            mainClock.Start();

            world.getFoodManager().createInitialFood();
            world.getOrganismManager().loadOrganismsFromMemory();
        }

        void MainWindow_OnFoodAddition(Food source, EventArgs e)
        {
            FoodVisual visual = null;
            Dispatcher.Invoke(new Action(() => visual = new FoodVisual(source, new Ellipse())));
            foodVisuals.Add(visual);
            Dispatcher.Invoke(new Action(() => canvas.Children.Add(visual.getVisual())));
            visual.OnSourceEaten += visual_OnSourceEaten;

            visual.update();
        }

        void visual_OnSourceEaten(FoodVisual source, EventArgs e)
        {
            foodVisuals.Remove(source);
            Dispatcher.Invoke(new Action(() => canvas.Children.Remove(source.getVisual())));
        }

        void MainWindow_OnOrganismAddition(Organism source, EventArgs e)
        {
            OrganismVisual visual = null;
            Dispatcher.Invoke(new Action(() => visual = new OrganismVisual(source, new Ellipse(), new Polygon(), new PointCollection(), new TextBlock())));
            organismVisuals.Add(visual);
            foreach(UIElement drawable in visual.getVisuals())
            {
                Dispatcher.Invoke(new Action(() => canvas.Children.Add(drawable)));
            }
            visual.OnSourceDeath += visual_OnSourceDeath;
        }

        void visual_OnSourceDeath(OrganismVisual source, EventArgs e)
        {
            organismVisuals.Remove(source);
            foreach(UIElement visual in source.getVisuals())
            {
                Dispatcher.Invoke(new Action(() => canvas.Children.Remove(visual)));
            }
        }

        void mainClock_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                for (int i = 0; i < organismVisuals.Count; i++)
                {
                    organismVisuals[i].update();
                }
            }
            catch(InvalidOperationException)
            {

            }
        }

    }
}

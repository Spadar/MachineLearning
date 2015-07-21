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
using System.Reflection;


namespace WpfApplication4
{
    class Organism
    {
        Behaviors behaviors;

        Point position;

        Sight sight;

        GeneticCode geneticCode;

        double initialEnergy;

        double energy;

        Boolean alive;

        Guid id;

        public int index;

        bool executing;

        int size;

        double angle;

        int linesExecuted;

        int foodEaten;
        
        int timesReproduced;

        int fitness;

        int historicalSum;

        int generationNumber;

        //Event Delegate for death
        public delegate void DeathEventHandler(object sender, EventArgs e);
        //Event Delegate for Reproduction
        public delegate void ReproductionEventHandler(object sender, EventArgs e);

        //Events
        public event DeathEventHandler OnDeath;

        public event ReproductionEventHandler OnReproduction;



        public Organism(GeneticCode geneticCode, Behaviors behaviors, Point position)
        {
            inititializationProcedure(geneticCode, behaviors, position);

            id = System.Guid.NewGuid();

            generationNumber = 1;

        }

        public Organism(GeneticCode geneticCode, Behaviors behaviors, Point position, int generationNumber)
        {
            inititializationProcedure(geneticCode, behaviors, position);

            id = System.Guid.NewGuid();

            this.generationNumber = generationNumber;

        }

        public Organism(GeneticCode geneticCode, Guid guid, Point position, Behaviors behaviors, int historicalSum, int generationNumber)
        {
            this.historicalSum = historicalSum;
            this.generationNumber = generationNumber;

            inititializationProcedure(geneticCode, behaviors, position);

            id = guid;
        }

        private void inititializationProcedure(GeneticCode geneticCode, Behaviors behaviors, Point position)
        {
            this.geneticCode = geneticCode;

            this.position = position;

            initialEnergy = 100;

            energy = initialEnergy;

            alive = true;

            index = 0;

            size = 10;

            this.behaviors = behaviors;

            linesExecuted = 0;

            executing = false;

            angle = 0;

            sight = new Sight(this, 100, 40);
        }

        protected virtual void OnDeathEvent(EventArgs e)
        {
            if(OnDeath != null)
            {
                OnDeath(this, e);
            }
        }

        protected virtual void OnReproductionEvent(EventArgs e)
        {
            if(OnReproduction != null)
            {
                OnReproduction(this, e);
            }
        }

        public bool isAlive()
        {
            return alive;
        }

        public Point getPosition()
        {
            return position;
        }

        public double getEnergy()
        {
            return energy;
        }

        public double getAngle()
        {
            return angle;
        }

        public void setAngle(double angle)
        {
            this.angle = angle;
        }

        public int getHistoricalSum()
        {
            return historicalSum;
        }

        public int getHistoricalAverage()
        {
            return historicalSum / generationNumber;
        }

        public int getGeneration()
        {
            return generationNumber;
        }

        public void incrementGeneration()
        {
            generationNumber++;
        }

        public GeneticCode getGeneticCode()
        {
            return geneticCode;
        }

        public int getFitness()
        {
            return fitness;
        }

        public int getSize()
        {
            return size;
        }

        public Sight getSight()
        {
            return sight;
        }

        public Guid getGuid()
        {
            return id;
        }

        public void modifyPosition(Point position)
        {
            this.position = position;
        }

        public double getSpeed()
        {
            return geneticCode.getSpeed();
        }

        public void drainEnergy(double amount)
        {
            energy -= amount;
            if(energy < 0)
            {
                alive = false;
                fitness = evaluate();
                historicalSum =+ fitness;
                OnDeathEvent(EventArgs.Empty);
            }
        }

        public void reset()
        {
            linesExecuted = 0;
            foodEaten = 0;
            fitness = 0;
            timesReproduced = 0;
            alive = true;
            energy = initialEnergy;
            index = 0;
            angle = 0;
            executing = false;
        }

        public void restoreEnergy(double amount)
        {
            foodEaten += 1;
            energy += amount;
        }

        public bool isExecuting()
        {
            return executing;
        }

        public void setExecuting(bool executing)
        {
            this.executing = executing;
        }

        public void executeLine()
        {
            linesExecuted++;

            //Loop back to the start of the code if index goes past the end...
            if (index > geneticCode.getBehaviors().Count - 1)
            {
                index = 0;
            }

            Type behaviorType = Type.GetType(typeof(Behaviors).FullName);
            MethodInfo method = behaviorType.GetMethod(geneticCode.getBehaviors()[index]);
            ParameterInfo[] methodParameters = method.GetParameters();

            object[] parameters = new object[methodParameters.Length];

            BindingFlags flags = BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.NonPublic |
                    BindingFlags.Public;

            for (int i = 0; i < methodParameters.Length; i++)
            {
                string parameter = methodParameters[i].Name;


                if (parameter != "target")
                {
                    parameters[i] = GetType().GetField(parameter, flags).GetValue(this);
                }
                else
                {
                    parameters[i] = this;
                }

            }

            object response = method.Invoke(behaviors, parameters);

            index = (int)response;


            if(energy > 700)
            {
                OnReproductionEvent(EventArgs.Empty);
            }

            executing = false;
        }
        public int evaluate()
        {
            return (foodEaten * 70) + (timesReproduced * 35);
        }
    }
}

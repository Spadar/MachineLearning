using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace WpfApplication4
{
    class OrganismManager
    {
        Behaviors behaviors;

        World world;

        Random ranGen;

        List<Organism> activeOrganisms;

        List<Organism> deadOrganisms;

        List<Organism> genOriginals;

        int defaultGenSize;

        string genDirectory;

        string organismTerminate;

        string behaviorsInitiate;

        string constantsInitiate;

        double insertionRate;

        double deletionRate;

        double mutationRate;

        double maxSpeed;

        bool writing;

        //The number of 'top' organisms to carry over to a new simulation in the event of an extinction event
        int extinctionCarryOver;

        //Event for extinction
        public delegate void ExtinctionEventHandler(object sender, EventArgs e);
        public event ExtinctionEventHandler OnExtinction;


        //These events will track whenever an organism is added or removed from activeOrganisms
        public delegate void OrganismAdditionEventHandler(Organism source, EventArgs e);
        public event OrganismAdditionEventHandler OnOrganismAddition;

        public delegate void OrganismDeletionEventHandler(Organism source, EventArgs e);
        public event OrganismDeletionEventHandler OnOrganismDeletion;

        public OrganismManager(Behaviors behaviors, World world, Random ranGen)
        {

            this.world = world;

            this.ranGen = ranGen;

            readConfig();

            maxSpeed = 1.5;

            defaultGenSize = 30;

            extinctionCarryOver = 15;

            this.behaviors = behaviors;

            genDirectory = "Organisms";

            organismTerminate = "[/Organism]";
            behaviorsInitiate = "[Behaviors]";
            constantsInitiate = "[Constants]";

            if(!System.IO.Directory.Exists(genDirectory))
            {
                Directory.CreateDirectory(genDirectory);
            }

            if(!File.Exists(genDirectory + "//" + "Organisms.txt"))
            {
                List<Organism> defaultOrganisms = new List<Organism>();

                //Add 10 random organisms
                for(int i = 0; i<defaultGenSize;i++)
                {
                    defaultOrganisms.Add(newRandomOrganism());
                }

                writeOrganismsToFile(defaultOrganisms);
            }

            activeOrganisms = new List<Organism>();

            deadOrganisms = new List<Organism>();

            genOriginals = new List<Organism>();

            OnExtinction += OrganismManager_OnExtinction;

        }

        public void loadOrganismsFromMemory()
        {
            activeOrganisms = readOrganismsFromFile();

            //Register death and reproduction events for each organism loaded
            foreach (Organism organism in activeOrganisms)
            {
                genOriginals.Add(organism);
                organism.OnDeath += organism_Death;
                organism.OnReproduction += organism_Reproduction;
                OnOrganismAdditionEvent(organism, EventArgs.Empty);
            }

        }

        protected virtual void OnOrganismAdditionEvent(Organism source, EventArgs e)
        {
            if (OnOrganismAddition != null)
            {
                OnOrganismAddition(source, e);
            }
        }
        protected virtual void OnOrganismDeletionEvent(Organism source, EventArgs e)
        {
            if (OnOrganismDeletion != null)
            {
                OnOrganismDeletion(source, e);
            }
        }

        public List<Organism> getActiveOrganisms()
        {
            return activeOrganisms;
        }

        void OrganismManager_OnExtinction(object sender, EventArgs e)
        {
            world.pause();

            //unhook events from all organisms
            foreach(Organism organism in deadOrganisms)
            {
                organism.OnDeath -= organism_Death;
                organism.OnReproduction -= organism_Reproduction;
            }

            //Evaluate all dead organisms, find the top 5.
            Organism[] leaders = new Organism[extinctionCarryOver];

            IEnumerable<Organism> sortedOrganisms = deadOrganisms.OrderBy(criteria => criteria.getHistoricalAverage());

            for (int i = 0; i < extinctionCarryOver; i++)
            {
                leaders[i] = sortedOrganisms.ElementAt((deadOrganisms.Count - i)-1);
            }

            logBest(leaders[0].getHistoricalAverage());

            deadOrganisms = new List<Organism>();

            genOriginals = new List<Organism>();

            foreach(Organism leader in leaders)
            {
                leader.reset();
                leader.modifyPosition(world.randomPointInWorld());
                leader.incrementGeneration();
            }

            //Carry the leaders onto the next generation
            activeOrganisms.AddRange(leaders);

                List<Organism> newOrganisms = new List<Organism>();

                for (int y = 0; y < (defaultGenSize - extinctionCarryOver); y++)
                {
                    if (y == 0 | y == 1)
                    {
                        newOrganisms.Add(newRandomOrganism());
                    }
                    else
                    {
                        newOrganisms.Add(reproduceAsexually(leaders[y - 2]));
                    }
                }

                activeOrganisms.AddRange(newOrganisms);

            //Register events for organisms
            foreach (Organism organism in activeOrganisms)
            {
                genOriginals.Add(organism);
                organism.OnDeath += organism_Death;
                organism.OnReproduction += organism_Reproduction;
                OnOrganismAdditionEvent(organism, EventArgs.Empty);
            }

            saveOrganisms();

            world.resume();
        }

        protected virtual void OnExtinctionEvent(EventArgs e)
        {
            if (OnExtinction != null)
            {
                OnExtinction(this, e);
            }
        }

        void organism_Reproduction(object sender, EventArgs e)
        {
            if (activeOrganisms.Count < 100)
            {
                int cost = 300;
                Organism organism = (Organism)sender;
                organism.drainEnergy(cost);

                if (organism.isAlive())
                {
                    Organism child = reproduceAsexually(organism);
                    child.OnDeath += organism_Death;
                    child.OnReproduction += organism_Reproduction;
                    activeOrganisms.Add(child);
                    OnOrganismAdditionEvent(child, EventArgs.Empty);
                }
            }
        }

        void organism_Death(object sender, EventArgs e)
        {
            deadOrganisms.Add((Organism)sender);
            activeOrganisms.Remove((Organism)sender);
            OnOrganismDeletionEvent((Organism)sender, EventArgs.Empty);

            if(activeOrganisms.Count == 0)
            {
                OnExtinctionEvent(EventArgs.Empty);
            }
        }

        //This method will be used to create a completely new randomly generated organism,
        //with absolutely no inherited. 
        public Organism newRandomOrganism()
        {

            List<string> code = new List<string>();

            List<int> constants = new List<int>();

            for(int i = 0; i < 3; i++)
            {
                int index = ranGen.Next(behaviors.getBehaviorsLength());
                string behavior = behaviors.getBehavior(index);
                code.Add(behavior);
                if(behavior == "constantValue")
                {
                    constants.Add(ranGen.Next(30));
                }
            }

            GeneticCode genes = new GeneticCode(code, ranGen.NextDouble()*maxSpeed, constants);

            return new Organism(genes, behaviors, world.randomPointInWorld());
        }

        public void writeOrganismsToFile(List<Organism> organisms)
        {

            if (!writing)
            {
                writing = true;

                StreamWriter writer = new StreamWriter(genDirectory + "//" + "Organisms.txt");

                foreach (Organism organism in organisms)
                {
                    writer.WriteLine(organism.getGuid());
                    writer.WriteLine(organism.getGeneticCode().getSpeed());
                    writer.WriteLine(organism.getHistoricalSum());
                    writer.WriteLine(organism.getGeneration());
                    writer.WriteLine(behaviorsInitiate);

                    foreach (String op in organism.getGeneticCode().getBehaviors())
                    {
                        writer.WriteLine(op);
                    }

                    writer.WriteLine(constantsInitiate);

                    foreach (int constant in organism.getGeneticCode().getConstants())
                    {
                        writer.WriteLine(constant);
                    }

                    writer.WriteLine(organismTerminate);
                }

                writer.Close();

                writing = false;
            }
        }

        public List<Organism> readOrganismsFromFile()
        {
            StreamReader reader = new StreamReader(genDirectory + "//" + "Organisms.txt");

            List<Organism> output = new List<Organism>();

            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    Guid guid = Guid.Parse(line);
                    line = reader.ReadLine();
                    double speed = double.Parse(line);
                    line = reader.ReadLine();
                    int historicalSum = int.Parse(line);
                    line = reader.ReadLine();
                    int generation = int.Parse(line);
                    List<String> behaviors = new List<String>();
                    List<int> constants = new List<int>();
                    if (reader.ReadLine() == behaviorsInitiate)
                    {
                        while ((line = reader.ReadLine()) != constantsInitiate)
                        {
                            behaviors.Add(line);
                        }
                        while ((line = reader.ReadLine()) != organismTerminate)
                        {
                            constants.Add(int.Parse(line));
                        }
                    }

                    GeneticCode genes = new GeneticCode(behaviors, speed, constants);

                    output.Add(new Organism(genes, guid, world.randomPointInWorld(), this.behaviors, historicalSum, generation));
                }
                catch (FormatException)
                {
                }
            }

            reader.Close();

            return output;
        }

        private void readConfig()
        {
            if(!File.Exists("Configuration.txt"))
            {
                createDefaultConfig();
            }

            StreamReader reader = new StreamReader("Configuration.txt");

            string line;
            while((line = reader.ReadLine()) != null)
            {
                if(line.Contains("[Insertion Rate] = "))
                {
                    line = line.Replace("[Insertion Rate] = ", "");
                    insertionRate = double.Parse(line);
                }
                else if (line.Contains("[Deletion Rate] = "))
                {
                    line = line.Replace("[Deletion Rate] = ", "");
                    deletionRate = double.Parse(line);
                }
                else if (line.Contains("[Mutation Rate] = "))
                {
                    line = line.Replace("[Mutation Rate] = ", "");
                    mutationRate = double.Parse(line);
                }
            }

            reader.Close();
        }

        public void saveOrganisms()
        {
            List<Organism> allCurrentOrganisms = new List<Organism>();
            allCurrentOrganisms.AddRange(genOriginals);

            foreach (Organism organism in activeOrganisms)
            {
                if (!allCurrentOrganisms.Contains(organism))
                {
                    allCurrentOrganisms.Add(organism);
                }
            }

            writeOrganismsToFile(allCurrentOrganisms);
        }

        private void createDefaultConfig()
        {
            StreamWriter writer = new StreamWriter("Configuration.txt");

            writer.WriteLine("[Insertion Rate] = 0.00");
            writer.WriteLine("[Deletion Rate] = 0.00");
            writer.WriteLine("[Mutation Rate] = 0.00");

            writer.Close();

        }

        public Organism reproduceAsexually(Organism parent)
        {
            List<string> newCode = new List<string>();

            List<int> newConstants = new List<int>();

            int constantCounter = 0;

            double newSpeed;

            foreach (string behavior in parent.getGeneticCode().getBehaviors())
            {
                bool changed = false;
                string newBehavior = copyBehaviorAsexually(behavior, out changed);

                if (insertionRoll())
                {
                    string insertedBehavior = behaviors.getRandomBehavior();
                    newCode.Add(insertedBehavior);
                    if(insertedBehavior == "constantValue")
                    {
                        newConstants.Insert(constantCounter, ranGen.Next(behaviors.getBehaviorsLength()));
                    }
                    
                }

                if (!deletionRoll())
                {
                    newCode.Add(newBehavior);
                    if(changed)
                    {
                        newConstants.Insert(constantCounter, ranGen.Next(behaviors.getBehaviorsLength()));
                    }
                    else
                    {
                        if(newBehavior == "constantValue")
                        {
                            newConstants.Insert(constantCounter, parent.getGeneticCode().getConstants()[constantCounter]);
                            constantCounter++;
                        }
                    }
                }
                else 
                {
                }
            }



            if (mutationRoll())
            {
                newSpeed = ranGen.NextDouble() * maxSpeed;
            }
            else
            {
                newSpeed = parent.getGeneticCode().getSpeed();
            }

            //An organism cannot have 0 behaviors
            if(newCode.Count == 0)
            {
                int index = ranGen.Next(behaviors.getBehaviorsLength());
                newCode.Add(behaviors.getBehavior(index));
            }

            GeneticCode newGeneticCode = new GeneticCode(newCode, newSpeed, newConstants);

            return new Organism(newGeneticCode, behaviors, world.randomPointInWorld(), parent.getGeneration() + 1);
        }

        private string copyBehaviorAsexually(string behavior, out bool changed)
        {
            double chance = ranGen.NextDouble();

            string newBehavior;

            changed = false;

            if (chance <= mutationRate)
            {
                int index = ranGen.Next(behaviors.getBehaviorsLength());
                newBehavior = behaviors.getBehavior(index);

                if(newBehavior == "constantValue" & behavior != "constantValue")
                {
                    changed = true;
                }
            }
            else
            {
                newBehavior = behavior;
            }

            return newBehavior;
        }

        private bool mutationRoll()
        {
            double chance = ranGen.NextDouble();
            if (chance <= mutationRate)
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        private bool insertionRoll()
        {
            double chance = ranGen.NextDouble();
            if (chance <= insertionRate)
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        private bool deletionRoll()
        {
            double chance = ranGen.NextDouble();
            if (chance <= deletionRate)
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        private void logBest(int topFitness)
        {
            using (StreamWriter sw = File.AppendText("Log.txt"))
            {
                sw.WriteLine(topFitness);
                sw.Close();
            }
        }

    }
}

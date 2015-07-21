using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication4
{
    //This represents a list of behaviors that will be used by a specific progeny in their evaluation
    class GeneticCode
    {
        List<String> behaviors;

        double speed;

        List<int> constants;


        public GeneticCode(List<String> behaviors, double speed, List<int> constants)
        {
            this.behaviors = behaviors;
            this.speed = speed;
            this.constants = constants;
        }

        public List<String> getBehaviors()
        {
            return behaviors;
        }

        public List<int> getConstants()
        {
            return constants;
        }

        public double getSpeed()
        {
            return speed;
        }

    }
}

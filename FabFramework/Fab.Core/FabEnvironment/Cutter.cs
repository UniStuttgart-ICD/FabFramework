using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Fab.Core;

namespace Fab.Core.FabEnvironment
{
    public class Cutter : Tool
    {
        public double Length
        {
            get { return length; }
            set { length = value; } 
        }

        public double Diameter
        {
            get { return diameter; }
            set { diameter = value; }
        }

        public double Width
        {
            get { return width; }
            set { width = value; }
        }

        public double MinImmersion
        {
            get { return minImmersion; }
            set { minImmersion = value; }
        }

        private double length;
        private double diameter;
        private double width;
        private double minImmersion;

        public Cutter() : base() { }
        public Cutter(int toolNum, string name) : base(toolNum, name) { }
        public Cutter(
            int toolNum, string name, Plane tcpPln, Plane mountPln, List<GeometryBase> geometry, Dictionary<string, Action> actions, 
            double length, double diameter, double width, double minImmersion)
            : base(toolNum, name, tcpPln, mountPln, geometry, actions)
        { 
            this.length = length;
            this.diameter = diameter;
            this.width = width;
            this.minImmersion = minImmersion;
        }
    }
}

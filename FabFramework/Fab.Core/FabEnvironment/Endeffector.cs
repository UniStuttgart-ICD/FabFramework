using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Fab.Core;

namespace Fab.Core.FabEnvironment
{
    public class Endeffector : Tool
    {

        public double MaxWeight
        {
            get { return maxWeight; }
            set { maxWeight = value; }
        }
        public double FootprintWidth
        {
            get { return footprintWidth; }
            set { footprintWidth = value; }
        }
        public double FootprintLength
        {
            get { return footprintLength; }
            set { footprintLength = value; }
        }
        public double FootprintHeight
        {
            get { return footprintHeight; }
            set { footprintHeight = value; }
        }
        public double ZOffset
        {
            get { return zOffset; }
            set { zOffset = value; }
        }

        private double maxWeight;
        private double footprintWidth;
        private double footprintLength;
        private double footprintHeight;
        private double zOffset;

        public Endeffector() : base() { }
        public Endeffector(int toolNum, string name) : base(toolNum, name) { }
        public Endeffector(
            int toolNum, string name, Plane tcpPln, Plane mountPln, List<GeometryBase> geometry, Dictionary<string, Action> actions,
            double maxWeight, double footprintWidth, double footprintLength, double footprintHeight, double zOffset)
            : base(toolNum, name, tcpPln, mountPln, geometry, actions)
        {
            this.maxWeight = maxWeight;
            this.footprintWidth = footprintWidth;
            this.footprintLength = footprintLength;
            this.footprintHeight = footprintHeight;
            this.zOffset = zOffset;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Fab.Core.DesignElement
{
    public class DesignBeam : DesignElement
    {
        //Properties

        public Line BaseLine
        {
            get { return baseLine; }
            set { 
                baseLine = value;
                startPoint = baseLine.From;
                endPoint = baseLine.To;
            }
        }
        public Point3d StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; }
        }
        public Point3d EndPoint
        {
            get { return endPoint; }
            set { endPoint = value; }
        }

        private Line baseLine;
        private Point3d startPoint;
        private Point3d endPoint;

        public DesignBeam() : base() { }
        public DesignBeam(string name) : base(name) { }

    }
}

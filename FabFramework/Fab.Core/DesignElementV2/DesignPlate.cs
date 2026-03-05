using Rhino.Geometry;

namespace Fab.Core.DesignElement
{
    public class DesignPlate : DesignElement
    {

        public Polyline Outline
        {
            get { return outline; }
            set { outline = value; }
        }

        private Polyline outline;

        public DesignPlate() : base() { }
        public DesignPlate(string name) : base(name) { }

    }
}

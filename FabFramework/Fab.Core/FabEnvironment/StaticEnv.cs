using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace Fab.Core.FabEnvironment
{

    public class StaticEnv
    {
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public List<Plane> RefPln
        {
            get { return refPln; }
            set { refPln = value; }
        }
        public List<Plane> AlignPln
        {
            get { return alignPln; }
            set { alignPln = value; }
        }

        public List<Plane> SafePosition
        {
            get { return safePosition; }
            set { safePosition = value; }
        }

        public double Width
        {
            get { return width; }
            set { width = value; }
        }
        public double Length
        {
            get { return length; }
            set { length = value; }
        }
        public double Height
        {
            get { return height; }
            set { height = value; }
        }
        public List<GeometryBase> Geometry
        {
            get { return geometry; }
            set
            {
                geometry = value;
                geometryColors = new List<Color>();
                for (int i = 0; i < geometry.Count; i++) geometryColors.Add(Color.LightGray);
            }
        }

        /// Dictionary for optional additional data.
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();

        private int index;
        private string name;
        private List<Plane> refPln;
        private List<Plane> alignPln;
        private List<Plane> safePosition;
        private double width;
        private double length;
        private double height;
        private List<GeometryBase> geometry;
        private List<Color> geometryColors;

        public StaticEnv() { }

        public StaticEnv(int index, string name, List<Plane> refPlane, List<Plane> alignPln, double width, double length, double height, List<GeometryBase> geometry)
        {

            this.index = index;
            this.name = name;
            this.refPln = refPlane;
            this.alignPln = alignPln;
            this.width = width;
            this.length = length;
            this.height = height;

            this.geometry = geometry;
        }

    }
}

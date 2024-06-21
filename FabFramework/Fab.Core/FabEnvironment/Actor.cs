using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using System.Drawing;

namespace Fab.Core.FabEnvironment
{
    public class Actor
    {
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public Plane RefPln
        {
            get { return refPln; }
            set { refPln = value; }
        }
        public Dictionary<string, Tool> Tools
        {
            get { return tools; }
            set { tools = value; }
        }
        public Dictionary<string, Action> Actions
        {
            get { return actions; }
            set { actions = value; }
        }
        public Line LinearAxis
        {
            get { return linearAxis; }
            set { linearAxis = value; }
        }
        public List<GeometryBase> Geometry
        {
            get { return geometry; }
            set
            {
                geometry = value;
                geometryColors = new List<Color>();
                for (int i = 0; i < geometry.Count; i++) geometryColors.Add(Color.LightGray);
                this.boundingBox = FabUtilities.FabUtilities.CreateBoundingBox(geometry);
                this.length = boundingBox.Max.X - boundingBox.Min.X;
                this.width = boundingBox.Max.Y - boundingBox.Min.Y;
                this.height = boundingBox.Max.Z - boundingBox.Min.Z;
            }
        }
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
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
        public double MaxWeight
        {
            get { return maxWeight; }
            set { maxWeight = value; }
        }

        private string name;
        private Plane refPln;

        private Dictionary<string, Tool> tools;
        private Dictionary<string, Action> actions;

        private Line linearAxis;

        private List<GeometryBase> geometry;
        private List<Color> geometryColors;

        private BoundingBox boundingBox;
        private double width;
        private double length;
        private double height;

        private double maxWeight;

        public Actor() { }

        public Actor(string name) { this.name = name; }

        public Actor(string name, Plane refPlane)
        { 
            this.name = name;
            this.refPln = refPlane;
        }

        public Actor(string name, Plane refPlane, List<GeometryBase> geometry, Dictionary<string, Tool> tools, Dictionary<string, Action> actions)
        {
            this.name = name;
            this.refPln = refPlane;
            this.geometry = geometry;
            this.tools = tools;
            this.actions = actions;
        }


    }
}

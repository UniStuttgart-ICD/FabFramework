using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace Fab.Core.DesignElement
{
    public class DesignRegion
    {
        #region properties
        //Properties
        public string Name
        {
            get
            {
                if (name == null)
                {
                    throw new InvalidOperationException("Name is null");
                }
                return name;
            }
        }
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public Dictionary<string, bool> BoolDict
        {
            get { return boolDict; }
            set { boolDict = value; }
        }
        public Dictionary<string, int> IntDict
        {
            get { return intDict; }
            set { intDict = value; }
        }
        public Dictionary<string, double> DoubleDict
        {
            get { return doubleDict; }
            set { doubleDict = value; }
        }
        public Dictionary<string, string> StringDict
        {
            get { return stringDict; }
            set { stringDict = value; }
        }

        public Dictionary<string, Point3d> PointDict
        {
            get { return pointDict; }
            set { pointDict = value; }
        }
        public Dictionary<string, Vector3d> VectorDict
        {
            get { return vectorDict; }
            set { vectorDict = value; }
        }
        public Dictionary<string, Plane> PlaneDict
        {
            get { return planeDict; }
            set { planeDict = value; }
        }
        public Dictionary<string, Line> LineDict
        {
            get { return lineDict; }
            set { lineDict = value; }
        }

        public Dictionary<string, List<bool>> BoolsDict
        {
            get { return boolsDict; }
            set { boolsDict = value; }
        }
        public Dictionary<string, List<int>> IntsDict
        {
            get { return intsDict; }
            set { intsDict = value; }
        }

        public Dictionary<string, List<double>> DoublesDict
        {
            get { return doublesDict; }
            set { doublesDict = value; }
        }
        public Dictionary<string, List<string>> StringsDict
        {
            get { return stringsDict; }
            set { stringsDict = value; }
        }

        public Dictionary<string, List<Point3d>> PointsDict
        {
            get { return pointsDict; }
            set { pointsDict = value; }
        }
        public Dictionary<string, List<Vector3d>> VectorsDict
        {
            get { return vectorsDict; }
            set { vectorsDict = value; }
        }
        public Dictionary<string, List<Plane>> PlanesDict
        {
            get { return planesDict; }
            set { planesDict = value; }
        }
        public Dictionary<string, List<Line>> LinesDict
        {
            get { return linesDict; }
            set { linesDict = value; }
        }


        #endregion

        //Field of Variables
        private string name;
        private int index;
        private string type;

        //Single Variables
        private Dictionary<string, int> intDict;
        private Dictionary<string, double> doubleDict;
        private Dictionary<string, string> stringDict;
        private Dictionary<string, bool> boolDict;

        private Dictionary<string, Point3d> pointDict;
        private Dictionary<string, Vector3d> vectorDict;
        private Dictionary<string, Plane> planeDict;
        private Dictionary<string, Line> lineDict;

        //List Variables
        private Dictionary<string, List<int>> intsDict;
        private Dictionary<string, List<double>> doublesDict;
        private Dictionary<string, List<string>> stringsDict;
        private Dictionary<string, List<bool>> boolsDict;

        private Dictionary<string, List<Point3d>> pointsDict;
        private Dictionary<string, List<Vector3d>> vectorsDict;
        private Dictionary<string, List<Plane>> planesDict;
        private Dictionary<string, List<Line>> linesDict;


        public DesignRegion() { }

        public DesignRegion(string name)
        {
            this.name = name;

            intDict = new Dictionary<string, int>();
            doubleDict = new Dictionary<string, double>();
            stringDict = new Dictionary<string, string>();
            boolDict = new Dictionary<string, bool>();

            pointDict = new Dictionary<string, Point3d>();
            vectorDict = new Dictionary<string, Vector3d>();
            planeDict = new Dictionary<string, Plane>();
            lineDict = new Dictionary<string, Line>();

            intsDict = new Dictionary<string, List<int>>();
            doublesDict = new Dictionary<string, List<double>>();
            stringsDict = new Dictionary<string, List<string>>();
            boolsDict = new Dictionary<string, List<bool>>();

            pointsDict = new Dictionary<string, List<Point3d>>();
            vectorsDict = new Dictionary<string, List<Vector3d>>();
            planesDict = new Dictionary<string, List<Plane>>();
            linesDict = new Dictionary<string, List<Line>>();

        }

    }
}

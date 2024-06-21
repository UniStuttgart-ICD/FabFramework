using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fab.Core.FabEnvironment
{
    public class Tool
    {
        public int ToolNum
        {
            get { return toolNum; }
            set { toolNum = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Id
        {
            get
            {
                if (id == null)
                {
                    id = Guid.NewGuid().ToString();
                }
                return id;
            }

            set { id = value; }
        }

        public Plane TcpPln
        {
            get { return tcpPln; }
            set { tcpPln = value; }
        }

        public Plane MountPln
        {
            get { return mountPln; }
            set { mountPln = value; }
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

        public Dictionary<string, Action> Actions
        {
            get { return actions; }
            set { actions = value; }
        }


        private int toolNum;
        private string name;
        private string id;

        private Plane tcpPln;
        private Plane mountPln;
        private List<GeometryBase> geometry;
        private List<Color> geometryColors;

        private Dictionary<string, Action> actions;

        public Tool()
        {
            this.id = Guid.NewGuid().ToString();
        }

        public Tool(int toolNum, string name) : this()
        {
            this.toolNum = toolNum;
            this.name = name;
        }

        public Tool(int toolNum, string name, Plane tcpPln, Plane mountPln, List<GeometryBase> geometry, Dictionary<string, Action> actions)
            : this(toolNum, name)
        {
            TcpPln = tcpPln;
            MountPln = mountPln;
            Geometry = geometry;

            geometryColors = new List<Color>();
            for (int i = 0; i < geometry.Count; i++) geometryColors.Add(Color.LightGray);

            Actions = actions;
        }

    }
}

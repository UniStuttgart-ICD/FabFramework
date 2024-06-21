using Fab.Core.FabEnvironment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fab.Core.FabElement
{
    public class FabElement
    {
        #region properties
        //Properties
        FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();
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

        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        public string ElementName
        {
            get { return elementName; }
            set { elementName = value; }
        }
        public string ParentComponentName
        {
            get { return parentComponentName; }
            set { parentComponentName = value; }
        }
        public string DesignElementName
        {
            get { return designElementName; }
            set { designElementName = value; }
        }
        public GeometryBase Geometry
        {
            get { return geometry; }
            set { geometry = value; }
        }
        public Plane RefPln_Situ
        {
            get { return refPln_Situ; }
            set
            {
                refPln_Situ = value;
                transform_Box = Transform.PlaneToPlane(RefPln_Situ, RefPln_SituBox);
            }
        }
        public Plane RefPln_SituBox
        {
            get { return refPln_SituBox; }
            set { refPln_SituBox = value; }
        }
        public Plane RefPln_Mag
        {
            get { return refPln_Mag; }
            set
            {
                refPln_Mag = value;
                Transform_Spawn = Transform.PlaneToPlane(RefPln_Situ, RefPln_Mag);
            }
        }
        public Plane RefPln_Fab
        {
            get { return refPln_Fab; }
            set { refPln_Fab = value; }
        }
        public Plane RefPln_FabOut
        {
            get { return refPln_FabOut; }
            set { refPln_FabOut = value; }
        }
        public Double Angle_FabOut
        {
            get { return angle_FabOut; }
            set { angle_FabOut = value; }
        }

        public Transform Transform_Box
        {
            get { return transform_Box; }
            set { transform_Box = value; }
        }
        public Transform Transform_Spawn
        {
            get { return transform_Spawn; }
            set { transform_Spawn = value; }
        }
        public StaticEnv EnvFab
        {
            get { return envFab; }
            set { envFab = value; }
        }
        public StaticEnv EnvMag
        {
            get { return envMag; }
            set { envMag = value; }
        }
        public List<string> FabTasksName
        {
            get { return fabTasksName; }
            set { fabTasksName = value; }
        }


        #endregion

        //Field of Variables
        private string name;
        private string id;
        private int index;

        private string elementName;
        private string parentComponentName;
        private string designElementName;

        private GeometryBase geometry;

        private Plane refPln_Situ;
        private Plane refPln_SituBox;
        private Plane refPln_Mag;
        private Plane refPln_Fab;
        private Plane refPln_FabOut;
        private Double angle_FabOut;

        private Transform transform_Spawn;
        private Transform transform_Box;

        private StaticEnv envFab;
        private StaticEnv envMag;

        private List<string> fabTasksName;

        public FabElement()
        {
            this.id = Guid.NewGuid().ToString();
            this.fabTasksName = new List<string>();
        }

        public FabElement(string name)
        {
            this.id = Guid.NewGuid().ToString();
            this.fabTasksName = new List<string>();

            if (name != null)
            {
                this.name = SetFabElementName(name, this);
                fabCollection.AddFabElement(this);
            }

        }

        public FabElement(string name, FabComponent parentComponent, DesignElement.DesignElement designElement, StaticEnv envMag) : base()
        {
            this.id = Guid.NewGuid().ToString();
            this.index = designElement.Index;
            this.fabTasksName = new List<string>();

            if (name != null)
            {
                this.name = SetFabElementName(name, this);
                fabCollection.AddFabElement(this);
            }

            this.elementName = designElement.ElementName;
            this.designElementName = designElement.Name;
            this.parentComponentName = parentComponent.Name;
            this.designElementName = designElement.Name;
            this.envMag = envMag;
            this.envFab = parentComponent.envFab;

            this.geometry = FabUtilities.FabUtilities.ConvertToJoinedGeometry(designElement.Geometry);
        }

        private static string SetFabElementName(string name, FabElement designElement)
        {
            // Check if name is null
            if (name == null)
            {
                throw new ArgumentNullException("Name cannot be null");
            }

            StringBuilder sb = new StringBuilder();

            // FabElement
            sb.Append("FE_");

            if (designElement is FabPlate designPlate)
            {
                sb.Append("P_");
            }
            else if (designElement is FabBeam designBeam)
            {
                sb.Append("B_");
            }
            else if (designElement is FabComponent designComponent)
            {
                sb.Append("C_");
            }

            sb.Append(name);

            return sb.ToString();
        }


        public FabElement ShallowCopy()
        {
            return (FabElement)MemberwiseClone();
        }

        public FabComponent GetParentComponent()
        {
            if (fabCollection.fabComponentCollection.ContainsKey(parentComponentName))
            {
                return fabCollection.fabComponentCollection[parentComponentName];
            }
            else
            {
                throw new KeyNotFoundException($"Key '{parentComponentName}' not found in the fabComponentCollection dictionary.");
            }
        }

        public Plane GetRefPlnSituBox()
        {
            DesignElement.DesignElement designElement = fabCollection.designElementCollection[DesignElementName];
            Plane elementRefPln_SituBox = designElement.FrameBase.Clone();
            elementRefPln_SituBox.Origin = designElement.BoundingBox.PointAt(0, 0.0, 0.0);

            return elementRefPln_SituBox;
        }

        public Plane GetRefPlnSitu()
        {
            DesignElement.DesignElement designElement = fabCollection.designElementCollection[DesignElementName];
            Plane elementRefPln_Situ = RefPln_SituBox.Clone();
            elementRefPln_Situ.Origin = designElement.BoundingBox.PointAt(0.5, 0.5, 0.0);

            return elementRefPln_Situ;
        }

        public DesignElement.DesignElement GetDesignElement()
        {
            if (fabCollection.designElementCollection.ContainsKey(designElementName))
            {
                return fabCollection.designElementCollection[designElementName];
            }
            else
            {
                throw new KeyNotFoundException($"Key '{designElementName}' not found in the designElementCollection dictionary.");
            }
        }

    }
}

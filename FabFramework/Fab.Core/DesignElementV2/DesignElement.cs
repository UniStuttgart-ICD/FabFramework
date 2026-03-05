using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;


namespace Fab.Core.DesignElement
{
    public class DesignElement
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
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string ParentComponentName
        {
            get { return parentComponentName; }
            set { parentComponentName = value; }
        }

        public Dictionary<string, DesignRegion> DesignRegion
        {
            get { return designRegion; }
            set { designRegion = value; }
        }

        public Brep Geometry
        {
            get { return geometry; }
            set { geometry = value; }
        }
        public Box BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                FrameLower = FabUtilities.FabUtilities.OffsetPlane(FrameBase, (-1) * Height / 2, FrameBase.Normal);
                FrameUpper = FabUtilities.FabUtilities.OffsetPlane(FrameBase, Height / 2, FrameBase.Normal);
            }
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
        public double Density
        {
            get { return density; }
            set { density = value; }
        }
        public Plane FrameBase
        {
            get { return frameBase; }
            set
            {
                frameBase = value;

                //ADD HERE IF BOUNDINGBOX NOT SET
                if (BoundingBox.Equals(Box.Empty) || BoundingBox.IsValid == false)
                {
                    BoundingBox = FabUtilities.FabUtilities.CreateAlignedBox(Geometry, frameBase);
                }

                //Get Centre frame of boundingBox
                Plane centreFrameBase = frameBase.Clone();
                centreFrameBase.Origin = BoundingBox.Center;

                // Project the frame Origin onto the plane
                Point3d projected_frame_origin = centreFrameBase.ClosestPoint(frameBase.Origin);
                frameBase.Origin = projected_frame_origin;

                Height = Math.Round(boundingBox.Z[1] - boundingBox.Z[0], 2);
                Width = Math.Round(boundingBox.Y[1] - boundingBox.Y[0], 2);
                Length = Math.Round(boundingBox.X[1] - boundingBox.X[0], 2);
            }
        }
        public Plane FrameLower
        {
            get { return frameLower; }
            set { frameLower = value; }
        }
        public Plane FrameUpper
        {
            get { return frameUpper; }
            set { frameUpper = value; }
        }

        #endregion

        //Field of Variables
        private string name;
        private string id;
        private int index;
        private string elementName;
        private string type;

        private string parentComponentName;
        private Dictionary<string, DesignRegion> designRegion;

        private Brep geometry;
        private Box boundingBox;

        private double height;
        private double width;
        private double length;
        private double density;

        private Plane frameBase;
        private Plane frameLower;
        private Plane frameUpper;

        public DesignElement() { }

        public DesignElement(string name)
        {
            this.id = Guid.NewGuid().ToString();

            if (name != null)
            {
                this.name = SetDesignElementName(name, this);
                // Add Element to FabCollection
                FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();
                fabCollection.AddDesignElement(this);
            }
        }

        private static string SetDesignElementName(string name, DesignElement designElement)
        {
            // Check if name is null
            if (name == null)
            {
                throw new ArgumentNullException("Name cannot be null");
            }

            StringBuilder sb = new StringBuilder();

            // DesignElement
            sb.Append("DE_");

            if (designElement is DesignPlate designPlate)
            {
                sb.Append("P_");
            }
            else if (designElement is DesignBeam designBeam)
            {
                sb.Append("B_");
            }
            else if (designElement is DesignComponent designComponent)
            {
                sb.Append("C_");
            }

            sb.Append(name);

            return sb.ToString();
        }



        public void AddCoreAttributes(Brep geometry)
        {
            Geometry = geometry;
            Box minimumBoundingBox = FabUtilities.FabUtilities.GetMinimumBoundingBox3DFromBrep(geometry);
            FrameBase = FabUtilities.FabUtilities.AdjustBaseFrameToLongestEdgeAndClosestNormal(minimumBoundingBox);
        }

        public void AddCoreAttributes(Brep geometry, Plane baseFrame)
        {
            Geometry = geometry;
            FrameBase = baseFrame;
        }

        public void AddCoreAttributesOutline(Brep geometry, Plane baseFrame, Polyline polyline)
        {
            Geometry = geometry;
            BoundingBox = FabUtilities.FabUtilitiesElement.GetFabElementBoundingBox(geometry, baseFrame, polyline);
            FrameBase = baseFrame;
        }
    }
}

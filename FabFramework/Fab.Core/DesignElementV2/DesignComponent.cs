using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Core.DesignElement
{
    public class DesignComponent : DesignElement
    {
        #region properties
        //Properties
        public List<Brep> ComponentGeometries
        {
            get { return componentGeometries; }
            set { componentGeometries = value; }
        }

        public List<string> DesignPlatesName
        {
            get { return designPlatesName; }
            set { designPlatesName = value; }
        }
        public List<string> DesignBeamsName
        {
            get { return designBeamsName; }
            set { designBeamsName = value; }
        }

        #endregion

        //Field of Variables
        private List<Brep> componentGeometries;
        private List<string> designPlatesName;
        private List<string> designBeamsName;


        public DesignComponent() : base() { }
        public DesignComponent(string name) : base(name) { }

        public new void AddCoreAttributes(Brep geometry)
        {
            if (ComponentGeometries == null)
                ComponentGeometries = new List<Brep>();

            ComponentGeometries.Add(geometry);
            Geometry = geometry;

            Box minimumBoundingBox = FabUtilities.FabUtilities.GetMinimumBoundingBox3DFromBrep(geometry);
            FrameBase = FabUtilities.FabUtilities.AdjustBaseFrameToLongestEdgeAndClosestNormal(minimumBoundingBox);
        }

        public void AddCoreAttributes(List<Brep> geometry)
        {
            Box componentBox = FabUtilities.FabUtilities.CreateBoxFromBreps(geometry);

            ComponentGeometries = geometry;
            Geometry = componentBox.ToBrep(); ;
            FrameBase = FabUtilities.FabUtilities.AdjustBaseFrameToLongestEdgeAndClosestNormal(componentBox);
        }

        public new void AddCoreAttributes(Brep geometry, Plane baseFrame)
        {
            if (ComponentGeometries == null)
                ComponentGeometries = new List<Brep>();
            ComponentGeometries.Add(geometry);
            Geometry = geometry;
            FrameBase = baseFrame;
        }

        public void AddCoreAttributes(List<Brep> geometry, Plane baseFrame)
        {
            Box componentBox = FabUtilities.FabUtilities.CreateAlignedBoxFromBreps(geometry, baseFrame);

            ComponentGeometries = geometry;
            Geometry = componentBox.ToBrep(); ;
            FrameBase = baseFrame;
        }

        public new void AddCoreAttributesOutline(Brep geometry, Plane baseFrame, Polyline polyline)
        {
            if (ComponentGeometries == null)
                ComponentGeometries = new List<Brep>();
            ComponentGeometries.Add(geometry);

            Geometry = geometry;
            BoundingBox = FabUtilities.FabUtilitiesElement.GetFabElementBoundingBox(geometry, baseFrame, polyline);
            FrameBase = baseFrame;
        }
        public void AddCoreAttributesOutline(List<Brep> geometry, Plane baseFrame, Polyline polyline)
        {
            Box componentBox = FabUtilities.FabUtilities.CreateAlignedBoxFromBreps(geometry, baseFrame);

            ComponentGeometries = geometry;
            Geometry = componentBox.ToBrep(); ;
            BoundingBox = FabUtilities.FabUtilitiesElement.GetFabElementBoundingBox(Geometry, baseFrame, polyline);
            FrameBase = baseFrame;
        }

        // Method to add a new FabPlate to the FabPlates list
        public void AddDesignPlate(DesignPlate newDesignPlate)
        {
            AddDesignPlates(new List<DesignPlate> { newDesignPlate });
        }

        public void AddDesignPlates(List<DesignPlate> newDesignPlates)
        {
            if (designPlatesName == null)
                designPlatesName = new List<string>();

            foreach (var newDesignPlate in newDesignPlates)
            {
                // Check if name is null
                if (newDesignPlate.Name == null)
                {
                    throw new ArgumentNullException("Name cannot be null");
                }

                newDesignPlate.ParentComponentName = this.Name;
                designPlatesName.Add(newDesignPlate.Name);
            }
        }



        // Method to add a new DesignBeam to the DesignBeams list
        public void AddDesignBeam(DesignBeam newDesignBeam)
        {
            AddDesignBeams(new List<DesignBeam> { newDesignBeam });
        }
        public void AddDesignBeams(List<DesignBeam> newDesignBeams)
        {

            if (designBeamsName == null)
                designBeamsName = new List<string>();

            foreach (var newDesignBeam in newDesignBeams)
            {
                // Check if name is null
                if (newDesignBeam.Name == null)
                {
                    throw new ArgumentNullException("Name cannot be null");
                }

                newDesignBeam.ParentComponentName = this.Name;
                designBeamsName.Add(newDesignBeam.Name);
            }
        }

    }
}

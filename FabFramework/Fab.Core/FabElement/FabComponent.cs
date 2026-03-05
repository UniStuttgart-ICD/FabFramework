using Fab.Core.DesignElement;
using Fab.Core.FabEnvironment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Core.FabElement
{
    public class FabComponent : FabElement
    {
        #region properties
        //Properties
        FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

        public string DesignComponentName
        {
            get { return designComponentName; }
            set { designComponentName = value; }
        }

        public List<string> FabPlatesName
        {
            get { return fabPlatesName; }
            set { fabPlatesName = value; }
        }
        public List<string> FabBeamsName
        {
            get { return fabBeamsName; }
            set { fabBeamsName = value; }
        }
        public List<string> CompTasksName
        {
            get { return compTasksName; }
            set { compTasksName = value; }
        }

        #endregion

        //Field of Variables
        private string designComponentName;
        private List<string> fabPlatesName;
        private List<string> fabBeamsName;

        private List<string> compTasksName;

        public FabComponent() : base() { }

        public FabComponent(string name, DesignElement.DesignComponent designComponent, StaticEnv envFab) : base(name)
        {
            this.ElementName = designComponent.ElementName;
            this.DesignElementName = designComponent.Name;
            this.DesignComponentName = designComponent.Name;
            this.compTasksName = new List<string>();
            this.EnvFab = envFab;

            this.Geometry = FabUtilities.FabUtilities.ConvertToJoinedGeometry(designComponent.ComponentGeometries);
            this.RefPln_SituBox = FabUtilities.FabUtilities.GetBoxBottomLeftCornerPlane(fabCollection.designComponentCollection[DesignComponentName].BoundingBox);
            this.RefPln_Situ = FabUtilities.FabUtilities.GetBoxCenterPlane(fabCollection.designComponentCollection[DesignComponentName].BoundingBox);
            this.RefPln_Fab = envFab.RefPln[0];
            this.RefPln_FabOut = this.RefPln_Fab;
            this.Angle_FabOut = 0;

            this.Transform_Spawn = Transform.PlaneToPlane(RefPln_Situ, RefPln_FabOut);
        }


        // Method to add a new FabPlate to the FabPlates list
        public void AddFabPlate(FabPlate newFabPlate)
        {
            AddFabPlates(new List<FabPlate> { newFabPlate });
        }

        public void AddFabPlates(List<FabPlate> newFabPlates)
        {
            if (fabPlatesName == null)
                fabPlatesName = new List<string>();

            foreach (var newFabPlate in newFabPlates)
            {
                // Check if name is null
                if (newFabPlate.Name == null)
                {
                    throw new ArgumentNullException("Name can not be null.");
                }

                newFabPlate.ParentComponentName = this.Name;
                if (!fabPlatesName.Contains(newFabPlate.Name))
                { fabPlatesName.Add(newFabPlate.Name); }
            }
        }


        // Method to add a new FabBeam to the FabBeams list
        public void AddFabBeam(FabBeam newFabBeam)
        {
            AddFabBeams(new List<FabBeam> { newFabBeam });
        }

        public void AddFabBeams(List<FabBeam> newFabBeams)
        {
            if (fabBeamsName == null)
                fabBeamsName = new List<string>();

            foreach (var newFabBeam in newFabBeams)
            {
                // Check if name is null
                if (newFabBeam.Name == null)
                {
                    throw new ArgumentNullException("Name cannot be null");
                }

                newFabBeam.ParentComponentName = this.Name;

                if (!fabBeamsName.Contains(newFabBeam.Name))
                { fabBeamsName.Add(newFabBeam.Name); }
            }
        }


        public List<FabPlate> GetFabPlates()
        {
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

            List<FabPlate> fabPlates = new List<FabPlate>();

            foreach (var fabPlateName in this.FabPlatesName)
            {
                if (fabCollection.fabPlateCollection.ContainsKey(fabPlateName))
                {
                    fabPlates.Add(fabCollection.fabPlateCollection[fabPlateName]);
                }
                else
                {
                    throw new KeyNotFoundException($"Key '{fabPlateName}' not found in the fabPlateCollection dictionary.");
                }
            }

            return fabPlates;
        }

        public List<FabBeam> GetFabBeams()
        {
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

            List<FabBeam> fabBeams = new List<FabBeam>();

            foreach (var fabBeamName in this.FabBeamsName)
            {
                if (fabCollection.fabBeamCollection.ContainsKey(fabBeamName))
                {
                    fabBeams.Add(fabCollection.fabBeamCollection[fabBeamName]);
                }
                else
                {
                    throw new KeyNotFoundException($"Key '{fabBeamName}' not found in the fabBeamCollection dictionary.");
                }
            }

            return fabBeams;
        }

        public DesignComponent GetDesignComponent()
        {
            if (fabCollection.designComponentCollection.ContainsKey(designComponentName))
            {
                return fabCollection.designComponentCollection[designComponentName];
            }
            else
            {
                throw new KeyNotFoundException($"Key '{designComponentName}' not found in the designComponentCollection dictionary.");
            }
        }
    }

}

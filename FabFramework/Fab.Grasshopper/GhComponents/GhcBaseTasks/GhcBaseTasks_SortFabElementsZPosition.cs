using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;


namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcBaseTasks_SortFabElementsZPosition : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcBaseTasks_SortFabElementsZPosition()
          : base("Sort By Z Height",
                "SortZ",
                "Sort the FabElements according to their Origin.Z Height and overwrite Index Value.",
                "Fab",
                "BaseTasks")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FabCollection fabCollection = FabCollection.GetFabCollection();
            //-----------
            //INPUTS
            //-----------
            FabComponent iFabComponent = new FabComponent();
            DA.GetData<FabComponent>("FabComponent", ref iFabComponent);

            FabComponent fabComponent = iFabComponent.ShallowCopy() as FabComponent;

            //-----------
            //EDIT
            //-----------

            List<FabElement> allFabElements = FabUtilitiesElement.GetAllFabComponentElements(fabComponent);

            //iterate trough all elements and sort them by their Z position
            allFabElements.Sort((x1, y1) => x1.RefPln_FabOut.Origin.Z.CompareTo(y1.RefPln_FabOut.Origin.Z));

            // Sort by Z, Y, and X position
            allFabElements.Sort((x, y) =>
            {
                int zComparison = x.RefPln_FabOut.Origin.Z.CompareTo(y.RefPln_FabOut.Origin.Z);
                if (zComparison != 0) return zComparison;

                int yComparison = x.RefPln_FabOut.Origin.Y.CompareTo(y.RefPln_FabOut.Origin.Y);
                if (yComparison != 0) return yComparison;

                return x.RefPln_FabOut.Origin.X.CompareTo(y.RefPln_FabOut.Origin.X);
            });

            // Assign indices
            for (int i = 0; i < allFabElements.Count; i++)
            {
                allFabElements[i].Index = i;
            }


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabComponent", fabComponent);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.FabFramework_Icon_SortByZ_Height;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8bd611ab-264d-4662-8ec1-78e15c40bbc0"); }
        }
    }
}
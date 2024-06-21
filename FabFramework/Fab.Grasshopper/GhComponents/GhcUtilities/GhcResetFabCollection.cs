using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Fab.Core.FabEnvironment;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Linq;
using System.Collections;
using Fab.Core.DesignElement;
using Rhino.DocObjects;
using Fab.Core.FabCollection;
using Fab.Grasshopper.Properties;
using Grasshopper.GUI.Canvas;

namespace Fab.Grasshopper.GhComponents.GhcUtilities
{
    public class GhcResetFabCollection : GH_Component
    {


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcResetFabCollection()
          : base("Reset FabCollection",
                "Reset",
                "Reset FabCollection. Recompute GH-Canvas afterwards.",
                "Fab",
                "Utilities")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "R", "Reset the collection", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCollection", "FC", "FabCollectionData", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iReset = false;
            DA.GetData("Reset", ref iReset);

            // Initialize FabCollection
            FabCollection fabCollection = FabCollection.GetFabCollection();


            if (iReset)
            {
                FabCollection.InitializeFabCollection();
            }

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCollection", fabCollection);
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
                return Resources.FabFramework_Icon_CollectionReset;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b61a798b-24a3-40ab-99ee-ab9ff4d2d668"); }
        }
    }
}
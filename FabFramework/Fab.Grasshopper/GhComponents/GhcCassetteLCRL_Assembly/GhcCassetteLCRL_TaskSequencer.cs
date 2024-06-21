using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Eto.Forms;
using System.Linq;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_TaskSequencer : GH_Component
    {
        //OLD Out of date
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_TaskSequencer()
          : base("LCRLCassette Task Sequencer", 
                "LCRL TaskSeq",
                "OLD: Sequence the FabTasks for the LCRLCassette.",
                "Fab",
                "LCRL")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("FabTasks", "FabTasks", "List of all FabTasks for the LCRL FabCassette", GH_ParamAccess.list);
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //-----------
            //INPUTS
            //-----------
            FabComponent fabCassette = new FabComponent();
            DA.GetData("FabCassette", ref fabCassette);

            //-----------
            //EDIT
            //-----------

            FabCollection fabCollection = FabCollection.GetFabCollection();
            List<FabTask> sortedTasks = new List<FabTask>();

            List<string> sortingOrder_BottomPlate = new List<string> { "Pick", "Place", "Glue" }; 
            List<string> sortingOrder_Beams = new List<string> { "Pick", "PlaceNail" };
            List<string> sortingOrder_TopPlate = new List<string> { "Glue", "Pick", "Place", "Nail" };
            List<string> sortingOrder_Component = new List<string> { "Pick", "Place" };

            sortedTasks.AddRange(FabTask.SortTasksFabPlate(fabCassette, fabCassette.GetFabPlates()[0].Name, sortingOrder_BottomPlate));
            sortedTasks.AddRange(FabTask.SortFabTasksByBeamIndex(fabCassette, sortingOrder_Beams));
            sortedTasks.AddRange(FabTask.SortTasksFabPlate(fabCassette, fabCassette.GetFabPlates()[1].Name, sortingOrder_TopPlate));
            sortedTasks.AddRange(FabTask.SortTasksFabComponent(fabCassette, sortingOrder_Component));

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);
            DA.SetDataList("FabTasks", sortedTasks);

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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("649d7ad7-2255-41d8-aeec-96853b5f3f75"); }
        }
    }
}
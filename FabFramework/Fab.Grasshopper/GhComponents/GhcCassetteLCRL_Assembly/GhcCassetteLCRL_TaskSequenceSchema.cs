using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabTask;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_TaskSequenceSchema : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_TaskSequenceSchema()
          : base("LCRLCassette Task Sequence Schema",
                "LCRL TaskSeq",
                "Sequence the FabTasks for the LCRLCassette.",
                "Fab",
                "LCRL")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
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


            //Add To FabTaskSequence
            fabCollection.AddToFabTaskSequence(fabCassette.GetFabPlates()[0].Name, "Pick", "Place", "Glue");
            fabCassette.GetFabBeams().ForEach(fabBeam => fabCollection.AddToFabTaskSequence(fabBeam.Name, "Pick", "PlaceNail"));
            fabCollection.AddToFabTaskSequence(fabCassette.GetFabPlates()[1].Name, "Glue", "Pick", "Place", "Nail");
            fabCollection.AddToFabTaskSequence(fabCassette.Name, "Pick", "Place");

            List<FabTask> sortedTasks = new List<FabTask>();
            sortedTasks = fabCollection.GetFabTaskAccordingToSequenceSchema();


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
            get { return new Guid("b06b0b39-31d6-459f-b889-7eafd51e7434"); }
        }
    }
}
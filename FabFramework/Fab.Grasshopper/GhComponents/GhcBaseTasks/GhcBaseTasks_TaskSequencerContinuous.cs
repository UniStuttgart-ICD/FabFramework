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

namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcBaseTasks_TaskSequencerContinuous : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcBaseTasks_TaskSequencerContinuous()
          : base("Task Sequencer Continuous",
                "TaskSeqCon",
                "Continuous sequencing of the FabTasks for the given FabElement.",
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
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data.", GH_ParamAccess.item);
    
            pManager.AddGenericParameter("Action", "A", "Action for the task sequencing.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("FabTasks", "FabTasks", "List of all FabTasks for the FabComponent.", GH_ParamAccess.list);
            pManager.AddGenericParameter("FabElements", "FE", "FabElement Data", GH_ParamAccess.list);
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

            String iFabElementName = String.Empty;
            DA.GetData("FabElementName", ref iFabElementName);

            List<Fab.Core.FabEnvironment.Action> iFabActions = new List<Fab.Core.FabEnvironment.Action>();
            DA.GetDataList("Action", iFabActions);

            //-----------
            //EDIT
            //-----------


            // Get all FabElements that contain the partial name in their key
            var matchingFabElements = fabCollection.fabElementCollection
                .Where(kvp => kvp.Key.Contains(iFabElementName))
                .Select(kvp => kvp.Value);


            List<FabTask> sortedTasks = new List<FabTask>();


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabComponent);
            DA.SetDataList("FabTasks", sortedTasks);
            DA.SetDataList("FabElements", matchingFabElements);

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
            get { return new Guid("ca160ea1-ea0f-48f5-9d8b-21125ca1f05d"); }
        }
    }
}
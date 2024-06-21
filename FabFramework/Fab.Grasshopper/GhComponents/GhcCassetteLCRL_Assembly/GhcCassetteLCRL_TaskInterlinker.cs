using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_TaskInterlinker : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_TaskInterlinker()
          : base("LCRLCassette Task Interlinker",
                "LCRL TaskLink",
                "Create necessary tasks to link prior tasks together for a functional fabrication.",
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
            pManager.AddGenericParameter("FabTasks", "FabTasks", "List of all FabTasks for the LCRL FabCassette", GH_ParamAccess.list);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
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

            List<FabTask> fabTasks = new List<FabTask>();
            DA.GetDataList<FabTask>("FabTasks", fabTasks);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            //-----------
            //EDIT
            //-----------


            //INTERLINK TOOLS
            List<string> toolActionNames = new List<string> { "TakeTool", "StoreTool", "InitTool", "CloseTool" };
            List<FabTask> interlinkedTasks = FabTask.InterlinkToolTasks(iFabActor, fabTasks, toolActionNames);


            //DEPENDENCY
            Core.FabEnvironment.Action dependencyTaskAction = iFabActor.Tools["GlueGun"].Actions["Spill"];
            interlinkedTasks = FabTask.CreateDependencyTasks(iFabActor, interlinkedTasks, "Glue", dependencyTaskAction, true);  // Modify interlinkedTasks to add dependency tasks


            // Set outputs
            DA.SetData("FabCassette", fabCassette);
            DA.SetDataList("FabTasks", interlinkedTasks);
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
            get { return new Guid("8347dccd-88b1-4f48-83f0-a5ef2cb135b6"); }
        }
    }
}
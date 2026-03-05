using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;


namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcAttachFabTask : GH_Component
    {
        // Static variable to store the counter value
        private static int counter = -1;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcAttachFabTask()
          : base("Attach FabTask",
                "AT",
                "Attach a FabTask to a FabComponent.",
                "Fab",
                "BaseTasks")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("FabTask", "FT", "FabTask Data", GH_ParamAccess.item);
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
            DA.GetData("FabComponent", ref iFabComponent);

            FabComponent fabComponent = iFabComponent.ShallowCopy() as FabComponent;

            FabTask iFabTask = new FabTask();
            DA.GetData("FabTask", ref iFabTask);

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
                return Resources.FabFramework_Icon_AttachFabTask_hammer;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a086aee7-6779-4815-88cc-2e1bcee82ca2"); }
        }
    }
}
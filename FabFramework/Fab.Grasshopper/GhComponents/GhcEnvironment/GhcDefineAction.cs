using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabEnvironment;
using Fab.Grasshopper.Properties;

namespace Fab.Grasshopper.GhComponents.GhcEnvironment
{
    public class GhcDefineAction : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineAction()
          : base(
              "Define Action", 
              "Action",
              "Define an Action",
              "Fab", 
              "Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("ActionID", "ID", "This is the index Number of the Action.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "The name of the Action.", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "T", "The type of the Action.", GH_ParamAccess.item);
            pManager.AddTextParameter("ProgramName", "PN", "The ProgramName of the Action.", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Action", "A", "Action Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int iActionID = -1 ;
            DA.GetData<int>("ActionID", ref iActionID);

            String iName = String.Empty;
            DA.GetData<String>("Name", ref iName);

            String iType = String.Empty;
            DA.GetData<String>("Type", ref iType);

            String iProgramName = String.Empty;
            DA.GetData<String>("ProgramName", ref iProgramName);

            Fab.Core.FabEnvironment.Action action = new Fab.Core.FabEnvironment.Action(iActionID, iName, iType);

            if (!string.IsNullOrEmpty(iProgramName))
            {
                action.ProgramName = iProgramName;
            }

            DA.SetData("Action", action);
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
                return Resources.FabFramework_Icon_Action;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c14851b3-7b77-4e5e-a8ba-7fba672d4a4d"); }
        }
    }
}
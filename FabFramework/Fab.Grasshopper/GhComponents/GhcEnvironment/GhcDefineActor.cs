using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabUtilities;
using Fab.Core.FabEnvironment;
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using Fab.Grasshopper.Properties;

namespace Fab.Grasshopper.GhComponents.GhcEnvironment
{
    public class GhcDefineActor : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineActor()
          : base(
              "Define Actor", 
              "Actor",
              "Define an Actor",
              "Fab", 
              "Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The name of the Actor.", GH_ParamAccess.item, "default");
            pManager.AddPlaneParameter("RefPln", "RP", "This is the reference plane of the Actor.", GH_ParamAccess.item);

            pManager.AddGeometryParameter("Geometry", "G", "Actor Geometry", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Tools", "T", "List of all tools used by this actor.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Actions", "A", "List of all actor specific actions.", GH_ParamAccess.list);
            pManager[4].Optional = true;
            pManager.AddLineParameter("LinearAxis", "LA", "Linear Axis of the robot", GH_ParamAccess.item);
            pManager[5].Optional = true;

            pManager.AddNumberParameter("Width", "W", "This is the width of the Actor.", GH_ParamAccess.item);
            pManager[6].Optional = true;
            pManager.AddNumberParameter("Length", "L", "This is the length of the Actor.", GH_ParamAccess.item);
            pManager[7].Optional = true;
            pManager.AddNumberParameter("Height", "H", "This is the height of the Actor.", GH_ParamAccess.item);
            pManager[8].Optional = true;
            pManager.AddNumberParameter("MaxWeight", "MW", "This is the height of the Actor.", GH_ParamAccess.item, -1.0);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Actor", "A", "Actor Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            String iName = String.Empty;
            DA.GetData<String>("Name", ref iName);

            Plane iRefPln = Plane.Unset;
            DA.GetData<Plane>("RefPln", ref iRefPln);

            List<GeometryBase> iGeometry = new List<GeometryBase>();
            DA.GetDataList<GeometryBase>("Geometry", iGeometry);

            List<Tool> iTools = new List<Tool>();
            DA.GetDataList("Tools", iTools);

            List<Fab.Core.FabEnvironment.Action> iActions = new List<Fab.Core.FabEnvironment.Action>();
            DA.GetDataList("Actions", iActions);

            Line iLinearAxis = Line.Unset;
            DA.GetData<Line>("LinearAxis", ref iLinearAxis);


            double iWidth = double.NaN;
            DA.GetData<double>("Width", ref iWidth);

            double iLength = double.NaN;
            DA.GetData<double>("Length", ref iLength);

            double iHeight = double.NaN;
            DA.GetData<double>("Height", ref iHeight);

            double iMaxWeight = double.NaN;
            DA.GetData<double>("MaxWeight", ref iMaxWeight);



            Actor actor = new Actor(iName, iRefPln);

            if (iGeometry != null)
            {
                List<GeometryBase> convertedGeometry = FabUtilities.ConvertToJoinedGeometryToList(iGeometry);
                actor.Geometry = convertedGeometry;
            }


            Dictionary<string, Tool> inputTools = new Dictionary<string, Tool>();
            foreach (Tool tool in iTools)
            {
                inputTools[tool.Name] = tool;
            }
            actor.Tools = inputTools;

            Dictionary<string, Fab.Core.FabEnvironment.Action> inputActions = new Dictionary<string, Fab.Core.FabEnvironment.Action>();
            if (iActions != null)
            {
                foreach (Fab.Core.FabEnvironment.Action action in iActions)
                {
                    inputActions[action.Name] = action;
                } 
            }
            actor.Actions = inputActions;


            if (iLinearAxis != Line.Unset)
            {
                actor.LinearAxis = iLinearAxis;
            }


            if (!double.IsNaN(iWidth))
            {
                actor.Width = iWidth;
            }

            if (!double.IsNaN(iLength))
            {
                actor.Length = iLength;
            }

            if (!double.IsNaN(iHeight))
            {
                actor.Height = iHeight;
            }

            actor.MaxWeight = iMaxWeight;


            DA.SetData("Actor", actor);
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
                return Resources.FabFramework_Icon_Actor;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1eb951d7-e97e-4b02-b443-69918e176477"); }
        }
    }
}
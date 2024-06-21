using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabEnvironment;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;

namespace Fab.Grasshopper.GhComponents.GhcEnvironment
{
    public class GhcDefineEndeffector : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineEndeffector()
          : base(
              "Define Endeffector",
              "Endeffector",
              "Define an Endeffector",
              "Fab",
              "Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //Tool Specific
            pManager.AddIntegerParameter("ToolNum", "TN", "Tool Number", GH_ParamAccess.item, -1);
            pManager.AddTextParameter("ToolName", "N", "The name of the tool", GH_ParamAccess.item, "undefined");
            pManager.AddGeometryParameter("Geometry", "G", "Tool Geometry", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddPlaneParameter("TCP Frame", "TCP", "The X, Y and Z axes of the tool center point (TCP)", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddPlaneParameter("Mounting Frame", "MCP", "The tool will be mounted on the robot such that the mounting frame coincides with the robot flange frame", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddGenericParameter("Actions", "A", "List of all tool specific actions.", GH_ParamAccess.list);



            //Endeffector Specific
            pManager.AddNumberParameter("MaxWeight", "MW", "This is the max weight the endeffector can operate with", GH_ParamAccess.item);
            pManager[6].Optional = true;
            pManager.AddNumberParameter("FootprintWidth", "W", "This is the footprint width of the Endeffector", GH_ParamAccess.item);
            pManager[7].Optional = true;
            pManager.AddNumberParameter("FootprintLength", "L", "This is the footprint length of the Endeffector", GH_ParamAccess.item);
            pManager[8].Optional = true;
            pManager.AddNumberParameter("FootprintHeight", "H", "This is the minimum footprint height of the Endeffector", GH_ParamAccess.item);
            pManager[9].Optional = true;
            pManager.AddNumberParameter("ZOffset", "ZO", "This is the minimum zOffset from of the Endeffector to the workpiece", GH_ParamAccess.item, 0.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Endeffector", "EE", "Endeffector data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int iToolNum = -1;
            DA.GetData<int>("ToolNum", ref iToolNum);

            string iName = string.Empty;
            DA.GetData<string>("ToolName", ref iName);

            List<GeometryBase> iGeometry = new List<GeometryBase>();
            DA.GetDataList<GeometryBase>("Geometry", iGeometry);

            Plane iTcpFrame = Plane.Unset;
            DA.GetData<Plane>("TCP Frame", ref iTcpFrame);

            Plane iMountingFrame = Plane.Unset;
            DA.GetData<Plane>("Mounting Frame", ref iMountingFrame);

            List<Fab.Core.FabEnvironment.Action> iActions = new List<Fab.Core.FabEnvironment.Action>();
            DA.GetDataList("Actions", iActions);

            double iMaxWeight = double.NaN;
            DA.GetData<double>("MaxWeight", ref iMaxWeight);

            double iFootprintWidth = double.NaN;
            DA.GetData<double>("FootprintWidth", ref iFootprintWidth);

            double iFootprintLength = double.NaN;
            DA.GetData<double>("FootprintLength", ref iFootprintLength);

            double iFootprintHeight = double.NaN;
            DA.GetData<double>("FootprintHeight", ref iFootprintHeight);

            double iZOffset = double.NaN;
            DA.GetData<double>("ZOffset", ref iZOffset);

            List<GeometryBase> convertedGeometry = new List<GeometryBase>();
            if (iGeometry != null)
            {
                convertedGeometry = FabUtilities.ConvertToJoinedGeometryToList(iGeometry);
            }

            Dictionary<string, Fab.Core.FabEnvironment.Action> inputActions = new Dictionary<string, Fab.Core.FabEnvironment.Action>();
            foreach (Fab.Core.FabEnvironment.Action action in iActions)
            {
                inputActions[action.Name] = action;
            }

            DA.SetData("Endeffector", new Endeffector(iToolNum, iName, iTcpFrame, iMountingFrame, convertedGeometry, inputActions, iMaxWeight, iFootprintWidth, iFootprintLength, iFootprintHeight, iZOffset));
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
                return Resources.FabFramework_Icon_Endeffector;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e5b81bbb-9b38-4d33-a915-0e7478d73a10"); }
        }
    }
}
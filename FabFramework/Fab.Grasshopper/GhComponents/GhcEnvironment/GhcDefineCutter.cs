﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabEnvironment;
using Fab.Core.FabUtilities;
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using Fab.Grasshopper.Properties;

namespace Fab.Grasshopper.GhComponents.GhcEnvironment
{
    public class GhcDefineCutter: GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineCutter()
          : base(
              "Define Cutter",
              "Cutter",
              "Define a Cutter",
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
            pManager.AddIntegerParameter("ToolNum", "TNum", "Tool Number", GH_ParamAccess.item, -1);
            pManager.AddTextParameter("ToolName", "N", "The name of the tool", GH_ParamAccess.item, "undefined");
            pManager.AddGeometryParameter("Geometry", "G", "Tool Geometry", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddPlaneParameter("TCP Frame", "TCP", "The X, Y and Z axes of the tool center point (TCP)", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddPlaneParameter("Mounting Frame", "Mount", "The tool will be mounted on the robot such that the mounting frame coincides with the robot flange frame", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddGenericParameter("Actions", "Acts", "List of all tool specific actions.", GH_ParamAccess.list);

            //Cutter Specific
            pManager.AddNumberParameter("Length", "L", "This is the length of the cutter", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Diameter", "D", "This is the diamter of the cutter", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Width", "W", "This is the width of the cutter", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Minimum Immersion", "MinI", "This is the minimum immersion of the cutter", GH_ParamAccess.item, 0.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cutter", "C", "Cutter data", GH_ParamAccess.item);
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

            double iLength = double.NaN;
            DA.GetData<double>("Length", ref iLength);

            double iDiameter = double.NaN;
            DA.GetData<double>("Diameter", ref iDiameter);

            double iWidth = double.NaN;
            DA.GetData<double>("Width", ref iWidth);

            double iMinImmersion = double.NaN;
            DA.GetData<double>("Minimum Immersion", ref iMinImmersion);

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

            DA.SetData("Cutter", new Cutter(iToolNum, iName, iTcpFrame, iMountingFrame, convertedGeometry, inputActions, iLength, iDiameter, iWidth, iMinImmersion));
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
                return Resources.FabFramework_Icon_MillBit;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("62fd0710-acd8-46b6-b78c-e7d07f56f729"); }
        }
    }
}
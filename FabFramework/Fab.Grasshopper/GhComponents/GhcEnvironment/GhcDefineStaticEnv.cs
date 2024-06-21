using Fab.Core.FabEnvironment;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcEnvironment
{
    public class GhcDefineStativEnv : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineStativEnv()
          : base(
              "Define StaticEnv",
              "StaticEnv",
              "Define a static Environment",
              "Fab",
              "Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Index", "I", "This is the index Number of the Static Environment.", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Name", "N", "The name of the Static Environment.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Reference Plane", "RP", "This is the reference plane of the static environment.", GH_ParamAccess.list, Plane.WorldXY);
            pManager.AddPlaneParameter("Align Plane", "AP", "This is the alignment plane of the static environment.", GH_ParamAccess.list, Plane.WorldXY);
            pManager.AddPlaneParameter("Safe Position", "SP", "This is the safe position of the static environment.", GH_ParamAccess.list);
            pManager[4].Optional = true;
            pManager.AddNumberParameter("Width", "W", "This is the width of the static environment.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Length", "L", "This is the length of the static environment.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Height", "H", "This is the height of the static environment.", GH_ParamAccess.item, 0.0);
            pManager.AddGeometryParameter("Geometry", "G", "Static Environment Geometry", GH_ParamAccess.list);
            pManager[8].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StaticEnv", "SE", "Static Environment Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int iIndex = -1;
            DA.GetData<int>("Index", ref iIndex);

            string iName = string.Empty;
            DA.GetData<string>("Name", ref iName);

            List<Plane> iRefPln = new List<Plane>();
            DA.GetDataList<Plane>("Reference Plane", iRefPln);

            List<Plane> iAlignPln = new List<Plane>();
            DA.GetDataList<Plane>("Align Plane", iAlignPln);

            List<Plane> iSafePosition = new List<Plane>();
            DA.GetDataList<Plane>("Safe Position", iSafePosition);

            double iWidth = double.NaN;
            DA.GetData<double>("Width", ref iWidth);

            double iLength = double.NaN;
            DA.GetData<double>("Length", ref iLength);

            double iHeight = double.NaN;
            DA.GetData<double>("Height", ref iHeight);

            List<GeometryBase> iGeometry = new List<GeometryBase>();
            DA.GetDataList<GeometryBase>("Geometry", iGeometry);

            // Pre-convert breps to meshes

            List<GeometryBase> convertedGeometry = new List<GeometryBase>();
            if (iGeometry != null)
            {
                convertedGeometry = FabUtilities.ConvertToJoinedGeometryToList(iGeometry);
            }

            StaticEnv staticEnv = new StaticEnv(iIndex, iName, iRefPln, iAlignPln, iWidth, iLength, iHeight, convertedGeometry);

            //check if safePosition is not empty and not null
            if (iSafePosition != null && iSafePosition.Count > 0)
            {
                staticEnv.SafePosition = iSafePosition;
            }
            else
            {
                staticEnv.SafePosition = new List<Plane>();

                Plane safePosition = iAlignPln[0].Clone(); ;
                //move plane up by 400
                safePosition.Origin = safePosition.Origin + safePosition.ZAxis * 400;

                staticEnv.SafePosition.Add(safePosition);
            }


            DA.SetData("StaticEnv", staticEnv);

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
                return Resources.FabFramework_Icon_StaticEnv;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3ccf3541-3d60-4254-a32e-e923d7ee45bb"); }
        }
    }
}
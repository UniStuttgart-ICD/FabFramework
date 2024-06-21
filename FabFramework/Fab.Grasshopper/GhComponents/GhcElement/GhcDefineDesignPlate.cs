using Fab.Core.DesignElement;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace Fab.Grasshopper.GhComponents.GhcElement
{
    public class GhcDefineDesignPlate : GH_Component
    {
        // Static variable to store the counter value
        private static int counter = -1;

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineDesignPlate()
          : base(
              "DesignPlate",
              "DP",
              "Define a DesignPlate",
              "Fab",
              "Element")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The name of the element", GH_ParamAccess.item);
            pManager.AddBrepParameter("Geometry", "G", "Element Geometry", GH_ParamAccess.item);
            pManager.AddPlaneParameter("BaseFrame", "F", "Reference BaseFrame with the wanted orientation of the plate.", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddCurveParameter("Outline", "O", "The outline of the plate.", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddNumberParameter("Height", "H", "The height of the element", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddNumberParameter("Width", "W", "The width of the element", GH_ParamAccess.item);
            pManager[5].Optional = true;
            pManager.AddNumberParameter("Length", "L", "The length of the element", GH_ParamAccess.item);
            pManager[6].Optional = true;
            pManager.AddTextParameter("ElementName", "EN", "The element name of the element", GH_ParamAccess.item);
            pManager[7].Optional = true;
            pManager.AddIntegerParameter("Index", "I", "The index of the element", GH_ParamAccess.item, -1);
            pManager[8].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DesignPlate", "DP", "DesignPlate data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string iName = string.Empty;
            DA.GetData<string>("Name", ref iName);

            Brep iGeometry = null;
            DA.GetData<Brep>("Geometry", ref iGeometry);

            Plane iBaseFrame = Plane.Unset;
            DA.GetData<Plane>("BaseFrame", ref iBaseFrame);

            Curve iOutline = null;
            DA.GetData<Curve>("Outline", ref iOutline);

            double iHeight = double.NaN;
            DA.GetData<double>("Height", ref iHeight);

            double iWidth = double.NaN;
            DA.GetData<double>("Width", ref iWidth);

            double iLength = double.NaN;
            DA.GetData<double>("Width", ref iWidth);

            string iElementName = string.Empty;
            DA.GetData<string>("ElementName", ref iElementName);

            int iIndex = -1;
            DA.GetData<int>("Index", ref iIndex);


            //Auto Name generation
            counter++;

            string autoName = iName + "_" + counter.ToString("D3");
            DesignPlate designPlate = new DesignPlate(autoName);
            designPlate.ElementName = iElementName;


            //check if Index is defined
            if (iIndex == -1)
            { designPlate.Index = counter; }
            else
            { designPlate.Index = iIndex; }


            if (iOutline != null && iOutline.IsPolyline())
            {
                Polyline polyline = new Polyline();
                iOutline.TryGetPolyline(out polyline);
                designPlate.AddCoreAttributesOutline(iGeometry, iBaseFrame, polyline);
                designPlate.Outline = polyline;
            }
            if (iOutline == null && iBaseFrame != Plane.Unset)
            {
                designPlate.AddCoreAttributes(iGeometry, iBaseFrame);
            }
            else
            {
                designPlate.AddCoreAttributes(iGeometry);
            }


            if (!double.IsNaN(iHeight))
            {
                designPlate.Height = iHeight;
            }

            if (!double.IsNaN(iWidth))
            {
                designPlate.Width = iWidth;
            }

            if (!double.IsNaN(iLength))
            {
                designPlate.Length = iLength;
            }


            DA.SetData("DesignPlate", designPlate);
        }

        public override void ExpireSolution(bool recompute)
        {
            base.ExpireSolution(recompute);
            counter = -1;
        }

        public override void ClearData()
        {
            base.ClearData();
            counter = -1;
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
                return Resources.FabFramework_DesignPlate;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1ca0ad35-e18e-4784-ba0e-8030f68beadd"); }
        }
    }
}
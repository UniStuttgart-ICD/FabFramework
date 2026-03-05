using Fab.Core.DesignElement;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace Fab.Grasshopper.GhComponents.GhcElement
{

    public class GhcDefineDesignBeam : GH_Component
    {
        // Static variable to store the counter value
        private static int counter = -1;

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineDesignBeam()
          : base(
              "DesignBeam",
              "DB",
              "Define a DesignBeam",
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
            pManager.AddPlaneParameter("BaseFrame", "F", "Reference BaseFrame of the beam. If no plane is provided, then the best-fit plane will be used.", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddLineParameter("BaseLine", "BL", "The baseline of the DesignBeam. Make sure to have correct orientation of baseline as wanted. If no line is provided, then the best-fit line will be used.", GH_ParamAccess.item);
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
            pManager.AddGenericParameter("DesignBeam", "DB", "DesignBeam data", GH_ParamAccess.item);
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

            Line iBaseLine = Line.Unset;
            DA.GetData<Line>("BaseLine", ref iBaseLine);

            double iHeight = double.NaN;
            DA.GetData<double>("Height", ref iHeight);

            double iWidth = double.NaN;
            DA.GetData<double>("Width", ref iWidth);

            double iLength = double.NaN;
            DA.GetData<double>("Length", ref iLength);

            string iElementName = string.Empty;
            DA.GetData<string>("ElementName", ref iElementName);

            int iIndex = -1;
            DA.GetData<int>("Index", ref iIndex);


            //Auto Name generation
            counter++;

            string autoName = iName + "_" + counter.ToString("D3");
            DesignBeam designBeam = new DesignBeam(autoName);
            designBeam.ElementName = iElementName;


            //check if Index is defined
            if (iIndex == -1)
            {
                designBeam.Index = counter;
                //FabCollection.InitializeFabCollection();
            }
            else
            { designBeam.Index = iIndex; }


            //check for baseFrame input
            if (iBaseFrame != Plane.Unset)
            {
                designBeam.AddCoreAttributes(iGeometry, iBaseFrame);
            }
            else
            {
                designBeam.AddCoreAttributes(iGeometry);
            }


            //check for iBaseLine input
            Line baseLine;
            if (iBaseLine != Line.Unset)
            {
                baseLine = iBaseLine;
            }
            else
            {
                //create function to create baseLine from Geometry
                Point3d startPoint = designBeam.BoundingBox.PointAt(0, 0.5, 0.5);
                Point3d endPoint = designBeam.BoundingBox.PointAt(1, 0.5, 0.5);
                baseLine = new Line(startPoint, endPoint);
            }
            designBeam.BaseLine = baseLine;


            if (!double.IsNaN(iHeight))
            {
                designBeam.Height = iHeight;
            }

            if (!double.IsNaN(iWidth))
            {
                designBeam.Width = iWidth;
            }

            if (!double.IsNaN(iLength))
            {
                designBeam.Length = iLength;
            }


            DA.SetData("DesignBeam", designBeam);
        }

        public override void ExpireSolution(bool recompute)
        {
            base.ExpireSolution(recompute);
            //FabCollection.InitializeFabCollection();
            counter = -1;
        }

        public override void ClearData()
        {
            //FabCollection.InitializeFabCollection();
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
                return Resources.FabFramework_DesignBeam;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1a39de19-135e-4aec-a274-e2c9ae897c57"); }
        }
    }
}
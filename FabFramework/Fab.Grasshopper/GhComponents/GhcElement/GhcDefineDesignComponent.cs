using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabEnvironment;
using Fab.Core.DesignElement;
using Fab.Core.FabUtilities;
using Fab.Core.FabCollection;
using Fab.Grasshopper.Properties;

namespace Fab.Grasshopper.GhComponents.GhcElement
{
    public class GhcDefineDesignComponent: GH_Component
    {
        // Static variable to store the counter value
        private static int counter = -1;

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDefineDesignComponent()
          : base(
              "DesignComponent",
              "DC",
              "Define a DesignComponent",
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
            pManager.AddGenericParameter("DesignPlates", "DPs", "List of all design plates making up the design component.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddGenericParameter("DesignBeams", "DBs", "List of all design beams making up the design component.", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddPlaneParameter("BaseFrame", "F", "Reference BaseFrame with the wanted orientation of the component. If no plane is provided, then the best-fit plane will be used.", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddNumberParameter("Height", "H", "The height of the element", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddNumberParameter("Width", "W", "The widht of the element", GH_ParamAccess.item);
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
            pManager.AddGenericParameter("DesignComponent", "DC", "DesignComponent data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string iName = string.Empty;
            DA.GetData<string>("Name", ref iName);

            List<DesignPlate> iDesignPlates = new List<DesignPlate>();
            DA.GetDataList<DesignPlate>("DesignPlates", iDesignPlates);

            List<DesignBeam> iDesignBeams = new List<DesignBeam>();
            DA.GetDataList<DesignBeam>("DesignBeams", iDesignBeams);

            Plane iBaseFrame = Plane.Unset;
            DA.GetData<Plane>("BaseFrame", ref iBaseFrame);

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

            //MUST BE INCLUDED!
            FabCollection fabCollection = FabCollection.GetFabCollection();

            //Auto Name generation
            counter++;

            string autoName = iName + "_" + counter.ToString("D3");
            DesignComponent designComponent = new DesignComponent(autoName);
            designComponent.ElementName = iElementName;


            //check if Index is defined
            if (iIndex == -1)
            { designComponent.Index = counter; }
            else
            { designComponent.Index = iIndex; }


            List<Brep> componentGeometries = new List<Brep>();


            if (iDesignPlates.Count > 0)
            {
                designComponent.AddDesignPlates(iDesignPlates);
                for (int i = 0; i < designComponent.DesignPlatesName.Count; i++)
                { componentGeometries.Add(fabCollection.designPlateCollection[designComponent.DesignPlatesName[i]].Geometry); }
            }

            if (iDesignBeams.Count > 0)
            {
                designComponent.AddDesignBeams(iDesignBeams);
                for (int i = 0; i < designComponent.DesignBeamsName.Count; i++)
                { componentGeometries.Add(fabCollection.designBeamCollection[designComponent.DesignBeamsName[i]].Geometry); }
            }

            //CHeck if componentGeometries is empty
            if (componentGeometries.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No DesignPlates or DesignBeams were provided.");
                return;
            }
            else
            { 
                //check for baseFrame input
                if (iBaseFrame != Plane.Unset)
                {
                    designComponent.AddCoreAttributes(componentGeometries, iBaseFrame);
                }
                else
                {
                    designComponent.AddCoreAttributes(componentGeometries);
                }
            }



            if (!double.IsNaN(iHeight))
            {
                designComponent.Height = iHeight;
            }

            if (!double.IsNaN(iWidth))
            {
                designComponent.Width = iWidth;
            }

            if (!double.IsNaN(iLength))
            {
                designComponent.Length = iLength;
            }


            DA.SetData("DesignComponent", designComponent);
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
                return Resources.FabFramework_DesignComponent;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("57f53eb0-b3a3-44c6-934f-c1d74ad3e524"); }
        }
    }
}
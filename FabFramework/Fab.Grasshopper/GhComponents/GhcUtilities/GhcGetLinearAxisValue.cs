using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Fab.Core.FabEnvironment;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Eto.Forms;
using Fab.Core.FabTask;
using Fab.Core.FabCollection;
using System.Linq;
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using Fab.Core.DesignElement;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;
using System.Collections;
using Newtonsoft.Json.Linq;
using Fab.Grasshopper.Properties;


namespace Fab.Grasshopper.GhComponents.GhcUtilities
{
    public class GhcGetLinearAxisValue : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcGetLinearAxisValue()
          : base("GetLinearAxisValue",
                "E1",
                "Get Linear Axis Value E1.",
                "Fab",
                "Utilities")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Plane", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MainExtValues", "MEV", "MainExtValues in correct dictionary format.", GH_ParamAccess.item);
            pManager.AddNumberParameter("E1", "E1", "Linear Axis Value E1.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //-----------
            //INPUTS
            Plane iPlane = Plane.Unset;
            DA.GetData(0, ref iPlane);

            Actor iActor = new Actor();
            DA.GetData("Actor", ref iActor);

            //-----------
            //OUTPUTS
            Dictionary<string, List<double>> e1_dictionary = new Dictionary<string, List<double>>();
            double e1 = double.NaN;

            //check for linearAxis of actor
            if (iActor.LinearAxis != null)
            {
                if (iPlane != Plane.Unset)
                {
                    //get the value of the linear axis
                    e1 = FabUtilities.GetLinAxisRadiusBased(iPlane, iActor.LinearAxis);

                    // Add e1 to a List<double>
                    List<double> e1_list = new List<double> { e1 };
                    e1_dictionary.Add("E1", e1_list);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plane is null.");
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Actor has no LinearAxis.");
            }

            DA.SetData("MainExtValues", e1_dictionary);
            DA.SetData("E1", e1);
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
                return Resources.FabFramework_Icon_GetLinearAxisValue;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4c0d847c-43bf-460b-b049-0fdb129479e7"); }
        }
    }
}
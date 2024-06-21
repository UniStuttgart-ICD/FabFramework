using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Reflection;


namespace Fab.Grasshopper.GhComponents.GhcUtilities
{
    public class GhcGetName : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcGetName()
          : base("GetName",
                "Name",
                "Get the name of the object.",
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
            pManager.AddGenericParameter("Object", "O", "Object to get the name from.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Name", "N", "Name of the object.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare a variable for the input
            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            // Unwrap the object if it's a GH_ObjectWrapper
            if (obj is GH_ObjectWrapper objWrapper)
            {
                obj = objWrapper.Value;
            }

            // Try to get the name using reflection
            string nameValue = GetNameUsingReflection(obj);

            // Set the output to the name value
            DA.SetData(0, nameValue);
        }

        private string GetNameUsingReflection(object obj)
        {
            // Check if the object has a "Name" property
            PropertyInfo nameProp = obj.GetType().GetProperty("Name");

            if (nameProp != null && nameProp.CanRead)
            {
                return nameProp.GetValue(obj)?.ToString() ?? "";
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Object has no attribute Name or is of unsupported type.");
                return "";
            }
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
                return Resources.FabFramework_Icon_GetName;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cf6bb210-83ba-4c35-8981-f14527ecd82e"); }
        }
    }
}
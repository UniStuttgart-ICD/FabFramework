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
using Fab.Grasshopper.Properties;


namespace Fab.Grasshopper.GhComponents.GhcUtilities
{
    public class GhcCreateDictionary : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcCreateDictionary()
          : base("Create Dictionary",
                "Dict",
                "Creates a dictionary from a string key and a list of values.",
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
            pManager.AddTextParameter("Key", "K", "String key for the dictionary entry.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Values", "V", "List of values to associate with the key.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Dictionary", "D", "Resulting dictionary", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string key = string.Empty;
            List<object> values = new List<object>();

            if (!DA.GetData("Key", ref key)) return;
            if (!DA.GetDataList("Values", values)) return;

            //get values type
            Type type = values[0].GetType();

            // Create the dictionary
            Dictionary<string, List<object>> dictionary =
                new Dictionary<string, List<object>>();
            dictionary.Add(key, values);

            // Output the dictionary
            DA.SetData("Dictionary", dictionary);
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
                return Resources.FabFramework_Icon_CreateDictionary;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("385d256b-6e01-4456-865d-fe72e5062c35"); }
        }
    }
}
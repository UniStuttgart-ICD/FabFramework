using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabEnvironment;
using Fab.Core.FabUtilities;
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using Fab.Grasshopper.Properties;
using Fab.Core.FabElement;
using Fab.Core.FabTask;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Newtonsoft.Json.Linq;
using System.Linq;
using Eto.Forms;
using Fab.Core.FabCollection;
using Rhino.Display;
using System.Drawing;
using Eto.Drawing;
using Rhino.Runtime;
using System.Xml.Linq;
using Fab.Core.DesignElement;
using static System.Net.Mime.MediaTypeNames;
using Grasshopper.Kernel.Types;

namespace Fab.Grasshopper.GhComponents.GhcUtilities
{

    public class GhcFilterElementsByName : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcFilterElementsByName()
          : base(
              "Filter Elements by Name",
              "FilterName",
              "Filter elements by name.",
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
            pManager.AddTextParameter("Name", "N", "Names to look for.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Element", "E", "Elements to filter.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FoundElements", "E", "Found Elements", GH_ParamAccess.list);
            pManager.AddNumberParameter("Index", "I", "Index of the found Elements.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FabCollection fabCollection = FabCollection.GetFabCollection();
            //-----------
            //INPUTS
            //-----------
            List<string> iName = new List<string>();
            DA.GetDataList("Name", iName);

            List<object> iElements = new List<object>();
            DA.GetDataList("Element", iElements);

            //-----------
            //EDIT
            //-----------

            List<object> foundElements = new List<object>();
            List<int> foundIndex = new List<int>();

            for (int i = 0; i < iElements.Count; i++)
            {
                object obj = iElements[i];

                // Check if obj is a wrapped IGH_Goo and unwrap it
                if (obj is IGH_Goo goo)
                {
                    obj = goo.ScriptVariable(); // Extract the actual object
                }

                // Check if obj is derived from FabElement or DesignElement
                if (obj is FabElement || obj is FabBeam || obj is FabPlate ||
                    obj is DesignElement || obj is DesignElement || obj is DesignElement)
                {
                    // Directly cast obj to either FabElement or DesignElement
                    if (obj is Fab.Core.FabElement.FabElement fabElement)
                    {
                        string objName = fabElement.Name; // Assuming FabElement has a Name property
                        foreach (string name in iName)
                        {
                            if (objName != null && objName.Contains(name))
                            {
                                foundElements.Add(obj);
                                foundIndex.Add(i);
                                break;
                            }
                        }
                    }
                    else if (obj is Fab.Core.DesignElement.DesignElement designElement)
                    {
                        string objName = designElement.Name; // Assuming DesignElement has a Name property
                        foreach (string name in iName)
                        {
                            if (objName != null && objName.Contains(name))
                            {
                                foundElements.Add(obj);
                                foundIndex.Add(i);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Element is not a FabElement or DesignElement");
                    return;
                }

            }

            //-----------
            //OUTPUTS
            //-----------

            DA.SetDataList("FoundElements", foundElements);
            DA.SetDataList("Index", foundIndex);
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
                return Resources.FabFramework_Icon_FilterElementsByName;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ec6f10c9-2abb-4b9e-ba72-4c035b028296"); }
        }
    }
}
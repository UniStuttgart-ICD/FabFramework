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
using System.Linq;
using System.Collections;
using Fab.Core.DesignElement;
using Rhino.DocObjects;
using Fab.Core.FabCollection;
using Fab.Grasshopper.Properties;
using System.Reflection;

namespace Fab.Grasshopper.GhComponents.GhcUtilities
{
    public class GhcGetAllCollectionIDs : GH_Component
    {

        bool iRefresh = false;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcGetAllCollectionIDs()
          : base("All Collection IDs",
                "AllIDs",
                "Get all IDs from the FabCollection.",
                "Fab",
                "Utilities")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Refresh", "R", "Refresh FabCollection", GH_ParamAccess.item);
            pManager.AddGenericParameter("FabCollection", "FC", "FabCollectionData", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

            pManager.AddGenericParameter("DesignElementNames", "DE", "List of names from all DesignElements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("DesignPlateNames", "DP", "List of names from all DesignPlates.", GH_ParamAccess.list);
            pManager.AddGenericParameter("DesignBeamNames", "DB", "List of names from all DesignBeams.", GH_ParamAccess.list);
            pManager.AddGenericParameter("DesignComponentNames", "DC", "List of names from all DesignComponents.", GH_ParamAccess.list);

            pManager.AddGenericParameter("FabElementNames", "FE", "List of names from all FabElements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("FabPlateNames", "FP", "List of names from all FabPlates.", GH_ParamAccess.list);
            pManager.AddGenericParameter("FabBeamNames", "FB", "List of names from all FabBeams.", GH_ParamAccess.list);
            pManager.AddGenericParameter("FabComponentNames", "FC", "List of names from all FabComponents.", GH_ParamAccess.list);

            pManager.AddGenericParameter("FabTaskNames", "FT", "List of names from all FabTasks.", GH_ParamAccess.list);
            pManager.AddGenericParameter("FabTaskSequenceSchema", "FTS", "FabTask Sequence Schema.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //-----------
            //INPUTS
            //-----------

            DA.GetData("Refresh", ref iRefresh);

            FabCollection fabCollection = FabCollection.GetFabCollection();
            DA.GetData("FabCollection", ref fabCollection);


            //-----------
            //EDIT
            //-----------
            List<string> designElementCollectionNames = new List<string>();
            List<string> designPlateCollectionNames = new List<string>();
            List<string> designBeamCollectionNames = new List<string>();
            List<string> designComponentCollectionNames = new List<string>();

            List<string> fabElementCollectionNames = new List<string>();
            List<string> fabPlateCollectionNames = new List<string>();
            List<string> fabBeamCollectionNames = new List<string>();
            List<string> fabComponentCollectionNames = new List<string>();

            List<string> fabTaskCollectionNames = new List<string>();
            List<Tuple<string, string>> fabTaskSequenceSchema = new List<Tuple<string, string>>();

            if (iRefresh != true)
            {
                designElementCollectionNames = fabCollection.designElementCollection.Keys.ToList();
                designPlateCollectionNames = fabCollection.designPlateCollection.Keys.ToList();
                designBeamCollectionNames = fabCollection.designBeamCollection.Keys.ToList();
                designComponentCollectionNames = fabCollection.designComponentCollection.Keys.ToList();

                fabElementCollectionNames = fabCollection.fabElementCollection.Keys.ToList();
                fabPlateCollectionNames = fabCollection.fabPlateCollection.Keys.ToList();
                fabBeamCollectionNames = fabCollection.fabBeamCollection.Keys.ToList();
                fabComponentCollectionNames = fabCollection.fabComponentCollection.Keys.ToList();

                fabTaskCollectionNames = fabCollection.fabTaskCollection.Keys.ToList();

                foreach (var entry in fabCollection.fabTaskSequenceSchema)
                {
                    string key = entry.Key;
                    foreach (string value in entry.Value)
                    {
                        fabTaskSequenceSchema.Add(new Tuple<string, string>(key, value));
                    }
                }

                iRefresh = false;

            }

            //-----------
            //OUTPUTS
            //-----------
            DA.SetDataList("DesignElementNames", designElementCollectionNames);
            DA.SetDataList("DesignPlateNames", designPlateCollectionNames);
            DA.SetDataList("DesignBeamNames", designBeamCollectionNames);
            DA.SetDataList("DesignComponentNames", designComponentCollectionNames);

            DA.SetDataList("FabElementNames", fabElementCollectionNames);
            DA.SetDataList("FabPlateNames", fabPlateCollectionNames);
            DA.SetDataList("FabBeamNames", fabBeamCollectionNames);
            DA.SetDataList("FabComponentNames", fabComponentCollectionNames);

            DA.SetDataList("FabTaskNames", fabTaskCollectionNames);
            DA.SetDataList("FabTaskSequenceSchema", fabTaskSequenceSchema);

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
                return Resources.FabFramework_Icon_Collection;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bb98a03d-7091-475e-b09e-f3c32e7870b8"); }
        }
    }
}
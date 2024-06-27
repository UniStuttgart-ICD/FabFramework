using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace Fab.Grasshopper.GhComponents.GhcDigitalFUTURES
{
    public class GhcDF_UpdateMagazine : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcDF_UpdateMagazine()
            : base("UpdateMagazine",
                  "UpdMag",
                  "Update magazine slots dependent on magazine slot length.",
                  "Fab",
                  "DigitalFUTURES")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddTextParameter("FabElementName", "FEN", "Name of the FabElement", GH_ParamAccess.list);
            pManager.AddGenericParameter("StaticEnv", "SE", "Static Environment for the taskGeneration.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
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
            FabComponent iFabComponent = new FabComponent();
            DA.GetData("FabComponent", ref iFabComponent);

            FabComponent fabComponent = iFabComponent.ShallowCopy() as FabComponent;

            List<string> iFabElementName = new List<string>();
            DA.GetDataList("FabElementName", iFabElementName);

            List<StaticEnv> iStaticEnvs = new List<StaticEnv>();
            DA.GetDataList("StaticEnv", iStaticEnvs);

            //-----------
            //EDIT
            //-----------

            List<FabElement> matchingFabElements = FabUtilitiesElement.GetMatchingFabElement(fabComponent, iFabElementName);

            //sort matchingFabElements by matchingFabElements.Index
            matchingFabElements.Sort((x1, y1) => x1.Index.CompareTo(y1.Index));

            // Initialize a dictionary to keep track of counters for each key
            Dictionary<string, int> magazineCounters = new Dictionary<string, int>();

            for (int i = 0; i < matchingFabElements.Count; i++)
            {
                FabElement fabElement = matchingFabElements[i];
                double elementLength = fabElement.GetDesignElement().Length;

                StaticEnv closestStaticEnv = null;
                double smallestDifference = double.MaxValue;

                foreach (StaticEnv staticEnv in iStaticEnvs)
                {
                    double staticEnvLength = staticEnv.Length;

                    if (staticEnvLength == 0)
                    {
                        throw new Exception("StaticEnv Length is not defined.");
                    }

                    double difference = Math.Abs(staticEnvLength - elementLength);
                    if (difference < smallestDifference)
                    {
                        smallestDifference = difference;
                        closestStaticEnv = staticEnv;
                    }
                }

                if (closestStaticEnv == null)
                {
                    throw new Exception("No matching StaticEnv found.");
                }

                string closestKeyString = closestStaticEnv.Name;

                // Initialize the counter for this key if it doesn't exist
                if (!magazineCounters.ContainsKey(closestKeyString))
                {
                    magazineCounters[closestKeyString] = 0;
                }

                // Update magazine slots
                int magazineCounter = magazineCounters[closestKeyString];

                fabElement.RefPln_Mag = FabUtilities.OrientPlane(fabElement.RefPln_Situ, fabElement.RefPln_SituBox, closestStaticEnv.AlignPln[magazineCounter]);
                fabElement.EnvMag = closestStaticEnv;


                magazineCounter += 1;
                if (magazineCounter >= closestStaticEnv.AlignPln.Count)
                {
                    magazineCounter = 0;
                }

                // Update the counter in the dictionary
                magazineCounters[closestKeyString] = magazineCounter;
            }

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabComponent", fabComponent);
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
                return Resources.FabFramework_Icon_FillMagazineByIndex;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6f8956f8-7f93-43d1-bf75-e41381da398b"); }
        }
    }
}
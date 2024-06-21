using Fab.Core.DesignElement;
using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using System;

namespace Fab.Grasshopper.GhComponents.GhcElement
{
    public class GhcDefineFabComponent : GH_Component
    {


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcDefineFabComponent()
          : base("Define FabComponent",
                "FabComponent",
                "Define FabComponent from DesignComponent and StaticEnvironments.",
                "Fab",
                "Element")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("DesignComponent", "DC", "Design component Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("StaticEnvFab", "SEF", "Static environment for the fabrication.", GH_ParamAccess.item);
            pManager.AddGenericParameter("StaticEnvPlate", "SEP", "Static environment for the plates.", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("StaticEnvBeam", "SEB", "Static environment for the beams.", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "Defined FabComponent Data", GH_ParamAccess.item);
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
            DesignComponent iDesignComponent = new DesignComponent();
            DA.GetData("DesignComponent", ref iDesignComponent);

            StaticEnv iFabEnvironment = new StaticEnv();
            DA.GetData("StaticEnvFab", ref iFabEnvironment);

            StaticEnv iStaticEnvPlate = null;
            DA.GetData("StaticEnvPlate", ref iStaticEnvPlate);

            StaticEnv iStaticEnvBeam = null;
            DA.GetData("StaticEnvBeam", ref iStaticEnvBeam);

            //------------------------------------------------------------------------------------------------------
            //GENERATE FABELEMENTS
            //------------------------------------------------------------------------------------------------------

            FabCollection fabCollection = FabCollection.GetFabCollection();

            string fabComponentName = iDesignComponent.Name;

            // Check if the string has at least four characters
            if (fabComponentName.Length >= 5)
            {
                // Remove the first four characters
                fabComponentName = fabComponentName.Substring(5);
            }

            FabComponent fabComponent = new FabComponent(fabComponentName, iDesignComponent, iFabEnvironment);


            if (iStaticEnvPlate != null)
            {
                if (iDesignComponent != null && iDesignComponent.DesignPlatesName != null)
                {
                    for (int i = 0; i < iDesignComponent.DesignPlatesName.Count; i++)
                    {
                        string fabPlateName = iDesignComponent.DesignPlatesName[i];
                        if (fabPlateName.Length >= 5)
                        {
                            // Remove the first four characters
                            fabPlateName = fabPlateName.Substring(5);
                        }

                        FabPlate fabPlate = new FabPlate(fabPlateName, fabComponent, fabCollection.designPlateCollection[iDesignComponent.DesignPlatesName[i]], iStaticEnvPlate);
                    }
                }
            }

            if (iStaticEnvBeam != null)
            {
                if (iDesignComponent != null && iDesignComponent.DesignBeamsName != null)
                {
                    for (int i = 0; i < iDesignComponent.DesignBeamsName.Count; i++)
                    {
                        string fabBeamName = iDesignComponent.DesignBeamsName[i];
                        if (fabBeamName.Length >= 5)
                        {
                            // Remove the first four characters
                            fabBeamName = fabBeamName.Substring(5);
                        }

                        FabBeam fabBeam = new FabBeam(fabBeamName, fabComponent, fabCollection.designBeamCollection[iDesignComponent.DesignBeamsName[i]], iStaticEnvBeam);
                    }
                }
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
                return Resources.FabFramework_FabComponent;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0d18a9f4-b6b2-4cb6-ac3f-e57dae68af35"); }
        }
    }
}
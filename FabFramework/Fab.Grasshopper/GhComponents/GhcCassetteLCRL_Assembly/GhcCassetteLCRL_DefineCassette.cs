using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_DefineCassette : GH_Component
    {


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_DefineCassette()
          : base("LCRLAssemlbyCassette",
                "LCRL FCas",
                "Convert the FabComponent data for LCRL Assembly FabCassette data.",
                "Fab",
                "LCRL")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent data", GH_ParamAccess.item);
            pManager.AddPlaneParameter("TurnTablePosition", "TTP", "Cassette Position on the turntable", GH_ParamAccess.item);
            pManager.AddNumberParameter("TurnTableAngle", "TTA", "Cassette Angle on the turntable", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "Converted FabCassette Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //-----------
            //GLOBALS
            //-----------
            Double turnTable_StartAngle = 180;

            FabCollection fabCollection = FabCollection.GetFabCollection();

            //-----------
            //INPUTS
            //-----------

            FabComponent iFabComponent = new FabComponent();
            DA.GetData<FabComponent>("FabComponent", ref iFabComponent);

            Plane iTurnTablePosition = Plane.Unset;
            DA.GetData<Plane>("TurnTablePosition", ref iTurnTablePosition);

            double iTurnTableAngle = double.NaN;
            DA.GetData<double>("TurnTableAngle", ref iTurnTableAngle);


            FabComponent fabComponent = iFabComponent.ShallowCopy() as FabComponent;

            FabPlate fabBotPlate = fabCollection.fabPlateCollection[fabComponent.FabPlatesName[0]];
            FabPlate fabTopPlate = fabCollection.fabPlateCollection[fabComponent.FabPlatesName[1]];

            //------------------------------------------------------------------------------------------------------    
            //OVERWRITE ATTRIBUTES (this is specific to the LCRL fabrication setup)
            //------------------------------------------------------------------------------------------------------

            //fabComponent.Name = "LCRL_Cassette"; need to create a custom function in order for this to work

            //Adjust RefFab & RefFabOut of all FabElements
            FabUtilitiesElement.AdjustAllElementRefFabAndFabOutPlanes(fabComponent, iTurnTablePosition, turnTable_StartAngle);

            fabComponent.EnvMag = fabBotPlate.EnvMag;
            fabBotPlate.RefPln_Mag = FabUtilities.TransformPlaneByVector(fabBotPlate.RefPln_Mag, fabBotPlate.RefPln_Mag.ZAxis, fabTopPlate.GetDesignPlate().Height);


            //BEAMS.FABOUT: Change RefPln_FabOut from Beams to ensure that they are all reachable with the robot
            List<FabBeam> fabBeams = new List<FabBeam>();
            for (int i = 0; i < fabComponent.FabBeamsName.Count; i++)
            {

                // Check if the key exists in the dictionary
                if (fabCollection.fabBeamCollection.ContainsKey(fabComponent.FabBeamsName[i]))
                {
                    // The key exists, retrieve the value
                    FabBeam fabBeam = fabCollection.fabBeamCollection[fabComponent.FabBeamsName[i]];

                    // Perform any other operations with fabBeam here
                    fabBeam.RefPln_FabOut = FabUtilitiesBeam.Optimized_FabOut_BeamTurnTable(fabBeam);

                    // Add fabBeam to the fabBeams list
                    fabBeams.Add(fabBeam);
                }
                else
                {
                    // The key does not exist, throw an exception
                    throw new KeyNotFoundException($"Key '{fabComponent.FabBeamsName[i]}' does not exist in the dictionary.");
                }

            }


            //BEAMS.FABOUT: Sort all beams in which they should be placed onto the turntable, so they can be placed from 180° to -180° in a clockwise motion
            fabBeams = FabUtilitiesBeam.SortBeamFabricationOrderForTurnTable(fabBeams, turnTable_StartAngle);

            //Update Order of beams in fabComponent
            fabComponent.FabBeamsName.Clear();

            for (int i = 0; i < fabBeams.Count; i++)
            {
                fabComponent.FabBeamsName.Add(fabBeams[i].Name);
            }


            //BEAMS.MAG: Sort all beams in magazine, accoriding to specific LCRL beam magazine setup
            FabUtilitiesBeam.AdjustBeamRefPln_Mag(fabBeams);


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabComponent);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("40856ba2-757a-4424-9c6f-0a409bc3a920"); }
        }
    }
}
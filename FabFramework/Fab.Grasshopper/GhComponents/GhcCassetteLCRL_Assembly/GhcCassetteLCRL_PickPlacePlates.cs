using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;


namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_PickPlacePlates : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_PickPlacePlates()
          : base("LCRLCassette PnP Plates",
                "LCRL PnpPlates",
                "Get pick and place tasks for the bot & top plate and the finished cassette of the LCRL cassette.",
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
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("Endeffector", "EE", "Specific endeeffector by the actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("PickAction", "Pick", "Pick action for the taskGeneration", GH_ParamAccess.item);
            pManager.AddGenericParameter("PlaceAction", "Place", "Place action for the taskGeneration", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);

            pManager.AddGenericParameter("PickBottomPlate", "PickBP", "Fabrication Task for picking the bottom Plate from the plate table.", GH_ParamAccess.item);
            pManager.AddGenericParameter("PlaceBottomPlate", "PlaceBP", "Fabrication Task for placing the bottom Plate onto the turn table.", GH_ParamAccess.item);

            pManager.AddGenericParameter("PickTopPlate", "PickTP", "Fabrication Task for picking the top Plate from the plate table.", GH_ParamAccess.item);
            pManager.AddGenericParameter("PlaceTopPlate", "PlaceTP", "Fabrication Task for placing the top Plate onto the turn table.", GH_ParamAccess.item);

            pManager.AddGenericParameter("PickCassette", "PickCas", "Fabrication Task for picking the Cassette from the plate table.", GH_ParamAccess.item);
            pManager.AddGenericParameter("PlaceCassette", "PlaceCas", "Fabrication Task for placing the Cassette onto the turn table.", GH_ParamAccess.item);
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
            FabComponent iFabCassette = new FabComponent();
            DA.GetData<FabComponent>("FabCassette", ref iFabCassette);

            FabComponent fabCassette = iFabCassette.ShallowCopy() as FabComponent;

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            Endeffector iEndeffector = new Endeffector();
            DA.GetData("Endeffector", ref iEndeffector);

            Fab.Core.FabEnvironment.Action iPickAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("PickAction", ref iPickAction);

            Fab.Core.FabEnvironment.Action iPlaceAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("PlaceAction", ref iPlaceAction);
            //-----------
            //EDIT
            //-----------
            List<double> offsetList = new List<double> { 0.0, 0.0, 300.0 };


            FabPlate fabBotPlate = fabCollection.fabPlateCollection[fabCassette.FabPlatesName[0]];
            FabPlate fabTopPlate = fabCollection.fabPlateCollection[fabCassette.FabPlatesName[1]];

            //BOTPLATE
            FabTaskFrame fabTask_PickBotPlate = new FabTaskFrame(iPickAction.Name + "_" + fabBotPlate.Name); ;
            FabTaskFrame fabTask_PlaceBotPlate = new FabTaskFrame(iPlaceAction.Name + "_" + fabBotPlate.Name);

            FabTaskFrame.SetFabPlatePickPlaceTask_TurnTable(fabTask_PickBotPlate, fabTask_PlaceBotPlate, fabBotPlate, iPickAction, iPlaceAction, iEndeffector, iFabActor, offsetList);

            bool invertAngleBotPlate_FabOut = false; // Start at turntable -180 --> set to true

            if (invertAngleBotPlate_FabOut == true)
            {
                fabTask_PickBotPlate.InvertExternalValues(fabTask_PickBotPlate.Main_ExtValues, "E2");
                fabTask_PlaceBotPlate.InvertExternalValues(fabTask_PlaceBotPlate.Main_ExtValues, "E2");
            }



            //TOPPLATE
            FabTaskFrame fabTask_PickTopPlate = new FabTaskFrame(iPickAction.Name + "_" + fabTopPlate.Name); ;
            FabTaskFrame fabTask_PlaceTopPlate = new FabTaskFrame(iPlaceAction.Name + "_" + fabTopPlate.Name);
            FabTaskFrame.SetFabPlatePickPlaceTask_TurnTable(fabTask_PickTopPlate, fabTask_PlaceTopPlate, fabTopPlate, iPickAction, iPlaceAction, iEndeffector, iFabActor, offsetList);

            bool invertAngleTopPlate_FabOut = false; // Start at turntable -180
            if (invertAngleTopPlate_FabOut == true)
            {
                fabTask_PickTopPlate.InvertExternalValues(fabTask_PickTopPlate.Main_ExtValues, "E2");
                fabTask_PlaceTopPlate.InvertExternalValues(fabTask_PlaceTopPlate.Main_ExtValues, "E2");
            }


            //CASSETTE
            FabTaskFrame fabTask_PickCassette = new FabTaskFrame(iPickAction.Name + "_" + fabCassette.Name);
            FabTaskFrame fabTask_PlaceCassette = new FabTaskFrame(iPlaceAction.Name + "_" + fabCassette.Name);


            //offset missing
            FabTaskFrame.SetFabCassettePickPlaceTask_TurnTable(fabTask_PickCassette, fabTask_PlaceCassette, fabTopPlate, fabCassette, iPickAction, iPlaceAction, iEndeffector, iFabActor, offsetList);

            //Overwrite: Offset Place Cassette
            FabTaskFrame.ShiftOffsetPlaceCassette(fabTask_PlaceCassette, fabCassette, iFabActor, 200.0);

            bool invertAngleCassette_FabOut = false; // Start at turntable -180
            if (invertAngleCassette_FabOut == true)
            {
                fabTask_PickCassette.InvertExternalValues(fabTask_PickCassette.Main_ExtValues, "E2");
                fabTask_PlaceCassette.InvertExternalValues(fabTask_PlaceCassette.Main_ExtValues, "E2");
            }


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);

            DA.SetData("PickBottomPlate", fabTask_PickBotPlate);
            DA.SetData("PlaceBottomPlate", fabTask_PlaceBotPlate);

            DA.SetData("PickTopPlate", fabTask_PickTopPlate);
            DA.SetData("PlaceTopPlate", fabTask_PlaceTopPlate);

            DA.SetData("PickCassette", fabTask_PickCassette);
            DA.SetData("PlaceCassette", fabTask_PlaceCassette);
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
            get { return new Guid("6defc429-18f6-4ac0-bf32-54f4bd4990f7"); }
        }
    }
}
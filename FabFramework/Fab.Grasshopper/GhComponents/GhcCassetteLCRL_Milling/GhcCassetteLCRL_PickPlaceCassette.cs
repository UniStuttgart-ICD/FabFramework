using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;


namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_PickPlaceCassette : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_PickPlaceCassette()
          : base("LCRLCassette PnP Cassette",
                "LCRL PnpCassette",
                "Get pick and place tasks for the cassette plate and the finished cassette of the LCRL cassette.",
                "Fab",
                "LCRL")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
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

            pManager.AddGenericParameter("PickCassettePlate", "PickCAS", "Fabrication Task for picking the cassette Plate from the plate table.", GH_ParamAccess.item);
            pManager.AddGenericParameter("PlaceCassettePlate", "PlaceCAS", "Fabrication Task for placing the cassette Plate onto the turn table.", GH_ParamAccess.item);
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
            List<double> offsetList = new List<double> { 0.0, 0.0, 200.0 };


            FabPlate cassettePlate = fabCollection.fabPlateCollection[fabCassette.FabPlatesName[0]];

            //BOTPLATE
            FabTaskFrame fabTask_PickCassettePlate = new FabTaskFrame(iPickAction.Name + "_" + cassettePlate.Name); ;
            FabTaskFrame fabTask_PlaceCassettePlate = new FabTaskFrame(iPlaceAction.Name + "_" + cassettePlate.Name);

            FabTaskFrame.SetFabPlatePickPlaceTask(fabTask_PickCassettePlate, fabTask_PlaceCassettePlate, cassettePlate, iPickAction, iPlaceAction, iEndeffector, iFabActor, offsetList);

            bool invertAngleCassettePlate_FabOut = true; // Start at turntable -180 --> set to true

            if (invertAngleCassettePlate_FabOut == true)
            {
                fabTask_PickCassettePlate.InvertExternalValues(fabTask_PickCassettePlate.Main_ExtValues, "E2");
                fabTask_PlaceCassettePlate.InvertExternalValues(fabTask_PlaceCassettePlate.Main_ExtValues, "E2");
            }



            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);

            DA.SetData("PickCassettePlate", fabTask_PickCassettePlate);
            DA.SetData("PlaceCassettePlate", fabTask_PlaceCassettePlate);
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
            get { return new Guid("4691836c-23a3-44f8-aebc-f3c1a37a5f8f"); }
        }
    }
}
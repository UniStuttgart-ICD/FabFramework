using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_PickPlaceNailBeams : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_PickPlaceNailBeams()
          : base("LCRL Cassette PickPlaceNail Beam",
                "LCRL PnpNailBeam",
                "Get pick, place and nail tasks for the beams of the LCRL Cassette.",
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
            pManager.AddGenericParameter("PlaceNailAction", "PlaceNail", "PlaceNail action for the taskGeneration", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);

            pManager.AddGenericParameter("PickBeam", "PickBeam", "Fabrication Task for picking the beams from the beam table.", GH_ParamAccess.list);
            pManager.AddGenericParameter("PlaceNailBeam", "PlaceNailBeam", "Fabrication Task for placing the beams onto the turn table and nailing them down.", GH_ParamAccess.list);
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
            FabComponent fabCassette = new FabComponent();
            DA.GetData("FabCassette", ref fabCassette);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            Endeffector iEndeffector = new Endeffector();
            DA.GetData("Endeffector", ref iEndeffector);

            Fab.Core.FabEnvironment.Action iPickAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("PickAction", ref iPickAction);

            Fab.Core.FabEnvironment.Action iPlaceNailAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("PlaceNailAction", ref iPlaceNailAction);

            //-----------
            //EDIT
            //-----------
            List<double> offsetList = new List<double> { 0.0, 0.0, 200.0 };

            List<FabTaskFrame> fabTask_PickBeams = new List<FabTaskFrame>();
            List<FabTaskFrame> fabTask_PlaceNailBeams = new List<FabTaskFrame>();

            for (int i = 0; i < fabCassette.FabBeamsName.Count; i++)
            {
                // Check if the key exists in the dictionary
                if (fabCollection.fabBeamCollection.ContainsKey(fabCassette.FabBeamsName[i]))
                {
                    // The key exists, retrieve the value
                    FabBeam fabBeam = fabCollection.fabBeamCollection[fabCassette.FabBeamsName[i]];

                    FabTaskFrame fabTask_PlaceNailBeam = new FabTaskFrame(iPlaceNailAction.Name + "_" + fabBeam.Name);
                    FabUtilitiesBeam.GetBeamPlaceNailPlanes(fabBeam, out List<Plane> placeNailPlanes, out List<int> placeNailStates, out List<Plane> nailPositionPlanes);
                    FabTaskFrame.SetBeamsPlaceNailTask(fabTask_PlaceNailBeam, fabBeam, placeNailPlanes, placeNailStates, nailPositionPlanes, iPlaceNailAction, iEndeffector, iFabActor, offsetList);
                    fabTask_PlaceNailBeams.Add(fabTask_PlaceNailBeam);
                    AddPlaceNailTaskGeo(fabTask_PlaceNailBeam, fabCassette); //Add Geo

                    FabTaskFrame fabTask_PickBeam = new FabTaskFrame(iPickAction.Name + "_" + fabBeam.Name);
                    Plane beam_PickPlane = FabUtilities.OrientPlane(placeNailPlanes[0], fabBeam.RefPln_FabOut, fabBeam.RefPln_Mag);
                    FabTaskFrame.SetBeamsPickTask(fabTask_PickBeam, fabBeam, beam_PickPlane, iPickAction, iEndeffector, iFabActor, offsetList);
                    fabTask_PickBeams.Add(fabTask_PickBeam);

                }
                else
                {
                    // The key does not exist, throw an exception
                    throw new KeyNotFoundException($"Key '{fabCassette.FabBeamsName[i]}' does not exist in the dictionary.");
                }

            }


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);

            DA.SetDataList("PickBeam", fabTask_PickBeams);
            DA.SetDataList("PlaceNailBeam", fabTask_PlaceNailBeams);

        }

        public static bool AddPlaceNailTaskGeo(FabTaskFrame nailTask, FabComponent fabCassette)
        {
            FabCollection fabCollection = FabCollection.GetFabCollection();

            List<GeometryBase> placeNailGeos = new List<GeometryBase>();
            fabCollection.fabBeamCollection.TryGetValue(nailTask.FabElementsName[0], out FabBeam fabBeam);

            //Beam
            placeNailGeos.Add(FabUtilities.OrientGeometryBase(fabBeam.Geometry, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut));

            //BotPlate
            fabCollection.fabPlateCollection.TryGetValue(fabCassette.FabPlatesName[0], out FabPlate fabPlate);
            GeometryBase botPlateGeo = FabUtilities.OrientGeometryBase(fabPlate.Geometry, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);
            placeNailGeos.Add(botPlateGeo);


            nailTask.Geometry.Add(FabUtilities.ConvertToJoinedMesh(placeNailGeos));


            return true;
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
            get { return new Guid("d224416d-a5e9-43f5-a5b5-4b489da2e164"); }
        }
    }
}
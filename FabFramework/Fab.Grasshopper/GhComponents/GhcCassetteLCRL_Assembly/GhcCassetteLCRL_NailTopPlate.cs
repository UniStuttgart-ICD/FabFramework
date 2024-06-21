using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Eto.Forms;
using System.Linq;
using Fab.Core.FabCollection;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_NailTopPlate : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_NailTopPlate()
          : base("LCRLCassette Nail TopPlate", 
                "LCRL NailTP",
                "Get nail task for the top plate of the LCRL Cassette.",
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
            pManager.AddGenericParameter("NailAction", "Nail", "Pick action for the taskGeneration", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("NailTopPlate", "NailTP", "Fabrication Task for nailing down the top plate.", GH_ParamAccess.item);
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
            FabComponent fabCassette = new FabComponent();
            DA.GetData("FabCassette", ref fabCassette);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            Endeffector iEndeffector = new Endeffector();
            DA.GetData("Endeffector", ref iEndeffector);

            Fab.Core.FabEnvironment.Action iNailAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("NailAction", ref iNailAction);

            //-----------
            //EDIT
            //-----------
            List<double> offsetList = new List<double> { 0.0, 0.0, 200.0 };

            List<Plane> topPlateNailPlanes = new List<Plane>();
            List<int> topPlateNailStates = new List<int>();
            List<double> topPlateNailTTAngle = new List<double>();

            for (int i = 0; i < fabCassette.FabBeamsName.Count; i++)
            {
                FabUtilitiesPlate.GetTopPlateNailPlanes(fabCassette.GetFabBeams()[i], fabCassette.GetFabPlates()[1], out List<Plane> topPlateNailPlanesSide, out List<int> topPlateNailStatesSide, out List<double> topPlateNailTTAngleSide);
                topPlateNailPlanes.AddRange(topPlateNailPlanesSide);
                topPlateNailStates.AddRange(topPlateNailStatesSide);
                topPlateNailTTAngle.AddRange(topPlateNailTTAngleSide);
            }
            FabTaskFrame nailTopPlateTask = new FabTaskFrame(iNailAction.Name + "_" + fabCassette.GetFabPlates()[1].Name);
            FabTaskFrame.SetNailPlateTask(nailTopPlateTask, fabCassette.GetFabPlates()[1], topPlateNailPlanes, topPlateNailStates, topPlateNailTTAngle, iNailAction, iEndeffector, iFabActor, offsetList);
            AddNailTaskGeo(nailTopPlateTask, fabCassette);

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);

            DA.SetData("NailTopPlate", nailTopPlateTask);

        }

        public static bool AddNailTaskGeo(FabTaskFrame nailTask, FabComponent fabCassette)
        {
            FabCollection fabCollection = FabCollection.GetFabCollection();
            // Geometry
            List<Plane> mainPlanes = nailTask.Main_Frames;
            List<double> mainAngles = nailTask.Main_ExtValues["E2"];
            fabCollection.fabPlateCollection.TryGetValue(nailTask.FabElementsName[0], out FabPlate fabPlate);

            for (int i = 0; i < mainPlanes.Count ; i++)
            {
                    GeometryBase nailPlateGeo = FabUtilities.OrientGeometryBase(fabCassette.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_FabOut);
                    double turnStartPlateAngle = FabUtilities.DegreeToRadian(mainAngles[i] - fabPlate.Angle_FabOut);
                    Transform rotationStartTransform = Transform.Rotation(turnStartPlateAngle, fabPlate.EnvFab.RefPln[0].ZAxis, fabPlate.EnvFab.RefPln[0].Origin);
                    nailPlateGeo.Transform(rotationStartTransform);
                    nailTask.Geometry.Add(nailPlateGeo);
            }


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
            get { return new Guid("64cbc9b6-4843-4f2f-85a9-0b5e52a1a820"); }
        }
    }
}
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_TaskTravelGenerator : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_TaskTravelGenerator()
          : base("LCRLCassette Task TravelGenerator",
                "LCRL TaskTravel",
                "Generate all travel specific tasks for the fabrication.",
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
            pManager.AddGenericParameter("FabTasks", "FabTasks", "List of all FabTasks for the LCRL FabCassette", GH_ParamAccess.list);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("StaticEnv", "SE", "Actor", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("FabTasks", "FabTasks", "List of all FabTasks for the LCRL FabCassette", GH_ParamAccess.list);
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

            List<FabTask> fabTasks = new List<FabTask>();
            DA.GetDataList<FabTask>("FabTasks", fabTasks);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            List<StaticEnv> iStaticEnvs = new List<StaticEnv>();
            DA.GetDataList<StaticEnv>("StaticEnv", iStaticEnvs);

            //-----------
            //EDIT
            //-----------


            //Set Travel Task Values: A1, A2, A3, A4, A5, A6, E1, E2
            var extValues_PT_VGrip = FabTaskFrame.CreateTravelAxis_ExtValues(95.884, -40.732, 46.038, 0.0, 84.694, -84.116, 785.0);
            Plane frames_PT_VGrip = new Plane(new Point3d(2200, -300, 1700), new Vector3d(0, -1, 0), new Vector3d(1, 0, 0));

            var extValues_BT_NGrip = FabTaskFrame.CreateTravelAxis_ExtValues(-102.241, -67.932, 88.877, 19.935, 26.085, -116.765, 3917.0);
            Plane frames_BT_NGrip = new Plane(new Point3d(-6875, -500, 1900), new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0));

            var extValues_TT_VGrip = FabTaskFrame.CreateTravelAxis_ExtValues(-179.961, -31.331, 21.752, 0.0, 99.579, -89.961, 1877.0);
            Plane frames_TT_VGrip = new Plane(new Point3d(-1876.790037, -3115.958475, 1923), new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0));

            var extValues_TT_NGrip = FabTaskFrame.CreateTravelAxis_ExtValues(141.461, -71.18, 88.211, -2.202, 27.995, -87.022, 4750.0);
            Plane frames_TT_NGrip = new Plane(new Point3d(-2876.790037, -2315.958475, 2023), new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0));

            var extValues_TT_GGun = FabTaskFrame.CreateTravelAxis_ExtValues(147.045, -69.976, 77.909, -28.754, 13.861, -59.534, 4750.0);
            Plane frames_TT_GGun = new Plane(new Point3d(-2876.790037, -2315.958475, 1823), new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0));

            var extValues_TT_NGun = FabTaskFrame.CreateTravelAxis_ExtValues(141.397, -59.61, 85.671, -176.822, 26.096, 87.146, 4750.0);
            Plane frames_TT_NGun = new Plane(new Point3d(-2876.790037, -2315.958475, 1823), new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0));


            //Define travek Tasks with name and respective Endeffector/StaticEnv
            var travelDictionary = new Dictionary<(string, string), (string, Dictionary<string, double>, Plane)> // Use a tuple to hold the additional data
            {
                {("PlateTable", "VacuumGripper"), ("Travel_PT_VGrip", extValues_PT_VGrip, frames_PT_VGrip)},
                {("BeamTable", "NailGripper"), ("Travel_BT_NGrip", extValues_BT_NGrip, frames_BT_NGrip)},
                {("TurnTable", "VacuumGripper"), ("Travel_TT_VGrip", extValues_TT_VGrip, frames_TT_VGrip)},
                {("TurnTable", "NailGripper"), ("Travel_TT_NGrip", extValues_TT_NGrip, frames_TT_NGrip)},
                {("TurnTable", "GlueGun"), ("Travel_TT_GGun", extValues_TT_GGun, frames_TT_GGun)},
                {("TurnTable", "NailGun"), ("Travel_TT_NGun", extValues_TT_NGun, frames_TT_NGun)}
            };

            List<FabTaskFrame> travelAxisTasks = FabTaskFrame.CreateTravelAxisTask(travelDictionary, iFabActor, iStaticEnvs);

            List<FabTask> interlinkedTravelTasks = FabTask.InterlinkTravelTasks(fabTasks, travelDictionary, travelAxisTasks);

            // Set outputs
            DA.SetData("FabCassette", fabCassette);
            DA.SetDataList("FabTasks", interlinkedTravelTasks);
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
            get { return new Guid("088121cb-b839-43f3-9692-affee8236dfb"); }
        }
    }
}
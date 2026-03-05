using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;


namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcTravelGenerator : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcTravelGenerator()
          : base("TravelGenerator",
                "Travel",
                "Create automated travel tasks between FabTask.SafePositions.",
                "Fab",
                "BaseTasks")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data.", GH_ParamAccess.item);
            pManager.AddGenericParameter("FabTasks", "FT", "List of FabTasks to generate travel tasks between.", GH_ParamAccess.list);
            pManager.AddGenericParameter("TravelAction", "A", "Travel action for the taskGeneration", GH_ParamAccess.item);
            pManager.AddBooleanParameter("PickOptimalZ", "PickZ", "Pick the optimal Z value for the target plane travel task.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("OptimalZ", "OptZ", "Offset for the optimal Z value.", GH_ParamAccess.item, 300.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("SequencedFabTasks", "SFT", "Sequenced FabTasks Data incl. travel Tasks.", GH_ParamAccess.item);
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
            DA.GetData<FabComponent>("FabComponent", ref iFabComponent);
            FabComponent fabComponent = iFabComponent.ShallowCopy() as FabComponent;

            List<FabTask> iFabTasks = new List<FabTask>();
            DA.GetDataList<FabTask>("FabTasks", iFabTasks);

            Fab.Core.FabEnvironment.Action iTravelAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("TravelAction", ref iTravelAction);

            bool iPickOptimalZ = false;
            DA.GetData("PickOptimalZ", ref iPickOptimalZ);

            double iOptimalZ = 300.0;
            DA.GetData("OptimalZ", ref iOptimalZ);

            //-----------
            //EDIT
            //-----------


            List<FabTask> sequencedFabTasks = new List<FabTask>();


            FabTask priorTask = iFabTasks[0];
            var priorStaticEnvVar = priorTask.StaticEnvs.FirstOrDefault();
            sequencedFabTasks.Add(priorTask);

            if (priorStaticEnvVar.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "FabTask does not have StaticEnvs.");
                return;
            }

            StaticEnv priorStaticEnv = priorStaticEnvVar.Value;


            for (int i = 1; i < iFabTasks.Count; i++)
            {
                FabTask currentTask = iFabTasks[i];

                var currentStaticEnvVar = currentTask.StaticEnvs.FirstOrDefault();

                if (currentStaticEnvVar.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "FabTask does not have StaticEnvs.");
                    return;
                }

                StaticEnv currentStaticEnv = currentStaticEnvVar.Value;

                if (currentStaticEnv.Name != priorStaticEnv.Name)
                {
                    string travelName = "Travel_" + priorStaticEnv.Name + "_to_" + currentStaticEnv.Name + "_" + i.ToString("D3");

                    // Create Travel Task
                    FabTaskFrame travelTask = new FabTaskFrame(travelName);
                    FabTaskFrame.SetTravelTaskSafePositions(travelTask, iTravelAction, priorTask, currentTask, iPickOptimalZ, iOptimalZ);

                    sequencedFabTasks.Add(travelTask);
                }

                // Update priorStaticEnv to currentStaticEnv for the next iteration
                sequencedFabTasks.Add(currentTask);
                priorTask = currentTask;
                priorStaticEnv = currentStaticEnv;
            }

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabComponent", fabComponent);
            DA.SetDataList("SequencedFabTasks", sequencedFabTasks);
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
                return Resources.FabFramework_Icon_TravelGenerator;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ede0d32f-958b-4654-bab7-6086d3851b82"); }
        }
    }
}
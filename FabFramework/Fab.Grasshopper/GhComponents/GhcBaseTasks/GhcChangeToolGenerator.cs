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
    public class GhcChangeToolGenerator : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcChangeToolGenerator()
          : base("ChangeToolGenerator",
                "ChangeTool",
                "Create automated change tool tasks.",
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
            pManager.AddGenericParameter("ChangeToolAction", "A", "ChangeTool action for the taskGeneration", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("SequencedFabTasks", "SFT", "Sequenced FabTasks Data incl. changed tool Tasks.", GH_ParamAccess.item);
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

            Fab.Core.FabEnvironment.Action iToolChangeAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("ChangeToolAction", ref iToolChangeAction);

            //-----------
            //EDIT
            //-----------


            List<FabTask> sequencedFabTasks = new List<FabTask>();


            FabTask priorTask = iFabTasks[0];
            var priorToolVar = priorTask.Tools.FirstOrDefault();
            sequencedFabTasks.Add(priorTask);

            if (priorToolVar.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "FabTask does not have Tool.");
                return;
            }

            Tool priorTool = priorToolVar.Value;


            for (int i = 1; i < iFabTasks.Count; i++)
            {
                FabTask currentTask = iFabTasks[i];

                var currentToolVar = currentTask.Tools.FirstOrDefault();

                if (currentToolVar.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "FabTask does not have Tool.");
                    return;
                }

                Tool currentTool = currentToolVar.Value;

                if (currentTool.Name != priorTool.Name)
                {
                    string changeToolName = "ChangeTool_" + priorTool.Name + "_to_" + currentTool.Name + "_" + i.ToString("D3");

                    // Create Travel Task
                    FabTask toolChangeTask = new FabTask(changeToolName);
                    FabTask.SetToolChangeTask(toolChangeTask, iToolChangeAction, priorTask, currentTask); //add current task

                    sequencedFabTasks.Add(toolChangeTask);               
                }
                sequencedFabTasks.Add(currentTask);

                // Update priorStaticEnv to currentStaticEnv for the next iteration
                priorTask = currentTask;
                priorTool = currentTool;
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
                return Resources.FabFramework_Icon_ChangeToolGenerator;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d540f1a2-c32b-41bf-b000-af25216fcc77"); }
        }
    }
}
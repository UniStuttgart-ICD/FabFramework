using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcSequenceTasks : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcSequenceTasks()
          : base("Sequence FabTasks",
                "SeqTask",
                "Sequence FabTasks according to Task Schema. IterateTasks allows you to loop through same tasks of that dictionary. Only works if those FabTasks have an Index value assigned.",
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
            pManager.AddGenericParameter("TaskSchema", "TS", "Task Schema. List of dictionary - Key: FabElementName, Value: List of ActionNames.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("IterateTasks", "IT", "Define if TaskSchema dictionary should be iterated through by FabTask index numbers. Match list length with TaskSchema list length.", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("SequencedFabTasks", "SFT", "Sequenced FabTasks Data.", GH_ParamAccess.item);
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

            List<Dictionary<string, List<object>>> iTaskSchema = new List<Dictionary<string, List<object>>>();
            DA.GetDataList("TaskSchema", iTaskSchema);

            List<bool> iIterateTasks = new List<bool>();
            DA.GetDataList("IterateTasks", iIterateTasks);

            if (iIterateTasks.Count == 0)
            {
                //create a list of bools with the same length as iTaskSchema
                for (int i = 0; i < iTaskSchema.Count; i++)
                {
                    iIterateTasks.Add(false);
                }
            }
            else if (iIterateTasks.Count != iTaskSchema.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "IterateTasks list length does not match TaskSchema list length.");
                return;
            }

            //-----------
            //EDIT
            //-----------

            List<FabTask> sequencedFabTasks = new List<FabTask>();

            List<FabElement> componentFabElements = FabUtilitiesElement.GetAllFabComponentElements(fabComponent, true);

            //sort componentFabElements by componentFabElements.Index
            componentFabElements.Sort((x1, y1) => x1.Index.CompareTo(y1.Index));

            int counterDebug = 0;

            //iterate through componentFabElements
            foreach (FabElement fabElement in componentFabElements)
            {
                string searchFabElementName = fabElement.Name;


                //Check if TaskSchema is valid
                if (iTaskSchema != null && iTaskSchema.Count > 0)
                {

                    for (int i = 0; i < iTaskSchema.Count; i++)
                    {
                        var taskSchema = iTaskSchema[i];


                        //Iterate through each kvp (fabElementName) in taskSchema
                        foreach (var kvp in taskSchema) //currently taskSchema only has one kvp, not multiple
                        {
                            string key = kvp.Key;

                            //Check if searchFabElementName contains fabElementName from taskSchema
                            if (searchFabElementName.Contains(key))
                            {
                                List<FabTask> unsortedSelectedTasks = new List<FabTask>();

                                //Modify dictionary values to List<string>
                                var dummyList = kvp.Value.ToList();
                                List<string> values = new List<string>();
                                foreach (var value in dummyList)
                                {
                                    if (value is GH_String)
                                    {
                                        GH_String dummy = value as GH_String;
                                        values.Add(Convert.ToString(dummy.Value));
                                    }
                                    else
                                    {
                                        throw new Exception("Type not supported");
                                    }
                                }

                                //Iterate through each actionName in values (specified actions in dictionary input)
                                foreach (string actionName in values)
                                {

                                    //check associated fabElement tasks, get name as string
                                    foreach (string fabTaskName in fabElement.FabTasksName)
                                    {
                                        //Get FabTask from 
                                        fabCollection.fabTaskCollection.TryGetValue(fabTaskName, out FabTask fabTask);

                                        fabTask.Action.TryGetValue(actionName, out Fab.Core.FabEnvironment.Action action);

                                        //Check if action is not null
                                        if (action != null && actionName == action.Name)
                                        {
                                            if (iIterateTasks[i] == false)
                                            {
                                                sequencedFabTasks.Add(fabTask);
                                            }
                                            else if (iIterateTasks[i] == true)
                                            {
                                                unsortedSelectedTasks.Add(fabTask);
                                            }
                                            else
                                            {
                                                throw new Exception($"iIterateTasks contains a non boolean value.");
                                            }

                                        }

                                    }
                                }

                                //SHOULD work, but not tested yet
                                if (iIterateTasks[i] == true)
                                {
                                    // Check that all unsortedTasks have an Index value
                                    foreach (FabTask unsortedTask in unsortedSelectedTasks)
                                    {
                                        if (unsortedTask.Index == null)
                                        {
                                            throw new Exception($"FabTask {unsortedTask.Name} does not have an index value.");
                                        }
                                    }

                                    // Group unsortedSelectedTasks by Index value
                                    var groupedTasks = unsortedSelectedTasks
                                        .GroupBy(task => task.Index)
                                        .OrderBy(group => group.Key);

                                    // Process each group
                                    foreach (var group in groupedTasks)
                                    {
                                        var tasksInGroup = group.ToList();

                                        // For each actionName in the specified values
                                        foreach (string actionName in values)
                                        {
                                            // Iterate over tasks in the group and add matching actions to sequencedFabTasks
                                            foreach (FabTask task in tasksInGroup)
                                            {
                                                task.Action.TryGetValue(actionName, out Fab.Core.FabEnvironment.Action action);

                                                if (action != null && actionName == action.Name)
                                                {
                                                    sequencedFabTasks.Add(task);
                                                }
                                            }
                                        }
                                    }
                                }



                            }

                        }

                    }

                }

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
                return Resources.FabFramework_Icon_SequenceFabTasks;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1ecf1f4d-4648-4e70-b1e3-63f8e6e281a0"); }
        }
    }
}
using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabEnvironment;
using Fab.Core.FabUtilities;
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using Fab.Grasshopper.Properties;
using Fab.Core.FabElement;
using Fab.Core.FabTask;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Newtonsoft.Json.Linq;
using System.Linq;
using Eto.Forms;
using System.Xml.Linq;
using Fab.Core.FabCollection;
using Fab.Core.DesignElement;

namespace Fab.Grasshopper.GhComponents.GhcFabCommunication
{

    public class GhcConvertoToDesFabTask : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcConvertoToDesFabTask()
          : base(
              "ConvertoToDesFabTasks",
              "DesFabTasks",
              "Convert FabTasks to DesFabTasks.",
              "Fab",
              "Communication")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabTasks", "FT", "List of all FabTasks for the LCRL FabCassette", GH_ParamAccess.list);
            pManager.AddIntegerParameter("JobIndex", "JI", "JobIndex of the FabTask data", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            //output dictionary of all the tasks
            pManager.AddGenericParameter("DesFabTasks", "T", "DesFabTasks", GH_ParamAccess.list);
            pManager.AddGenericParameter("DesFabActorData", "AC", "DesFabActorData", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<FabTask> fabTasks = new List<FabTask>();
            DA.GetDataList("FabTasks", fabTasks);

            int iJobIndex = -1;
            DA.GetData("JobIndex", ref iJobIndex);

            FabCollection fabCollection = FabCollection.GetFabCollection();

            List<Dictionary<string, object>> listOfDesFabTaskDictionaries = new List<Dictionary<string, object>>();
            Dictionary<string, object> desFabTaskDictionaryStructure = new Dictionary<string, object>
            {
                { "Name", null },
                { "Type", null },
                { "Main_Actor", null },
                { "Description", null },
                { "Message", null },
                { "Job", null },
                { "Level", null },
                { "Index", null },
                { "Elements_Id", null },
                { "Elements_Name", null },
                { "Actors_Data", null }
            };

            List<Dictionary<string, object>> listOfDesFabActorDataDictionaries = new List<Dictionary<string, object>>();
            Dictionary<string, object> desFabActorDataDictionaryStructure = new Dictionary<string, object>
            {
                { "ActorName", null },
                { "ToolName", null },
                { "ToolNumber", null },
                { "ActionID", null },
                { "MainPlanes", null },
                { "SecondaryPlanes", null },
                { "MainExternalAxis", null },
                { "SecondaryExternalAxis", null },
                { "States", null },
                { "Speed", null },
                { "Offset", null }
            };

            for (int i = 0; i < fabTasks.Count; i++)
            {
                Dictionary<string, object> taskDictionary = new Dictionary<string, object>(desFabTaskDictionaryStructure);
                Dictionary<string, object> actorDataDictionary = new Dictionary<string, object>(desFabActorDataDictionaryStructure);

                //Task Dictionary
                taskDictionary["Name"] = fabTasks[i].Name;
                taskDictionary["Type"] = fabTasks[i].Action.FirstOrDefault().Value.Name;
                taskDictionary["Main_Actor"] = fabTasks[i].Actors.FirstOrDefault().Value.Name;
                taskDictionary["Description"] = $"Job No. {iJobIndex} || {taskDictionary["Name"]}.";
                taskDictionary["Job"] = iJobIndex;
                taskDictionary["Level"] = i;
                taskDictionary["Index"] = -1;

                if (fabTasks[i].FabElementsName != null && fabTasks[i].FabElementsName.Count > 0)
                {
                    object test = fabTasks[i].FabElementsName[0];
                    FabElement fabElement = fabCollection.fabElementCollection[fabTasks[i].FabElementsName[0]];
                    DesignElement designElement = fabCollection.designElementCollection[fabElement.DesignElementName];
                    taskDictionary["Elements_Id"] = designElement.ElementName;
                    taskDictionary["Elements_Name"] = designElement.Name;
                }

                //ActorData Dictionary
                actorDataDictionary["ActorName"] = taskDictionary["Main_Actor"];

                if (fabTasks[i].Tools != null && fabTasks[i].Tools.Count > 0)
                {
                    actorDataDictionary["ToolName"] = fabTasks[i].Tools.FirstOrDefault().Value.Name;
                    actorDataDictionary["ToolNumber"] = fabTasks[i].Tools.FirstOrDefault().Value.ToolNum;
                }

                actorDataDictionary["ActionID"] = fabTasks[i].Action.FirstOrDefault().Value.ActionID;

                //ceck if fabTasks[i] is FabTaskFrame
                if (fabTasks[i].GetType() == typeof(FabTaskFrame))
                {
                    FabTaskFrame fabTaskFrame = fabTasks[i] as FabTaskFrame;
                    actorDataDictionary["MainPlanes"] = fabTaskFrame.Main_Frames;
                    actorDataDictionary["SecondaryPlanes"] = fabTaskFrame.Sub_Frames;
                    actorDataDictionary["MainExternalAxis"] = fabTaskFrame.Main_ExtValues;
                    actorDataDictionary["SecondaryExternalAxis"] = fabTaskFrame.Sub_ExtValues;
                    actorDataDictionary["States"] = fabTaskFrame.State;
                    actorDataDictionary["Speed"] = fabTaskFrame.Speed;
                    actorDataDictionary["Offset"] = fabTaskFrame.Offset;
                }

                taskDictionary["Actors_Data"] = actorDataDictionary;


                listOfDesFabTaskDictionaries.Add(taskDictionary);
                listOfDesFabActorDataDictionaries.Add(actorDataDictionary);
            }



            DA.SetDataList("DesFabTasks", listOfDesFabTaskDictionaries);
            DA.SetDataList("DesFabActorData", listOfDesFabActorDataDictionaries);


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
                return Resources.DesFab_TransferIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6d9cf75d-ca62-48be-bfd0-54300d7a568e"); }
        }
    }
}
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Fab.Core.FabTask
{
    public class FabTask
    {


        #region properties
        //Properties
        FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

        public string Name
        {
            get
            {
                if (name == null)
                {
                    throw new InvalidOperationException("Name is null");
                }
                return name;
            }
        }
        public string Id
        {
            get
            {
                if (id == null)
                {
                    id = Guid.NewGuid().ToString();
                }
                return id;
            }

            set { id = value; }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public List<GeometryBase> Geometry
        {
            get { return geometry; }
            set
            {
                geometry = value;
                geometryColors = new List<Color>();
                for (int i = 0; i < geometry.Count; i++)
                {
                    geometryColors.Add(Color.LightGray);
                }
            }
        }
        public List<Transform> TransformGeo
        {
            get { return transformGeo; }
            set
            {
                transformGeo = new List<Transform>();
                for (int i = 0; i < geometry.Count; i++)
                {
                    Transform transform = new Transform();
                    transformGeo.Add(transform);
                }

            }
        }
        public List<string> FabElementsName
        {
            get { return fabElementsName; }
            set { fabElementsName = value; }
        }


        public Dictionary<string, Fab.Core.FabEnvironment.Action> Action
        {
            get { return action; }
            set { action = value; }
        }

        public Dictionary<string, Actor> Actors
        {
            get { return actors; }
            set { actors = value; }
        }

        public Dictionary<string, Tool> Tools
        {
            get { return tools; }
            set { tools = value; }
        }

        public Dictionary<string, StaticEnv> StaticEnvs
        {
            get { return staticEnvs; }
            set { staticEnvs = value; }
        }



        #endregion

        //Field of Variables
        private string name;
        private string id;
        private int index;
        private List<GeometryBase> geometry;
        private List<Color> geometryColors;
        private List<Transform> transformGeo;

        private List<string> fabElementsName;

        private Dictionary<string, Fab.Core.FabEnvironment.Action> action;
        private Dictionary<string, Actor> actors;
        private Dictionary<string, StaticEnv> staticEnvs;
        private Dictionary<string, Tool> tools;



        public FabTask()
        {
            this.id = Guid.NewGuid().ToString();
            this.fabElementsName = new List<string>();

            this.Geometry = new List<GeometryBase>();
            this.action = new Dictionary<string, FabEnvironment.Action> { };
            this.actors = new Dictionary<string, Actor>();
            this.staticEnvs = new Dictionary<string, StaticEnv>();
            this.tools = new Dictionary<string, Tool>();
        }
        public FabTask(string name)
        {
            if (name != null)
            {
                this.name = name;
                fabCollection.AddFabTask(this);
            }
            this.id = Guid.NewGuid().ToString();
            this.fabElementsName = new List<string>();

            this.Geometry = new List<GeometryBase>();
            this.action = new Dictionary<string, FabEnvironment.Action> { };
            this.actors = new Dictionary<string, Actor>();
            this.staticEnvs = new Dictionary<string, StaticEnv>();
            this.tools = new Dictionary<string, Tool>();
        }


        public FabTask ShallowCopy(string newName)
        {
            FabTask copy = (FabTask)MemberwiseClone();
            copy.name = newName;
            fabCollection.AddFabTask(copy);

            return copy;
        }

        public void AssociateElement(FabElement.FabElement fabElement)
        {

            if (fabElementsName == null)
            {
                fabElementsName = new List<string>();
            }

            if (Name == null)
            {
                throw new ArgumentNullException("FabTask name can not be null.");
            }

            if (fabElement.Name == null)
            {
                throw new ArgumentNullException("FabElement name can not be null.");
            }

            if (!fabElementsName.Contains(fabElement.Name))
            { fabElementsName.Add(fabElement.Name); }

            if (!fabElement.FabTasksName.Contains(Name))
            { fabElement.FabTasksName.Add(Name); }

            //check if fabElement has ParentComponent
            if (fabElement.ParentComponentName != null)
            {
                if (!fabElement.GetParentComponent().CompTasksName.Contains(Name))
                { fabElement.GetParentComponent().CompTasksName.Add(Name); }
            }
            //check if FabElement is FabComponent
            if (fabElement is FabElement.FabComponent fabComponent)
            {
                if (!fabComponent.CompTasksName.Contains(Name))
                { fabComponent.CompTasksName.Add(Name); }
            }

        }

        public static List<FabTask> SortTasksFabComponent(FabComponent fabComponent, List<string> sortingOrder)
        {
            var fabCollection = FabCollection.FabCollection.GetFabCollection();

            List<string> fabTaskNames = fabComponent.FabTasksName;

            var sortedTasks = fabTaskNames
                .Where(taskName => fabCollection.fabTaskCollection.ContainsKey(taskName))
                .Select(taskName => fabCollection.fabTaskCollection[taskName])
                .Where(task => sortingOrder.Contains(task.Action.FirstOrDefault().Key))
                .OrderBy(task =>
                {
                    var actionKey = task.Action.FirstOrDefault().Key;
                    return sortingOrder.IndexOf(actionKey) != -1 ? sortingOrder.IndexOf(actionKey) : sortingOrder.Count;
                })
                .ToList();

            return sortedTasks;
        }

        public static List<FabTask> SortTasksFabPlate(FabComponent fabComponent, string plateName, List<string> sortingOrder)
        {
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

            List<FabPlate> fabPlates = fabComponent.GetFabPlates();

            var unsortedTasks = fabPlates
                .Where(plate => plate.Name == plateName)
                .SelectMany(plate => plate.FabTasksName
                    .Where(taskName => fabCollection.fabTaskCollection.ContainsKey(taskName))
                    .Select(taskName => fabCollection.fabTaskCollection[taskName]))
                .ToList();

            var sortedTasks = unsortedTasks
                .Where(task => sortingOrder.Contains(task.Action.FirstOrDefault().Key))
                .OrderBy(task =>
                {
                    var actionKey = task.Action.FirstOrDefault().Key;
                    return sortingOrder.IndexOf(actionKey) != -1 ? sortingOrder.IndexOf(actionKey) : sortingOrder.Count;
                })
                .ToList();

            return sortedTasks;
        }

        // Function to sort FabTasks by beam index and custom sorting order
        public static List<FabTask> SortFabTasksByBeamIndex(FabComponent fabComponent, List<string> sortingOrder)
        {
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

            List<FabBeam> fabBeamsList = new List<FabBeam>();
            for (int i = 0; i < fabComponent.FabBeamsName.Count; i++)
            {
                if (fabCollection.fabBeamCollection.ContainsKey(fabComponent.FabBeamsName[i]))
                {
                    fabBeamsList.Add(fabCollection.fabBeamCollection[fabComponent.FabBeamsName[i]]);
                }
            }

            return fabBeamsList
                .OrderBy(beam => beam.Index)
                .SelectMany(beam => SortFabTasksWithinBeam(beam, sortingOrder))
                .ToList();
        }

        // Function to sort FabTasks within a beam
        public static List<FabTask> SortFabTasksWithinBeam(FabBeam fabBeam, List<string> sortingOrder)
        {
            var fabCollection = FabCollection.FabCollection.GetFabCollection();

            var sortedTasks = fabBeam.FabTasksName
                .Where(taskName => fabCollection.fabTaskCollection.ContainsKey(taskName))
                .Select(taskName => fabCollection.fabTaskCollection[taskName])
                .Where(task => sortingOrder.Contains(task.Action.FirstOrDefault().Key))
                .OrderBy(task => GetTaskSortingKey(task, sortingOrder))
                .ToList();

            return sortedTasks;
        }

        // Function to get the sorting key for a FabTask based on custom sorting order
        public static int GetTaskSortingKey(FabTask fabTask, List<string> sortingOrder)
        {
            var actionKey = fabTask.Action.FirstOrDefault().Key;
            return sortingOrder.IndexOf(actionKey) != -1 ? sortingOrder.IndexOf(actionKey) : sortingOrder.Count;
        }
        public static void AddTakeInitTasks(Actor iFabActor, FabTask fabTask, Core.FabEnvironment.Action takeTool, Core.FabEnvironment.Action initTool, List<FabTask> tasks)
        {
            // Take Tool Task
            FabTask takeToolTask = CreateTask(iFabActor, fabTask.Tools.Values.First().Name, takeTool);
            tasks.Add(takeToolTask);

            // Init Tool Task
            FabTask initToolTask = CreateTask(iFabActor, fabTask.Tools.Values.First().Name, initTool);
            tasks.Add(initToolTask);
        }

        public static void AddCloseStoreTasks(Actor iFabActor, Core.FabEnvironment.Tool currentTool, Core.FabEnvironment.Action closeTool, Core.FabEnvironment.Action storeTool, List<FabTask> tasks)
        {
            // Close Tool Task
            FabTask closeToolTask = CreateTask(iFabActor, currentTool.Name, closeTool);
            tasks.Add(closeToolTask);

            // Store Tool Task
            FabTask storeToolTask = CreateTask(iFabActor, currentTool.Name, storeTool);
            tasks.Add(storeToolTask);
        }

        public static FabTask CreateTask(Actor iFabActor, string toolName, Core.FabEnvironment.Action action)
        {
            FabTask task = new FabTask($"{action.Name}: {toolName}");
            task.Actors.Add(iFabActor.Name, iFabActor);
            task.Action.Add(action.Name, action);
            task.Tools.Add(toolName, iFabActor.Tools[toolName]);
            return task;
        }

        public static List<FabTask> InterlinkToolTasks(Actor fabActor, List<FabTask> fabTasks, List<string> actionNames)
        {
            if (actionNames.Count != 4 ||
                !actionNames.Contains("TakeTool") || !actionNames.Contains("StoreTool") ||
                !actionNames.Contains("InitTool") || !actionNames.Contains("CloseTool"))
            {
                throw new ArgumentException("Action names are missing or invalid.");
            }

            FabEnvironment.Action takeTool = fabActor.Actions[actionNames[0]];
            FabEnvironment.Action storeTool = fabActor.Actions[actionNames[1]];
            FabEnvironment.Action initTool = fabActor.Actions[actionNames[2]];
            FabEnvironment.Action closeTool = fabActor.Actions[actionNames[3]];

            Tool currentTool = new Tool();
            List<FabTask> interlinkedTasks = new List<FabTask>();

            for (int i = 0; i < fabTasks.Count; i++)
            {
                if (i == 0)
                {
                    FabTask.AddTakeInitTasks(fabActor, fabTasks[i], takeTool, initTool, interlinkedTasks);
                }
                else if (fabTasks[i].Tools.Count > 0 && fabTasks[i].Tools.Values.First().Name != currentTool.Name)
                {
                    FabTask.AddCloseStoreTasks(fabActor, currentTool, closeTool, storeTool, interlinkedTasks);
                    FabTask.AddTakeInitTasks(fabActor, fabTasks[i], takeTool, initTool, interlinkedTasks);
                }

                currentTool = fabTasks[i].Tools.Values.First();
                interlinkedTasks.Add(fabTasks[i]);

                if (i == fabTasks.Count - 1)
                {
                    FabTask.AddCloseStoreTasks(fabActor, currentTool, closeTool, storeTool, interlinkedTasks);
                }
            }

            return interlinkedTasks;
        }


        public static List<FabTask> CreateDependencyTasks(Actor iFabActor, List<FabTask> interlinkedTasks, string dependencyTaskKeyword, Core.FabEnvironment.Action dependencyTaskAction, bool addBeforeKeywordTask)
        {
            List<FabTask> dependencyTasks = new List<FabTask>();

            foreach (var task in interlinkedTasks)
            {
                if (task.Action.Keys.First() == dependencyTaskKeyword)
                {
                    if (addBeforeKeywordTask)
                    {
                        FabTask dependencyTask = CreateTask(iFabActor, task.Tools.Values.First().Name, dependencyTaskAction);
                        dependencyTasks.Add(dependencyTask);
                    }
                }

                dependencyTasks.Add(task);

                if (!addBeforeKeywordTask)
                {
                    FabTask dependencyTask = CreateTask(iFabActor, task.Tools.Values.First().Name, dependencyTaskAction);
                    dependencyTasks.Add(dependencyTask);
                }
            }

            return dependencyTasks;
        }

        public static List<FabTask> InterlinkTravelTasks(List<FabTask> fabTasks, Dictionary<(string, string), (string, Dictionary<string, double>, Plane)> travelDictionary, List<FabTaskFrame> travelAxisTasks)
        {
            List<FabTask> newTasks = new List<FabTask>();

            foreach (var fabTask in fabTasks)
            {
                if (fabTask.HasStaticEnvs() && fabTask.HasTools())
                {
                    string taskStaticEnvName = fabTask.GetStaticEnvName();
                    string taskToolName = fabTask.GetToolName();

                    if (travelDictionary.TryGetValue((taskStaticEnvName, taskToolName), out var travelTaskData))
                    {
                        string travelTaskName = travelTaskData.Item1;
                        FabTaskFrame matchingTravelTask = travelAxisTasks.FirstOrDefault(task => task.Name == travelTaskName);

                        var fabCollection = FabCollection.FabCollection.GetFabCollection();

                        //Start Building FabTaskFrame Name
                        StringBuilder newTravelName = new StringBuilder();
                        newTravelName.Append(matchingTravelTask.Name);

                        bool containsFabElement = false;
                        if (fabCollection.fabElementCollection.ContainsKey(fabTask.FabElementsName[0]))
                        {
                            containsFabElement = true;
                            newTravelName.Append("_" + fabCollection.fabElementCollection[fabTask.FabElementsName[0]].Name);
                        }
                        else
                        {
                            Console.WriteLine($"FabElement with name '{fabTask.FabElementsName[0]}' not found.");
                        }


                        if (fabTask is FabTaskFrame currentFabTaskFrame)
                        {
                            if (currentFabTaskFrame.Main_ExtValues.TryGetValue("E2", out var e2Value))
                            {
                                var firstE2Value = e2Value.FirstOrDefault();
                                newTravelName.Append("_E2: " + Math.Round(firstE2Value).ToString());
                            }
                            else
                            {
                                Console.WriteLine($"FabTaskFrame '{fabTask.Name}' does not have a 'Main_ExtValues' entry for key 'E2'.");
                            }
                        }

                        // NEW TASK
                        FabTaskFrame newTravelTask = matchingTravelTask.ShallowCopy(newTravelName.ToString()) as FabTaskFrame;
                        if (containsFabElement == true)
                        {
                            newTravelTask.AssociateElement(fabCollection.fabElementCollection[fabTask.FabElementsName[0]]);
                        }


                        //Change E2 Value
                        if (fabTask is FabTaskFrame currentFabTaskFrame2)
                        {
                            if (currentFabTaskFrame2.Main_ExtValues.TryGetValue("E2", out var e2Value2))
                            {
                                var firstE2Value = e2Value2.FirstOrDefault();


                                var updatedExtValues = new Dictionary<string, List<double>>(newTravelTask.Main_ExtValues);
                                updatedExtValues["E2"] = new List<double> { firstE2Value };
                                newTravelTask.Main_ExtValues = updatedExtValues;
                            }
                            else
                            {
                                Console.WriteLine($"FabTaskFrame '{fabTask.Name}' does not have a 'Main_ExtValues' entry for key 'E2'.");
                            }
                        }

                        newTasks.Add(newTravelTask);
                    }
                    else
                    {
                        Console.WriteLine($"Combination not found in the dictionary for StaticEnv '{taskStaticEnvName}' and Tool '{taskToolName}'.");
                    }
                }
                else
                {
                    Console.WriteLine($"FabTask '{fabTask.Name}' does not have 'StaticEnvs' and 'Tools' properties.");
                }

                newTasks.Add(fabTask);
            }

            return newTasks;
        }

        public bool HasStaticEnvs()
        {
            return StaticEnvs != null && StaticEnvs.Count > 0;
        }

        public bool HasTools()
        {
            return Tools != null && Tools.Count > 0;
        }

        public string GetStaticEnvName()
        {
            return HasStaticEnvs() ? StaticEnvs.Values.FirstOrDefault()?.Name : null;
        }

        public string GetToolName()
        {
            return HasTools() ? Tools.Values.FirstOrDefault()?.Name : null;
        }

        public static bool SetToolChangeTask(
        FabTask toolChangeTask,
        Fab.Core.FabEnvironment.Action toolChange,
        FabTask priorTask, FabTask currentTask)
        {
            //add nextTask and get this tool Number!!!!!!!
            var priorStaticEnvVar = priorTask.StaticEnvs.FirstOrDefault();
            StaticEnv priorStaticEnv = priorStaticEnvVar.Value;

            //StaticEnvs
            toolChangeTask.StaticEnvs[priorStaticEnv.Name] = priorStaticEnv;

            //Action
            toolChangeTask.Action[toolChange.Name] = toolChange;

            //Actor
            var priorActorVar = priorTask.Actors.FirstOrDefault();
            Actor priorActor = priorActorVar.Value;
            toolChangeTask.Actors[priorActor.Name] = priorActor;

            //Tool
            var currentToolVar = currentTask.Tools.FirstOrDefault();
            Tool currentTool = currentToolVar.Value;
            toolChangeTask.Tools[currentTool.Name] = currentTool;

            return true;
        }
    }

}


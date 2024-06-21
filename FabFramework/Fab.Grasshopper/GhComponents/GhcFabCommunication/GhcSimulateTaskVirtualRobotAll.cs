using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Grasshopper.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fab.Grasshopper.GhComponents.GhcFabCommunication
{

    public class GhcSimulateTaskVirtualRobotAll : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcSimulateTaskVirtualRobotAll()
          : base(
              "Simulate FabTask Virtual Robot",
              "SimFabTaskAll",
              "Generate All FabTask Data VirtualRobot GhostSolver.",
              "Fab",
              "Communication")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabTasks", "FabTasks", "List of all FabTasks for the LCRL FabCassette", GH_ParamAccess.list);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("TaskIndex", "I", "FabTask Index", GH_ParamAccess.tree);
            pManager.AddNumberParameter("TaskSubIndex", "SI", "FabTask SubIndex", GH_ParamAccess.tree);
            pManager.AddTextParameter("Name", "N", "Name of the FabTask data", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("ToolNumber", "TN", "ToolNumber of the FabTask data", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Geometry", "G", "Geometry of the FabTask data", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("TCP", "TCP", "TCP of the FabTask data", GH_ParamAccess.tree);
            pManager.AddNumberParameter("E1Value", "E1", "E1Value of the FabTask data", GH_ParamAccess.tree);
            pManager.AddNumberParameter("E2Value", "E2", "E2Value of the FabTask data", GH_ParamAccess.tree);
            pManager.AddNumberParameter("E3Value", "E3", "E3Value of the FabTask data", GH_ParamAccess.tree);

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
            List<FabTask> fabTasks = new List<FabTask>();
            DA.GetDataList("FabTasks", fabTasks);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            //-----------
            //EDIT
            //-----------

            // Define Outputs from counter
            double defaultIntValue = 360 * 1000000 * -1;


            DataTree<int> taskIndexTree = new DataTree<int>();
            DataTree<int> taskSubIndexTree = new DataTree<int>();
            DataTree<string> fabTaskNameTree = new DataTree<string>();
            DataTree<int> fabTaskToolNumTree = new DataTree<int>();
            DataTree<GeometryBase> fabTaskGeometryTree = new DataTree<GeometryBase>();
            DataTree<Plane> fabTaskPlaneTree = new DataTree<Plane>();
            DataTree<double> fabE1ValueTree = new DataTree<double>();
            DataTree<double> fabE2ValueTree = new DataTree<double>();
            DataTree<double> fabE3ValueTree = new DataTree<double>();

            //Set correct iterate values
            //iterate through fabtask
            for (int taskIndex = 0; taskIndex < fabTasks.Count; taskIndex++)
            {


                if (fabTasks[taskIndex] is FabTaskFrame fabTaskFrame)
                {

                    GH_Path path = new GH_Path(taskIndex);

                    string fabTaskName = null;
                    int fabTaskToolNum = -1;
                    GeometryBase fabTaskGeometry = null;
                    Plane fabTaskPlane = Plane.Unset;
                    double fabE1Value = defaultIntValue;
                    double fabE2Value = defaultIntValue;
                    double fabE3Value = defaultIntValue;


                    fabTaskName = fabTasks[taskIndex].Name;
                    fabTaskToolNum = fabTasks[taskIndex].Tools.Values.First().ToolNum;

                    List<Plane> mainFrames = fabTaskFrame.Main_Frames?.ToList();
                    List<Plane> subFrames = fabTaskFrame.Sub_Frames?.ToList();
                    List<double> e1MainValues = fabTaskFrame.Main_ExtValues != null && fabTaskFrame.Main_ExtValues.TryGetValue("E1", out List<double> tempE1MainValues) ? tempE1MainValues : null;
                    List<double> e1SubValues = fabTaskFrame.Sub_ExtValues != null && fabTaskFrame.Sub_ExtValues.TryGetValue("E1", out List<double> tempE1SubValues) ? tempE1SubValues : null;
                    List<double> e2MainValues = fabTaskFrame.Main_ExtValues != null && fabTaskFrame.Main_ExtValues.TryGetValue("E2", out List<double> tempE2MainValues) ? tempE2MainValues : null;
                    List<double> e2SubValues = fabTaskFrame.Sub_ExtValues != null && fabTaskFrame.Sub_ExtValues.TryGetValue("E2", out List<double> tempE2SubValues) ? tempE2SubValues : null;
                    List<double> e3MainValues = fabTaskFrame.Main_ExtValues != null && fabTaskFrame.Main_ExtValues.TryGetValue("E3", out List<double> tempE3MainValues) ? tempE3MainValues : null;
                    List<double> e3SubValues = fabTaskFrame.Sub_ExtValues != null && fabTaskFrame.Sub_ExtValues.TryGetValue("E3", out List<double> tempE3SubValues) ? tempE3SubValues : null;

                    //Checks for exisiting values and check if list length match
                    int mainFramesCount = mainFrames?.Count ?? 0;
                    int subFramesCount = subFrames?.Count ?? 0;
                    int e1MainValuesCount = e1MainValues?.Count ?? 0;
                    int e1SubValuesCount = e1SubValues?.Count ?? 0;
                    int e2MainValuesCount = e2MainValues?.Count ?? 0;
                    int e2SubValuesCount = e2SubValues?.Count ?? 0;
                    int e3MainValuesCount = e3MainValues?.Count ?? 0;
                    int e3SubValuesCount = e3SubValues?.Count ?? 0;

                    if (e1MainValuesCount != mainFramesCount && e1MainValues != null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "E1Values count is not equal to Main_Frames count.");
                    }
                    if (e1SubValuesCount != subFramesCount && e1SubValues != null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "E1Values count is not equal to Sub_Frames count.");
                    }
                    if (e2MainValuesCount != mainFramesCount && e2MainValues != null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "E2Values count is not equal to Main_Frames count.");
                    }
                    if (e2SubValuesCount != subFramesCount && e2SubValues != null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "E2Values count is not equal to Sub_Frames count.");
                    }
                    if (e3MainValuesCount != mainFramesCount && e3MainValues != null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "E3Values count is not equal to Main_Frames count.");
                    }
                    if (e3SubValuesCount != subFramesCount && e3SubValues != null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "E3Values count is not equal to Sub_Frames count.");
                    }

                    // Determine the total number of frames to consider
                    int totalFramesCount = mainFramesCount + subFramesCount;

                    List<int> taskIndexList = new List<int>();
                    List<int> taskSubIndexList = new List<int>();
                    List<string> fabTaskNameList = new List<string>();
                    List<int> fabTaskToolNumList = new List<int>();
                    List<GeometryBase> fabTaskGeometryList = new List<GeometryBase>();
                    List<Plane> fabTaskPlaneList = new List<Plane>();
                    List<double> fabE1ValueList = new List<double>();
                    List<double> fabE2ValueList = new List<double>();
                    List<double> fabE3ValueList = new List<double>();

                    for (int taskSubIndex = 0; taskSubIndex < totalFramesCount; taskSubIndex++)
                    {

                        //Add Geometry if Geometry list exists
                        if (fabTasks[taskIndex].Geometry != null)
                        {
                            if (fabTasks[taskIndex].Geometry.Count == 0)
                            {
                                fabTaskGeometry = null;
                            }
                            else if (fabTasks[taskIndex].Geometry.Count == 1)
                            {
                                fabTaskGeometry = fabTasks[taskIndex].Geometry[0];
                            }

                            else if (taskSubIndex < fabTasks[taskIndex].Geometry.Count && fabTasks[taskIndex].Geometry.Count > 1)
                            {
                                fabTaskGeometry = fabTasks[taskIndex].Geometry[taskSubIndex];
                            }

                            else
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "TaskSubIndex is out of range for Geometry.");
                            }
                        }

                        // Determine whether to use Main_Frames (M) or Sub_Frames (S)
                        if (mainFramesCount == subFramesCount)
                        {
                            // Alternate between M and S when they have the same count
                            if (taskSubIndex % 2 == 0)
                            {
                                fabTaskPlane = mainFrames[taskSubIndex / 2];

                                //External Values
                                if (e1MainValues != null && e1MainValues.Count != 0)
                                {
                                    fabE1Value = e1MainValues[taskSubIndex / 2];
                                }
                                if (e2MainValues != null && e2MainValues.Count != 0)
                                {
                                    fabE2Value = e2MainValues[taskSubIndex / 2];
                                }
                                if (e3MainValues != null && e3MainValues.Count != 0)
                                {
                                    fabE3Value = e3MainValues[taskSubIndex / 2];
                                }


                            }
                            else
                            {
                                fabTaskPlane = subFrames[taskSubIndex / 2];

                                //External Values
                                if (e1SubValues != null && e1SubValues.Count != 0)
                                {
                                    fabE1Value = e1SubValues[taskSubIndex / 2];
                                }
                                if (e2SubValues != null && e2SubValues.Count != 0)
                                {
                                    fabE2Value = e2SubValues[taskSubIndex / 2];
                                }
                                if (e3SubValues != null && e3SubValues.Count != 0)
                                {
                                    fabE3Value = e3SubValues[taskSubIndex / 2];
                                }
                            }
                        }
                        else
                        {
                            // Use all Main_Frames first, then Sub_Frames
                            if (taskSubIndex < mainFramesCount)
                            {
                                fabTaskPlane = mainFrames[taskSubIndex];

                                //External Values
                                if (e1MainValues != null && e1MainValues.Count != 0)
                                {
                                    fabE1Value = e1MainValues[taskSubIndex];
                                }
                                if (e2MainValues != null && e2MainValues.Count != 0)
                                {
                                    fabE2Value = e2MainValues[taskSubIndex];
                                }
                                if (e3MainValues != null && e3MainValues.Count != 0)
                                {
                                    fabE3Value = e3MainValues[taskSubIndex];
                                }
                            }
                            else
                            {
                                fabTaskPlane = subFrames[taskSubIndex];

                                //External Values
                                if (e1SubValues != null && e1SubValues.Count != 0)
                                {
                                    fabE1Value = e1SubValues[taskSubIndex];
                                }
                                if (e2SubValues != null && e2SubValues.Count != 0)
                                {
                                    fabE2Value = e2SubValues[taskSubIndex];
                                }
                                if (e3SubValues != null && e3SubValues.Count != 0)
                                {
                                    fabE3Value = e3SubValues[taskSubIndex];
                                }

                            }

                        }

                        taskIndexList.Add(taskIndex);
                        taskSubIndexList.Add(taskSubIndex);
                        fabTaskNameList.Add(fabTaskName);
                        fabTaskToolNumList.Add(fabTaskToolNum);
                        fabTaskGeometryList.Add(fabTaskGeometry);
                        fabTaskPlaneList.Add(fabTaskPlane);
                        fabE1ValueList.Add(fabE1Value);
                        fabE2ValueList.Add(fabE2Value);
                        fabE3ValueList.Add(fabE3Value);

                    }

                    //Add to DataTree
                    taskIndexTree.AddRange(taskIndexList, path);
                    taskSubIndexTree.AddRange(taskSubIndexList, path);
                    fabTaskNameTree.AddRange(fabTaskNameList, path);
                    fabTaskToolNumTree.AddRange(fabTaskToolNumList, path);
                    fabTaskGeometryTree.AddRange(fabTaskGeometryList, path);
                    fabTaskPlaneTree.AddRange(fabTaskPlaneList, path);
                    fabE1ValueTree.AddRange(fabE1ValueList, path);
                    fabE2ValueTree.AddRange(fabE2ValueList, path);
                    fabE3ValueTree.AddRange(fabE3ValueList, path);

                }



            }




            DA.SetDataTree(0, taskIndexTree);
            DA.SetDataTree(1, taskSubIndexTree);
            DA.SetDataTree(2, fabTaskNameTree);
            DA.SetDataTree(3, fabTaskToolNumTree);
            DA.SetDataTree(4, fabTaskGeometryTree);
            DA.SetDataTree(5, fabTaskPlaneTree);
            DA.SetDataTree(6, fabE1ValueTree);
            DA.SetDataTree(7, fabE2ValueTree);
            DA.SetDataTree(8, fabE3ValueTree);

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
                return Resources.FabFramework_VirutalRobotAll;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3c4b4236-57ae-4f8f-98f1-09c363f05559"); }
        }
    }
}
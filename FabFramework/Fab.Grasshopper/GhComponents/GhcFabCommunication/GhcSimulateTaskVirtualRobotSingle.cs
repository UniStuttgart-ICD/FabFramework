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

namespace Fab.Grasshopper.GhComponents.GhcFabCommunication
{

    public class GhcSimulateTaskVirtualRobotSingle : GH_Component
    {
        bool initialReset = false;
        int taskIndex = -1;
        int taskSubIndex = -1;
        int counterTaskIndex = -1;
        int counterTaskSubIndex = -1;

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcSimulateTaskVirtualRobotSingle()
          : base(
              "Simulate FabTask Virtual Robot",
              "SimFabTaskSingle",
              "Generate Simulation Data from FabTask for VirtualRobot.",
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
            pManager.AddGenericParameter("ActorDefaultPlane", "ADP", "Actor Default Plane", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddIntegerParameter("TaskIndex", "I", "FabTask Index", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("TaskSubIndex", "SI", "FabTask SubIndex", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Reset", "R", "Reset", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("TaskIndex", "I", "FabTask Index", GH_ParamAccess.item);
            pManager.AddNumberParameter("TaskSubIndex", "SI", "FabTask SubIndex", GH_ParamAccess.item);
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

            Plane iFabActorDefaultPlane = Plane.Unset;
            DA.GetData("ActorDefaultPlane", ref iFabActorDefaultPlane);

            int iTaskIndex = 0;
            DA.GetData("TaskIndex", ref iTaskIndex);

            int iTaskSubIndex = 0;
            DA.GetData("TaskSubIndex", ref iTaskSubIndex);

            bool iReset = true;
            DA.GetData("Reset", ref iReset);


            //-----------
            //EDIT
            //-----------

            // Counter logic

            if (initialReset == false)
            {
                iReset = true;
            }

            if (iReset)
            {
                initialReset = true;
                taskIndex = iTaskIndex;
                taskSubIndex = iTaskSubIndex;
                counterTaskIndex = iTaskIndex;
                counterTaskSubIndex = iTaskSubIndex;
            }
            else
            {
                taskIndex = counterTaskIndex;
                taskSubIndex = counterTaskSubIndex;

                if (taskIndex < fabTasks.Count - 1)
                {
                    if (fabTasks[taskIndex] is FabTaskFrame checkFabTaskFrame)
                    {
                        int mainFramesCheckCount = checkFabTaskFrame.Main_Frames?.Count ?? 0;
                        int subFramesCheckCount = checkFabTaskFrame.Sub_Frames?.Count ?? 0;

                        // Determine the total number of frames to consider
                        int maxFramesCount = mainFramesCheckCount + subFramesCheckCount;

                        if (taskSubIndex < maxFramesCount - 1)
                        {
                            counterTaskSubIndex += 1;
                        }
                        else
                        {
                            counterTaskIndex += 1;
                            counterTaskSubIndex = 0;
                        }
                    }
                    else
                    {
                        counterTaskIndex += 1;
                        counterTaskSubIndex = 0;
                    }
                }
                else
                {
                    counterTaskIndex = 0;
                    counterTaskSubIndex = 0;
                }
            }

            // Define Outputs from counter
            double defaultIntValue = 360 * 1000000 * -1;

            string fabTaskName = null;
            int fabTaskToolNum = -1;
            GeometryBase fabTaskGeometry = null;
            Plane fabTaskPlane = Plane.Unset;
            double fabE1Value = defaultIntValue;
            double fabE2Value = defaultIntValue;
            double fabE3Value = defaultIntValue;

            // Ensure taskIndex is within the valid range
            if (taskIndex >= 0 && taskIndex < fabTasks.Count)
            {
                fabTaskName = fabTasks[taskIndex].Name;
                fabTaskToolNum = fabTasks[taskIndex].Tools.Values.First().ToolNum;

                if (fabTasks[taskIndex].Geometry != null)
                {
                    if (fabTasks[taskIndex].Geometry.Count == 1)
                    { fabTaskGeometry = fabTasks[taskIndex].Geometry[0]; }
                }

                if (fabTasks[taskIndex] is FabTaskFrame fabTaskFrame)
                {

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

                    // Ensure taskSubIndex is within the valid range for Main_Frames and Sub_Frames
                    if (taskSubIndex >= 0 && taskSubIndex < totalFramesCount)
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
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "TaskSubIndex is out of range for Main_Frames and Sub_Frames.");
                    }
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "TaskIndex is out of range.");
            }


            //Default display --> e.g. for take/store tool
            if (iFabActorDefaultPlane != Plane.Unset)
            {
                if (fabTaskPlane == null || fabTaskPlane == Plane.Unset)
                {
                    fabTaskPlane = iFabActorDefaultPlane;
                    fabE1Value = FabUtilities.GetLinAxisRadiusBased(iFabActorDefaultPlane, iFabActor.LinearAxis);
                }
            }


            if (fabE2Value == defaultIntValue)
            { }



            DA.SetData("TaskIndex", taskIndex);
            DA.SetData("TaskSubIndex", taskSubIndex);
            DA.SetData("Name", fabTaskName);
            DA.SetData("ToolNumber", fabTaskToolNum);
            DA.SetData("Geometry", fabTaskGeometry);
            DA.SetData("TCP", fabTaskPlane);
            DA.SetData("E1Value", fabE1Value);
            DA.SetData("E2Value", fabE2Value);
            DA.SetData("E3Value", fabE3Value);

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
                return Resources.FabFramework_VirutalRobotSingle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("72fabdfc-ef81-45de-b865-4c87f1b400b2"); }
        }
    }
}
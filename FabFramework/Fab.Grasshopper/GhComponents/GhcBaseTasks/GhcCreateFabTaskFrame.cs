using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Fab.Core.FabEnvironment;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Eto.Forms;
using Fab.Core.FabTask;
using Fab.Core.FabCollection;
using System.Linq;
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using System.Reflection;
using Grasshopper.Kernel.Types;
using Fab.Grasshopper.Properties;


namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcCreateFabTaskFrame : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcCreateFabTaskFrame()
          : base("Create FabTaskFrame",
                "FTF",
                "Create a FabTaskFrame.",
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
            pManager.AddGenericParameter("FabTask", "FT", "FabTask Data", GH_ParamAccess.item);
            pManager.AddPlaneParameter("MainFrames", "MF", "MainFrames", GH_ParamAccess.list);
            pManager.AddPlaneParameter("SubFrames", "SF", "SubFrames", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("MainExtValues", "MEV", "MainExtValues, like Linear Axis Values, TurnTable, ...", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddGenericParameter("SubExtValues", "SEV", "SubExtValues, like Linear Axis Values, TurnTable, ...\"", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddIntegerParameter("State", "St", "List of int, linked as state for each MainFrame. ", GH_ParamAccess.list);
            pManager[5].Optional = true;
            pManager.AddNumberParameter("Speed", "Sp", "List of doubles, linked as state for each MainFrame. ", GH_ParamAccess.list);
            pManager[6].Optional = true;
            pManager.AddNumberParameter("Offset", "Of", "List of three doubles, like (0,0,-200).", GH_ParamAccess.list);
            pManager[7].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabTaskFrame", "FTF", "FabTaskFrame Data", GH_ParamAccess.item);
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
            FabTask iFabTask = new FabTask();
            DA.GetData("FabTask", ref iFabTask);

            List<Plane> iMainFrames = new List<Plane>();
            DA.GetDataList("MainFrames", iMainFrames);

            List<Plane> iSubFrames = new List<Plane>();
            DA.GetDataList("SubFrames", iSubFrames);

            Dictionary<string, List<object>> iMainExtValues = null;
            DA.GetData("MainExtValues", ref iMainExtValues);

            Dictionary<string, List<object>> iSubExtValues = null;
            DA.GetData("SubExtValues", ref iSubExtValues);

            List<int> iState = new List<int>();
            DA.GetDataList("State", iState);

            List<double> iOffset = new List<double>();
            DA.GetDataList("Offset", iOffset);

            List<double> iSpeed = new List<double>();
            DA.GetDataList("Speed", iSpeed);

            //-----------
            //EDIT
            //-----------

            string name = "Frame_" + iFabTask.Name;
            FabTaskFrame fabTaskFrame = new FabTaskFrame(name);

            //Copy everything
            fabTaskFrame.Action = iFabTask.Action;
            fabTaskFrame.Actors = iFabTask.Actors;
            fabTaskFrame.Tools = iFabTask.Tools;
            fabTaskFrame.StaticEnvs = iFabTask.StaticEnvs;
            foreach (string fabElementName in iFabTask.FabElementsName)
            { 
                fabCollection.fabElementCollection.TryGetValue(fabElementName, out FabElement fabElement);
                fabTaskFrame.AssociateElement(fabElement);
            }
            fabTaskFrame.Geometry = iFabTask.Geometry;
        

            if (iMainFrames.Count > 0)
            {
                fabTaskFrame.AddMainFrames(iMainFrames);
            }

            if (iSubFrames.Count > 0)
            {
                fabTaskFrame.AddSubFrames(iSubFrames);
            }

            // Add main external values if any
            if (iMainExtValues != null && iMainExtValues.Count > 0)
            {
                foreach (var kvp in iMainExtValues)
                {
                    string key = kvp.Key;
                    var dummyList = kvp.Value.ToList();
                    List<double> values = new List<double>();
                    foreach (var value in dummyList)
                    {
                        if (value is GH_String)
                        {
                            GH_String dummy = value as GH_String;
                            values.Add(Convert.ToDouble(dummy.Value));

                        }
                        else if (value is GH_Number)
                        {
                            GH_Number dummy = value as GH_Number;
                            values.Add(dummy.Value);
                        }
                        else if (value is GH_Integer)
                        {
                            GH_Integer dummy = value as GH_Integer;
                            values.Add(dummy.Value);
                        }
                        else
                        {
                            throw new Exception("Type not supported");
                        }
                    }
                    fabTaskFrame.AddMainExternalValues(key, values);
                }
            }

                // Add sub external values if any
                if (iSubExtValues != null && iSubExtValues.Count > 0)
                {
                foreach (var kvp in iSubExtValues)
                {
                    string key = kvp.Key;
                    var dummyList = kvp.Value.ToList();
                    List<double> values = new List<double>();
                    foreach (var value in dummyList)
                    {
                        if (value is GH_String)
                        {
                            GH_String dummy = value as GH_String;
                            values.Add(Convert.ToDouble(dummy.Value));

                        }
                        else if (value is GH_Number)
                        {
                            GH_Number dummy = value as GH_Number;
                            values.Add(dummy.Value);
                        }
                        else if (value is GH_Integer)
                        {
                            GH_Integer dummy = value as GH_Integer;
                            values.Add(dummy.Value);
                        }
                        else
                        {
                            throw new Exception("Type not supported");
                        }
                    }
                    fabTaskFrame.AddSubExternalValues(key, values);
                }
            }

                if (iState.Count > 0)
                {
                    fabTaskFrame.State = iState;
                }

                if (iSpeed.Count > 0)
                {
                    fabTaskFrame.Speed = iSpeed;
                }

                if (iOffset.Count > 0)
                {
                    fabTaskFrame.Offset = iOffset;
                }




                //-----------
                //OUTPUTS
                //-----------
                DA.SetData("FabTaskFrame", fabTaskFrame);
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
                return Resources.FabFramework_Icon_CreateFabTaskFrame;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bbb24542-6231-41b7-8cfa-9b93392e2e9c"); }
        }
    }
}
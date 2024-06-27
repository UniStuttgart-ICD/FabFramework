using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace Fab.Grasshopper.GhComponents.GhcDigitalFUTURES
{
    public class GhcDF_DipBeamWater : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcDF_DipBeamWater()
            : base("DipBeamWater",
                  "DipBeamWater",
                  "Dip the Beam in the water if specified action is contained.",
                  "Fab",
                  "DigitalFUTURES")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddTextParameter("FabElementName", "FEN", "Name of the FabElement", GH_ParamAccess.list);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("Endeffector", "EE", "Specific endeffector by the actor.", GH_ParamAccess.item);
            pManager.AddGenericParameter("DipAction", "Dip", "Dip action for the taskGeneration.", GH_ParamAccess.item);
            pManager.AddGenericParameter("DependentAction", "DipReq", "Required action for the dip taskGeneration to search for in order to trigger.", GH_ParamAccess.item);
            pManager.AddGenericParameter("StaticEnv", "SE", "Static Environment for the taskGeneration.", GH_ParamAccess.item);
            pManager.AddNumberParameter("ZDipDepth", "ZDip", "Z-Depth of the dip action from StaticEnv Plane.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("OffsetDip", "Off", "Offset for the dip task.", GH_ParamAccess.list, new List<double> { 0, 0, 200 });
            pManager.AddNumberParameter("Rotation", "Rot", "Rotate the planes by the given angle (degree).", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("DipDuration", "Dur", "Duration of the dip action.", GH_ParamAccess.item, 10.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("DipFabTask", "Dip", "Fabrication Task for dipping the TCP in water.", GH_ParamAccess.list);
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
            DA.GetData("FabComponent", ref iFabComponent);

            FabComponent fabComponent = iFabComponent.ShallowCopy() as FabComponent;

            List<string> iFabElementName = new List<string>();
            DA.GetDataList("FabElementName", iFabElementName);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            Endeffector iEndeffector = new Endeffector();
            DA.GetData("Endeffector", ref iEndeffector);

            Core.FabEnvironment.Action iDipAction = new Core.FabEnvironment.Action();
            DA.GetData("DipAction", ref iDipAction);

            Core.FabEnvironment.Action iDependentAction = new Core.FabEnvironment.Action();
            DA.GetData("DependentAction", ref iDependentAction);

            StaticEnv iStaticEnv = new StaticEnv();
            DA.GetData("StaticEnv", ref iStaticEnv);

            double iZDipDepth = double.NaN;
            DA.GetData("ZDipDepth", ref iZDipDepth);

            List<double> iOffsetDip = new List<double>();
            DA.GetDataList("OffsetDip", iOffsetDip);

            double iRotation = double.NaN;
            DA.GetData("Rotation", ref iRotation);

            double iDipDuration = double.NaN;
            DA.GetData("DipDuration", ref iDipDuration);


            //-----------
            //EDIT
            //-----------



            List<FabElement> matchingFabElements = FabUtilitiesElement.GetMatchingFabElement(fabComponent, iFabElementName);

            List<FabTaskFrame> fabTask_DipList = new List<FabTaskFrame>();

            //sort matchingFabElements by matchingFabElements.Index
            matchingFabElements.Sort((x1, y1) => x1.Index.CompareTo(y1.Index));


            for (int i = 0; i < matchingFabElements.Count; i++)
            {
                bool alreadyDipped = false;

                // The key exists, retrieve the value
                FabElement fabElement = matchingFabElements[i] as FabElement;

                int dipCounter = 0;
                //check associated fabElement tasks, get name as string
                for (int j = 0; j < fabElement.FabTasksName.Count; j++)
                {
                    string fabTaskName = fabElement.FabTasksName[j];

                    //Get FabTask from 
                    fabCollection.fabTaskCollection.TryGetValue(fabTaskName, out FabTask fabTask);
                    fabTask.Action.TryGetValue(iDependentAction.Name, out Core.FabEnvironment.Action dependentActionFound);

                    //Check if action is not null
                    if (dependentActionFound != null && iDependentAction.Name == dependentActionFound.Name)
                    {
                        if (alreadyDipped == false)
                        {                    
                            //create diptask
                            FabTaskFrame fabTask_Dip = new FabTaskFrame(iDipAction.Name + "_" + dipCounter.ToString("D3") + "_" + fabElement.Name);

                            //create diptask
                            FabTaskFrame.SetFabElementDipTask(fabTask_Dip, fabElement, iDipAction, iEndeffector, iFabActor, iStaticEnv, iZDipDepth, iDipDuration, iOffsetDip);


                            //Rotate the planes by the given angle
                            foreach (Plane frame in fabTask_Dip.Main_Frames)
                            {
                                frame.Rotate(FabUtilities.DegreeToRadian(iRotation), frame.ZAxis, frame.Origin);
                            }

                            fabTask_DipList.Add(fabTask_Dip);

                            alreadyDipped = true;
                        }
                    }

                }




            }

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabComponent", fabComponent);
            DA.SetDataList("DipFabTask", fabTask_DipList);

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
                return Resources.FabFramework_Icon_DipWaterBeam;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8a7ff1f5-e6d5-4cd2-b15c-3ad6960b0f9e"); }
        }
    }
}
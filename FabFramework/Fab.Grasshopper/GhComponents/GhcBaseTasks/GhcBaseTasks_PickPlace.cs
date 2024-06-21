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
using Fab.Grasshopper.Properties;


namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcBaseTasks_PickPlace : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcBaseTasks_PickPlace()
          : base("Pick & Place", 
                "PnP",               
                "Get pick and place task for FabElement. The PnP order will be decided on the Index value of each FabElement.",
                "Fab",
                "BaseTasks")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddTextParameter("FabElementName", "FEN", "Name of the FabElement", GH_ParamAccess.list);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("Endeffector", "EE", "Specific endeffector by the actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("PickAction", "Pick", "Pick action for the taskGeneration", GH_ParamAccess.item);
            pManager.AddGenericParameter("PlaceAction", "Place", "Place action for the taskGeneration", GH_ParamAccess.item);
            pManager.AddNumberParameter("OffsetPick", "Off1", "Offset for the pick task", GH_ParamAccess.list, new List<double> { 0, 0, 200 });
            pManager.AddNumberParameter("OffsetPlace", "Off2", "Offset for the place task", GH_ParamAccess.list, new List<double> { 0, 0, 200 });
            pManager.AddNumberParameter("Rotation", "Rot", "Rotate the planes by the given angle (degree).", GH_ParamAccess.item, 0.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("PickPlaceTask", "PnP", "Fabrication Task for pick and place the FabElement.", GH_ParamAccess.list);
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

            List<String> iFabElementName = new List<String>();
            DA.GetDataList("FabElementName", iFabElementName);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            Endeffector iEndeffector = new Endeffector();
            DA.GetData("Endeffector", ref iEndeffector);

            Fab.Core.FabEnvironment.Action iPickAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("PickAction", ref iPickAction);

            Fab.Core.FabEnvironment.Action iPlaceAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("PlaceAction", ref iPlaceAction);

            List<double> iOffsetPick = new List<double>();
            DA.GetDataList("OffsetPick", iOffsetPick);

            List<double> iOffsetPlace = new List<double>();
            DA.GetDataList("OffsetPlace", iOffsetPlace);

            double iRotation = 0.0;
            DA.GetData("Rotation", ref iRotation);

            //-----------
            //EDIT
            //-----------

            List<FabElement> matchingFabElements = FabUtilitiesElement.GetMatchingFabElement(fabComponent, iFabElementName);

            List<FabTaskFrame> fabTask_PickPlaceTaskList = new List<FabTaskFrame>();

            //sort matchingFabElements by matchingFabElements.Index
            matchingFabElements.Sort((x1, y1) => x1.Index.CompareTo(y1.Index));


            for (int i = 0; i < matchingFabElements.Count; i++)
            {

                // The key exists, retrieve the value
                FabElement fabElement = matchingFabElements[i] as FabElement;


                //Pick & Place
                FabTaskFrame fabTask_Pick = new FabTaskFrame(iPickAction.Name + "_" + fabElement.Name);
                FabTaskFrame fabTask_Place = new FabTaskFrame(iPlaceAction.Name + "_" + fabElement.Name);

                FabTaskFrame.SetFabElementPickPlaceTask(fabTask_Pick, fabTask_Place, fabElement, iPickAction, iPlaceAction, iEndeffector, iFabActor, iOffsetPick, iOffsetPlace);

                //Rotate the planes by the given angle
                foreach (Plane frame in fabTask_Pick.Main_Frames)
                {
                    frame.Rotate(FabUtilities.DegreeToRadian(iRotation), frame.ZAxis, frame.Origin);
                }
                foreach (Plane frame in fabTask_Place.Main_Frames)
                {
                    frame.Rotate(FabUtilities.DegreeToRadian(iRotation), frame.ZAxis, frame.Origin);
                }

                fabTask_PickPlaceTaskList.Add(fabTask_Pick);
                fabTask_PickPlaceTaskList.Add(fabTask_Place);

            }

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabComponent", fabComponent);

            DA.SetDataList("PickPlaceTask", fabTask_PickPlaceTaskList);
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
                return Resources.FabFramework_Icon_Pick_Place;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("787ffd2a-c030-47a4-98d3-a7a2169ad741"); }
        }
    }
}
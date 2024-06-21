using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_Testtask : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_Testtask()
          : base("LCRLCassette Test Task",
                "MyTestTask",
                "Get nail task for the top plate of the LCRL Cassette.",
                "Fab",
                "LCRL")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("Endeffector", "EE", "Specific endeeffector by the actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("TestAction", "Test", "Pick action for the taskGeneration", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("TestTaskOut", "TestTO", "Fabrication Task for nailing down the top plate.", GH_ParamAccess.item);
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
            FabComponent fabCassette = new FabComponent();
            DA.GetData("FabCassette", ref fabCassette);

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            Endeffector iEndeffector = new Endeffector();
            DA.GetData("Endeffector", ref iEndeffector);

            Fab.Core.FabEnvironment.Action iTestAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("TestAction", ref iTestAction);

            //-----------
            //EDIT
            //-----------

            FabTaskFrame myTestTaskFrame = new FabTaskFrame(iTestAction.Name + "_" + fabCassette.GetFabPlates()[1].Name);

            //My logic for frame
            Plane testFrame = Rhino.Geometry.Plane.WorldXY;

            double linAxisValue = FabUtilities.GetLinAxisRadiusBased(testFrame, iFabActor.LinearAxis);

            myTestTaskFrame.AddMainFrames(testFrame);
            myTestTaskFrame.AddMainExternalValues("E1", linAxisValue);
            myTestTaskFrame.AddMainExternalValues("E2", fabCassette.GetFabPlates()[1].Angle_FabOut);

            myTestTaskFrame.AssociateElement(fabCassette.GetFabPlates()[1]);


            myTestTaskFrame.Actors[iFabActor.Name] = iFabActor;




            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);

            DA.SetData("TestTaskOut", myTestTaskFrame);

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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1cc2b2de-3fe0-442a-aca3-b2d7376890db"); }
        }
    }
}
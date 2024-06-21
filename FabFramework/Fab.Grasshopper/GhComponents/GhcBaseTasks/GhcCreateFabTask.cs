using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;


namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcCreateFabTask : GH_Component
    {
        // Static variable to store the counter value
        private static int counter = -1;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class
        /// .
        /// </summary>
        public GhcCreateFabTask()
          : base("Create FabTask",
                "FT",
                "Create a FabTask.",
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
            pManager.AddTextParameter("Name", "N", "Name of the task.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Action", "A", "Action for the taskGeneration.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("Tool", "T", "Tool", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddGenericParameter("StaticEnvironment", "SE", "StaticEnvironment.", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddGenericParameter("FabElement", "FE", "FabElement", GH_ParamAccess.item);
            pManager[5].Optional = true;
            pManager.AddGeometryParameter("Geometry", "G", "Geometry", GH_ParamAccess.item);
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabTask", "FT", "FabTask Data", GH_ParamAccess.item);
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
            string iName = string.Empty;
            DA.GetData("Name", ref iName);

            Fab.Core.FabEnvironment.Action iAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("Action", ref iAction);

            Actor iActor = new Actor();
            DA.GetData("Actor", ref iActor);

            Fab.Core.FabEnvironment.Tool iTool = null;
            DA.GetData("Tool", ref iTool);

            StaticEnv iStaticEnv = null;
            DA.GetData("StaticEnvironment", ref iStaticEnv);

            FabElement iFabElement = null;
            DA.GetData("FabElement", ref iFabElement);

            GeometryBase iGeometry = null;
            DA.GetData("Geometry", ref iGeometry);


            //-----------
            //EDIT
            //-----------
            //Auto Name generation
            counter++;

            string autoName = iName + "_" + counter.ToString("D3");
            FabTask fabTask = new FabTask(autoName);


            //Action
            fabTask.Action[iAction.Name] = iAction;


            bool actorActionFound = false;

            if (iActor != null && iActor.Name != null)
            {
                // Check if Actors.Action has same ActionID
                foreach (var action in iActor.Actions.Values)
                {
                    if (action.ActionID == iAction.ActionID)
                    {
                        actorActionFound = true;
                        break;
                    }
                }

                // Check if the action is present in the actor's actions
                if (actorActionFound)
                {
                    fabTask.Actors[iActor.Name] = iActor;
                }
                else if (iTool != null && iTool.Name != null)
                {
                    // Check if the tool is inside the actor
                    if (iActor.Tools.ContainsKey(iTool.Name))
                    {
                        // Check if Tool.Action has same ActionID
                        bool toolActionFound = false;
                        foreach (var action in iTool.Actions.Values)
                        {
                            if (action.ActionID == iAction.ActionID)
                            {
                                toolActionFound = true;
                                break;
                            }
                        }

                        if (toolActionFound)
                        {
                            fabTask.Actors[iActor.Name] = iActor;
                            fabTask.Tools[iTool.Name] = iTool;
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The action is not present in the tool's actions.");
                            return;
                        }
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The tool is not present inside the actor's tools.");
                        return;
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The action is not present in the actor's actions. Otherwise. did you connect a tool?");
                    return;
                }
            }


            if (iStaticEnv != null)
            {
                fabTask.StaticEnvs[iStaticEnv.Name] = iStaticEnv;
            }

            if (iFabElement != null)
            {
                fabTask.AssociateElement(iFabElement);

                if (iGeometry == null)
                {
                    GeometryBase taskGeometry = FabUtilities.OrientGeometryBase(iFabElement.Geometry, iFabElement.RefPln_Situ, iFabElement.RefPln_FabOut);
                    fabTask.Geometry.Add(taskGeometry);
                }
            }

            if (iGeometry != null)
            {
                fabTask.Geometry.Add(iGeometry);
            }


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabTask", fabTask);
        }

        public override void ExpireSolution(bool recompute)
        {
            base.ExpireSolution(recompute);
            counter = -1;
        }

        public override void ClearData()
        {
            base.ClearData();
            counter = -1;
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
                return Resources.FabFramework_Icon_CreateFabTask;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b909e54b-2d48-4daa-8ca2-97a876460df2"); }
        }
    }
}
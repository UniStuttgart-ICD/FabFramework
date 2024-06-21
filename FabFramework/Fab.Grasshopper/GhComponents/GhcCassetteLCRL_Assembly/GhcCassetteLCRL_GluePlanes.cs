using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using Fab.Core.FabTask;
using Fab.Core.FabEnvironment;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.IO;
using System.ComponentModel;
using Fab.Core.FabCollection;
using Fab.Core.DesignElement;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_GluePlanes: GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_GluePlanes()
          : base("LCRLCassette Glue Plates", 
                "LCRL Glue",
                "Get glue task for the bot and top plate for the LCRL Cassette.",
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
            pManager.AddGenericParameter("GlueAction", "Glue", "Glue action for the taskGeneration", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);

            pManager.AddGenericParameter("GlueBotPlate", "GlueBP", "Fabrication Task for applying glue onto the bot plate.", GH_ParamAccess.item);
            pManager.AddGenericParameter("GlueTopPlate", "GlueTP", "Fabrication Task for applying glue onto the top plate.", GH_ParamAccess.item);
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

            Fab.Core.FabEnvironment.Action iGlueAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("GlueAction", ref iGlueAction);

            //-----------
            //EDIT
            //-----------
            bool reverseOrder = false;

            //NEEDS FUNCTION TO REVERSE GLUE TASKS BOTH TIME
            List<double> offsetList = new List<double> { 0.0, 0.0, 200.0 };


            FabTaskFrame glueBotPlateTask = new FabTaskFrame(iGlueAction.Name + "_" + fabCassette.GetFabPlates()[0].Name);
            FabTaskFrame glueTopPlateTask = new FabTaskFrame(iGlueAction.Name + "_" + fabCassette.GetFabPlates()[1].Name);

            (
            List<Plane> glueBotStartPlanes, 
            List<Plane>  glueBotEndPlanes, 
            List<double> glueBotStartTTAngles, 
            List<double>  glueBotEndTTAngles, 
            List<int>  glueBotStates
            )  = FabUtilitiesBeam.GetListofBeamsGlueLines(fabCassette.GetFabBeams(), iEndeffector, false, reverseOrder);
            


            (
            List<Plane> glueTopStartPlanes,
            List<Plane> glueTopEndPlanes,
            List<double> glueTopStartTTAngles,
            List<double> glueTopEndTTAngles,
            List<int> glueTopStates
            ) = FabUtilitiesBeam.GetListofBeamsGlueLines(fabCassette.GetFabBeams(), iEndeffector, true, reverseOrder);

            FabTaskFrame.SetGlueTask(glueBotPlateTask, fabCassette.GetFabPlates()[0], glueBotStartPlanes, glueBotEndPlanes, glueBotStartTTAngles, glueBotEndTTAngles, glueBotStates, iGlueAction, iEndeffector, iFabActor, offsetList);
            FabTaskFrame.SetGlueTask(glueTopPlateTask, fabCassette.GetFabPlates()[1], glueTopStartPlanes, glueTopEndPlanes, glueTopStartTTAngles, glueTopEndTTAngles, glueTopStates, iGlueAction, iEndeffector, iFabActor, offsetList);
            
            
            //Add Geometry, very custom for LCRL cassette
            AddGlueBotPlateGeo(glueBotPlateTask);
            AddGlueTopPlateGeo(glueTopPlateTask, fabCassette);


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);

            DA.SetData("GlueBotPlate", glueBotPlateTask);
            DA.SetData("GlueTopPlate", glueTopPlateTask);
        }

        public static bool AddGlueTopPlateGeo(FabTaskFrame glueTask, FabComponent fabCassette)
        {
            FabCollection fabCollection = FabCollection.GetFabCollection();
            // Geometry
            List<Plane> glueStartPlanes = glueTask.Main_Frames;
            List<Plane> glueEndPlanes = glueTask.Sub_Frames;
            List<double> glueStartAngles = glueTask.Main_ExtValues["E2"];
            List<double> glueEndAngles = glueTask.Sub_ExtValues["E2"];
            fabCollection.fabPlateCollection.TryGetValue(fabCassette.FabPlatesName[0], out FabPlate fabPlate);

            int maxCount = Math.Max(glueStartPlanes.Count, glueEndPlanes.Count);

            for (int i = 0; i < maxCount; i++)
            {
                if (i < glueStartPlanes.Count)
                {
                    List<GeometryBase> glueStartGeos = new List<GeometryBase>();
                    GeometryBase glueStartPlate= FabUtilities.OrientGeometryBase(fabPlate.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_FabOut);
                    double turnStartPlateAngle = FabUtilities.DegreeToRadian(glueStartAngles[i] - fabPlate.Angle_FabOut);
                    Transform rotationStartTransform = Transform.Rotation(turnStartPlateAngle, fabPlate.EnvFab.RefPln[0].ZAxis, fabPlate.EnvFab.RefPln[0].Origin);
                    glueStartPlate.Transform(rotationStartTransform);
                    glueStartGeos.Add(glueStartPlate);

                    for (int j = 0; j < fabCassette.FabBeamsName.Count; j++)
                    { 
                        fabCollection.fabBeamCollection.TryGetValue(fabCassette.FabBeamsName[j], out FabBeam fabBeam);

                        if (fabBeam.Angle_FabOut == glueStartAngles[i])
                        {
                            GeometryBase beamGeo = fabBeam.Geometry;
                            beamGeo = FabUtilities.OrientGeometryBase(beamGeo, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);
                            glueStartGeos.Add(beamGeo);
                        }
                    }

                    GeometryBase mergedGlueStartGeo = FabUtilities.ConvertToJoinedMesh(glueStartGeos);
                    glueTask.Geometry.Add(mergedGlueStartGeo);

                }

                if (i < glueEndPlanes.Count)
                {
                    List<GeometryBase> glueEndGeos = new List<GeometryBase>();
                    GeometryBase glueEndPlate = FabUtilities.OrientGeometryBase(fabPlate.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_FabOut);
                    double turnEndPlateAngle = FabUtilities.DegreeToRadian(glueEndAngles[i] - fabPlate.Angle_FabOut);
                    Transform rotationEndTransform = Transform.Rotation(turnEndPlateAngle, fabPlate.EnvFab.RefPln[0].ZAxis, fabPlate.EnvFab.RefPln[0].Origin);
                    glueEndPlate.Transform(rotationEndTransform);
                    glueEndGeos.Add(glueEndPlate);

                    for (int j = 0; j < fabCassette.FabBeamsName.Count; j++)
                    {
                        fabCollection.fabBeamCollection.TryGetValue(fabCassette.FabBeamsName[j], out FabBeam fabBeam);

                        if (fabBeam.Angle_FabOut == glueEndAngles[i])
                        {
                            GeometryBase beamGeo = fabBeam.Geometry;
                            beamGeo = FabUtilities.OrientGeometryBase(beamGeo, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);
                            glueEndGeos.Add(beamGeo);
                        }
                    }

                    GeometryBase mergedGlueEndGeo = FabUtilities.ConvertToJoinedMesh(glueEndGeos);
                    glueTask.Geometry.Add(mergedGlueEndGeo);

                }
            }


            return true;
        }


        public static bool AddGlueBotPlateGeo(FabTaskFrame glueTask)
        {
            FabCollection fabCollection = FabCollection.GetFabCollection();
            // Geometry
            List<Plane> glueStartPlanes = glueTask.Main_Frames;
            List<Plane> glueEndPlanes = glueTask.Sub_Frames;
            List<double> glueStartAngles = glueTask.Main_ExtValues["E2"];
            List<double> glueEndAngles = glueTask.Sub_ExtValues["E2"];
            fabCollection.fabPlateCollection.TryGetValue(glueTask.FabElementsName[0], out FabPlate fabPlate);


            int maxCount = Math.Max(glueStartPlanes.Count, glueEndPlanes.Count);

            for (int i = 0; i < maxCount; i++)
            {
                if (i < glueStartPlanes.Count)
                {
                    GeometryBase glueStartGeo = FabUtilities.OrientGeometryBase(fabPlate.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_FabOut);
                    double turnStartPlateAngle = FabUtilities.DegreeToRadian(glueStartAngles[i] - fabPlate.Angle_FabOut);
                    Transform rotationStartTransform = Transform.Rotation(turnStartPlateAngle, fabPlate.EnvFab.RefPln[0].ZAxis, fabPlate.EnvFab.RefPln[0].Origin);
                    glueStartGeo.Transform(rotationStartTransform);
                    glueTask.Geometry.Add(glueStartGeo);
                }

                if (i < glueEndPlanes.Count)
                {
                    GeometryBase glueEndGeo = FabUtilities.OrientGeometryBase(fabPlate.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_FabOut);
                    double turnEndPlateAngle = FabUtilities.DegreeToRadian(glueEndAngles[i] - fabPlate.Angle_FabOut);
                    Transform rotationEndTransform = Transform.Rotation(turnEndPlateAngle, fabPlate.EnvFab.RefPln[0].ZAxis, fabPlate.EnvFab.RefPln[0].Origin);
                    glueEndGeo.Transform(rotationEndTransform);
                    glueTask.Geometry.Add(glueEndGeo);
                }
            }


            return true;
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
            get { return new Guid("dd0f623c-d2a8-496a-99df-cd177ddaaa15"); }
        }
    }
}
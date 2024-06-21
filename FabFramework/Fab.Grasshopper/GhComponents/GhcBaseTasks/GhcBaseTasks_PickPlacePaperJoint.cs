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
using Rhino.Input.Custom;
using Rhino.Geometry.Intersect;
using System.Management.Instrumentation;
using System.Numerics;
using Rhino;
using static Rhino.Render.TextureGraphInfo;
using Fab.Grasshopper.Properties;

namespace Fab.Grasshopper.GhComponents.GhcBaseTasks
{
    public class GhcBaseTasks_PickPlacePaperJoint : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcBaseTasks_PickPlacePaperJoint()
          : base("Pick & Place for Paper Joint", 
                "PnP_Paper",
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
            pManager.AddTextParameter("FabElementName", "FEN", "Name of the FabElement.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("Endeffector", "EE", "Specific endeffector by the actor.", GH_ParamAccess.item);
            pManager.AddGenericParameter("PickAction", "Pick", "Pick action for the taskGeneration.", GH_ParamAccess.item);
            pManager.AddGenericParameter("PlaceAction", "Place", "Place action for the taskGeneration.", GH_ParamAccess.item);
            pManager.AddNumberParameter("OffsetPick", "Off1", "Offset for the pick task", GH_ParamAccess.list, new List<double> { 0, 0, 200 });
            pManager.AddNumberParameter("OffsetPlace", "Off2", "Offset for the place task", GH_ParamAccess.list, new List<double> { 0, 0, 200 });
            pManager.AddNumberParameter("Rotation", "Rot", "Rotate the planes by the given angle (degree).", GH_ParamAccess.item, 0.0);
            pManager.AddGenericParameter("StaticEnvPaper", "SEPaper", "Static Environment for the pick task of the paper.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("MultiMag", "M", "Are you using multiple magazine slots?", GH_ParamAccess.item, false);
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

            StaticEnv iStaticEnvPaper = new StaticEnv();
            DA.GetData("StaticEnvPaper", ref iStaticEnvPaper);

            bool iMultiMag = false;
            DA.GetData("MultiMag", ref iMultiMag);


            //-----------
            //EDIT
            //-----------

            int staticEnvCounter = 0;

            List<FabElement> matchingFabElements = FabUtilitiesElement.GetMatchingFabElement(fabComponent, iFabElementName);
            List<FabBeam> matchingFabBeams = matchingFabElements.OfType<FabBeam>().ToList();

            //sort matchingFabElements by matchingFabElements.Index
            matchingFabBeams.Sort((x1, y1) => x1.Index.CompareTo(y1.Index));

            List<FabTaskFrame> fabTask_PickPlaceTaskList = new List<FabTaskFrame>();

            for (int i = 0; i < matchingFabBeams.Count; i++)
            {
                // The key exists, retrieve the value
                FabBeam fabBeam = matchingFabBeams[i];

                List<Rhino.Geometry.Plane> placePlanesPaper = new List<Rhino.Geometry.Plane>();

                // Only iterate through beams with lower index number
                for (int j = 0; j < i; j++)
                {
                    FabBeam otherFabBeam = matchingFabBeams[j];

                    // Check for collision of geo1 and geo2
                    bool intersection = Rhino.Geometry.Intersect.Intersection.BrepBrep(
                        fabBeam.GetDesignBeam().Geometry, otherFabBeam.GetDesignBeam().Geometry, 0.001, out Curve[] intersectionCurves, out Point3d[] intersectionPoints);
                    //also check if intersectionCurves is not empty


                    if (intersection && intersectionCurves.Length > 0)
                    {
                        //try to join intersectionCurves
                        Curve[] joinedCurves = Curve.JoinCurves(intersectionCurves, 0.001);

                        //iterate though intersectionCurves and get StartPoint
                        foreach (Curve curve in joinedCurves)
                        {
                            // Check if the curve is closed
                            if (curve.IsClosed)
                            {
                                // Calculate the area mass properties to get the centroid
                                AreaMassProperties areaProps = AreaMassProperties.Compute(curve);
                                if (areaProps != null)
                                {
                                    Point3d centroid = areaProps.Centroid;

                                    Vector3d xAxisBaseLine = new Vector3d(fabBeam.GetDesignBeam().EndPoint - fabBeam.GetDesignBeam().StartPoint);
                                    xAxisBaseLine.Unitize();

                                    // 1. Get Brep from fabBeam.GetDesignBeam().Geometry
                                    Brep beamBrep = fabBeam.GetDesignBeam().Geometry as Brep;
                                    if (beamBrep != null)
                                    {
                                        // Find the closest point on the Brep
                                        double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

                                        Point3d closestPoint = beamBrep.ClosestPoint(centroid);

                                        // Check if closestPoint is not null
                                        if (closestPoint != null)
                                        {
                                            // Find the closest point on the Brep
                                            Point3d closestPt;
                                            ComponentIndex ci;
                                            double s, t;
                                            //double distance;
                                            Vector3d normal;
                                            beamBrep.ClosestPoint(centroid, out closestPt, out ci, out s, out t, 0.0, out normal);

                                            // Get the face index
                                            int faceIndex = ci.Index;

                                            // Get the face at the specified index
                                            BrepFace face = beamBrep.Faces[faceIndex];

                                            // Get the normal at the closest point on the face
                                            Vector3d faceNormal = face.NormalAt(s, t);
                                            faceNormal.Unitize();

                                            // Compute the Y-axis as orthogonal to the normal and xAxisBaseLine
                                            Vector3d yAxis = Vector3d.CrossProduct(faceNormal, xAxisBaseLine);
                                            yAxis.Unitize();

                                            // Check for potential degenerate cases
                                            if (yAxis.IsZero)
                                            {
                                                yAxis = Vector3d.CrossProduct(faceNormal, Vector3d.XAxis);
                                                yAxis.Unitize();

                                                // If still zero, use Z-axis
                                                if (yAxis.IsZero)
                                                {
                                                    yAxis = Vector3d.CrossProduct(faceNormal, Vector3d.ZAxis);
                                                    yAxis.Unitize();
                                                }
                                            }

                                            // Ensure the xAxisBaseLine is valid and unitized
                                            if (xAxisBaseLine.IsZero)
                                            {
                                                xAxisBaseLine = Vector3d.CrossProduct(Vector3d.XAxis, faceNormal);
                                                xAxisBaseLine.Unitize();

                                                if (xAxisBaseLine.IsZero)
                                                {
                                                    xAxisBaseLine = Vector3d.CrossProduct(Vector3d.ZAxis, faceNormal);
                                                    xAxisBaseLine.Unitize();
                                                }
                                            }

                                            if (fabBeam.Index == 23)
                                            {
                                                Rhino.RhinoApp.WriteLine("FaceIndex: " + faceIndex);
                                            }

                                            // Create the intersection plane with validated axes
                                            Rhino.Geometry.Plane intersectionPlane = new Rhino.Geometry.Plane(closestPoint, xAxisBaseLine, -yAxis);

                                            //Roate intersectionPlane by iRotateDowelPlane
                                            intersectionPlane.Rotate(FabUtilities.DegreeToRadian(iRotation), intersectionPlane.ZAxis, intersectionPlane.Origin);


                                             placePlanesPaper.Add(intersectionPlane);

                                        }

  

                                    }


                                }
                            }
                        }
                    }
                 
                }



                if (placePlanesPaper.Count > 0)
                {

                    for (int p = 0; p < placePlanesPaper.Count; p++)
                    {
                        Rhino.Geometry.Plane placePlanePaper = placePlanesPaper[p];
                        // Check if the dowelPlane is valid
                        if (!placePlanePaper.IsValid)
                        {
                            throw new Exception("Invalid dowel plane encountered.");
                        }

                        // Check StaticEnvs
                        if (iStaticEnvPaper == null || string.IsNullOrEmpty(iStaticEnvPaper.Name))
                        {
                            // Throw error
                            throw new Exception("StaticEnvPaper is not defined or its Name property is null or empty.");
                        }


                        //Get pickPlanePaper missing
                        //iterate through staticEnv
                        if (iMultiMag == true)
                            staticEnvCounter = 0;

                        Rhino.Geometry.Plane pickPlanePaper = iStaticEnvPaper.AlignPln[staticEnvCounter];

                        staticEnvCounter += 1;
                        if (staticEnvCounter >= iStaticEnvPaper.AlignPln.Count)
                        {
                        staticEnvCounter = 0;
                        }


                        //Orient to fabricationEnv
                        placePlanePaper = FabUtilities.OrientPlane(placePlanePaper, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);

                        FabTaskFrame fabTask_pickPaper = new FabTaskFrame(iPickAction.Name + "_" + p.ToString("D3") + "_" + fabBeam.Name);
                        FabTaskFrame fabTask_placePaper = new FabTaskFrame(iPlaceAction.Name + "_" + p.ToString("D3") + "_" + fabBeam.Name);

                        FabTaskFrame.SetPickPlaceTask(fabTask_pickPaper, fabTask_placePaper, 
                            pickPlanePaper, placePlanePaper,
                            fabBeam, iPickAction, iPlaceAction, iEndeffector, iFabActor, iOffsetPick, iOffsetPlace);

                        fabTask_pickPaper.Index = p;
                        fabTask_placePaper.Index = p;

                        fabTask_pickPaper.StaticEnvs[iStaticEnvPaper.Name] = iStaticEnvPaper;
                        fabTask_placePaper.StaticEnvs[fabBeam.EnvFab.Name] = fabBeam.EnvFab;


                        //Rotate the planes by the given angle
                        foreach (Rhino.Geometry.Plane frame in fabTask_pickPaper.Main_Frames)
                        {
                            frame.Rotate(FabUtilities.DegreeToRadian(iRotation), frame.ZAxis, frame.Origin);
                        }
                        foreach (Rhino.Geometry.Plane frame in fabTask_placePaper.Main_Frames)
                        {
                            frame.Rotate(FabUtilities.DegreeToRadian(iRotation), frame.ZAxis, frame.Origin);
                        }






                        //Add Geometry
                        double paper_width = 50;
                        double paper_thickness = 4;

                        //create rectangular brep
                        Rhino.Geometry.Plane updatedPlacePlanePaper= fabTask_placePaper.Main_Frames[0];
                        //gete YZ plane from updatedPlacePlanePaper
                        Rhino.Geometry.Plane flipped_placePlanePaper = new Rhino.Geometry.Plane(updatedPlacePlanePaper.Origin, updatedPlacePlanePaper.YAxis, updatedPlacePlanePaper.ZAxis);

                        //moove origin of plane by half of paper_width
                        Rhino.Geometry.Plane rectanglePlane = flipped_placePlanePaper.Clone();
                        rectanglePlane.Origin = flipped_placePlanePaper.Origin - flipped_placePlanePaper.XAxis * paper_width / 2 - flipped_placePlanePaper.YAxis * paper_width / 2;

                        Rectangle3d rect = new Rectangle3d(rectanglePlane, paper_width, paper_width);

                        Brep extrudedBrep = Brep.CreateFromSurface(Surface.CreateExtrusion(rect.ToNurbsCurve(), flipped_placePlanePaper.ZAxis * paper_thickness));

                        Brep cappedBrep = extrudedBrep.CapPlanarHoles(0.001);

                        fabTask_placePaper.Geometry.Add(cappedBrep);


                        fabTask_PickPlaceTaskList.Add(fabTask_pickPaper);
                        fabTask_PickPlaceTaskList.Add(fabTask_placePaper);

                    }
                }


            }


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabComponent", fabComponent);

            DA.SetDataList("PickPlaceTask", fabTask_PickPlaceTaskList);
        }

        private Line ProjectLineToPlane(Line line, Rhino.Geometry.Plane plane)
        {
            Point3d startPointProjected = FabUtilities.ProjectPointToPlane(line.From, plane);
            Point3d endPointProjected = FabUtilities.ProjectPointToPlane(line.To, plane);
            return new Line(startPointProjected, endPointProjected);
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
                return Resources.FabFramework_Icon_PickPlacePaper;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d5242bb1-1bde-4663-8b7d-112c501c4701"); }
        }
    }
}
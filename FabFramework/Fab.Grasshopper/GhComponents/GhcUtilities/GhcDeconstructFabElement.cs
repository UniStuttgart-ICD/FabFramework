using Fab.Core.DesignElement;
using Fab.Core.FabCollection;
using Fab.Core.FabElement;
using Fab.Core.FabTask;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcUtilities
{

    public class GhcDeconstructFabElement : GH_Component
    {
        double globalPreviewScale = 1.0;

        List<Plane> refPln_SituList;
        List<Plane> refPln_SituBoxList;
        List<Plane> refPln_MagList;
        List<Plane> refPln_FabList;
        List<Plane> refPln_FabOutList;

        List<Line> geometryBoxSituList;
        List<GeometryBase> geometrySituList;
        List<GeometryBase> geometryMagList;
        List<GeometryBase> geometryFabList;
        List<GeometryBase> geometryFabOutList;

        List<DesignElement> designElementsList;
        List<FabPlate> fabPlatesList;
        List<FabBeam> fabBeamsList;

        List<FabTask> fabTasksList;

        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDeconstructFabElement()
          : base(
              "Deconstruct FabElement",
              "DecFE",
              "Deconstruct FabElement for base features.",
              "Fab",
              "Utilities")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabElements", "FE", "FabElements to deconstruct.", GH_ParamAccess.list);
            pManager.AddNumberParameter("PreviewScale", "PS", "Scale for preview geometry", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the FabElement", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Index", "I", "Index of the FabElement", GH_ParamAccess.list);
            pManager.AddPlaneParameter("RefPln_Situ", "RPS", "Reference Plane of the FabElement in the Situ", GH_ParamAccess.list);
            pManager.AddPlaneParameter("RefPln_SituBox", "RPSB", "Reference Plane of the FabElement in the SituBox", GH_ParamAccess.list);
            pManager.AddPlaneParameter("RefPln_Mag", "RPM", "Reference Plane of the FabElement in the Mag", GH_ParamAccess.list);
            pManager.AddPlaneParameter("RefPln_Fab", "RPF", "Reference Plane of the FabElement in the Fab", GH_ParamAccess.list);
            pManager.AddPlaneParameter("RefPln_FabOut", "RPFO", "Reference Plane of the FabElement in the FabOut", GH_ParamAccess.list);
            pManager.HideParameter(2);
            pManager.HideParameter(3);
            pManager.HideParameter(4);
            pManager.HideParameter(5);
            pManager.HideParameter(6);
            pManager.AddGeometryParameter("GeometryBoxSitu", "GBS", "Box Edge Lines of the FabElement", GH_ParamAccess.list);
            pManager.HideParameter(7);
            pManager.AddGeometryParameter("GeometrySitu", "GS", "Geometry of the FabElement", GH_ParamAccess.list);
            pManager.AddGeometryParameter("GeometryMag", "GM", "Geometry of the FabElement", GH_ParamAccess.list);
            pManager.AddGeometryParameter("GeometryFab", "GF", "Geometry of the FabElement", GH_ParamAccess.list);
            pManager.AddGeometryParameter("GeometryFabOut", "GFO", "Geometry of the FabElement", GH_ParamAccess.list);

            pManager.AddGenericParameter("DesignElements", "DE", "DesignElements of the FabElement", GH_ParamAccess.list);
            pManager.AddGenericParameter("FabPlates", "FP", "FabPlates of the FabElement", GH_ParamAccess.list);
            pManager.AddGenericParameter("FabBeams", "FB", "FabBeams of the FabElement", GH_ParamAccess.list);

            pManager.AddGenericParameter("FabTasks", "FT", "FabTasks of the FabElement", GH_ParamAccess.list);


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
            List<FabElement> iFabElements = new List<FabElement>();
            DA.GetDataList("FabElements", iFabElements);

            double previewScale = 1.0;
            DA.GetData("PreviewScale", ref previewScale);
            //-----------
            //EDIT
            //-----------

            globalPreviewScale = previewScale;

            List<string> nameList = new List<string>();
            List<int> indexList = new List<int>();

            refPln_SituList = new List<Plane>();
            refPln_SituBoxList = new List<Plane>();
            refPln_MagList = new List<Plane>();
            refPln_FabList = new List<Plane>();
            refPln_FabOutList = new List<Plane>();

            geometryBoxSituList = new List<Line>();
            geometrySituList = new List<GeometryBase>();
            geometryMagList = new List<GeometryBase>();
            geometryFabList = new List<GeometryBase>();
            geometryFabOutList = new List<GeometryBase>();

            designElementsList = new List<DesignElement>();
            fabPlatesList = new List<FabPlate>();
            fabBeamsList = new List<FabBeam>();

            fabTasksList = new List<FabTask>();


            foreach (FabElement fabElement in iFabElements)
            {
                nameList.Add(fabElement.Name);
                indexList.Add(fabElement.Index);

                refPln_SituList.Add(fabElement.RefPln_Situ);
                refPln_SituBoxList.Add(fabElement.RefPln_SituBox);
                refPln_MagList.Add(fabElement.RefPln_Mag);
                refPln_FabList.Add(fabElement.RefPln_Fab);
                refPln_FabOutList.Add(fabElement.RefPln_FabOut);

                geometrySituList.Add(fabElement.Geometry);
                GeometryBase geometryMag = FabUtilities.OrientGeometryBase(fabElement.Geometry, fabElement.RefPln_Situ, fabElement.RefPln_Mag);
                geometryMagList.Add(geometryMag);
                GeometryBase geometryFab = FabUtilities.OrientGeometryBase(fabElement.Geometry, fabElement.RefPln_Situ, fabElement.RefPln_Fab);
                geometryFabList.Add(geometryFab);
                GeometryBase geometryFabOut = FabUtilities.OrientGeometryBase(fabElement.Geometry, fabElement.RefPln_Situ, fabElement.RefPln_FabOut);
                geometryFabOutList.Add(geometryFabOut);


                DesignElement designElement = fabCollection.designElementCollection.TryGetValue(fabElement.DesignElementName, out var designElementOut) ? designElementOut : null;
                designElementsList.Add(designElement);

                geometryBoxSituList.AddRange(GetBoxEdgeLines(designElement.BoundingBox));

                if (fabElement.GetType() == typeof(FabComponent))
                {
                    FabComponent fabComponent = fabElement as FabComponent;

                    if (fabComponent.FabPlatesName != null)
                    {
                        for (int i = 0; i < fabComponent.FabPlatesName.Count; i++)
                        {
                            if (fabCollection.fabPlateCollection.TryGetValue(fabComponent.FabPlatesName[i], out var fabPlate))
                            {
                                fabPlatesList.Add(fabPlate);
                            }
                        }
                    }
                    if (fabComponent.FabBeamsName != null)
                    {
                        for (int i = 0; i < fabComponent.FabBeamsName.Count; i++)
                        {
                            if (fabCollection.fabBeamCollection.TryGetValue(fabComponent.FabBeamsName[i], out var fabBeam))
                            {
                                fabBeamsList.Add(fabBeam);
                            }
                        }
                    }
                }

                foreach (var name in fabElement.FabTasksName)
                {
                    if (fabCollection.fabTaskCollection.TryGetValue(name, out var fabTask))
                    { fabTasksList.Add(fabTask); }
                }
            }




            //-----------
            //OUTPUTS
            //-----------
            DA.SetDataList("Name", nameList);
            DA.SetDataList("Index", indexList);

            DA.SetDataList("RefPln_Situ", refPln_SituList);
            DA.SetDataList("RefPln_SituBox", refPln_SituBoxList);
            DA.SetDataList("RefPln_Mag", refPln_MagList);
            DA.SetDataList("RefPln_Fab", refPln_FabList);
            DA.SetDataList("RefPln_FabOut", refPln_FabOutList);

            DA.SetDataList("GeometryBoxSitu", geometryBoxSituList);
            DA.SetDataList("GeometrySitu", geometrySituList);
            DA.SetDataList("GeometryMag", geometryMagList);
            DA.SetDataList("GeometryFab", geometryFabList);
            DA.SetDataList("GeometryFabOut", geometryFabOutList);

            DA.SetDataList("DesignElements", designElementsList);
            DA.SetDataList("FabPlates", fabPlatesList);
            DA.SetDataList("FabBeams", fabBeamsList);

            DA.SetDataList("FabTasks", fabTasksList);
        }

        public List<Line> GetBoxEdgeLines(Box box)
        {
            if (!box.IsValid)
            {
                throw new ArgumentException("Box is not valid.", nameof(box));
            }

            Point3d[] corners = box.GetCorners();

            if (corners == null)
            {
                throw new InvalidOperationException("Box.GetCorners() returned null.");
            }

            if (corners.Length != 8)
            {
                throw new InvalidOperationException("Box.GetCorners() did not return 8 corners.");
            }

            // Create lines from corners
            List<Line> edgeLines = new List<Line>
              {
                new Line(corners[0], corners[1]),
                new Line(corners[1], corners[2]),
                new Line(corners[2], corners[3]),
                new Line(corners[3], corners[0]),

                new Line(corners[4], corners[5]),
                new Line(corners[5], corners[6]),
                new Line(corners[6], corners[7]),
                new Line(corners[7], corners[4]),

                new Line(corners[0], corners[4]),
                new Line(corners[1], corners[5]),
                new Line(corners[2], corners[6]),
                new Line(corners[3], corners[7])
                };

            return edgeLines;
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // Define a list of tuples to store the planes and their corresponding colors.
            List<(GeometryBase, DisplayMaterial)> customMeshes = new List<(GeometryBase, DisplayMaterial)>();

            DisplayMaterial materialGeometrySitu = new DisplayMaterial(System.Drawing.Color.LightGray, 0.5);
            DisplayMaterial materialGeometryMag = new DisplayMaterial(System.Drawing.Color.LightSalmon, 0.5);
            DisplayMaterial materialGeometryFab = new DisplayMaterial(System.Drawing.Color.LightYellow, 0.5);
            DisplayMaterial materialGeometryFabOut = new DisplayMaterial(System.Drawing.Color.LightGreen, 0.5);

            if (geometrySituList != null)
                for (int i = 0; i < geometrySituList.Count; i++)
                    customMeshes.Add((geometrySituList[i], materialGeometrySitu));
            if (geometryMagList != null)
                for (int i = 0; i < geometryMagList.Count; i++)
                    customMeshes.Add((geometryMagList[i], materialGeometryMag));
            if (geometryFabList != null)
                for (int i = 0; i < geometryFabList.Count; i++)
                    customMeshes.Add((geometryFabList[i], materialGeometryFab));
            if (geometryFabOutList != null)
                for (int i = 0; i < geometryFabOutList.Count; i++)
                    customMeshes.Add((geometryFabOutList[i], materialGeometryFabOut));

            // Iterate through the custom planes and draw them with the specified colors.
            foreach (var (GeometryBase, DisplayMaterial) in customMeshes)
            {
                args.Display.DrawMeshShaded(GeometryBase as Mesh, DisplayMaterial);
            }

            //base.DrawViewportMeshes(args);
        }


        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            // Define a list of tuples to store the planes and their corresponding colors.
            List<(Plane, System.Drawing.Color)> customPlanes = new List<(Plane, System.Drawing.Color)>();

            // Add your custom planes and colors to the list.
            if (refPln_SituList != null && refPln_SituList.Count > 0)
                for (int i = 0; i < refPln_SituList.Count; i++)
                    if (refPln_SituList[i] != null)
                        customPlanes.Add((refPln_SituList[i], System.Drawing.Color.Blue));
            if (refPln_SituBoxList != null && refPln_SituBoxList.Count > 0)
                for (int i = 0; i < refPln_SituBoxList.Count; i++)
                    if (refPln_SituBoxList[i] != null)
                        customPlanes.Add((refPln_SituBoxList[i], System.Drawing.Color.Cyan));
            if (refPln_MagList != null && refPln_MagList.Count > 0)
                for (int i = 0; i < refPln_MagList.Count; i++)
                    if (refPln_MagList[i] != null)
                        customPlanes.Add((refPln_MagList[i], System.Drawing.Color.Magenta));
            if (refPln_FabList != null && refPln_FabList.Count > 0)
                for (int i = 0; i < refPln_FabList.Count; i++)
                    if (refPln_FabList[i] != null)
                        customPlanes.Add((refPln_FabList[i], System.Drawing.Color.Orange));
            if (refPln_FabOutList != null && refPln_FabOutList.Count > 0)
                for (int i = 0; i < refPln_FabOutList.Count; i++)
                    if (refPln_FabOutList[i] != null)
                        customPlanes.Add((refPln_FabOutList[i], System.Drawing.Color.Green));

            //Box EdgeLines
            if (geometryBoxSituList != null && geometryBoxSituList.Count > 0)
                for (int i = 0; i < geometryBoxSituList.Count; i++)
                    if (geometryBoxSituList[i] != null)
                        args.Display.DrawLine(geometryBoxSituList[i], System.Drawing.Color.Cyan);

            // Iterate through the custom planes and draw them with the specified colors.
            foreach (var (plane, color) in customPlanes)
            {
                DrawCustomPlaneWithGrid(args, plane, color);
            }

            base.DrawViewportWires(args);
        }

        public void DrawCustomPlaneWithGrid(IGH_PreviewArgs args, Plane customPlane, System.Drawing.Color customColor)
        {
            int divisions = 9;
            // Calculate the half extents of the rectangle
            double halfWidth = 100.0 * globalPreviewScale;
            double halfHeight = 100.0 * globalPreviewScale;

            // Get the center point of the custom plane
            Point3d center = customPlane.Origin;

            // Calculate the orientation vectors of the plane
            Vector3d xDirection = customPlane.XAxis * halfWidth;
            Vector3d yDirection = customPlane.YAxis * halfHeight;

            // Calculate the corner points of the rectangle based on the center and orientation
            Point3d corner1 = center - xDirection - yDirection;
            Point3d corner2 = center + xDirection - yDirection;
            Point3d corner3 = center + xDirection + yDirection;
            Point3d corner4 = center - xDirection + yDirection;

            // Draw lines to represent the outline of the plane with the custom color
            args.Display.DrawLine(corner1, corner2, customColor);
            args.Display.DrawLine(corner2, corner3, customColor);
            args.Display.DrawLine(corner3, corner4, customColor);
            args.Display.DrawLine(corner4, corner1, customColor);

            // Draw a grid inside the rectangle
            for (int i = 0; i < divisions; i++)
            {
                double t = i / (double)(divisions - 1);
                Point3d startH = corner1 + (corner2 - corner1) * t;
                Point3d endH = corner4 + (corner3 - corner4) * t;
                Point3d startV = corner1 + (corner4 - corner1) * t;
                Point3d endV = corner2 + (corner3 - corner2) * t;

                args.Display.DrawLine(startH, endH, customColor);
                args.Display.DrawLine(startV, endV, customColor);
            }

            // Draw the green YAxis line
            Point3d yAxisStart = center;
            Point3d yAxisEnd = center + customPlane.YAxis * halfHeight;
            args.Display.DrawLine(yAxisStart, yAxisEnd, System.Drawing.Color.Green, 5);

            // Draw the red XAxis line
            Point3d xAxisStart = center;
            Point3d xAxisEnd = center + customPlane.XAxis * halfWidth;
            args.Display.DrawLine(xAxisStart, xAxisEnd, System.Drawing.Color.Red, 5);
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
                return Resources.FabFramework_DeconstructFabElement;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0ec278cb-d74c-470e-a1bd-c8300f8150f4"); }
        }
    }
}
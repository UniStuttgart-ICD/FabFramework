using Fab.Core.DesignElement;
using Fab.Core.FabCollection;
using Fab.Core.FabUtilities;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcUtilities
{

    public class GhcDeconstructDesignElement : GH_Component
    {
        double globalPreviewScale = 1.0;

        List<Brep> geometry_List;

        List<Plane> frameBase_List;
        List<Plane> frameLower_List;
        List<Plane> frameUpper_List;

        List<Line> designBaseLine_List;
        List<Polyline> designPolyline_List;
        List<object> designLine_List;

        List<Line> geometryBox_List;


        /// <summary>
        /// Initializes a new instance of the GhcDefineTool class.
        /// </summary>
        public GhcDeconstructDesignElement()
          : base(
              "Deconstruct DesignElement",
              "DecDE",
              "Deconstruct DesignElement for base features.",
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
            pManager.AddGenericParameter("DesignElements", "DE", "DesignElements to deconstruct.", GH_ParamAccess.list);
            pManager.AddNumberParameter("PreviewScale", "PS", "Scale for preview geometry", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the FabElement", GH_ParamAccess.list);
            pManager.AddBrepParameter("Geometry", "G", "Geometry of the DesignElement", GH_ParamAccess.list);
            pManager.AddPlaneParameter("FrameBase", "FB", "FrameBase of the DesignElement", GH_ParamAccess.list);
            pManager.AddPlaneParameter("FrameLower", "FL", "FrameLower of the DesignElement", GH_ParamAccess.list);
            pManager.AddPlaneParameter("FrameUpper", "FU", "FrameUpper of the DesignElement", GH_ParamAccess.list);
            pManager.HideParameter(1);
            pManager.HideParameter(2);
            pManager.HideParameter(3);
            pManager.HideParameter(4);
            pManager.AddGenericParameter("DesignLine", "DL", "DesignLine of the DesignElement", GH_ParamAccess.list);
            pManager.HideParameter(5);
            pManager.AddGeometryParameter("BrepBox", "BB", "Box Edge Lines of the DesignElement", GH_ParamAccess.list);
            pManager.HideParameter(6);
            pManager.AddGenericParameter("DesignRegion", "DR", "DesignRegion of the DesignElement", GH_ParamAccess.list);

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
            List<DesignElement> iDesignElements = new List<DesignElement>();
            DA.GetDataList("DesignElements", iDesignElements);

            double previewScale = 1.0;
            DA.GetData("PreviewScale", ref previewScale);
            //-----------
            //EDIT
            //-----------

            globalPreviewScale = previewScale;

            List<string> nameList = new List<string>();

            geometry_List = new List<Brep>();

            frameBase_List = new List<Plane>();
            frameLower_List = new List<Plane>();
            frameUpper_List = new List<Plane>();

            designBaseLine_List = new List<Line>();
            designPolyline_List = new List<Polyline>();
            designLine_List = new List<object>();

            geometryBox_List = new List<Line>();
            Dictionary<string, DesignRegion> designRegion_Dictionary = new Dictionary<string, DesignRegion>();


            foreach (DesignElement designElement in iDesignElements)
            {
                nameList.Add(designElement.Name);

                geometry_List.Add(designElement.Geometry);

                frameBase_List.Add(designElement.FrameBase);
                frameLower_List.Add(designElement.FrameLower);
                frameUpper_List.Add(designElement.FrameUpper);

                //check if designElement is type DesignBeam
                if (designElement.GetType() == typeof(DesignBeam))
                {
                    DesignBeam designBeam = designElement as DesignBeam;
                    designBaseLine_List.Add(designBeam.BaseLine);
                    designLine_List.Add(designBeam.BaseLine);
                }
                else if (designElement.GetType() == typeof(DesignPlate))
                {
                    DesignPlate designPlate = designElement as DesignPlate;
                    designPolyline_List.Add(designPlate.Outline);
                    designLine_List.Add(designPlate.Outline);
                }

                geometryBox_List.AddRange(GetBoxEdgeLines(designElement.BoundingBox));

                designRegion_Dictionary = designElement.DesignRegion;
            }

            //-----------
            //OUTPUTS
            //-----------


            DA.SetDataList("Name", nameList);

            DA.SetDataList("Geometry", geometry_List);

            DA.SetDataList("FrameBase", frameBase_List);
            DA.SetDataList("FrameLower", frameLower_List);
            DA.SetDataList("FrameUpper", frameUpper_List);

            DA.SetDataList("DesignLine", designLine_List);

            DA.SetDataList("BrepBox", geometryBox_List);
            DA.SetDataList("DesignRegion", designRegion_Dictionary);
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

            DisplayMaterial materialGeometrySitu = new DisplayMaterial(System.Drawing.Color.White, 0.5);

            if (geometry_List != null)
                for (int i = 0; i < geometry_List.Count; i++)
                {
                    //convert brep to mesh
                    List<GeometryBase> convertedGeometries = FabUtilities.ConvertToGeometry(geometry_List[i]);

                    for (int j = 0; j < convertedGeometries.Count; j++)
                        customMeshes.Add((convertedGeometries[j], materialGeometrySitu));
                }

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
            if (frameBase_List != null && frameBase_List.Count > 0)
                for (int i = 0; i < frameBase_List.Count; i++)
                    if (frameBase_List[i] != null)
                        customPlanes.Add((frameBase_List[i], System.Drawing.Color.MediumVioletRed));

            if (frameLower_List != null && frameLower_List.Count > 0)
                for (int i = 0; i < frameLower_List.Count; i++)
                    if (frameLower_List[i] != null)
                        customPlanes.Add((frameLower_List[i], System.Drawing.Color.White));

            if (frameUpper_List != null && frameUpper_List.Count > 0)
                for (int i = 0; i < frameUpper_List.Count; i++)
                    if (frameUpper_List[i] != null)
                        customPlanes.Add((frameUpper_List[i], System.Drawing.Color.Black));

            //DesignLine
            if (designLine_List != null && designLine_List.Count > 0)
                for (int i = 0; i < designLine_List.Count; i++)
                    if (designLine_List[i] != null)
                        //convert to Line
                        if (designLine_List[i].GetType() == typeof(Line))
                            args.Display.DrawLine((Line)designLine_List[i], System.Drawing.Color.MediumVioletRed);
                        //convert to Polyline
                        else if (designLine_List[i].GetType() == typeof(Polyline))
                            args.Display.DrawPolyline((Polyline)designLine_List[i], System.Drawing.Color.MediumVioletRed);

            //Box EdgeLines
            if (geometryBox_List != null && geometryBox_List.Count > 0)
                for (int i = 0; i < geometryBox_List.Count; i++)
                    if (geometryBox_List[i] != null)
                        args.Display.DrawLine(geometryBox_List[i], System.Drawing.Color.MediumVioletRed);

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
                return Resources.FabFramework_DeconstructDesignElement;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("464f94ca-07ce-4e78-82d9-1b7206b23427"); }
        }
    }
}
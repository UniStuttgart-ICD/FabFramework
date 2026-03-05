using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fab.Core.FabUtilities
{
    public class FabUtilities
    {

        public static Line OrientLine(Line line, Plane basePlane, Plane targetPlane)
        {
            Line lineCopy = new Line(line.PointAt(0.0), line.PointAt(1.0));
            Transform xFormOrient = Transform.PlaneToPlane(basePlane, targetPlane);
            lineCopy.Transform(xFormOrient);

            return lineCopy;
        }

        public static Plane ChangePlaneBasis(Plane transformPlane, Plane basePlane, Plane targetPlane)
        {
            Plane transformPlaneCopy = transformPlane.Clone();
            Transform xFormOrient = Transform.ChangeBasis(basePlane, targetPlane);
            transformPlaneCopy.Transform(xFormOrient);

            return transformPlaneCopy;
        }

        public static Brep ChangeBrepBasis(Brep brep, Plane basePlane, Plane targetPlane)
        {
            Brep brepCopy = brep.DuplicateBrep();
            Transform xFormOrient = Transform.ChangeBasis(basePlane, targetPlane);
            brepCopy.Transform(xFormOrient);

            return brepCopy;
        }
        public static Plane OrientPlane(Plane transformPlane, Plane basePlane, Plane targetPlane)
        {
            Plane transformPlaneCopy = transformPlane.Clone();
            Transform xFormOrient = Transform.PlaneToPlane(basePlane, targetPlane);
            transformPlaneCopy.Transform(xFormOrient);

            return transformPlaneCopy;
        }
        public static GeometryBase OrientGeometryBase(GeometryBase geometryBase, Plane basePlane, Plane targetPlane)
        {
            GeometryBase geometryBaseCopy = geometryBase.Duplicate();
            Transform xFormOrient = Transform.PlaneToPlane(basePlane, targetPlane);
            geometryBaseCopy.Transform(xFormOrient);

            return geometryBaseCopy;
        }

        public static List<GeometryBase> OrientGeometryBaseList(List<GeometryBase> geometryBases, Plane basePlane, Plane targetPlane)
        {
            var orientedGeometries = new List<GeometryBase>();

            foreach (var geometryBase in geometryBases)
            {
                GeometryBase orientedGeometry = OrientGeometryBase(geometryBase, basePlane, targetPlane);
                orientedGeometries.Add(orientedGeometry);
            }

            return orientedGeometries;
        }

        public static Brep OrientBrep(Brep brep, Plane basePlane, Plane targetPlane)
        {
            Brep brepCopy = brep.DuplicateBrep();
            Transform xFormOrient = Transform.PlaneToPlane(basePlane, targetPlane);
            brepCopy.Transform(xFormOrient);

            return brepCopy;
        }

        public static Box CreateAlignedBox(Brep plate, Plane plane)
        {
            if (plate == null || !plate.IsValid || plate.Faces.Count == 0)
            {
                // Return an empty box (Unset) if the input Brep is null, invalid, or has no faces.
                return Box.Unset;
            }

            plate.GetBoundingBox(plane, out Box worldBox);


            //add check to see if the box is valid, throw error else
            if (!worldBox.IsValid)
            {
                throw new Exception("The box is not valid.");
            }

            return worldBox;
        }

        public static Box CreateBoxFromBreps(List<Brep> geoList)
        {
            if (geoList == null || geoList.Count == 0)
            {
                // Handle the case where the input list is null or empty
                throw new ArgumentException("geoList cannot be null or empty.");
            }

            BoundingBox localUnionBBox = BoundingBox.Empty;

            foreach (Brep brep in geoList)
            {
                if (brep == null)
                {
                    // Handle the case where a Brep in the list is null
                    continue; // Skip this Brep and continue with the next one
                }

                // Get the bounding box of the BRep in its own local coordinates
                BoundingBox localBox = brep.GetBoundingBox(Plane.WorldXY); // Use WorldXY plane

                if (localBox.IsValid)
                {
                    // Extend the overall bounding box to include the local box
                    localUnionBBox.Union(localBox);
                }
            }

            if (!localUnionBBox.IsValid)
            {
                // Handle the case where no valid bounding boxes were found
                return Box.Empty;
            }

            // Convert BoundingBox to Box
            //(BoundingBox can not be transformed properly! --> Will create a new BoundingBox, which is always aligned to WorldXY)
            Box localUnionBox = new Box(localUnionBBox);

            return localUnionBox;
        }


        public static Box CreateAlignedBoxFromBreps(List<Brep> geoList, Plane plane)
        {
            if (geoList == null || geoList.Count == 0)
            {
                // Handle the case where the input list is null or empty
                throw new ArgumentException("geoList cannot be null or empty.");
            }

            if (plane == null)
            {
                // Handle the case where the input plane is null
                throw new ArgumentNullException(nameof(plane), "plane cannot be null.");
            }

            BoundingBox localUnionBBox = BoundingBox.Empty;

            foreach (Brep brep in geoList)
            {
                if (brep == null)
                {
                    // Handle the case where a Brep in the list is null
                    continue; // Skip this Brep and continue with the next one
                }

                // Get the bounding box of the BRep in its own local coordinates
                BoundingBox localBox = brep.GetBoundingBox(plane);

                if (localBox.IsValid)
                {
                    // Extend the overall bounding box to include the local box
                    localUnionBBox.Union(localBox);
                }
            }

            if (!localUnionBBox.IsValid)
            {
                // Handle the case where no valid bounding boxes were found
                return Box.Empty;
            }

            // Convert BoundingBox to Box
            //(BoundingBox can not be transformed properly! --> Will create a new BoundingBox, which is always aligned to WorldXY)
            Box localUnionBox = new Box(localUnionBBox);

            // Transform the local box to the desired plane
            Transform transform = Transform.PlaneToPlane(Plane.WorldXY, plane);
            localUnionBox.Transform(transform);

            return localUnionBox;
        }

        public static BoundingBox ToBoundingBox(Box box)
        {
            BoundingBox boundingBox = new BoundingBox(
                new Point3d(box.X.Min, box.Y.Min, box.Z.Min),
                new Point3d(box.X.Max, box.Y.Max, box.Z.Max)
            );

            return boundingBox;
        }

        public static Plane GetBBoxBaseCentrePlane(Box boundingBox, Plane refPlane, bool projectToRefPln)
        {
            // Step 1: Deconstruct the bounding box to Surfaces with matching normals to refPlane
            List<BrepFace> matchingSurfaces = FabUtilities.SelectSurfacesWithMatchingNormal(boundingBox, refPlane);

            // Step 2: Convert BrepFace List to surface
            List<Surface> surfaces = new List<Surface>();
            for (int i = 0; i < matchingSurfaces.Count; i++)
            {
                surfaces.Add(matchingSurfaces[i].ToNurbsSurface());
            }
            Surface closestSurface = surfaces[0];

            // Step 3: Get the vertices of the closest surface.
            Brep brep = closestSurface.ToBrep();
            BrepVertexList brepVertices = brep.Vertices;
            List<Point3d> cornerPoints = new List<Point3d>();
            for (int i = 0; i < brepVertices.Count; i++)
            {
                BrepVertex vertex = brepVertices[i];
                cornerPoints.Add(vertex.Location);
            }

            // Step 4: Calculate the center point of the surface.
            Point3d centerPoint = FabUtilities.GetCenterPoint(cornerPoints);

            // Step 5: Create a new plane at the center point.
            Plane centerPlane = refPlane.Clone();

            // Step 6: Optionally project the center point to the reference plane.
            if (projectToRefPln == true)
            {
                centerPoint = FabUtilities.ProjectPointToPlane(centerPoint, centerPlane);
            }

            // Step 7: Set the plane's origin to the center point.
            centerPlane.Origin = centerPoint;

            // Step 8: Return the resulting plane.
            return centerPlane;
        }


        public static Plane GetBBoxBaseCentrePlaneOld(Box boundingBox, Plane refPlane, bool projectToRefPln)
        {
            // Step 1: Deconstruct the bounding box to get all surfaces.
            Brep boundingBrep = boundingBox.ToBrep();
            BrepFaceList boundingSurfaces = boundingBrep.Faces;

            List<Surface> surfaces = new List<Surface>();
            for (int i = 0; i < boundingSurfaces.Count; i++)
            {
                surfaces.Add(boundingSurfaces[i].ToNurbsSurface());
            }

            if (surfaces.Count == 0)
                throw new ArgumentException("Bounding box does not have any surfaces.");

            // Step 2: Find the surface with the closest distance to the platePlane's origin.
            double minDistance = double.MaxValue;
            Surface closestSurface = surfaces[0];
            foreach (Surface surface in surfaces)
            {
                Point3d surfaceCenter = surface.PointAt(0.5, 0.5);
                double distance = surfaceCenter.DistanceTo(refPlane.Origin);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSurface = surface;
                }
            }

            // Step 3: Get the vertices of the closest surface.
            Brep brep = closestSurface.ToBrep();
            BrepVertexList brepVertices = brep.Vertices;
            List<Point3d> cornerPoints = new List<Point3d>();
            for (int i = 0; i < brepVertices.Count; i++)
            {
                BrepVertex vertex = brepVertices[i];
                cornerPoints.Add(vertex.Location);
            }

            // Step 4: Calculate the center point of the surface.
            Point3d centerPoint = FabUtilities.GetCenterPoint(cornerPoints);

            // Step 5: Create a new plane at the center point.
            Plane centerPlane = refPlane.Clone();

            // Step 6: Optionally project the center point to the reference plane.
            if (projectToRefPln == true)
            {
                centerPoint = FabUtilities.ProjectPointToPlane(centerPoint, centerPlane);
            }

            // Step 7: Set the plane's origin to the center point.
            centerPlane.Origin = centerPoint;

            // Step 8: Return the resulting plane.
            return centerPlane;
        }

        public static Rectangle3d GetBoundingRectangle(Polyline poly, Plane frame, out double length, out double width)
        {
            if (!poly.IsValid)
            {
                length = -1;
                width = -1;
                return Rectangle3d.Unset;
            }

            PolylineCurve plate = poly.ToPolylineCurve();

            // Step 1: get the convex hull of the polygon in 2D
            Grasshopper.Kernel.Geometry.Node2List nodes = new Grasshopper.Kernel.Geometry.Node2List();
            double s, t = 0.0;
            foreach (Point3d point in poly)
            {
                frame.ClosestParameter(point, out s, out t);
                nodes.Append(new Grasshopper.Kernel.Geometry.Node2(s, t));
            }
            Polyline hull = Grasshopper.Kernel.Geometry.ConvexHull.Solver.ComputeHull(nodes);

            // Step2: check antipodal vertex-edge pairs
            Line[] lines = hull.GetSegments();
            List<double> maxDist = new List<double>();
            foreach (Line line in lines)
            {
                // measure distance to all the other points
                List<double> dist = new List<double>();
                foreach (Point3d point in hull)
                    dist.Add(line.DistanceTo(point, false));
                dist.Sort();
                // add the maximum distance to the list
                maxDist.Add(dist.Last());
            }

            // Step 3: sort the values to find the line L, which is parallel to the smallest width
            double[] maxDistArray = maxDist.ToArray();
            Array.Sort(maxDistArray, lines);

            // Step 4: create a coordinate system CS on the start point of the base line
            Line baseLine = lines[0];
            Point3d origin = baseLine.From;
            Vector3d xDirection = baseLine.Direction;
            Vector3d yDirection = Vector3d.CrossProduct(xDirection, -Vector3d.ZAxis);
            Plane coordinateSystem = new Plane(origin, xDirection, yDirection);

            // Step 5: Evaluate all the points of the hull on CS to find min and max values of X
            List<double> xList = new List<double>();
            foreach (Point3d point in hull)
            {
                coordinateSystem.ClosestParameter(point, out s, out t);
                xList.Add(s);
            }
            xList.Sort();
            double minX = xList.First();
            double maxX = xList.Last();

            // Step 6: create a rectangle R on CS X and Y interval
            Interval widthInterval = new Interval(minX, maxX);
            Interval heightInterval = new Interval(0, maxDistArray[0]);
            Rectangle3d rect = new Rectangle3d(coordinateSystem, widthInterval, heightInterval);

            // Step 7a: reorient R from WorldXY to F
            Transform xform = Transform.PlaneToPlane(Plane.WorldXY, frame);
            rect.Transform(xform);

            // Step 7b: reorient CS from WorldXY to F
            coordinateSystem.Transform(xform);

            width = rect.Width;
            length = rect.Height;
            return rect;

        }

        public static Plane GetBoxBottomLeftCornerPlane(Box box)
        {
            Point3d cornerPoint = box.PointAt(0.0, 0.0, 0.0);
            Plane cornerPlane = box.Plane;
            cornerPlane.Origin = cornerPoint;

            return cornerPlane;
        }

        public static Plane GetBoxCenterPlane(Box box)
        {
            Point3d cornerPoint = box.PointAt(0.5, 0.5, 0.0);
            Plane cornerPlane = box.Plane;
            cornerPlane.Origin = cornerPoint;

            return cornerPlane;
        }

        public static Plane GetBBoxBasePlane(Box alignedBoundingBox, Plane platePlane, bool projectToRefPln)
        {

            List<double> distances = new List<double>();

            Point3d plnOrigin = platePlane.Origin;
            Point3d[] corners = alignedBoundingBox.GetCorners();


            foreach (Point3d corner in corners)
            {
                double dist = plnOrigin.DistanceTo(corner);
                distances.Add(dist);
            }

            double[] distancesArr = distances.ToArray();

            System.Array.Sort(distancesArr, corners);

            plnOrigin = corners[0];

            Plane newBasePlane = platePlane.Clone();

            if (projectToRefPln == true)
            {
                plnOrigin = FabUtilities.ProjectPointToPlane(plnOrigin, platePlane);

            }

            newBasePlane.Origin = plnOrigin;

            return newBasePlane;

            /***
            double bBoxHeight = alignedBoundingBox.Z.T0;
            Vector3d zAxis = platePlane.ZAxis;
            zAxis.Unitize();
            Vector3d newZ = zAxis * bBoxHeight;

            plnOrigin += newZ;


            //---check mechanism---//
            Point3d checkPt = alignedBoundingBox.PointAt(0, 0, 0);
            //Point3d[] checkCorners = bBox.GetCorners();
            //double checkDist = checkCorners[0].DistanceTo(plnOrigin);
            double checkDist = checkPt.DistanceTo(plnOrigin);

            if (checkDist > 0.1)
            {
                GH_RuntimeMessage toleranceWarn = new GH_RuntimeMessage("Watch out: tolerance treshold of 0.1 mm in plane reorientatiin is exceeded. Change smthg in --> REPOSITION BASE PLANE TO bBOX CORNER <---", GH_RuntimeMessageLevel.Warning);
            }

            alignedBoundingBox.RepositionBasePlane(plnOrigin);
            Plane newBasePlane = alignedBoundingBox.Plane;

            return newBasePlane;
            ***/
        }

        public static void GetFaceEdges(Brep plate, BrepFace brepFace, ref List<Curve> outEdges)
        {
            BrepEdgeList allEdges = plate.Edges;
            List<Curve> selectedEdges = new List<Curve>();

            int[] edgeIndices = brepFace.AdjacentEdges();

            for (int i = 0; i < edgeIndices.Length; i++)
            {
                Curve edge = allEdges[edgeIndices[i]];
                selectedEdges.Add(edge);
            }

            outEdges = selectedEdges;

        }

        public static BrepFace GetUpperFace(Brep plate, Plane refPln)
        {
            List<BrepFace> upperFace = new List<BrepFace>(); // info: just a single face is stored in this list; necessary workaround because a brepFace cannot be declared / constructed independetly;
            Vector3d normal = new Vector3d();

            BrepFaceList faces = plate.Faces;
            foreach (BrepFace f in faces)
            {
                List<Point3d> checkPts = new List<Point3d>();
                Interval dom = new Interval(0, 1);
                f.SetDomain(0, dom);
                f.SetDomain(1, dom);

                normal = f.NormalAt(0.5, 0.5);

                Vector3d zVecMasterPln = refPln.ZAxis;

                double vecAngle = Vector3d.VectorAngle(normal, zVecMasterPln);
                //Print(Convert.ToString(vecAngle));
                if (vecAngle <= 0.01)
                {
                    upperFace.Add(f);
                }
            }
            return upperFace[0];
        }

        public static BrepFace GetLowerFace(Brep plate, Plane refPln)
        {
            List<BrepFace> lowerFace = new List<BrepFace>(); // info: just a single face is stored in this list; necessary workaround because a brepFace cannot be declared / constructed independetly;
            Vector3d normal = new Vector3d();

            BrepFaceList faces = plate.Faces;
            foreach (BrepFace f in faces)
            {
                List<Point3d> checkPts = new List<Point3d>();
                Interval dom = new Interval(0, 1);
                f.SetDomain(0, dom);
                f.SetDomain(1, dom);

                normal = f.NormalAt(0.5, 0.5);

                Vector3d zVecMasterPln = refPln.ZAxis;
                zVecMasterPln *= -1;

                double vecAngle = Vector3d.VectorAngle(normal, zVecMasterPln);
                //Print(Convert.ToString(vecAngle));
                if (vecAngle <= 0.01)
                {
                    lowerFace.Add(f);
                }
            }
            return lowerFace[0];
        }

        public static Point3d GetCenterOfGravity(Brep plate, Plane refPln, ref Curve OuterCrv, ref Curve InnerCrv, ref int CrvCount)
        {
            List<BrepFace> facesList = new List<BrepFace>();
            List<double> zVals = new List<double>();

            BrepFaceList faces = plate.Faces;

            BrepFace highestFace = FabUtilities.GetUpperFace(plate, refPln);
            /*
            foreach (BrepFace f in faces)
            {
                Point3d faceCen = AreaMassProperties.Compute(f).Centroid;
                double zVal = faceCen.Z;
                zVals.Add(zVal);
                facesList.Add(f);
            }

            BrepFace[] facesArr = facesList.ToArray();
            double[] zValsArr = zVals.ToArray();

            System.Array.Sort(zValsArr, facesArr);

            BrepFace highestFace = facesArr[facesArr.Length - 1];
            */
            // sort curves

            List<Curve> faceEdges = new List<Curve>();
            FabUtilities.GetFaceEdges(plate, highestFace, ref faceEdges); //hint: embedded custom funtion

            Curve[] edgeCrvsArr = Curve.JoinCurves(faceEdges);

            List<double> crvsLen = new List<double>();

            foreach (Curve c in edgeCrvsArr)
            {
                double len = c.GetLength();
                crvsLen.Add(len);
            }

            double[] crvsLenArr = crvsLen.ToArray();

            System.Array.Sort(crvsLenArr, edgeCrvsArr);

            // get outer and inner edge curve
            int crvCount = 0;
            Curve innerCrv = new LineCurve();
            Curve outerCrv = new LineCurve();
            Point3d cog = new Point3d();

            if (edgeCrvsArr.Length == 2)
            {
                crvCount = 2;

                innerCrv = edgeCrvsArr[0];
                outerCrv = edgeCrvsArr[edgeCrvsArr.Length - 1];

                // get curve areas
                double areaInnerCrv = AreaMassProperties.Compute(innerCrv).Area;
                double areaOuterCrv = AreaMassProperties.Compute(outerCrv).Area;

                // get curve centers
                Point3d cenInnerCrv = AreaMassProperties.Compute(innerCrv).Centroid;
                Point3d cenOuterCrv = AreaMassProperties.Compute(outerCrv).Centroid;

                // calculate center of gravity
                double xCOG = (cenOuterCrv.X * areaOuterCrv - cenInnerCrv.X * areaInnerCrv) / (areaOuterCrv - areaInnerCrv);
                double yCOG = (cenOuterCrv.Y * areaOuterCrv - cenInnerCrv.Y * areaInnerCrv) / (areaOuterCrv - areaInnerCrv);
                double zCOG = AreaMassProperties.Compute(outerCrv).Centroid.Z;

                cog = new Point3d(xCOG, yCOG, zCOG);

            }
            else if (edgeCrvsArr.Length == 1)
            {
                crvCount = 1;

                innerCrv = null;
                outerCrv = edgeCrvsArr[0];

                // get curve areas
                double areaOuterCrv = AreaMassProperties.Compute(outerCrv).Area;

                // calculate center of gravity
                cog = AreaMassProperties.Compute(outerCrv).Centroid;
            }
            else
            {
                //Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in curve sorting --> outer crv & inner crv");
            }


            OuterCrv = outerCrv;
            InnerCrv = innerCrv;
            CrvCount = crvCount;
            return cog;
        }

        public static Point3d ProjectPointToPlane(Point3d point, Plane targetPlane)
        {

            Transform transformProject = Transform.PlanarProjection(targetPlane);
            point.Transform(transformProject);

            return point;

        }

        public static Plane AdjustTargetPlane(Plane originalTargetPlane, double stackHeight)
        {
            Plane targetPlane = originalTargetPlane.Clone();
            Vector3d stackZVec = targetPlane.ZAxis;
            stackZVec.Unitize();
            double zLen = stackHeight;
            stackZVec *= zLen;

            targetPlane.Origin += stackZVec;

            return targetPlane;
        }

        public static Point3d GetCurveCenterPoint(Curve curve)
        {
            if (curve == null)
                throw new ArgumentException("The curve is null.", nameof(curve));

            Polyline estimatedPolyline = new Polyline();
            curve.TryGetPolyline(out estimatedPolyline);
            Point3d centerPoint = estimatedPolyline.CenterPoint();

            return centerPoint;
        }
        public static Brep MergeBreps(List<Brep> geometries, double tolerance = 0.001)
        {
            if (geometries == null || geometries.Count == 0)
            {
                return null;
            }

            Brep mergedBrep = geometries[0].DuplicateBrep();

            for (int i = 1; i < geometries.Count; i++)
            {
                mergedBrep = Brep.MergeBreps(new List<Brep> { mergedBrep, geometries[i] }, tolerance);
            }

            return mergedBrep;
        }

        public static BoundingBox CreateBoundingBox(List<GeometryBase> geometries)
        {
            BoundingBox bbox = BoundingBox.Empty;

            foreach (GeometryBase geometry in geometries)
            {
                BoundingBox geometryBBox = geometry.GetBoundingBox(false);
                bbox.Union(geometryBBox);
            }

            return bbox;
        }

        public static Mesh ConvertToJoinedMesh(object input)
        {
            List<Mesh> meshes = new List<Mesh>();

            if (input is Brep singleBrep)
            {
                Mesh[] brepMeshes = Mesh.CreateFromBrep(singleBrep, MeshingParameters.FastRenderMesh);
                meshes.AddRange(brepMeshes);
            }
            else if (input is IEnumerable<Brep> brepList)
            {
                foreach (Brep brep in brepList)
                {
                    Mesh[] brepMeshes = Mesh.CreateFromBrep(brep, MeshingParameters.FastRenderMesh);
                    meshes.AddRange(brepMeshes);
                }
            }
            else if (input is GeometryBase geometry)
            {
                if (geometry is Brep brepGeom)
                {
                    Mesh[] brepMeshes = Mesh.CreateFromBrep(brepGeom, MeshingParameters.FastRenderMesh);
                    meshes.AddRange(brepMeshes);
                }
                else if (geometry is Mesh meshGeom)
                {
                    meshes.Add(meshGeom);
                }
            }
            else if (input is IEnumerable<GeometryBase> geometryList)
            {
                foreach (GeometryBase geom in geometryList)
                {
                    if (geom is Brep brepGeom)
                    {
                        Mesh[] brepMeshes = Mesh.CreateFromBrep(brepGeom, MeshingParameters.FastRenderMesh);
                        meshes.AddRange(brepMeshes);
                    }
                    else if (geom is Mesh meshGeom)
                    {
                        meshes.Add(meshGeom);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Input must be a Brep, a list of Breps, or a GeometryBase object.");
            }

            Mesh joinedMesh = new Mesh();
            foreach (Mesh mesh in meshes)
            {
                joinedMesh.Append(mesh);
            }

            return joinedMesh;
        }

        public static GeometryBase ConvertToJoinedGeometry(object input)
        {
            if (input is Brep singleBrep)
            {
                Mesh[] brepMeshes = Mesh.CreateFromBrep(singleBrep, MeshingParameters.FastRenderMesh);
                Mesh joinedMesh = new Mesh();
                foreach (Mesh mesh in brepMeshes)
                {
                    joinedMesh.Append(mesh);
                }
                return joinedMesh;
            }
            else if (input is IEnumerable<Brep> brepList)
            {
                Mesh joinedMesh = new Mesh();
                foreach (Brep brep in brepList)
                {
                    Mesh[] brepMeshes = Mesh.CreateFromBrep(brep, MeshingParameters.FastRenderMesh);
                    foreach (Mesh mesh in brepMeshes)
                    {
                        joinedMesh.Append(mesh);
                    }
                }
                return joinedMesh;
            }
            else if (input is GeometryBase geometry)
            {
                if (geometry is Brep brepGeom)
                {
                    Mesh[] brepMeshes = Mesh.CreateFromBrep(brepGeom, MeshingParameters.FastRenderMesh);
                    Mesh joinedMesh = new Mesh();
                    foreach (Mesh mesh in brepMeshes)
                    {
                        joinedMesh.Append(mesh);
                    }
                    return joinedMesh;
                }
                else if (geometry is Mesh meshGeom)
                {
                    return meshGeom;
                }
                else
                {
                    return geometry;
                }
            }
            else if (input is IEnumerable<GeometryBase> geometryList)
            {
                Mesh joinedMesh = new Mesh();
                foreach (GeometryBase geom in geometryList)
                {
                    if (geom is Brep brepGeom)
                    {
                        Mesh[] brepMeshes = Mesh.CreateFromBrep(brepGeom, MeshingParameters.FastRenderMesh);
                        foreach (Mesh mesh in brepMeshes)
                        {
                            joinedMesh.Append(mesh);
                        }
                    }
                    else if (geom is Mesh meshGeom)
                    {
                        return meshGeom;
                    }
                    else
                    {
                        return geom;
                    }
                }
                return joinedMesh;
            }
            else
            {
                throw new ArgumentException("Input must be a Brep, a list of Breps, or a GeometryBase object.");
            }
        }


        public static List<GeometryBase> ConvertToJoinedGeometryToList(object input)
        {
            List<GeometryBase> convertedGeometry = new List<GeometryBase>();

            if (input is Brep singleBrep)
            {
                Mesh[] brepMeshes = Mesh.CreateFromBrep(singleBrep, MeshingParameters.FastRenderMesh);
                Mesh joinedMesh = new Mesh();
                foreach (Mesh mesh in brepMeshes)
                {
                    joinedMesh.Append(mesh);
                }
                convertedGeometry.Add(joinedMesh);
            }
            else if (input is IEnumerable<Brep> brepList)
            {
                foreach (Brep brep in brepList)
                {
                    Mesh[] brepMeshes = Mesh.CreateFromBrep(brep, MeshingParameters.FastRenderMesh);
                    Mesh joinedMesh = new Mesh();
                    foreach (Mesh mesh in brepMeshes)
                    {
                        joinedMesh.Append(mesh);
                    }
                    convertedGeometry.Add(joinedMesh);
                }
            }
            else if (input is GeometryBase geometry)
            {
                if (geometry is Brep brepGeom)
                {
                    Mesh[] brepMeshes = Mesh.CreateFromBrep(brepGeom, MeshingParameters.FastRenderMesh);
                    Mesh joinedMesh = new Mesh();
                    foreach (Mesh mesh in brepMeshes)
                    {
                        joinedMesh.Append(mesh);
                    }
                    convertedGeometry.Add(joinedMesh);
                }
                else if (geometry is Mesh meshGeom)
                {
                    convertedGeometry.Add(meshGeom);
                }
                else
                {
                    convertedGeometry.Add(geometry);
                }
            }
            else if (input is IEnumerable<GeometryBase> geometryList)
            {
                foreach (GeometryBase geom in geometryList)
                {
                    if (geom is Brep brepGeom)
                    {
                        Mesh[] brepMeshes = Mesh.CreateFromBrep(brepGeom, MeshingParameters.FastRenderMesh);
                        Mesh joinedMesh = new Mesh();
                        foreach (Mesh mesh in brepMeshes)
                        {
                            joinedMesh.Append(mesh);
                        }
                        convertedGeometry.Add(joinedMesh);
                    }
                    else if (geom is Mesh meshGeom)
                    {
                        convertedGeometry.Add(meshGeom);
                    }
                    else
                    {
                        convertedGeometry.Add(geom);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Input must be a Brep, a list of Breps, or a GeometryBase object.");
            }

            return convertedGeometry;
        }

        public static List<GeometryBase> ConvertToGeometry(object input)
        {
            List<GeometryBase> convertedGeometry = new List<GeometryBase>();

            if (input is Brep singleBrep)
            {
                Mesh[] meshes = Mesh.CreateFromBrep(singleBrep, MeshingParameters.FastRenderMesh);
                convertedGeometry.AddRange(meshes);
            }
            else if (input is IEnumerable<Brep> brepList)
            {
                foreach (Brep brep in brepList)
                {
                    Mesh[] meshes = Mesh.CreateFromBrep(brep, MeshingParameters.FastRenderMesh);
                    convertedGeometry.AddRange(meshes);
                }
            }
            else if (input is GeometryBase geometry)
            {
                convertedGeometry.Add(geometry);
            }
            else if (input is IEnumerable<GeometryBase> geometryList)
            {
                foreach (GeometryBase geom in geometryList)
                {
                    if (geom is Brep brepGeom)
                    {
                        Mesh[] meshes = Mesh.CreateFromBrep(brepGeom, MeshingParameters.FastRenderMesh);
                        convertedGeometry.AddRange(meshes);
                    }
                    else
                    {
                        convertedGeometry.Add(geom);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Input must be a Brep, a list of Breps, or a GeometryBase object.");
            }

            return convertedGeometry;
        }

        public static Point3d FindMidPoint(List<Point3d> points)
        {
            if (points == null || points.Count == 0)
                return Point3d.Unset;

            // Initialize min and max values
            Point3d minPoint = new Point3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            Point3d maxPoint = new Point3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

            // Iterate through points to find min and max values
            foreach (Point3d point in points)
            {
                minPoint = new Point3d(Math.Min(minPoint.X, point.X), Math.Min(minPoint.Y, point.Y), Math.Min(minPoint.Z, point.Z));
                maxPoint = new Point3d(Math.Max(maxPoint.X, point.X), Math.Max(maxPoint.Y, point.Y), Math.Max(maxPoint.Z, point.Z));
            }

            // Calculate the midpoint
            Point3d midPoint = new Point3d((minPoint.X + maxPoint.X) / 2, (minPoint.Y + maxPoint.Y) / 2, (minPoint.Z + maxPoint.Z) / 2);

            return midPoint;
        }

        public static double GetLinAxisRadiusBasedList(List<Plane> frames, Line linAxis, bool sortBiggest = true, double maxDistance = 2700, Vector3d toolOffsetVector = new Vector3d())
        {
            List<Point3d> origins = new List<Point3d>();
            foreach (Plane frame in frames)
            {
                origins.Add(frame.Origin);
            }

            Point3d midPoint = FindMidPoint(origins);
            Plane midPlane = frames[0].Clone();
            midPlane.Origin = midPoint;

            return GetLinAxisRadiusBased(midPlane, linAxis, sortBiggest, maxDistance, toolOffsetVector);
        }


        public static double GetLinAxisRadiusBased(Plane frame, Line linAxis, bool sortBiggest = true, double maxDistance = 2000, Vector3d toolOffsetVector = new Vector3d())
        {
            // Convert Line to Curve
            Polyline polyAxis = new Polyline(new Point3d[] { linAxis.From, linAxis.To });
            Curve crvAxis = polyAxis.ToNurbsCurve();

            // Create a plane with X-axis aligned to the Line's direction, projected onto the XY plane
            Vector3d x_axis = new Vector3d(linAxis.To.X - linAxis.From.X, linAxis.To.Y - linAxis.From.Y, 0);
            x_axis.Unitize();

            // Compute the Y-axis as orthogonal to the X-axis and lying in the XY plane
            Vector3d y_axis;
            if (x_axis.X != 0 || x_axis.Y != 0)
            {
                y_axis = new Vector3d(-x_axis.Y, x_axis.X, 0);
            }
            else
            {
                y_axis = new Vector3d(0, 1, 0);
            }
            y_axis.Unitize();

            // Create the plane
            Plane linePlane = new Plane(linAxis.From, x_axis, y_axis);

            // Move Plane according to toolOffset
            Plane frameCopy = frame;
            frameCopy.Origin += toolOffsetVector;

            // Project the frame Origin onto the plane
            Point3d projected_frame_origin = linePlane.ClosestPoint(frameCopy.Origin);

            // Find the closest point on the curve to the projected frame origin
            double tClosestPoint;
            Point3d closestPoint;
            crvAxis.ClosestPoint(projected_frame_origin, out tClosestPoint);

            closestPoint = crvAxis.PointAt(tClosestPoint);

            // Check for Distance to linear axis
            double axisDis = closestPoint.DistanceTo(projected_frame_origin);
            if (axisDis <= maxDistance)
            {

                // Calculate radius distance here
                Circle testCircle = new Circle(projected_frame_origin, maxDistance);

                double intT1, intT2;
                Point3d intPoint1, intPoint2;

                Rhino.Geometry.Intersect.Intersection.LineCircle(linAxis, testCircle, out intT1, out intPoint1, out intT2, out intPoint2);
                List<Point3d> intersectionPoints = new List<Point3d> { intPoint1, intPoint2 };
                List<double> intersectionsT = new List<double> { intT1, intT2 };


                if (intersectionPoints.Count != 0)
                {
                    // Sort for biggest value
                    intersectionsT.Sort();

                    // Check if inside boundaries
                    List<double> checkedIntersectionsT = new List<double>();
                    double averageT_Diff = intersectionsT[1] - (intersectionsT[0] + intersectionsT[1]) / 2;

                    for (int i = 0; i < intersectionsT.Count; i++)
                    {
                        if (intersectionsT[i] <= (1.00 + averageT_Diff) && intersectionsT[i] >= (0.00 - averageT_Diff))
                        {
                            checkedIntersectionsT.Add(intersectionsT[i]);
                        }
                    }

                    // Sort either for biggest or smallest linearAxis Value (move robot to ride or left side)
                    double checkedIntersection;
                    if (sortBiggest)
                    {
                        checkedIntersection = checkedIntersectionsT[checkedIntersectionsT.Count - 1];
                    }
                    else
                    {
                        checkedIntersection = checkedIntersectionsT[0];
                    }

                    checkedIntersection *= crvAxis.GetLength();
                    tClosestPoint = checkedIntersection / crvAxis.GetLength();
                }
            }

            // Safety Check to not exceed linear Axis domain
            if (tClosestPoint >= 1.0)
            {
                tClosestPoint = 1.0;
            }
            if (tClosestPoint <= 0.00)
            {
                tClosestPoint = 0.00;
            }


            Point3d new_closestPoint = crvAxis.PointAt(tClosestPoint);

            // Final linear axis value
            double linAxisValue = tClosestPoint * crvAxis.GetLength();

            linAxisValue -= 600;

            return linAxisValue;
        }


        public static double RadianToDegree(double rad)
        {
            double deg = 180 * rad / Math.PI;
            return deg;
        }

        public static double DegreeToRadian(double angle)
        {
            double rad = (Math.PI * angle) / 180;
            return rad;
        }
        public static void ShiftRight<T>(List<T> lst, int shifts)
        {
            for (int i = lst.Count - shifts - 1; i >= 0; i--)
            {
                lst[i + shifts] = lst[i];
            }

            for (int i = 0; i < shifts; i++)
            {
                lst[i] = default(T);
            }
        }

        public static Plane OffsetPlane(Plane pln, double offset, Vector3d direction)
        {
            Vector3d vector3d = new Vector3d(direction);
            vector3d.Unitize();
            Transform xform = Transform.Translation(vector3d * offset);
            Plane result = new Plane(pln);
            result.Transform(xform);

            return result;
        }

        public static Plane TransformPlaneByVector(Plane plane, Vector3d vector, double distance)
        {
            vector.Unitize();
            vector *= distance;
            plane.Transform(Transform.Translation(vector));

            return plane;
        }

        public static Point3d GetCenterPoint(List<Point3d> points)
        {
            int count = points.Count;

            if (count == 0)
                throw new ArgumentException("The list of points is empty.");

            double sumX = 0.0;
            double sumY = 0.0;
            double sumZ = 0.0;

            foreach (Point3d point in points)
            {
                sumX += point.X;
                sumY += point.Y;
                sumZ += point.Z;
            }

            double centerX = sumX / count;
            double centerY = sumY / count;
            double centerZ = sumZ / count;

            Point3d centerPoint = new Point3d(centerX, centerY, centerZ);
            return centerPoint;
        }

        public static List<BrepFace> SelectSurfacesWithMatchingNormal(Box boundingBox, Plane referencePlane)
        {
            // Create a box using the bounding box
            Brep boxBrep = boundingBox.ToBrep();

            // Get the face count of the box
            int faceCount = boxBrep.Faces.Count;

            // Create a list to store the surfaces with matching normals
            List<BrepFace> matchingSurfaces = new List<BrepFace>();

            // Loop through each face of the box
            for (int i = 0; i < faceCount; i++)
            {
                BrepFace face = boxBrep.Faces[i];

                // Get the normal of the face using the face's surface normal
                Vector3d faceNormal = face.NormalAt(0.5, 0.5);

                // Check if the face normal matches the reference plane normal
                if (faceNormal.IsParallelTo(-referencePlane.Normal, 0.01) == 1)
                {
                    // If the normals match, add the face to the list of matching surfaces
                    matchingSurfaces.Add(face);
                }
            }

            return matchingSurfaces;
        }

        public static Box GetMinimumBoundingBox3DFromBrep(Brep inputBrep)
        {
            List<BrepFace> brepFaces = new List<BrepFace>();
            foreach (BrepFace face in inputBrep.Faces)
            {
                brepFaces.Add(face);
            }

            List<Plane> planes = new List<Plane>();

            // Get all the possible planes
            foreach (BrepFace face in brepFaces)
            {
                // Extract the plane of each BrepFace
                Plane facePlane;
                if (face.TryGetPlane(out facePlane))
                {
                    planes.Add(facePlane);
                }
            }

            List<Box> orientedBoxes = new List<Box>();

            foreach (Plane pln in planes)
            {
                Box bb = new Box();
                inputBrep.GetBoundingBox(pln, out bb);

                if (bb.IsValid)
                {
                    orientedBoxes.Add(bb);
                }
            }

            if (orientedBoxes.Count == 0)
            {
                // Handle case where no valid bounding boxes were found
                throw new InvalidOperationException("No valid bounding boxes found.");
            }

            // Sort the bounding boxes by volume
            List<Box> SortedBoudningBoxes = orientedBoxes.OrderBy(o => o.Volume).ToList();

            //Check if SortedBoudningBoxes is valid

            // Return the smallest one
            return SortedBoudningBoxes[0];
        }

        public Plane GetPlaneFromBoundingBoxAlignedToWorldZNormal(Box minimumBoundingBox)
        {
            // Get the Brep representation of the bounding box
            Brep brep = minimumBoundingBox.ToBrep();
            BrepFaceList faces = brep.Faces;

            // World Z-axis unit vector
            Vector3d worldZ = new Vector3d(0, 0, 1);

            // Initialize variables to keep track of the best face
            BrepFace bestFace = null;
            double closestDot = double.MinValue;

            // Iterate over each face to find the one with the normal closest to the Z-axis
            foreach (BrepFace face in faces)
            {
                // Get the normal vector of the face at the midpoint of its domain
                Interval uDomain = face.Domain(0);
                Interval vDomain = face.Domain(1);
                Point2d mid = new Point2d(uDomain.Mid, vDomain.Mid);
                Vector3d faceNormal = face.NormalAt(mid.X, mid.Y);

                // Calculate the dot product with the world Z-axis
                double dot = Math.Abs(Vector3d.Multiply(faceNormal, worldZ));

                // Check if this face's normal is closer to the Z-axis
                if (dot > closestDot)
                {
                    closestDot = dot;
                    bestFace = face;
                }
            }

            // Get the plane of the best face
            Plane bestFitPlane;
            if (bestFace != null && bestFace.TryGetPlane(out bestFitPlane))
            {
                // Adjust the base frame to match this plane
                Plane baseFrame = bestFitPlane;
                baseFrame.Origin = minimumBoundingBox.Center;

                return baseFrame;
            }
            else
            {
                // If no best face was found or plane extraction failed, return the original plane
                return minimumBoundingBox.Plane;
            }
        }

        public static Plane AdjustBaseFrameToLongestEdgeAndClosestNormal(Box minimumBoundingBox)
        {
            // Get the Brep representation of the bounding box
            Brep brep = minimumBoundingBox.ToBrep();

            // Ensure the bounding box has exactly 12 edges
            if (brep.Edges.Count != 12)
            {
                throw new InvalidOperationException("Bounding box does not have exactly 12 edges.");
            }

            // Initialize lists to categorize edges based on their index order
            List<BrepEdge> xEdges = new List<BrepEdge>();
            List<BrepEdge> yEdges = new List<BrepEdge>();
            List<BrepEdge> zEdges = new List<BrepEdge>();

            // Categorize edges based on the specified order
            for (int i = 0; i < 12; i++)
            {
                if (i == 0 || i == 2 || i == 4 || i == 6)
                {
                    xEdges.Add(brep.Edges[i]);
                }
                else if (i == 1 || i == 3 || i == 5 || i == 7)
                {
                    yEdges.Add(brep.Edges[i]);
                }
                else
                {
                    zEdges.Add(brep.Edges[i]);
                }
            }

            // Ensure each list has exactly 4 edges
            if (xEdges.Count != 4 || yEdges.Count != 4 || zEdges.Count != 4)
            {
                throw new InvalidOperationException("Bounding box edges are not categorized correctly.");
            }

            // Determine which list has the longest edges by comparing the length of the first edge in each list
            double xEdgeLength = xEdges[0].GetLength();
            double yEdgeLength = yEdges[0].GetLength();
            double zEdgeLength = zEdges[0].GetLength();

            double longestEdgeLength = Math.Max(xEdgeLength, Math.Max(yEdgeLength, zEdgeLength));

            // Collect the edges that match the longest length category
            List<BrepEdge> matchingLongestEdges;
            string matchingLongestEdgesString = string.Empty;
            if (Math.Abs(longestEdgeLength - xEdgeLength) < Rhino.RhinoMath.ZeroTolerance)
            {
                matchingLongestEdges = xEdges;
                matchingLongestEdgesString = "xEdges";
            }
            else if (Math.Abs(longestEdgeLength - yEdgeLength) < Rhino.RhinoMath.ZeroTolerance)
            {
                matchingLongestEdges = yEdges;
                matchingLongestEdgesString = "yEdges";
            }
            else
            {
                matchingLongestEdges = zEdges;
                matchingLongestEdgesString = "zEdges";
            }


            //Throw error if matchingLongestEdges.Count != 4
            if (matchingLongestEdges.Count != 4)
            {
                throw new InvalidOperationException("Bounding box does not have exactly 4 longest edges.");
            }

            // If no edges were found, return the original base frame
            if (matchingLongestEdges.Count == 0)
            {
                return minimumBoundingBox.Plane;
            }


            // Use the first edge in the matching list to get the direction of the longest edge
            BrepEdge longestEdge = matchingLongestEdges[0];
            Vector3d edgeDirection = new Vector3d(longestEdge.PointAtEnd - longestEdge.PointAtStart);
            edgeDirection.Unitize();


            // Ensure edgeDirection aligns more closely with the world X-axis
            Vector3d worldX = new Vector3d(1, 0, 0);
            double dotX = Vector3d.Multiply(edgeDirection, worldX);

            // Handle case when edgeDirection is parallel to the world X-axis
            if (Math.Abs(dotX) < Rhino.RhinoMath.ZeroTolerance)
            {
                // Get the cross product of edgeDirection with worldX to determine the direction perpendicular to both
                Vector3d cross = Vector3d.CrossProduct(edgeDirection, worldX);

                // Check the direction of the cross product to determine alignment
                if (cross.Length > Rhino.RhinoMath.ZeroTolerance) // Non-zero cross product, edgeDirection is not parallel to worldX
                {
                    // In case of ambiguity, ensure consistency by choosing the direction with the positive cross product
                    edgeDirection = (cross.Z > 0) ? edgeDirection : -edgeDirection;
                }
                // If the cross product is zero, edgeDirection is parallel to worldX, and no correction is needed
            }
            else if (dotX < 0)
            {
                edgeDirection = -edgeDirection;
            }


            // World Z-axis unit vector
            Vector3d worldZ = new Vector3d(0, 0, 1);

            // Initialize variables to keep track of the best face normal
            BrepFace bestFace = null;
            double closestDot = double.MinValue;

            // Iterate over each face to find the one with the normal closest to the Z-axis
            foreach (BrepFace face in brep.Faces)
            {
                // Check if the face contains any of the matching longest edges
                bool containsMatchingEdge = false;
                foreach (BrepLoop loop in face.Loops)
                {
                    foreach (BrepTrim trim in loop.Trims)
                    {
                        if (matchingLongestEdges.Contains(trim.Edge))
                        {
                            containsMatchingEdge = true;
                            break;
                        }
                    }
                    if (containsMatchingEdge)
                    {
                        break;
                    }
                }

                // If the face does not contain any matching edges, skip it
                if (!containsMatchingEdge)
                {
                    continue;
                }

                // Get the normal vector of the face
                Interval uDomain = face.Domain(0);
                Interval vDomain = face.Domain(1);
                Point2d mid = new Point2d(uDomain.Mid, vDomain.Mid);
                Vector3d faceNormal = face.NormalAt(mid.X, mid.Y);

                // Calculate the dot product with the world Z-axis
                double dot = Vector3d.Multiply(faceNormal, worldZ);

                // Check if this face's normal is closer to the Z-axis
                if (dot > closestDot)
                {
                    closestDot = dot;
                    bestFace = face;
                }
            }

            // If no best face was found, return the original base frame
            if (bestFace == null)
            {
                return minimumBoundingBox.Plane;
            }

            // Get the normal of the best face
            Interval bestU = bestFace.Domain(0);
            Interval bestV = bestFace.Domain(1);
            Point2d bestMid = new Point2d(bestU.Mid, bestV.Mid);
            Vector3d bestNormal = bestFace.NormalAt(bestMid.X, bestMid.Y);
            bestNormal.Unitize();

            // Ensure the normal vector aligns more closely with the world Z-axis
            if (Vector3d.Multiply(bestNormal, worldZ) < 0)
            {
                bestNormal = -bestNormal;
            }

            // Compute the Y-axis of the new base frame
            Vector3d yAxis = Vector3d.CrossProduct(bestNormal, edgeDirection);
            yAxis.Unitize();

            // Recompute the X-axis to ensure orthogonality
            Vector3d xAxis = Vector3d.CrossProduct(yAxis, bestNormal);
            xAxis.Unitize();

            // Create a new base frame with the X-axis aligned to the longest edge and Z-axis aligned to the best face normal
            Plane baseFrame = new Plane(minimumBoundingBox.Center, xAxis, yAxis);
            //Plane baseFrame2 = new Plane(minimumBoundingBox.Center, baseFrame.XAxis, -baseFrame.ZAxis);


            return baseFrame;
        }






    }
}

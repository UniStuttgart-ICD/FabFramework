using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry.Collections;
using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Grasshopper.GUI;
using Rhino.Geometry.Intersect;
using Fab.Core.FabCollection;
using Fab.Core.DesignElement;
using System.Xml.Linq;
using System.Runtime.Remoting.Messaging;

namespace Fab.Core.FabUtilities
{
    public class FabUtilitiesElement
    {

        public static void GetFabComponents(FabComponent cassette, out FabPlate botPlate, out FabPlate topPlate, out List<FabBeam> beams)
        {
            if (cassette == null)
            {
                throw new ArgumentNullException(nameof(cassette), "The 'cassette' parameter cannot be null.");
            }

            var fabPlates = cassette.GetFabPlates();
            if (fabPlates.Count < 2)
            {
                throw new InvalidOperationException("The 'cassette' must have at least 2 FabPlates.");
            }

            botPlate = fabPlates[0];
            topPlate = fabPlates[1];
            beams = cassette.GetFabBeams();
        }

        public static void AdjustAllElementRefFabAndFabOutPlanes(FabComponent fabComponent, Plane newRefPln_Fab, double turnTable_startAngle)
        {
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

            fabComponent.RefPln_Fab = newRefPln_Fab;

            //Change RefPln_FabOut from Plates to ensure that they are all reachable with the robot
            Plane turnTable_Pln = fabComponent.EnvFab.RefPln[0];
            double angleDiff_fabPln = Vector3d.VectorAngle(fabComponent.RefPln_Fab.XAxis, turnTable_Pln.XAxis);
            Transform rotate_fabOutPln = Transform.Rotation(-angleDiff_fabPln, turnTable_Pln.ZAxis, turnTable_Pln.Origin);
            Plane newFabOutPlane = fabComponent.RefPln_Fab.Clone();

            newFabOutPlane.Transform(rotate_fabOutPln);
            fabComponent.RefPln_FabOut = newFabOutPlane;
            fabComponent.Angle_FabOut = turnTable_startAngle - FabUtilities.RadianToDegree(angleDiff_fabPln);

            if (fabComponent.FabPlatesName != null && fabComponent.FabPlatesName.Count > 0)
            {
                for (int i = 0; i < fabComponent.FabPlatesName.Count; i++)
                {
                    if (fabCollection.fabPlateCollection.ContainsKey(fabComponent.FabPlatesName[i]))
                    {
                        fabCollection.fabPlateCollection[fabComponent.FabPlatesName[i]].RefPln_Fab = FabUtilities.OrientPlane(fabCollection.fabPlateCollection[fabComponent.FabPlatesName[i]].RefPln_Situ, fabComponent.RefPln_Situ, fabComponent.RefPln_Fab);
                        fabCollection.fabPlateCollection[fabComponent.FabPlatesName[i]].RefPln_FabOut = FabUtilities.OrientPlane(fabCollection.fabPlateCollection[fabComponent.FabPlatesName[i]].RefPln_Situ, fabComponent.RefPln_Situ, fabComponent.RefPln_FabOut);
                        fabCollection.fabPlateCollection[fabComponent.FabPlatesName[i]].Angle_FabOut = fabComponent.Angle_FabOut;
                    }
                }
            }

            if (fabComponent.FabBeamsName != null && fabComponent.FabBeamsName.Count > 0)
            {
                for (int i = 0; i < fabComponent.FabBeamsName.Count; i++)
                {
                    if (fabCollection.fabBeamCollection.ContainsKey(fabComponent.FabBeamsName[i]))
                    {
                        fabCollection.fabBeamCollection[fabComponent.FabBeamsName[i]].RefPln_Fab = FabUtilities.OrientPlane(fabCollection.fabBeamCollection[fabComponent.FabBeamsName[i]].RefPln_Situ, fabComponent.RefPln_Situ, fabComponent.RefPln_Fab);
                        fabCollection.fabBeamCollection[fabComponent.FabBeamsName[i]].RefPln_FabOut = FabUtilities.OrientPlane(fabCollection.fabBeamCollection[fabComponent.FabBeamsName[i]].RefPln_Situ, fabComponent.RefPln_Situ, fabComponent.RefPln_FabOut);
                        fabCollection.fabBeamCollection[fabComponent.FabBeamsName[i]].Angle_FabOut = fabComponent.Angle_FabOut;
                    }
                }
            }


        }

        public static Box GetFabElementBoundingBox(Brep geometry, Plane frameBase)
        {
            Box box = new Box();
            Curve[] intersectionCurves;
            Point3d[] intersectionPoints;
            bool success = Intersection.BrepPlane(geometry, frameBase, 0.01, out intersectionCurves, out intersectionPoints);
            if (success)
            {
                if (intersectionCurves != null && intersectionCurves.Length > 0)
                {
                    List<Curve> boundingBaseCrvs = new List<Curve>();
                    for (int i = 0; i < intersectionCurves.Length; i++)
                    {
                        try
                        {
                            Curve boundingBaseCrv = Curve.JoinCurves(intersectionCurves)[i];
                            boundingBaseCrvs.Add(boundingBaseCrv);
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            // Handle the exception or skip to the next iteration as needed.
                            // You can log an error or take appropriate action.
                            continue; // Skip to the next iteration of the loop.
                        }
                    }
                    //check if boundingBaseCrvs is empty or else it will throw an error
                    if (boundingBaseCrvs.Count == 0)
                    {
                        throw new InvalidOperationException("No intersection found between Brep and FrameBase to compute Minimum aligned BoundingBox.");
                    }

                    boundingBaseCrvs.Sort((a, b) => b.GetLength().CompareTo(a.GetLength()));
                    Curve longestCurve = boundingBaseCrvs[0];

                    bool convertSuccess = longestCurve.TryGetPolyline(out Polyline boundingBasePolyline);
                    if (convertSuccess)
                    {
                        Rectangle3d boundingRectangle = FabUtilities.GetBoundingRectangle(boundingBasePolyline, frameBase, out double length, out double width);
                        Plane boundingPlane = boundingRectangle.Plane;
                        box = FabUtilities.CreateAlignedBox(geometry, boundingPlane);
                        return box;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to convert intersectionCrv to Polyline.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("No intersection found between Brep and Plane.");
                }
            }
            return box;
        }
        public static Box GetFabElementBoundingBox(Brep geometry, Plane frameBase, Polyline boundingPolyline)
        {
            Box box = new Box();

            Rectangle3d boundingRectangle = FabUtilities.GetBoundingRectangle(boundingPolyline, frameBase, out double length, out double width);
            Plane boundingPlane = boundingRectangle.Plane;
            box = FabUtilities.CreateAlignedBox(geometry, boundingPlane);

            return box;
        }


        public static Plane GetElementCenterPlane(Brep plate, Plane refPln)
        {
            List<BrepFace> facesList = new List<BrepFace>();
            List<double> zVals = new List<double>();
            BrepFaceList faces = plate.Faces;
            BrepFace lowestFace = FabUtilities.GetLowerFace(plate, refPln);
            List<Curve> faceEdges = new List<Curve>();
            FabUtilities.GetFaceEdges(plate, lowestFace, ref faceEdges); //hint: embedded custom funtion

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
            Curve innerCrv = new LineCurve();
            Curve outerCrv = new LineCurve();
            Point3d cog = new Point3d();

            if (edgeCrvsArr.Length == 2)
            {

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

            Plane newRefPln = new Plane(cog, refPln.XAxis, refPln.YAxis);

            return newRefPln;
        }

        public static List<FabElement.FabElement> GetMatchingFabElement(FabComponent fabComponent, List<String> fabElementName) {

            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

            var matchingFabElements = new List<FabElement.FabElement>();

            // Collect all matching FabElements for each iFabElementName
            foreach (var name in fabElementName)
            {
                var elements = fabCollection.fabElementCollection
                    .Where(kvp => kvp.Key.Contains(name))
                    .Select(kvp => kvp.Value)
                    .ToList(); // Convert to List to enable indexing

                if (!elements.Any())
                {
                    throw new KeyNotFoundException($"No matching FabElement found for '{name}'.");
                }

                matchingFabElements.AddRange(elements);
            }


            //check if matchingFabElements are also either in fabComponent.FabPlatesName or fabComponent.FabBeamsName
            //if not, throw an exception
            foreach (FabElement.FabElement fabElement in matchingFabElements)
            {
                if ((fabComponent.FabPlatesName == null || !fabComponent.FabPlatesName.Contains(fabElement.Name)) &&
                    (fabComponent.FabBeamsName == null || !fabComponent.FabBeamsName.Contains(fabElement.Name)))
                {
                    throw new KeyNotFoundException($"Key '{fabElement.Name}' does not exist in the FabComponent.");
                }
            }

            //check if matchingFabElements are also either in fabComponent.FabPlatesName or fabComponent.FabBeamsName
            //if not, throw an exception
            foreach (FabElement.FabElement fabElement in matchingFabElements)
            {
                if ((fabComponent.FabPlatesName == null || !fabComponent.FabPlatesName.Contains(fabElement.Name)) &&
                    (fabComponent.FabBeamsName == null || !fabComponent.FabBeamsName.Contains(fabElement.Name)))
                {
                    throw new KeyNotFoundException($"Key '{fabElement.Name}' does not exist in the FabComponent.");
                }
            }

            return matchingFabElements;
        }

        public static List<FabElement.FabElement> GetAllFabComponentElements(FabComponent fabComponent, bool includeFabComponent = false)
        {
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

            List<FabElement.FabElement> allFabElements = new List<FabElement.FabElement>();

            if (fabComponent != null)
            {
                if (includeFabComponent)
                {
                    if (fabCollection.fabElementCollection.TryGetValue(fabComponent.Name, out var fabElement))
                    {
                        allFabElements.Add(fabElement);
                    }
                    else
                    {
                        // Handle the case when the fab component name is not found in the collection
                        throw new Exception($"Fab component '{fabComponent.Name}' not found in the collection.");
                    }
                }

                if (fabComponent.FabBeamsName != null)
                {
                    // Add fab beams
                    foreach (var beamName in fabComponent.FabBeamsName)
                    {
                        if (fabCollection.fabElementCollection.TryGetValue(beamName, out var fabElement))
                        {
                            allFabElements.Add(fabElement);
                        }
                        else
                        {
                            // Handle the case when the fab beam name is not found in the collection
                            throw new Exception($"Fab beam '{beamName}' not found in the collection.");
                        }
                    }
                }

                if (fabComponent.FabPlatesName != null)
                {
                    // Add fab plates
                    foreach (var plateName in fabComponent.FabPlatesName)
                    {
                        if (fabCollection.fabElementCollection.TryGetValue(plateName, out var fabElement))
                        {
                            allFabElements.Add(fabElement);
                        }
                        else
                        {
                            // Handle the case when the fab plate name is not found in the collection
                            throw new Exception($"Fab plate '{plateName}' not found in the collection.");
                        }
                    }
                }
            }
            else
            {
                // Handle the case when fabComponent or its properties are null
                throw new ArgumentNullException(nameof(fabComponent), "FabComponent cannot be null.");
            }

            return allFabElements;
        }
    }

}
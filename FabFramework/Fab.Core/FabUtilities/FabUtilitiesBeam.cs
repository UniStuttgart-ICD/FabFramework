using Fab.Core.FabElement;
using Fab.Core.FabEnvironment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fab.Core.FabUtilities
{
    public class FabUtilitiesBeam
    {


        public static (List<Plane>, List<Plane>, List<double>, List<double>, List<int>) GetListofBeamsGlueLines(List<FabBeam> fabBeams, Endeffector glueGun, bool upperSide = false, bool reverseGlueLines = false)
        {
            List<Plane> glueStartPlanes = new List<Plane>();
            List<Plane> glueEndPlanes = new List<Plane>();
            List<double> glueStartTTAngles = new List<double>();
            List<double> glueEndTTAngles = new List<double>();
            List<int> glueStates = new List<int>();


            int startIndex = reverseGlueLines ? fabBeams.Count - 1 : 0;
            int endIndex = reverseGlueLines ? -1 : fabBeams.Count;

            for (int i = startIndex; i != endIndex; i += reverseGlueLines ? -1 : 1)
            {
                List<Plane> glueStartPlanesBeam;
                List<Plane> glueEndPlanesBeam;
                List<double> glueStartTTAnglesBeam;
                List<double> glueEndTTAnglesBeam;
                List<int> glueStatesBeam;

                (glueStartPlanesBeam, glueEndPlanesBeam, glueStartTTAnglesBeam, glueEndTTAnglesBeam, glueStatesBeam) = FabUtilitiesBeam.GetBeamGlueLines(fabBeams[i], glueGun, upperSide, reverseGlueLines);

                glueStartPlanes.AddRange(glueStartPlanesBeam);
                glueEndPlanes.AddRange(glueEndPlanesBeam);
                glueStartTTAngles.AddRange(glueStartTTAnglesBeam);
                glueEndTTAngles.AddRange(glueEndTTAnglesBeam);
                glueStates.AddRange(glueStatesBeam);
            }


            return (glueStartPlanes, glueEndPlanes, glueStartTTAngles, glueEndTTAngles, glueStates);
        }

        public static (List<Plane>, List<Plane>, List<double>, List<double>, List<int>) GetBeamGlueLines(FabBeam fabBeam, Endeffector glueGun, bool upperSide = false, bool reverseGlueLines = false)
        {
            List<Plane> glueStartPlanes = new List<Plane>();
            List<Plane> glueEndPlanes = new List<Plane>();
            List<double> glueStartTTAngles = new List<double>();
            List<double> glueEndTTAngles = new List<double>();
            List<int> glueStates = new List<int>();

            //Start for Loop here for generating glueLines

            //Calculate necessary glueLine per Beam


            if (fabBeam.GetDesignBeam().Width == 0)
            {
                //Add error message out text
            }

            List<double> glueLineOffsetList = FabUtilitiesBeam.CalculateGlueLineOffsets(fabBeam.GetDesignBeam().Width, glueGun.FootprintWidth);

            for (int j = 0; j < glueLineOffsetList.Count; j++)
            {
                //Initial variables for GlueLine

                Line glueLine = new Line(fabBeam.GetDesignBeam().StartPoint, fabBeam.GetDesignBeam().EndPoint);
                Plane glueStartPln = fabBeam.RefPln_Situ.Clone();
                Plane glueEndPln = fabBeam.RefPln_Situ.Clone();
                double glueStartTTAngle = fabBeam.Angle_FabOut;
                double glueEndTTAngle = fabBeam.Angle_FabOut;
                int glueState;

                if (j == glueLineOffsetList.Count)
                { glueState = 0; }
                else
                { glueState = 1; }


                //Start of code for glueLines

                //Offset Glue Line according to offSetWitdthList
                Vector3d offsetWidthVector = glueStartPln.YAxis;
                offsetWidthVector.Unitize();
                offsetWidthVector *= glueLineOffsetList[j];
                glueLine.Transform(Transform.Translation(offsetWidthVector));

                //transformation Vector: Translate glueLine to zOffset from GlueGun
                Vector3d zOffsetGlueGunVector = glueStartPln.ZAxis;
                zOffsetGlueGunVector.Unitize();
                zOffsetGlueGunVector *= glueGun.ZOffset;
                glueLine.Transform(Transform.Translation(zOffsetGlueGunVector));

                //transformation Vector: Translate  Glue line from beam center to face
                Vector3d lowerBeamBaseVector = glueStartPln.ZAxis;
                lowerBeamBaseVector.Unitize();
                if (upperSide == false) //check wether lower or upperSurface
                { lowerBeamBaseVector *= (-1) * (fabBeam.GetDesignBeam().Height / 2); }
                else
                { lowerBeamBaseVector *= (fabBeam.GetDesignBeam().Height / 2); }
                glueLine.Transform(Transform.Translation(lowerBeamBaseVector));


                //Flip uneven glue lines
                if (j % 2 != 0)
                {
                    glueLine.Flip();
                }
                if (reverseGlueLines == true)
                {
                    glueLine.Flip();
                }


                //Create Bot Glue Start and End Point from GlueLine
                Point3d glueStartPoint = glueLine.PointAt(0.0);
                glueStartPln.Origin = glueStartPoint;
                Point3d glueEndPoint = glueLine.PointAt(1.0);
                glueEndPln.Origin = glueEndPoint;


                //Orient Bot Planes To Fabrication Out
                glueStartPln = FabUtilities.OrientPlane(glueStartPln, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);
                glueEndPln = FabUtilities.OrientPlane(glueEndPln, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);


                //Add all glue data to list
                glueStartPlanes.Add(glueStartPln);
                glueEndPlanes.Add(glueEndPln);
                glueStartTTAngles.Add(glueStartTTAngle);
                glueEndTTAngles.Add(glueEndTTAngle);
                glueStates.Add(glueState);
            }


            return (glueStartPlanes, glueEndPlanes, glueStartTTAngles, glueEndTTAngles, glueStates);
        }


        public static bool GetBeamPlaceNailPlanes(FabBeam fabBeam, out List<Plane> placeNailPlanes, out List<int> nailStates, out List<Plane> nailPositionPlanes)
        {

            //Fixed variables nailing and beams
            Double minBeamLength = 300.00;
            Double shortBeamLength = 700.00;
            Double mediumBeamLength = 1500.00;
            Double longBeamLength = 2400.00;
            Double distanceNailGuns = 400;

            int nailBothSides = 2;
            int nailRightSide = 1;
            int nailLeftSide = -1;
            int nailNothing = 0;

            Point3d beam_PlacePlnPoint = new Point3d();

            //nail variables
            Point3d beam_NailPlnPoint = new Point3d();
            List<Point3d> beams_NailPointsList = new List<Point3d>();

            Plane beam_NailPlane = new Plane();
            List<Plane> beams_NailPlanesList = new List<Plane>();
            List<Double> beam_NailTTAngleList = new List<Double>();
            List<int> beam_NailTypeList = new List<int>();
            List<Plane> beam_NailPositionPlanesList = new List<Plane>();


            //Define Gripping Position according to length of beam
            Line beamBaseLine = fabBeam.GetDesignBeam().BaseLine;
            Curve beamBaseCurve = beamBaseLine.ToNurbsCurve();
            Double beamBaseLength = beamBaseCurve.GetLength();


            //Check if beam waaaay to short
            if (beamBaseLength <= minBeamLength)
            {
                //"BEAM WAAAAY TO SHORT: " + beamBaseLength.ToString() + " mm"));
                beam_PlacePlnPoint = beamBaseCurve.PointAt((beamBaseLength / 2));
            }

            //very short beam 300-500m
            if (beamBaseLength > minBeamLength && beamBaseLength <= shortBeamLength)
            {
                //this needs extra logic
                //"BEAM LOGIC NOT SET YET! very short beam");
                beam_PlacePlnPoint = beamBaseCurve.PointAt((beamBaseLength / 2));

                //Nail #1
                beam_NailPlnPoint = beam_PlacePlnPoint;
                beams_NailPointsList.Add(beam_NailPlnPoint);
                beam_NailTypeList.Add(nailBothSides);
            }

            //short beam 500-1500mm
            else if (beamBaseLength > shortBeamLength && beamBaseLength <= mediumBeamLength)
            {
                beam_PlacePlnPoint = beamBaseCurve.PointAt((beamBaseLength / 2));

                //Nail #1
                beam_NailPlnPoint = beam_PlacePlnPoint;
                beams_NailPointsList.Add(beam_NailPlnPoint);
                beam_NailTypeList.Add(nailBothSides);
            }

            //medium beam 1500-2400mm
            else if (beamBaseLength > mediumBeamLength && beamBaseLength <= longBeamLength)
            {
                beam_PlacePlnPoint = beamBaseCurve.PointAt((beamBaseLength / 3) * 2);

                //Nail #1
                beam_NailPlnPoint = beam_PlacePlnPoint;
                beams_NailPointsList.Add(beam_NailPlnPoint);
                beam_NailTypeList.Add(nailBothSides);

                //Nail #2
                //beam_NailPlnPoint = beamBaseCurve.PointAt(distanceNailGuns);
                beam_NailPlnPoint = beamBaseCurve.PointAt((beamBaseLength / 3));
                beams_NailPointsList.Add(beam_NailPlnPoint);
                beam_NailTypeList.Add(nailLeftSide);
            }

            //long beam +2400mm
            else if (beamBaseLength > longBeamLength)
            {
                beam_PlacePlnPoint = beamBaseCurve.PointAt((beamBaseLength / 2));

                //Nail #1
                beam_NailPlnPoint = beam_PlacePlnPoint;
                beams_NailPointsList.Add(beam_NailPlnPoint);
                beam_NailTypeList.Add(nailBothSides);

                //Nail #2
                beam_NailPlnPoint = beamBaseCurve.PointAt((beamBaseLength - (distanceNailGuns)));
                beams_NailPointsList.Add(beam_NailPlnPoint);
                beam_NailTypeList.Add(nailRightSide);

                //Nail #3
                beam_NailPlnPoint = beamBaseCurve.PointAt((distanceNailGuns));
                beams_NailPointsList.Add(beam_NailPlnPoint);
                beam_NailTypeList.Add(nailLeftSide);
            }



            //NAIL PLANES
            for (int j = 0; j < beams_NailPointsList.Count; j++)
            {
                beam_NailPlane = FabUtilitiesBeam.ConvertBeamBasePointToUpperFacePlane(beams_NailPointsList[j], fabBeam);
                beam_NailPlane = FabUtilities.OrientPlane(beam_NailPlane, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);
                beams_NailPlanesList.Add(beam_NailPlane);

                if (beam_NailTypeList[j] == 0) //Both Sides
                {
                    Plane beam_NailPositionPlane = beam_NailPlane.Clone();
                    beam_NailPositionPlane = FabUtilities.TransformPlaneByVector(beam_NailPositionPlane, beam_NailPlane.XAxis * (-1), distanceNailGuns / 2);
                    //Adjust beamNail Position
                    beam_NailPositionPlanesList.Add(beam_NailPositionPlane);

                    beam_NailPositionPlane = beam_NailPlane.Clone();
                    beam_NailPositionPlane = FabUtilities.TransformPlaneByVector(beam_NailPositionPlane, beam_NailPlane.XAxis, distanceNailGuns / 2);
                    beam_NailPositionPlanesList.Add(beam_NailPositionPlane);

                }
                else if (beam_NailTypeList[j] == 1) //Right Side
                {
                    Plane beam_NailPositionPlane = beam_NailPlane.Clone();
                    beam_NailPositionPlane = FabUtilities.TransformPlaneByVector(beam_NailPositionPlane, beam_NailPlane.XAxis, distanceNailGuns / 2);
                    beam_NailPositionPlanesList.Add(beam_NailPositionPlane);
                }
                else if (beam_NailTypeList[j] == 2) //Left Side
                {
                    Plane beam_NailPositionPlane = beam_NailPlane.Clone();
                    beam_NailPositionPlane = FabUtilities.TransformPlaneByVector(beam_NailPositionPlane, beam_NailPlane.XAxis * (-1), distanceNailGuns / 2);
                    beam_NailPositionPlanesList.Add(beam_NailPositionPlane);
                }
            }

            placeNailPlanes = beams_NailPlanesList;
            nailStates = beam_NailTypeList;
            nailPositionPlanes = beam_NailPositionPlanesList;

            return true;
        }



        public static List<double> CalculateGlueLineOffsets(double beamWidth, double strokeWidth)
        {
            List<double> widthOffsetList = new List<double>();

            int n = (int)Math.Ceiling(beamWidth / strokeWidth);

            if (n == 1)
            {
                widthOffsetList.Add(beamWidth / 2);
            }
            else if (n == 2)
            {
                widthOffsetList.Add(strokeWidth / 2);
                widthOffsetList.Add(beamWidth - (strokeWidth / 2));
            }
            else
            {
                widthOffsetList.Add(strokeWidth / 2);

                // Calculate offsets for additional strokes
                double excessWidth = beamWidth - (2 * strokeWidth);
                int excessN = (int)Math.Ceiling(excessWidth / strokeWidth);
                double overlappExcessWidth = excessN * strokeWidth - excessWidth;
                int overlappCount = excessN + 1;

                for (int e = 0; e < excessN; e++)
                {
                    double widthOffset = strokeWidth - ((e + 1) * (overlappExcessWidth / overlappCount)) + (strokeWidth * e) + (strokeWidth / 2);
                    widthOffsetList.Add(widthOffset);
                }

                widthOffsetList.Add(beamWidth - (strokeWidth / 2));
            }


            return (widthOffsetList);

        }


        public static Plane ConvertBeamBasePointToUpperFacePlane(Point3d beamBasePoint, FabBeam beam)
        {
            //Define beam target plane
            Plane beam_TempTargetPlane = beam.RefPln_Situ.Clone();
            beam_TempTargetPlane.Origin = beamBasePoint;

            //adjust pick Plane to correct Height: move pick plane up by half the beam height
            Vector3d targetPlaneHeight_Vector = beam_TempTargetPlane.ZAxis;
            targetPlaneHeight_Vector.Unitize();
            targetPlaneHeight_Vector *= beam.GetDesignBeam().Height / 2;
            beam_TempTargetPlane.Transform(Transform.Translation(targetPlaneHeight_Vector));

            //pick plane not on the correct position
            //temporary adjustment of correct width of beam pick position
            //adjust pick Plane to correct Height: move pick plane up by half the beam height
            Vector3d targetPlaneWidth_Vector = beam_TempTargetPlane.YAxis;
            targetPlaneWidth_Vector.Unitize();
            targetPlaneWidth_Vector *= beam.GetDesignBeam().Width / 2;
            beam_TempTargetPlane.Transform(Transform.Translation(targetPlaneWidth_Vector));

            return beam_TempTargetPlane;
        }


        public static Plane Optimized_FabOut_BeamTurnTable(FabBeam fabBeam)
        {
            Plane newRefPln_FabOut = fabBeam.RefPln_FabOut.Clone();

            //OPTIMIZATION FOR RefPln_FabOut
            //1. Angle Variation
            //2. Check if new angle is in target domain

            bool optimized_RefPln_FabOutBool = false;

            for (int j = 0; j < 200; j++)
            {
                //create angle values to check in with positive and negative alternating
                Double angleStepSize = 0.043;
                Double angleIterate = angleStepSize * j;
                if (j % 2 == 0)
                {
                    angleIterate *= -1;
                }


                Plane turnTable_AlignPln = fabBeam.EnvFab.AlignPln[0];
                Vector3d beam_XAxis = fabBeam.RefPln_Fab.XAxis;
                Vector3d turnTable_AlignPln_XAxis = turnTable_AlignPln.XAxis;
                Double turnTable_AlignPlnAngle = Vector3d.VectorAngle(beam_XAxis, turnTable_AlignPln_XAxis, fabBeam.EnvFab.RefPln[0]);

                //added iteration step
                turnTable_AlignPlnAngle += angleIterate;

                Plane turnTable_AlignPlnRotated = turnTable_AlignPln.Clone();
                turnTable_AlignPlnRotated.Rotate(turnTable_AlignPlnAngle, turnTable_AlignPlnRotated.ZAxis);


                //newBeam.RefPln_FabOut = FabUtilities.OrientPlane(newBeam.RefPln_Fab, turnTable_AlignPln, turnTable_AlignPlnRotated);
                Plane oriented_RefPln_FabOut = FabUtilities.OrientPlane(fabBeam.RefPln_Fab, turnTable_AlignPln, turnTable_AlignPlnRotated);
                newRefPln_FabOut = oriented_RefPln_FabOut;

                //check here if refPln_FabOut in Domain
                Double xMax_Domain = -500;
                Double xMin_Domain = -4500;
                Double yMax_Domain = -1000;
                Double yMin_Domain = -3000;
                if (j == 0)
                {
                    //save first iteration results, incase no solution is found
                    newRefPln_FabOut = oriented_RefPln_FabOut;
                }

                //HARD NECESSARY 
                Line testLine = FabUtilities.OrientLine(fabBeam.GetDesignBeam().BaseLine, fabBeam.RefPln_Situ, oriented_RefPln_FabOut);
                Point3d startTestPoint = testLine.PointAt(0.0);
                Point3d endTestPoint = testLine.PointAt(1.0);

                if (startTestPoint.Y > turnTable_AlignPln.OriginY &&
                    endTestPoint.Y > turnTable_AlignPln.OriginY
                    )
                {
                    if (startTestPoint.Y < (-750) &&
                        endTestPoint.Y < (-750)
                        ) //check so that beams are never on top of platform
                    {

                        //Should work solution
                        newRefPln_FabOut = oriented_RefPln_FabOut;

                        //EXIT CRITERIA
                        if (
                        oriented_RefPln_FabOut.OriginX <= xMax_Domain &&
                        oriented_RefPln_FabOut.OriginX >= xMin_Domain &&
                        oriented_RefPln_FabOut.OriginY <= yMax_Domain &&
                        oriented_RefPln_FabOut.OriginY >= yMin_Domain)
                        {
                            //optimized solution
                            newRefPln_FabOut = oriented_RefPln_FabOut;
                            optimized_RefPln_FabOutBool = true;
                            break;
                        }
                    }
                }
            }

            if (optimized_RefPln_FabOutBool == false)
            {
                throw new InvalidOperationException("No solution found for RefPln_FabOut. Beam No: " + fabBeam.Index.ToString());
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "refPln_FabOut invalid! Beam No: " + i.ToString());
            }

            return newRefPln_FabOut;
        }


        public static List<FabBeam> SortBeamFabricationOrderForTurnTableOLD(List<FabBeam> fabBeams, double turnTable_StartAngle)
        {
            //sort beams according to smallest Angle from refPln_FabOut to refPln_Fab
            int closestIndex = -1;
            Double smallestAngle = Double.MaxValue;

            for (int i = 0; i < fabBeams.Count; i++)
            {
                Plane flippedTT_Plane = new Plane(fabBeams[i].EnvFab.RefPln[0].Origin, fabBeams[i].EnvFab.RefPln[0].XAxis, fabBeams[i].EnvFab.RefPln[0].YAxis); ;
                flippedTT_Plane.Flip();
                Double diff_BeamAngle = Vector3d.VectorAngle(fabBeams[i].RefPln_Fab.XAxis, fabBeams[i].RefPln_FabOut.XAxis, flippedTT_Plane);
                Double fab_BeamAngle = turnTable_StartAngle - FabUtilities.RadianToDegree(diff_BeamAngle);

                //Add TurnTable Angle to FabBeam
                fabBeams[i].Angle_FabOut = fab_BeamAngle;

                if (diff_BeamAngle < smallestAngle)
                {
                    smallestAngle = diff_BeamAngle;
                    closestIndex = i;
                }
            }

            // Shift the list of the beams to the left by 'closestIndex'
            fabBeams.AddRange(fabBeams.GetRange(0, closestIndex));
            fabBeams.RemoveRange(0, closestIndex);

            //Set Index according to list order for each beam
            for (int i = 0; i < fabBeams.Count; i++)
            {
                //fabBeams[i].Name = "Beam_" + i.ToString(); //needs custom function to modify
                fabBeams[i].Index = i;
            }


            return fabBeams;
        }

        public static List<FabBeam> SortBeamFabricationOrderForTurnTable(List<FabBeam> fabBeams, double turnTable_StartAngle)
        {
            for (int i = 0; i < fabBeams.Count; i++)
            {
                Plane flippedTT_Plane = new Plane(fabBeams[i].EnvFab.RefPln[0].Origin, fabBeams[i].EnvFab.RefPln[0].XAxis, fabBeams[i].EnvFab.RefPln[0].YAxis); ;
                flippedTT_Plane.Flip();
                Double diff_BeamAngle = Vector3d.VectorAngle(fabBeams[i].RefPln_Fab.XAxis, fabBeams[i].RefPln_FabOut.XAxis, flippedTT_Plane);
                Double fab_BeamAngle = turnTable_StartAngle - FabUtilities.RadianToDegree(diff_BeamAngle);

                //Add TurnTable Angle to FabBeam
                fabBeams[i].Angle_FabOut = fab_BeamAngle;

            }

            // Sort the list in descending order based on the "Angle" property
            fabBeams.Sort((a, b) => b.Angle_FabOut.CompareTo(a.Angle_FabOut));

            return fabBeams;
        }

        public static void AdjustBeamRefPln_Mag(List<FabBeam> fabBeams)
        {
            //SORT BEAMS IN MAGAZINE
            List<FabBeam> mag_Beams = new List<FabBeam>();
            for (int i = 0; i < fabBeams.Count; i++)
            {
                mag_Beams.Add(fabBeams[i]);
            }

            //Sort Mag Beams according to longest Beam first
            mag_Beams.Sort((beam1, beam2) => beam1.GetDesignBeam().BaseLine.Length.CompareTo(beam2.GetDesignBeam().BaseLine.Length));
            mag_Beams.Reverse();

            double maxBeamLength = 3700;

            int leftIndex = 0;
            int rightIndex = mag_Beams.Count - 1;

            int magSlotCount = 0;
            Plane beamMagTargetPlane = fabBeams[0].EnvMag.RefPln[0];
            FabBeam mag_Beam_Selected = mag_Beams[0];
            for (int i = 0; i < mag_Beams.Count; i++)
            {

                //check if maxBeamLength is reached for first row in beam Magazine
                if (magSlotCount % 2 == 0)
                {
                    mag_Beam_Selected = mag_Beams[leftIndex];
                    leftIndex++;
                }
                else
                {
                    if (mag_Beams[rightIndex].GetDesignBeam().BaseLine.Length + mag_Beams[leftIndex - 1].GetDesignBeam().BaseLine.Length >= maxBeamLength)
                    {
                        mag_Beam_Selected = mag_Beams[leftIndex];
                        leftIndex++;
                        magSlotCount++;
                    }
                    else
                    {
                        mag_Beam_Selected = mag_Beams[rightIndex];
                        rightIndex--;
                    }
                }

                //shift beams negative if necessary
                beamMagTargetPlane = fabBeams[0].EnvMag.RefPln[magSlotCount];
                if (magSlotCount % 2 != 0)
                {
                    //move beamtargetPlane.Origin -X by beam Length
                    //could be modified in the future
                    Vector3d translationVector = beamMagTargetPlane.XAxis * mag_Beam_Selected.GetDesignBeam().BaseLine.Length * (-1);
                    beamMagTargetPlane.Translate(translationVector);
                }

                mag_Beam_Selected.RefPln_Mag = FabUtilities.OrientPlane(mag_Beam_Selected.RefPln_Situ, mag_Beam_Selected.RefPln_SituBox, beamMagTargetPlane);
                magSlotCount++;

                if (leftIndex >= mag_Beams.Count)
                {
                    break;
                }
            }


        }


    }

}

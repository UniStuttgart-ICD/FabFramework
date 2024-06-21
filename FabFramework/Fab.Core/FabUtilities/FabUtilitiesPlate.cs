using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry.Collections;
using Fab.Core.FabEnvironment;
using Fab.Core.FabUtilities;
using Fab.Core.FabElement;
using Grasshopper.Kernel.Types.Transforms;

namespace Fab.Core.FabUtilities
{
    public class FabUtilitiesPlate
    {
        public static bool GetTopPlateNailPlanes(FabBeam fabBeam, FabPlate fabPlate, out List<Plane> topPlateNailPlanesList, out List<int> topPlateNailStates, out List<double> topPlateNailTTAngle)
        {
            topPlateNailPlanesList = new List<Plane>();
            topPlateNailStates = new List<int>();
            topPlateNailTTAngle = new List<double>();

            //Fixed variables
            Double nailDistance = 400.0;

            //Define Nail Positions according to length of beam
            Line beamBaseLine = fabBeam.GetDesignBeam().BaseLine;
            Curve beamBaseCurve = beamBaseLine.ToNurbsCurve();
            Double beamBaseLength = beamBaseCurve.GetLength();

            int nailAmount = (int)Math.Ceiling(beamBaseLength / nailDistance);
            Double nailSpacingDistance = beamBaseLength / nailAmount;

            //NAIL PLANES
            Point3d nailPositionPoint = new Point3d();
            int topPlateNailState;
            List<Point3d> nailPositionPointList = new List<Point3d>();
            for (int j = 0; j < nailAmount; j++)
            {
                if (j == 0)
                {
                    nailPositionPoint = beamBaseCurve.PointAt((nailSpacingDistance / 2));
                    topPlateNailState = -1;
                }
                else if (j == nailAmount - 1)
                {
                    nailPositionPoint = beamBaseCurve.PointAt((beamBaseLength - (nailSpacingDistance / 2)));
                    topPlateNailState = 0;
                }
                else
                {
                    nailPositionPoint = beamBaseCurve.PointAt(((nailSpacingDistance / 2) + (nailSpacingDistance * j)));
                    topPlateNailState = 1;
                }

                nailPositionPointList.Add(nailPositionPoint);
                topPlateNailStates.Add(topPlateNailState);
                
            }

            Plane nail_TempPlane = new Plane();
            int tempCHeck = nailPositionPointList.Count;
            for (int k = 0; k < nailPositionPointList.Count; k++)
            {
                //change height, is incorrect

                nail_TempPlane = FabUtilitiesPlate.ConvertBeamBasePointToTopPlateSurface(nailPositionPointList[k], fabBeam, fabPlate);


                nail_TempPlane = FabUtilities.OrientPlane(nail_TempPlane, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut);
                topPlateNailPlanesList.Add(nail_TempPlane);
                topPlateNailTTAngle.Add(fabBeam.Angle_FabOut);
            }


            return true;

        }
        public static bool GetPlatePNP(FabPlate fabPlate, Endeffector endEffector, out Plane pickPln, out Plane placePln)
        {
            Plane envMagPln = fabPlate.EnvMag.RefPln[0];
            Plane uncheck_pickPln = fabPlate.RefPln_Mag;
            Point3d cPt_envMagPln = envMagPln.ClosestPoint(uncheck_pickPln.Origin);
            envMagPln.ClosestParameter(cPt_envMagPln, out double envMagPln_U_dis, out double envMagPln_V_dis);

            //Define Safety Values to Avoid Collision from Endeffector with PlateTable Wall
            Double envMagPln_extraDis = 50.0;
            Double envMagPln_safetyDis_X = endEffector.FootprintWidth / 2 + envMagPln_extraDis;
            Double envMagPln_safetyDis_Y = endEffector.FootprintLength / 2 + envMagPln_extraDis;


            Plane default_pickPln = envMagPln.Clone();
            default_pickPln.Origin = envMagPln.PointAt(envMagPln_safetyDis_X, envMagPln_safetyDis_Y); //X & Y
            Point3d cPT_uncheck_PickPln = uncheck_pickPln.ClosestPoint(default_pickPln.Origin); //Z-Height
            default_pickPln.Origin = cPT_uncheck_PickPln;

            if (envMagPln_V_dis > envMagPln_safetyDis_Y && envMagPln_U_dis > envMagPln_safetyDis_X)
                {
                    pickPln = uncheck_pickPln;
                }
            else if (envMagPln_V_dis > envMagPln_safetyDis_Y)
            {
                //incorrect
                default_pickPln.OriginX = uncheck_pickPln.OriginY;
                pickPln = default_pickPln;
            }
            else if (envMagPln_U_dis > envMagPln_safetyDis_X)
            {
                //incorrect
                default_pickPln.OriginY = uncheck_pickPln.OriginY;
                pickPln = default_pickPln;
            }
            else
            {
                pickPln = default_pickPln;
            }

            //adjust pick Plane to correct Height
            Vector3d targetPlaneHeight_Vector = pickPln.ZAxis;
            targetPlaneHeight_Vector.Unitize();
            targetPlaneHeight_Vector *= fabPlate.GetDesignPlate().Height;
            pickPln.Transform(Transform.Translation(targetPlaneHeight_Vector));

            //adjust pick Plane to vaccumgripper ZOffset
            Vector3d vacuumGripperZOffset_Vector = pickPln.ZAxis;
            vacuumGripperZOffset_Vector.Unitize();
            vacuumGripperZOffset_Vector *= endEffector.ZOffset;
            pickPln.Transform(Transform.Translation(vacuumGripperZOffset_Vector));

            placePln = FabUtilities.OrientPlane(pickPln, fabPlate.RefPln_Mag, fabPlate.RefPln_FabOut);

            return true;
        }


        public static bool GetPlatePNP_TurnTable(FabPlate fabPlate, Endeffector endEffector, out Plane pickPln, out Plane placePln, out double pickAngle, out double placeAngle)
        {
            Plane magPln = fabPlate.EnvMag.RefPln[0];
            Plane uncheck_pickPln = fabPlate.RefPln_Mag;
            Point3d closestPt_magPln = magPln.ClosestPoint(uncheck_pickPln.Origin);
            magPln.ClosestParameter(closestPt_magPln, out double magPln_u_dis, out double magPln_v_dis);

            //Define Safety Values to Avoid Collision from Endeffector with PlateTable Wall
            Double magPln_extraDis = 50.0;
            Double magPln_safetyDis_X = endEffector.FootprintWidth / 2 + magPln_extraDis;
            Double magPln_safetyDis_Y = endEffector.FootprintLength / 2 + magPln_extraDis;


            Plane default_pickPln = magPln.Clone();
            default_pickPln.Origin = magPln.PointAt(magPln_safetyDis_X, magPln_safetyDis_Y);
            Point3d cPT_uncheck_PickPln = uncheck_pickPln.ClosestPoint(default_pickPln.Origin); //Z-Height
            default_pickPln.Origin = cPT_uncheck_PickPln;


            if (magPln_v_dis > magPln_safetyDis_Y && magPln_u_dis > magPln_safetyDis_X)
            {
                pickPln = uncheck_pickPln;
            }
            else if (magPln_v_dis > magPln_safetyDis_Y)
            {
                //incorrect
                default_pickPln.OriginX = uncheck_pickPln.OriginX;
                pickPln = default_pickPln;
            }
            else if (magPln_u_dis > magPln_safetyDis_X)
            {
                //incorrect
                default_pickPln.OriginY = uncheck_pickPln.OriginY;
                pickPln = default_pickPln;
            }
            else
            {
                pickPln = default_pickPln;
            }

            //adjust pick Plane to correct Height: move pick plane up by half the beam height + top plate thickness
            Vector3d targetPlaneHeight_Vector = pickPln.ZAxis;
            targetPlaneHeight_Vector.Unitize();
            targetPlaneHeight_Vector *= fabPlate.GetDesignPlate().Height;
            pickPln.Transform(Transform.Translation(targetPlaneHeight_Vector));

            //adjust pick Plane to vaccumgripper ZOffset
            Vector3d vacuumGripperZOffset_Vector = pickPln.ZAxis;
            vacuumGripperZOffset_Vector.Unitize();
            vacuumGripperZOffset_Vector *= endEffector.ZOffset;
            pickPln.Transform(Transform.Translation(vacuumGripperZOffset_Vector));


            //Orient PlacePlane around TurnTable
            placePln = FabUtilities.OrientPlane(pickPln, fabPlate.RefPln_Mag, fabPlate.RefPln_FabOut);

            pickAngle = fabPlate.Angle_FabOut;
            placeAngle = fabPlate.Angle_FabOut;


            return true;
        }


        public static Plane ConvertBeamBasePointToTopPlateSurface(Point3d beamBasePoint, FabBeam beam, FabPlate plate)
        {
            //Define beam target plane
            Plane beam_TempTargetPlane = new Plane();
            beam_TempTargetPlane = beam.RefPln_Situ.Clone();
            beam_TempTargetPlane.Origin = beamBasePoint;

            //adjust pick Plane to correct Height: move pick plane up by half the beam height + top plate thickness
            Vector3d targetPlaneHeight_Vector = beam_TempTargetPlane.ZAxis;
            targetPlaneHeight_Vector.Unitize();
            targetPlaneHeight_Vector *= ((beam.GetDesignBeam().Height / 2) + plate.GetDesignPlate().Height);
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


        public static Plane GetPlateSituRefPln(Curve curve, Plane refPln)
        {
            Plane situRefPln = refPln.Clone();

            Point3d centerPoint = FabUtilities.GetCurveCenterPoint(curve);

            Point3d projectedPoint = FabUtilities.ProjectPointToPlane(centerPoint, refPln);

            situRefPln.Origin = projectedPoint;

            return situRefPln;
        }



    }
}

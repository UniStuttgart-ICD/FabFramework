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
using System.Linq;
using System.Collections;
using Fab.Core.DesignElement;
using Rhino.DocObjects;
using Fab.Core.FabCollection;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_DefineMillingCassette : GH_Component
    {
        

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_DefineMillingCassette()
          : base("LCRLMillingCassette", 
                "LCRL FCas", 
                "Convert the FabComponent data for LCRL Milling FabCassette data.",
                "Fab",
                "LCRL")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FabComponent", "FC", "FabComponent data", GH_ParamAccess.item);
            pManager.AddPlaneParameter("TurnTablePosition", "TTP", "Cassette Position on the turntable", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "Converted FabCassette Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //-----------
            //GLOBALS
            //-----------
            Double turnTable_StartAngle = 180;

            FabCollection fabCollection = FabCollection.GetFabCollection();

            //-----------
            //INPUTS
            //-----------
            FabComponent iFabComponent = new FabComponent();
            DA.GetData<FabComponent>("FabComponent", ref iFabComponent);
            FabComponent fabComponent = iFabComponent.ShallowCopy() as FabComponent;

            Plane iTurnTablePosition = Plane.Unset;
            DA.GetData<Plane>("TurnTablePosition", ref iTurnTablePosition);


            FabPlate cassettePlate = fabCollection.fabPlateCollection[iFabComponent.FabPlatesName[0]];

            //------------------------------------------------------------------------------------------------------
            //OVERWRITE ATTRIBUTES (this is specific to the LCRL fabrication setup)
            //------------------------------------------------------------------------------------------------------

            //Adjust RefFab & RefFabOut of all FabElements
            FabUtilitiesElement.AdjustAllElementRefFabAndFabOutPlanes(fabComponent, iTurnTablePosition, turnTable_StartAngle);

            //DesignRegion
            foreach (var kvp in cassettePlate.GetDesignPlate().DesignRegion)
            {
                string key = kvp.Key;
                DesignRegion designRegion = kvp.Value;

                designRegion.PlaneDict["RefPln_FabOut"] = DesignRegionFabOutTurnTable(cassettePlate, designRegion);
                designRegion.DoubleDict["Angle_FabOut"] = GetDesignRegionAngleFabOutForTurnTable(cassettePlate, designRegion, turnTable_StartAngle);
            }

            
            //RESORT DESIGN REGION DICTIONARY ACCORDING TO TURNTABLE ANGLE
            //Extract the dictionary entries and sort them by TT_Angle
            var dictionaryEntries = cassettePlate.GetDesignPlate().DesignRegion
                .OrderBy(entry => entry.Value.DoubleDict["Angle_FabOut"])
                .ToList();

            dictionaryEntries.Reverse();

            // Create a new dictionary with sorted entries
            var sortedDictionary = new Dictionary<string, Fab.Core.DesignElement.DesignRegion>();
            foreach (var entry in dictionaryEntries)
            {
                sortedDictionary.Add(entry.Key, entry.Value);
            }

            // Replace the old dictionary with the sorted one
            cassettePlate.GetDesignPlate().DesignRegion = sortedDictionary;


            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabComponent);
        }

        static double GetDesignRegionAngleFabOutForTurnTable(FabPlate fabPlate, DesignRegion designRegion, double turnTable_StartAngle)
        {
                Plane flippedTT_Plane = new Plane(fabPlate.EnvFab.RefPln[0].Origin, fabPlate.EnvFab.RefPln[0].XAxis, fabPlate.EnvFab.RefPln[0].YAxis); ;
                flippedTT_Plane.Flip();
                Double diff_FabOutAngle = Vector3d.VectorAngle(fabPlate.RefPln_FabOut.XAxis, designRegion.PlaneDict["RefPln_FabOut"].XAxis, flippedTT_Plane);
                Double designRegion_FabOutAngle = turnTable_StartAngle - FabUtilities.RadianToDegree(diff_FabOutAngle);

                return designRegion_FabOutAngle;
        }

        static Plane DesignRegionFabOutTurnTable(FabPlate fabPlate, DesignRegion designRegion)
        {

            Plane newRefPln_FabOut = fabPlate.RefPln_FabOut.Clone();
              

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
                    

                Plane turnTable_AlignPln = fabPlate.EnvFab.AlignPln[0];
                Plane fabMitrePlate = FabUtilities.OrientPlane(designRegion.PlaneDict["MitrePlane"], fabPlate.RefPln_Situ, fabPlate.RefPln_FabOut);
                Vector3d plateEdge_XAxis = fabMitrePlate.XAxis;
                Vector3d turnTable_AlignPln_XAxis = turnTable_AlignPln.XAxis;
                Double turnTable_AlignPlnAngle = Vector3d.VectorAngle(plateEdge_XAxis, turnTable_AlignPln_XAxis, fabPlate.EnvFab.RefPln[0]);

                //added iteration step
                turnTable_AlignPlnAngle += angleIterate;

                Plane turnTable_AlignPlnRotated = turnTable_AlignPln.Clone();
                turnTable_AlignPlnRotated.Rotate(turnTable_AlignPlnAngle, turnTable_AlignPlnRotated.ZAxis);


                //newBeam.RefPln_FabOut = FabUtilities.OrientPlane(newBeam.RefPln_Fab, turnTable_AlignPln, turnTable_AlignPlnRotated);
                Plane oriented_RefPln_FabOut = FabUtilities.OrientPlane(fabPlate.RefPln_Fab, turnTable_AlignPln, turnTable_AlignPlnRotated);
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
                Line testLine = FabUtilities.OrientLine(designRegion.LineDict["OuterEdge"], fabPlate.RefPln_Situ, oriented_RefPln_FabOut);
                Point3d startTestPoint = testLine.PointAt(0.0);
                Point3d endTestPoint = testLine.PointAt(1.0);

                if (startTestPoint.Y > turnTable_AlignPln.OriginY &&
                    endTestPoint.Y > turnTable_AlignPln.OriginY
                    )
                {

                    //EXIT CRITERIA
                    if (
                    startTestPoint.X <= xMax_Domain &&
                    startTestPoint.X >= xMin_Domain &&
                    startTestPoint.Y <= yMax_Domain &&
                    startTestPoint.Y >= yMin_Domain &&

                    endTestPoint.X <= xMax_Domain &&
                    endTestPoint.X >= xMin_Domain &&
                    endTestPoint.Y <= yMax_Domain &&
                    endTestPoint.Y >= yMin_Domain)
                    {
                        //optimized solution
                        newRefPln_FabOut = oriented_RefPln_FabOut;
                        optimized_RefPln_FabOutBool = true;
                        break;
                    }
                    
                }
            }

            if (optimized_RefPln_FabOutBool == false)
            {
                throw new InvalidOperationException("No solution found for designRegion: " + designRegion.Name.ToString());
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "refPln_FabOut invalid! Beam No: " + i.ToString());
            }

            return newRefPln_FabOut;
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
            get { return new Guid("3e012cf3-0c7f-4a8e-b59c-9251d7beb33a"); }
        }
    }
}
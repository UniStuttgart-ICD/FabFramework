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
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using Fab.Core.DesignElement;
using Rhino.Render.DataSources;
using static System.Collections.Specialized.BitVector32;

namespace Fab.Grasshopper.GhComponents.GhcCassette.LCRL
{
    public class GhcCassetteLCRL_SawCutEdges : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCassetteLCRL_SawCutEdges()
          : base("LCRLCassette Saw Cut Edges Cassette",
                "LCRL SawCutEdge",               
                "Get Task to cut the edges of the LCRL Cassette with sawblade.",
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
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actor", "A", "Actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("Cutter", "C", "Specific cutter by the actor", GH_ParamAccess.item);
            pManager.AddGenericParameter("SawCutAction", "S", "Milling action for the taskGeneration", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FabCassette", "FCas", "LCRL FabCassette Data", GH_ParamAccess.item);

            pManager.AddGenericParameter("SawCutEdges", "ST", "Fabrication Task for cutting the cassette edges with sawblade.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("SawMillingPlanesSitu", "SP", "SawMillingPlanes In-Situ", GH_ParamAccess.list);
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
            FabComponent iFabCassette = new FabComponent();
            DA.GetData<FabComponent>("FabCassette", ref iFabCassette);

            FabComponent fabCassette = iFabCassette.ShallowCopy() as FabComponent;

            Actor iFabActor = new Actor();
            DA.GetData("Actor", ref iFabActor);

            Cutter iCutter = new Cutter();
            DA.GetData("Cutter", ref iCutter);

            Fab.Core.FabEnvironment.Action iSawCutAction = new Fab.Core.FabEnvironment.Action();
            DA.GetData("SawCutAction", ref iSawCutAction);

            //-----------
            //EDIT
            //-----------
            List<double> offsetList = new List<double> { 0.0, 0.0, 200.0 }; 


            FabPlate cassettePlate = fabCollection.fabPlateCollection[fabCassette.FabPlatesName[0]];

            //PLATE
            FabTaskFrame fabTask_SawCutEdges = new FabTaskFrame(iSawCutAction.Name + "_" + cassettePlate.Name);
            List<Plane> sawMillingPlanes = new List<Plane>();

            int i = -1;
            foreach (var kvp in cassettePlate.GetDesignPlate().DesignRegion)
            {
                List<Plane> sawPlaneDesignRegion = new List<Plane>();

                string key = kvp.Key;
                DesignRegion designRegion = kvp.Value;
                i++;

                Plane mitrePlane = designRegion.PlaneDict["MitrePlane"];
                Line outerEdgeBotLine = designRegion.LineDict["OuterEdgeBot"];
                Line outerEdgeTopLine = designRegion.LineDict["OuterEdgeTop"];


                GetSawStartEndPlane(mitrePlane, outerEdgeBotLine, outerEdgeTopLine, out Plane sawStartPlane, out Plane sawEndPlane);

                //ImmersionIn & Out Planes
                Plane sawStartPlane_ImmersionIn = sawStartPlane.Clone();
                Plane sawEndPlane_ImmersionOut = sawEndPlane.Clone();

                //X-Offset
                double sawBladeSafetyOffset = 50.0;
                double immersionOffsetDisX = iCutter.Diameter / 2 + sawBladeSafetyOffset;
                ApplyOffset(ref sawStartPlane_ImmersionIn, sawStartPlane_ImmersionIn.XAxis, -immersionOffsetDisX);
                ApplyOffset(ref sawEndPlane_ImmersionOut, sawEndPlane_ImmersionOut.XAxis, immersionOffsetDisX);

                //Y-Offset  
                double sawBladeExtraLowerSideImmersion = 2.0;
                double immersionOffsetDisY = iCutter.Diameter / 2 - sawBladeExtraLowerSideImmersion;
                ApplyOffset(ref sawStartPlane_ImmersionIn, sawStartPlane_ImmersionIn.YAxis, immersionOffsetDisY);
                ApplyOffset(ref sawEndPlane_ImmersionOut, sawEndPlane_ImmersionOut.YAxis, immersionOffsetDisY);


                //OffsetIn & Out Planes
                Plane sawStartPlane_OffsetIn = sawStartPlane_ImmersionIn.Clone();
                Plane sawEndPlane_OffsetOut = sawEndPlane_ImmersionOut.Clone();

                //X-Offset
                double offsetInOffsetDisY = cassettePlate.GetDesignPlate().Height + sawBladeSafetyOffset;
                ApplyOffset(ref sawStartPlane_OffsetIn, sawStartPlane_ImmersionIn.YAxis, offsetInOffsetDisY);
                ApplyOffset(ref sawEndPlane_OffsetOut, sawEndPlane_ImmersionOut.YAxis, offsetInOffsetDisY);


                //Start Frame
                if (i == 0)
                {
                    Plane sawStartTaskPlane_OffsetIn = sawStartPlane_OffsetIn.Clone();
                    double sawTask_OffsetInDistance = iCutter.Diameter / 2 + sawBladeSafetyOffset * 2;

                    ApplyOffset(ref sawStartTaskPlane_OffsetIn, sawStartTaskPlane_OffsetIn.YAxis, sawTask_OffsetInDistance);

                    sawPlaneDesignRegion.Add(sawStartTaskPlane_OffsetIn);
                }


                sawPlaneDesignRegion.Add(sawStartPlane_OffsetIn);
                sawPlaneDesignRegion.Add(sawStartPlane_ImmersionIn);
                sawPlaneDesignRegion.Add(sawEndPlane_ImmersionOut);
                sawPlaneDesignRegion.Add(sawEndPlane_OffsetOut);


                //EndFrame
                if (i == cassettePlate.GetDesignPlate().DesignRegion.Count-1)
                {
                    Plane sawStartTaskPlane_OffsetOut = sawEndPlane_OffsetOut.Clone();
                    double sawTask_OffsetOutDistance = iCutter.Diameter / 2 + sawBladeSafetyOffset * 2;

                    ApplyOffset(ref sawStartTaskPlane_OffsetOut, sawStartTaskPlane_OffsetOut.YAxis, sawTask_OffsetOutDistance);

                    sawPlaneDesignRegion.Add(sawStartTaskPlane_OffsetOut);
                }

                //Orient Planes
                List<Plane> orientedPlanes = new List<Plane>();
                foreach (Plane plane in sawPlaneDesignRegion)
                {
                    //Rotate by 180 degree
                    plane.Rotate(Math.PI/2, plane.ZAxis, plane.Origin);

                    sawMillingPlanes.Add(plane);
                    Plane orientedPlane = FabUtilities.OrientPlane(plane, cassettePlate.RefPln_Situ, designRegion.PlaneDict["RefPln_FabOut"]);
                    orientedPlanes.Add(orientedPlane);
                }

                //Add All Milling Task Informations
                double linearAxisValue = FabUtilities.GetLinAxisRadiusBasedList(orientedPlanes, iFabActor.LinearAxis);
                foreach (Plane orientedPlane in orientedPlanes)
                { 
                    fabTask_SawCutEdges.AddMainFrames(orientedPlane);
                    //Add E1 & E2
                    fabTask_SawCutEdges.AddMainExternalValues("E1", linearAxisValue);
                    fabTask_SawCutEdges.AddMainExternalValues("E2", designRegion.DoubleDict["Angle_FabOut"]);

                    //Add Remain Information
                    fabTask_SawCutEdges.AssociateElement(cassettePlate);
                    fabTask_SawCutEdges.StaticEnvs[cassettePlate.EnvFab.Name] = cassettePlate.EnvFab;
                    fabTask_SawCutEdges.Action[iSawCutAction.Name] = iSawCutAction;
                    fabTask_SawCutEdges.Actors[iFabActor.Name] = iFabActor;
                    fabTask_SawCutEdges.Tools[iCutter.Name] = iCutter;

                    GeometryBase sawCutGeo = FabUtilities.OrientGeometryBase(cassettePlate.Geometry, cassettePlate.RefPln_Situ, cassettePlate.RefPln_Fab);
                    sawCutGeo = FabUtilities.OrientGeometryBase(sawCutGeo, cassettePlate.RefPln_Fab, designRegion.PlaneDict["RefPln_FabOut"]);
                    fabTask_SawCutEdges.Geometry.Add(sawCutGeo);


                }


            }

            //-----------
            //OUTPUTS
            //-----------
            DA.SetData("FabCassette", fabCassette);
            DA.SetData("SawCutEdges", fabTask_SawCutEdges);
            DA.SetDataList("SawMillingPlanesSitu", sawMillingPlanes);
        }

        public void GetSawStartEndPlane(Plane mitrePlane, Line outerEdgeBotLine, Line outerEdgeTopLine, out Plane sawStartPlane, out Plane sawEndPlane)
        {
            //Get Start Plane for SawCut
            Plane botStartMitrePlane = mitrePlane.Clone();
            Plane topStartMitrePlane = mitrePlane.Clone();
            botStartMitrePlane.Origin = outerEdgeBotLine.PointAt(0.0);
            topStartMitrePlane.Origin = outerEdgeTopLine.PointAt(0.0);

            //get distance from botStartMitrePlane & topStartMitrePlane to MitrePlane and select Plane with closest distance       
            mitrePlane.RemapToPlaneSpace(botStartMitrePlane.Origin, out Point3d relativeOriginBotStartPlane);
            mitrePlane.RemapToPlaneSpace(topStartMitrePlane.Origin, out Point3d relativeOriginTopStartPlane);

            sawStartPlane = botStartMitrePlane.Clone();
            if (relativeOriginBotStartPlane.X > relativeOriginTopStartPlane.X)
            {
                Vector3d moveSawStartPlane = mitrePlane.XAxis;
                moveSawStartPlane.Unitize();
                moveSawStartPlane *= (relativeOriginTopStartPlane.X - relativeOriginBotStartPlane.X);

                sawStartPlane.Origin += moveSawStartPlane;
            }

            //Get End Plane for SawCut
            Plane botEndMitrePlane = mitrePlane.Clone();
            Plane topEndMitrePlane = mitrePlane.Clone();
            botEndMitrePlane.Origin = outerEdgeBotLine.PointAt(1.0);
            topEndMitrePlane.Origin = outerEdgeTopLine.PointAt(1.0);

            //get distance from botStartMitrePlane & topStartMitrePlane to MitrePlane and select Plane with closest distance       
            mitrePlane.RemapToPlaneSpace(botEndMitrePlane.Origin, out Point3d relativeOriginBotEndPlane);
            mitrePlane.RemapToPlaneSpace(topEndMitrePlane.Origin, out Point3d relativeOriginTopEndPlane);

            sawEndPlane = botEndMitrePlane.Clone();
            if (relativeOriginBotEndPlane.X < relativeOriginTopEndPlane.X)
            {
                Vector3d moveSawEndPlane = mitrePlane.XAxis;
                moveSawEndPlane.Unitize();
                moveSawEndPlane *= (relativeOriginTopEndPlane.X - relativeOriginBotEndPlane.X);

                sawEndPlane.Origin += moveSawEndPlane;
            }

        }


        void ApplyOffset(ref Plane plane, Vector3d axis, double offset)
        {
            axis.Unitize();
            axis *= offset;
            plane.Origin += axis;
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
            get { return new Guid("8edfec29-22ce-4385-bb32-9ac346a0acbf"); }
        }
    }
}
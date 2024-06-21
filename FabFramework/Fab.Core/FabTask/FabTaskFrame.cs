using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Fab.Core.FabEnvironment;
using Fab.Core.FabElement;
using Fab.Core.FabUtilities;
using System.Net;
using Rhino.Render.DataSources;
using static Rhino.UI.Controls.CollapsibleSectionImpl;
using Eto.Forms;
using GH_IO.Serialization;
using static System.Collections.Specialized.BitVector32;
using System.Xml.Linq;
using Fab.Core.FabCollection;
using Grasshopper.Kernel.Geometry;
using Rhino.Geometry.Intersect;

namespace Fab.Core.FabTask
{
    public class FabTaskFrame : FabTask
    {
        #region properties
        //Properties
        public List<Rhino.Geometry.Plane> Main_Frames
        {
            get { return main_Frames; }
            set { main_Frames = value; }
        }

        public List<Rhino.Geometry.Plane> Sub_Frames
        {
            get { return sub_Frames; }
            set { sub_Frames = value; }
        }

        public Dictionary<string, List<double>> Main_ExtValues
        {
            get { return main_ExtValues; }
            set { main_ExtValues = value; }
        }
        public Dictionary<string, List<double>> Sub_ExtValues
        {
            get { return sub_ExtValues; }
            set { sub_ExtValues = value; }
        }

        public List<int> State
        {
            get { return state; }
            set { state = value; }
        }

        public List<double> Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public List<double> Speed
        {
            get { return speed; }
            set { speed = value; }
        }


        #endregion

        //Field of Variables

        private List<Rhino.Geometry.Plane> main_Frames;
        private List<Rhino.Geometry.Plane> sub_Frames;

        private Dictionary<string, List<double>> main_ExtValues;
        private Dictionary<string, List<double>> sub_ExtValues;

        private List<int> state;
        private List<double> offset;
        private List<double> speed;

        public FabTaskFrame() : base()
        {
            main_ExtValues = new Dictionary<string, List<double>>();
            sub_ExtValues = new Dictionary<string, List<double>>();
        }
        public FabTaskFrame(string name) : base(name)
        {
            main_ExtValues = new Dictionary<string, List<double>>();
            sub_ExtValues = new Dictionary<string, List<double>>();
        }

        public void AddMainFrames(Rhino.Geometry.Plane frame)
        {
            AddFramesToList(frame, true);
        }

        public void AddSubFrames(Rhino.Geometry.Plane frame)
        {
            AddFramesToList(frame, false);
        }

        public void AddMainFrames(IEnumerable<Rhino.Geometry.Plane> frames)
        {
            AddFramesToList(frames, true);
        }

        public void AddSubFrames(IEnumerable<Rhino.Geometry.Plane> frames)
        {
            AddFramesToList(frames, false);
        }

        private void AddFramesToList(Rhino.Geometry.Plane frame, bool isMain)
        {
            AddFramesToList(new List<Rhino.Geometry.Plane> { frame }, isMain);
        }

        private void AddFramesToList(IEnumerable<Rhino.Geometry.Plane> frames, bool isMain)
        {
            if (isMain)
            {
                if (main_Frames == null)
                    main_Frames = new List<Rhino.Geometry.Plane>();

                main_Frames.AddRange(frames);
            }
            else
            {
                if (sub_Frames == null)
                    sub_Frames = new List<Rhino.Geometry.Plane>();

                sub_Frames.AddRange(frames);
            }
        }

        public void AddMainExternalValues(string key, double value)
        {
            AddExternalValue(key, value, true);
        }

        public void AddSubExternalValues(string key, double value)
        {
            AddExternalValue(key, value, false);
        }

        public void AddMainExternalValues(string key, List<double> values)
        {
            AddExternalValue(key, values, true);
        }

        public void AddSubExternalValues(string key, List<double> values)
        {
            AddExternalValue(key, values, false);
        }

        private void AddExternalValue(string key, double value, bool isMain)
        {
            Dictionary<string, List<double>> externalAxisDictionary;

            if (isMain)
                externalAxisDictionary = main_ExtValues;
            else
                externalAxisDictionary = sub_ExtValues;

            if (!externalAxisDictionary.ContainsKey(key))
            {
                externalAxisDictionary[key] = new List<double>();
            }

            externalAxisDictionary[key].Add(value);
        }

        private void AddExternalValue(string key, List<double> values, bool isMain)
        {
            Dictionary<string, List<double>> externalAxisDictionary;

            if (isMain)
                externalAxisDictionary = main_ExtValues;
            else
                externalAxisDictionary = sub_ExtValues;

            if (!externalAxisDictionary.ContainsKey(key))
            {
                externalAxisDictionary[key] = new List<double>();
            }

            externalAxisDictionary[key].AddRange(values);
        }


        public void AddStates(int state)
        {
            if (State == null)
                State = new List<int>();

            State.Add(state);
        }

        public void AddStates(IEnumerable<int> state)
        {
            if (State == null)
                State = new List<int>();

            State.AddRange(state);
        }

        public static bool SetGlueTask(FabTaskFrame glueTask, FabPlate fabPlate, List<Rhino.Geometry.Plane> glueStartPlanes, List<Rhino.Geometry.Plane> glueEndPlanes,
            List<double> glueStartAngles, List<double> glueEndAngles, List<int> states,
            Fab.Core.FabEnvironment.Action action, Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {
            glueTask.AddMainFrames(glueStartPlanes);
            glueTask.AddSubFrames(glueEndPlanes);

            List<double> linearStartAxisValues = new List<double>();
            List<double> linearEndAxisValues = new List<double>();
            for (int i = 0; i < glueStartPlanes.Count; i++)
            {
                linearStartAxisValues.Add(FabUtilities.FabUtilities.GetLinAxisRadiusBased(glueStartPlanes[i], actor.LinearAxis));
                linearEndAxisValues.Add(FabUtilities.FabUtilities.GetLinAxisRadiusBased(glueEndPlanes[i], actor.LinearAxis));
            }

            glueTask.AddMainExternalValues("E1", linearStartAxisValues);
            glueTask.AddSubExternalValues("E1", linearEndAxisValues);

            glueTask.AddMainExternalValues("E2", glueStartAngles);
            glueTask.AddSubExternalValues("E2", glueEndAngles);

            glueTask.AddStates(states);
            glueTask.AssociateElement(fabPlate);

            glueTask.StaticEnvs[fabPlate.EnvFab.Name] = fabPlate.EnvFab;

            glueTask.Action[action.Name] = action;
            glueTask.Actors[actor.Name] = actor;
            glueTask.Tools[endeffector.Name] = endeffector; ;

            if (offsetList != null)
            {
                glueTask.Offset = offsetList;
            }

            return true;
        }

        public static bool SetNailPlateTask(FabTaskFrame nailPlateTask, FabPlate fabPlate,
            List<Rhino.Geometry.Plane> nailPlanes, List<int> states, List<double> nailAngles,
            Fab.Core.FabEnvironment.Action action, Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {

            List<double> linearAxisValues = nailPlanes.Select(plane => FabUtilities.FabUtilities.GetLinAxisRadiusBased(plane, actor.LinearAxis)).ToList();

            nailPlateTask.AddMainFrames(nailPlanes);
            nailPlateTask.AddMainExternalValues("E1", linearAxisValues);
            nailPlateTask.AddMainExternalValues("E2", nailAngles);

            nailPlateTask.AssociateElement(fabPlate);
            nailPlateTask.StaticEnvs[fabPlate.EnvFab.Name] = fabPlate.EnvFab;
            nailPlateTask.AddStates(states);
            nailPlateTask.Action[action.Name] = action;
            nailPlateTask.Actors[actor.Name] = actor;
            nailPlateTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                nailPlateTask.Offset = offsetList;
            }

            return true;
        }


        public static bool SetFabPlatePickPlaceTask_TurnTable(
            FabTaskFrame pickPlateFrameTask, FabTaskFrame placePlateFrameTask, FabPlate fabPlate,
            Fab.Core.FabEnvironment.Action pickAction, Fab.Core.FabEnvironment.Action placeAction,
            Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {

            FabUtilitiesPlate.GetPlatePNP_TurnTable(fabPlate, endeffector, out Rhino.Geometry.Plane pickPln, out Rhino.Geometry.Plane placePln, out double pickAngle, out double placeAngle);

            pickPlateFrameTask.AddMainFrames(pickPln);
            placePlateFrameTask.AddMainFrames(placePln);

            double pickLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(pickPln, actor.LinearAxis);
            double placeLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(placePln, actor.LinearAxis);

            pickPlateFrameTask.AddMainExternalValues("E1", pickLinAxisValue);
            pickPlateFrameTask.AddMainExternalValues("E2", pickAngle);

            placePlateFrameTask.AddMainExternalValues("E1", placeLinAxisValue);
            placePlateFrameTask.AddMainExternalValues("E2", placeAngle);

            pickPlateFrameTask.AssociateElement(fabPlate);
            placePlateFrameTask.AssociateElement(fabPlate);

            pickPlateFrameTask.StaticEnvs[fabPlate.EnvMag.Name] = fabPlate.EnvMag;
            placePlateFrameTask.StaticEnvs[fabPlate.EnvFab.Name] = fabPlate.EnvFab;

            pickPlateFrameTask.Action[pickAction.Name] = pickAction;
            placePlateFrameTask.Action[placeAction.Name] = placeAction;

            pickPlateFrameTask.Actors[actor.Name] = actor;
            placePlateFrameTask.Actors[actor.Name] = actor;

            pickPlateFrameTask.Tools[endeffector.Name] = endeffector;
            placePlateFrameTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                pickPlateFrameTask.Offset = offsetList;
                placePlateFrameTask.Offset = offsetList;
            }

            GeometryBase pickGeometry = FabUtilities.FabUtilities.OrientGeometryBase(fabPlate.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_Mag);
            pickPlateFrameTask.Geometry.Add(pickGeometry);

            GeometryBase placeGeometry = FabUtilities.FabUtilities.OrientGeometryBase(pickGeometry, pickPln, placePln);
            placePlateFrameTask.Geometry.Add(placeGeometry);

            return true;
        }

        public static bool SetFabElementPickPlaceTask(
        FabTaskFrame pickBeamFrameTask, FabTaskFrame placeBeamFrameTask, FabElement.FabElement fabElement,
        Fab.Core.FabEnvironment.Action pickAction, Fab.Core.FabEnvironment.Action placeAction,
        Endeffector endeffector, Actor actor, List<double> offsetPickList = null, List<double> offsetPlaceList = null, Rhino.Geometry.Plane? placePlane = null)
        {

            Rhino.Geometry.Plane pickPln = fabElement.GetDesignElement().FrameUpper;
            pickPln = FabUtilities.FabUtilities.OrientPlane(pickPln, fabElement.RefPln_Situ, fabElement.RefPln_Mag);


            Rhino.Geometry.Plane placePln;

            if (placePlane == null || placePlane == Rhino.Geometry.Plane.Unset)
            {
                placePln = fabElement.GetDesignElement().FrameUpper;
            }
            else
            {
                placePln = placePlane.Value;
            }
            placePln = FabUtilities.FabUtilities.OrientPlane(placePln, fabElement.RefPln_Situ, fabElement.RefPln_FabOut);


            pickBeamFrameTask.AddMainFrames(pickPln);
            placeBeamFrameTask.AddMainFrames(placePln);

            //check for linearAxis of actor
            if (actor.LinearAxis != null)
            {
                double pickLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(pickPln, actor.LinearAxis);
                double placeLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(placePln, actor.LinearAxis);

                pickBeamFrameTask.AddMainExternalValues("E1", pickLinAxisValue);
                placeBeamFrameTask.AddMainExternalValues("E1", placeLinAxisValue);
            }


            pickBeamFrameTask.AssociateElement(fabElement);
            placeBeamFrameTask.AssociateElement(fabElement);

            pickBeamFrameTask.StaticEnvs[fabElement.EnvMag.Name] = fabElement.EnvMag;
            placeBeamFrameTask.StaticEnvs[fabElement.EnvFab.Name] = fabElement.EnvFab;

            pickBeamFrameTask.Action[pickAction.Name] = pickAction;
            placeBeamFrameTask.Action[placeAction.Name] = placeAction;

            pickBeamFrameTask.Actors[actor.Name] = actor;
            placeBeamFrameTask.Actors[actor.Name] = actor;

            pickBeamFrameTask.Tools[endeffector.Name] = endeffector;
            placeBeamFrameTask.Tools[endeffector.Name] = endeffector;

            if (offsetPickList != null)
            {
                pickBeamFrameTask.Offset = offsetPickList;
            }

            if (offsetPlaceList != null)
            {
                placeBeamFrameTask.Offset = offsetPlaceList;
            }

            GeometryBase pickGeometry = FabUtilities.FabUtilities.OrientGeometryBase(fabElement.Geometry, fabElement.RefPln_Situ, fabElement.RefPln_Mag);
            pickBeamFrameTask.Geometry.Add(pickGeometry);

            GeometryBase placeGeometry = FabUtilities.FabUtilities.OrientGeometryBase(pickGeometry, pickPln, placePln);
            placeBeamFrameTask.Geometry.Add(placeGeometry);

            //Add To FabTaskSequence
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();
            fabCollection.AddToFabTaskSequence(fabElement.Name, pickAction.Name, placeAction.Name);

            return true;
        }


        public static bool SetFabElementDipTask(
       FabTaskFrame dipFrameTask, FabElement.FabElement fabElement,
       Fab.Core.FabEnvironment.Action dipAction,
       Endeffector endeffector, Actor actor, StaticEnv staticEnv, double dipZDepth = 0.0, double dipDuration = 10.0, List<double> offsetDipList = null)
        {


            Rhino.Geometry.Plane dipPln = staticEnv.AlignPln[0];
            //adjust dipPln by dipZDepth in Z direction
            dipPln.Origin = dipPln.Origin + dipZDepth * dipPln.ZAxis;

            dipFrameTask.AddMainFrames(dipPln);

            //check for linearAxis of actor
            if (actor.LinearAxis != null)
            {
                double dipLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(dipPln, actor.LinearAxis);

                dipFrameTask.AddMainExternalValues("E1", dipLinAxisValue);
            }

            List<double> dipDurationList = new List<double>();
            dipDurationList.Add(dipDuration);
            dipFrameTask.Speed = dipDurationList;

            dipFrameTask.AssociateElement(fabElement);

            dipFrameTask.StaticEnvs[fabElement.EnvMag.Name] = fabElement.EnvMag;

            dipFrameTask.Action[dipAction.Name] = dipAction;

            dipFrameTask.Actors[actor.Name] = actor;

            dipFrameTask.Tools[endeffector.Name] = endeffector;

            if (offsetDipList != null)
            {
                dipFrameTask.Offset = offsetDipList;
            }


            //Add To FabTaskSequence
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();
            fabCollection.AddToFabTaskSequence(fabElement.Name, dipAction.Name);

            return true;
        }


        public static bool SetPickPlaceTask(
        FabTaskFrame pickTask, FabTaskFrame placeTask, 
        Rhino.Geometry.Plane pickPlane, Rhino.Geometry.Plane placePlane,
        FabElement.FabElement fabElement,
        Fab.Core.FabEnvironment.Action pickAction, Fab.Core.FabEnvironment.Action placeAction,
        Endeffector endeffector, Actor actor, List<double> offsetPickList = null, List<double> offsetPlaceList = null)
        {

            pickTask.AddMainFrames(pickPlane);
            placeTask.AddMainFrames(placePlane);

            //check for linearAxis of actor
            if (actor.LinearAxis != null)
            {
                double pickLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(pickPlane, actor.LinearAxis);
                double placeLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(placePlane, actor.LinearAxis);

                pickTask.AddMainExternalValues("E1", pickLinAxisValue);
                placeTask.AddMainExternalValues("E1", placeLinAxisValue);
            }


            pickTask.AssociateElement(fabElement);
            placeTask.AssociateElement(fabElement);

            pickTask.Action[pickAction.Name] = pickAction;
            placeTask.Action[placeAction.Name] = placeAction;

            pickTask.Actors[actor.Name] = actor;
            placeTask.Actors[actor.Name] = actor;

            pickTask.Tools[endeffector.Name] = endeffector;
            placeTask.Tools[endeffector.Name] = endeffector;

            if (offsetPickList != null)
            {
                pickTask.Offset = offsetPickList;
            }

            if (offsetPlaceList != null)
            {
                placeTask.Offset = offsetPlaceList;
            }

            //Add To FabTaskSequence
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();
            fabCollection.AddToFabTaskSequence(fabElement.Name, pickAction.Name, placeAction.Name);

            return true;
        }


        public static bool SetFabElement(List<Rhino.Geometry.Plane> dowelPlanes,
       FabTaskFrame dowelBeamFrameTask, FabElement.FabElement fabElement,
       Fab.Core.FabEnvironment.Action dowelAction,
       Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {

            //iterate through dowelPlanes
            List<Rhino.Geometry.Plane> orientedDowelPlanes = new List<Rhino.Geometry.Plane>();
            foreach (Rhino.Geometry.Plane dowelPlane in dowelPlanes)
            {
                Rhino.Geometry.Plane orientedDowelPlane = FabUtilities.FabUtilities.OrientPlane(dowelPlane, fabElement.RefPln_Situ, fabElement.RefPln_FabOut);
                orientedDowelPlanes.Add(orientedDowelPlane);

                //check for linearAxis of actor
                if (actor.LinearAxis != null)
                {
                    double linAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(orientedDowelPlane, actor.LinearAxis);

                    dowelBeamFrameTask.AddMainExternalValues("E1", linAxisValue);
                }

            }
            dowelBeamFrameTask.AddMainFrames(orientedDowelPlanes);




            dowelBeamFrameTask.AssociateElement(fabElement);

            dowelBeamFrameTask.StaticEnvs[fabElement.EnvMag.Name] = fabElement.EnvMag;
            dowelBeamFrameTask.StaticEnvs[fabElement.EnvFab.Name] = fabElement.EnvFab;

            dowelBeamFrameTask.Action[dowelAction.Name] = dowelAction;
            dowelBeamFrameTask.Actors[actor.Name] = actor;
            dowelBeamFrameTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                dowelBeamFrameTask.Offset = offsetList;
            }


            foreach (Rhino.Geometry.Plane dowelPlane in dowelPlanes)
            {

                // Check if the dowelPlane is valid
                if (!dowelPlane.IsValid)
                {
                    throw new Exception("Invalid dowel plane encountered.");
                }


                double radiusD = 10;
                double lengthD = 80;

                //create small cyclinder
                Circle dowelCircle = new Circle(dowelPlane, radiusD);
                Cylinder dowelCylinder = new Cylinder(dowelCircle, lengthD);
                // Convert cylinder to a revolved surface
                Surface revSurface = dowelCylinder.ToRevSurface();
                // Convert the surface to a GeometryBase object
                GeometryBase dowelGeometry = revSurface.ToBrep().Surfaces[0].ToNurbsSurface();

                //Move Dowel
                // Calculate the translation vector
                Vector3d translationVector = -0.5 * lengthD * dowelPlane.ZAxis;
                // Create the transformation
                Transform translation = Transform.Translation(translationVector);
                // Apply the transformation to the dowelGeometry
                dowelGeometry.Transform(translation);

                GeometryBase orientedDowelGeometry = FabUtilities.FabUtilities.OrientGeometryBase(dowelGeometry, fabElement.RefPln_Situ, fabElement.RefPln_FabOut);
                dowelBeamFrameTask.Geometry.Add(orientedDowelGeometry);
            }


            //Add To FabTaskSequence
            FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();
            fabCollection.AddToFabTaskSequence(fabElement.Name, dowelAction.Name);

            return true;
        }

        public static bool SetFabBeamPickPlaceTask(
          FabTaskFrame pickBeamFrameTask, FabTaskFrame placeBeamFrameTask, FabBeam fabBeam,
          Fab.Core.FabEnvironment.Action pickAction, Fab.Core.FabEnvironment.Action placeAction,
          Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {

            Rhino.Geometry.Plane pickPln = fabBeam.GetDesignBeam().FrameUpper;
            pickPln = FabUtilities.FabUtilities.OrientPlane(pickPln, fabBeam.RefPln_Situ, fabBeam.RefPln_Mag);

            Rhino.Geometry.Plane placePln = fabBeam.GetDesignBeam().FrameUpper;
            placePln = FabUtilities.FabUtilities.OrientPlane(placePln, fabBeam.RefPln_Situ, fabBeam.RefPln_FabOut); 

            pickBeamFrameTask.AddMainFrames(pickPln);
            placeBeamFrameTask.AddMainFrames(placePln);


            pickBeamFrameTask.AssociateElement(fabBeam);
            placeBeamFrameTask.AssociateElement(fabBeam);

            pickBeamFrameTask.StaticEnvs[fabBeam.EnvMag.Name] = fabBeam.EnvMag;
            placeBeamFrameTask.StaticEnvs[fabBeam.EnvFab.Name] = fabBeam.EnvFab;

            pickBeamFrameTask.Action[pickAction.Name] = pickAction;
            placeBeamFrameTask.Action[placeAction.Name] = placeAction;

            pickBeamFrameTask.Actors[actor.Name] = actor;
            placeBeamFrameTask.Actors[actor.Name] = actor;

            pickBeamFrameTask.Tools[endeffector.Name] = endeffector;
            placeBeamFrameTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                pickBeamFrameTask.Offset = offsetList;
                placeBeamFrameTask.Offset = offsetList;
            }

            GeometryBase pickGeometry = FabUtilities.FabUtilities.OrientGeometryBase(fabBeam.Geometry, fabBeam.RefPln_Situ, fabBeam.RefPln_Mag);
            pickBeamFrameTask.Geometry.Add(pickGeometry);

            GeometryBase placeGeometry = FabUtilities.FabUtilities.OrientGeometryBase(pickGeometry, pickPln, placePln);
            placeBeamFrameTask.Geometry.Add(placeGeometry);

            return true;
        }

        public static bool SetFabPlatePickPlaceTask(
            FabTaskFrame pickPlateFrameTask, FabTaskFrame placePlateFrameTask, FabPlate fabPlate,
            Fab.Core.FabEnvironment.Action pickAction, Fab.Core.FabEnvironment.Action placeAction,
            Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {

            FabUtilitiesPlate.GetPlatePNP(fabPlate, endeffector, out Rhino.Geometry.Plane pickPln, out Rhino.Geometry.Plane placePln);

            pickPlateFrameTask.AddMainFrames(pickPln);
            placePlateFrameTask.AddMainFrames(placePln);

            double pickLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(pickPln, actor.LinearAxis);
            double placeLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(placePln, actor.LinearAxis);

            pickPlateFrameTask.AddMainExternalValues("E1", pickLinAxisValue);
            pickPlateFrameTask.AddMainExternalValues("E2", fabPlate.Angle_FabOut);

            placePlateFrameTask.AddMainExternalValues("E1", placeLinAxisValue);
            placePlateFrameTask.AddMainExternalValues("E2", fabPlate.Angle_FabOut);

            pickPlateFrameTask.AssociateElement(fabPlate);
            placePlateFrameTask.AssociateElement(fabPlate);

            pickPlateFrameTask.StaticEnvs[fabPlate.EnvMag.Name] = fabPlate.EnvMag;
            placePlateFrameTask.StaticEnvs[fabPlate.EnvFab.Name] = fabPlate.EnvFab;

            pickPlateFrameTask.Action[pickAction.Name] = pickAction;
            placePlateFrameTask.Action[placeAction.Name] = placeAction;

            pickPlateFrameTask.Actors[actor.Name] = actor;
            placePlateFrameTask.Actors[actor.Name] = actor;

            pickPlateFrameTask.Tools[endeffector.Name] = endeffector;
            placePlateFrameTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                pickPlateFrameTask.Offset = offsetList;
                placePlateFrameTask.Offset = offsetList;
            }

            GeometryBase pickGeometry = FabUtilities.FabUtilities.OrientGeometryBase(fabPlate.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_Mag);
            pickPlateFrameTask.Geometry.Add(pickGeometry);

            GeometryBase placeGeometry = FabUtilities.FabUtilities.OrientGeometryBase(pickGeometry, pickPln, placePln);
            placePlateFrameTask.Geometry.Add(placeGeometry);

            return true;
        }

        public static bool SetFabCassettePickPlaceTask_TurnTable(
            FabTaskFrame pickPlateFrameTask, FabTaskFrame placePlateFrameTask, FabPlate fabPlate, FabComponent fabCassette,
            Fab.Core.FabEnvironment.Action pickAction, Fab.Core.FabEnvironment.Action placeAction,
            Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {

            FabUtilitiesPlate.GetPlatePNP_TurnTable(fabPlate, endeffector, out Rhino.Geometry.Plane placePln, out Rhino.Geometry.Plane pickPln, out double placeAngle, out double pickAngle);

            //Adjust to correct cassette height
            double moveZHeight = fabCassette.GetDesignComponent().Height - fabPlate.GetDesignPlate().Height;
            placePln = FabUtilities.FabUtilities.TransformPlaneByVector(placePln, placePln.ZAxis, moveZHeight);


            pickPlateFrameTask.AddMainFrames(pickPln);
            placePlateFrameTask.AddMainFrames(placePln);

            double pickLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(pickPln, actor.LinearAxis);
            double placeLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(placePln, actor.LinearAxis);

            pickPlateFrameTask.AddMainExternalValues("E1", pickLinAxisValue);
            pickPlateFrameTask.AddMainExternalValues("E2", pickAngle);

            placePlateFrameTask.AddMainExternalValues("E1", placeLinAxisValue);
            placePlateFrameTask.AddMainExternalValues("E2", placeAngle);

            pickPlateFrameTask.AssociateElement(fabCassette);
            placePlateFrameTask.AssociateElement(fabCassette);

            pickPlateFrameTask.StaticEnvs[fabCassette.EnvFab.Name] = fabCassette.EnvFab;
            placePlateFrameTask.StaticEnvs[fabCassette.EnvMag.Name] = fabCassette.EnvMag;

            pickPlateFrameTask.Action[pickAction.Name] = pickAction;
            placePlateFrameTask.Action[placeAction.Name] = placeAction;

            pickPlateFrameTask.Actors[actor.Name] = actor;
            placePlateFrameTask.Actors[actor.Name] = actor;

            pickPlateFrameTask.Tools[endeffector.Name] = endeffector;
            placePlateFrameTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                pickPlateFrameTask.Offset = offsetList;
                placePlateFrameTask.Offset = offsetList;
            }

            GeometryBase pickGeometry = FabUtilities.FabUtilities.OrientGeometryBase(fabCassette.Geometry, fabPlate.RefPln_Situ, fabPlate.RefPln_FabOut);
            pickPlateFrameTask.Geometry.Add(pickGeometry);

            GeometryBase placeGeoemetry = FabUtilities.FabUtilities.OrientGeometryBase(pickGeometry, pickPln, placePln);
            placePlateFrameTask.Geometry.Add(placeGeoemetry);



            return true;
        }

        public static void ShiftOffsetPlaceCassette(FabTaskFrame placeCassette, FabComponent fabCassette, Actor actor, double offsetAmount)
        {
            Rhino.Geometry.Plane oldCassettePlacePlane = placeCassette.Main_Frames[0];
            Vector3d shiftedPlaceCassetteVector = new Vector3d((fabCassette.EnvMag.RefPln[0].XAxis + fabCassette.EnvMag.RefPln[0].YAxis) / 2);
            Rhino.Geometry.Plane shiftedCassettePlacePlane = FabUtilities.FabUtilities.TransformPlaneByVector(placeCassette.Main_Frames[0], shiftedPlaceCassetteVector, offsetAmount);
            Double shiftedCassetteLinearAxis = FabUtilities.FabUtilities.GetLinAxisRadiusBased(placeCassette.Main_Frames[0], actor.LinearAxis);

            placeCassette.Main_Frames[0] = shiftedCassettePlacePlane;
            placeCassette.Main_ExtValues["E1"][0] = shiftedCassetteLinearAxis;

            GeometryBase newPlaceCassetteGeo = FabUtilities.FabUtilities.OrientGeometryBase(placeCassette.Geometry[0], oldCassettePlacePlane, shiftedCassettePlacePlane);
            placeCassette.Geometry.Clear();
            placeCassette.Geometry.Add(newPlaceCassetteGeo);
        }


        public static bool SetBeamsPlaceNailTask(FabTaskFrame placeBeamTask, FabBeam fabBeam,
            List<Rhino.Geometry.Plane> placeNailPlanes, List<int> placeNailStates, List<Rhino.Geometry.Plane> nailPositionPlanes,
            Fab.Core.FabEnvironment.Action action, Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {
            placeBeamTask.AddMainFrames(placeNailPlanes);
            //placeBeamTask.AddSubFrames(nailPositionPlanes);

            for (int i = 0; i < placeNailPlanes.Count; i++)
            {

                double placeBeamLinAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(placeNailPlanes[i], actor.LinearAxis);

                placeBeamTask.AddMainExternalValues("E1", placeBeamLinAxisValue);
                //placeBeamTask.AddSubExternalValues("E1", placeBeamLinAxisValue);

                placeBeamTask.AddMainExternalValues("E2", fabBeam.Angle_FabOut);
                //placeBeamTask.AddSubExternalValues("E2", fabBeam.Angle_FabOut);
            }


            placeBeamTask.AssociateElement(fabBeam);
            placeBeamTask.StaticEnvs[fabBeam.EnvFab.Name] = fabBeam.EnvFab;
            placeBeamTask.AddStates(placeNailStates);
            placeBeamTask.Action[action.Name] = action;
            placeBeamTask.Actors[actor.Name] = actor;
            placeBeamTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                placeBeamTask.Offset = offsetList;
            }

            return true;
        }


        public static bool SetBeamsPickTask(FabTaskFrame pickBeamTask, FabBeam fabBeam, Rhino.Geometry.Plane pickPln,
            Fab.Core.FabEnvironment.Action action, Endeffector endeffector, Actor actor, List<double> offsetList = null)
        {
            pickBeamTask.AddMainFrames(pickPln);

            double linearAxisValue = FabUtilities.FabUtilities.GetLinAxisRadiusBased(pickPln, actor.LinearAxis);
            pickBeamTask.AddMainExternalValues("E1", linearAxisValue);
            pickBeamTask.AddMainExternalValues("E2", fabBeam.Angle_FabOut);

            pickBeamTask.AssociateElement(fabBeam);
            pickBeamTask.StaticEnvs[fabBeam.EnvMag.Name] = fabBeam.EnvMag;
            pickBeamTask.Action[action.Name] = action;
            pickBeamTask.Actors[actor.Name] = actor;
            pickBeamTask.Tools[endeffector.Name] = endeffector;

            if (offsetList != null)
            {
                pickBeamTask.Offset = offsetList;
            }

            pickBeamTask.Geometry.Add(FabUtilities.FabUtilities.OrientGeometryBase(fabBeam.Geometry, fabBeam.RefPln_Situ, fabBeam.RefPln_Mag));

            return true;
        }

        public void InvertExternalValues(Dictionary<string, List<double>> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                List<double> values = dict[key];
                for (int i = 0; i < values.Count; i++)
                {
                    values[i] = -values[i];
                }
            }
            else
            {
                throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");
            }
        }

        public static Dictionary<string, double> CreateTravelAxis_ExtValues(double a1 = 0.0, double a2 = 0.0, double a3 = 0.0, double a4 = 0.0, double a5 = 0.0, double a6 = 0.0, double e1 = 0.0)
        {
            return new Dictionary<string, double>
            {
                { "A1", a1 },
                { "A2", a2 },
                { "A3", a3 },
                { "A4", a4 },
                { "A5", a5 },
                { "A6", a6 },
                { "E1", e1 },
            };
        }


        public static List<FabTaskFrame> CreateTravelAxisTask(Dictionary<(string, string), (string, Dictionary<string, double>, Rhino.Geometry.Plane)> travelDictionary, Actor iFabActor, List<StaticEnv> iStaticEnvs)
        {
            List<FabTaskFrame> fabTaskFrames = new List<FabTaskFrame>();

            foreach (var kvp in travelDictionary)
            {
                string static_travel_Name = kvp.Key.Item1;
                string tool_travel_Name = kvp.Key.Item2;
                string travel_Name = kvp.Value.Item1;
                Dictionary<string, double> extValues = kvp.Value.Item2 ?? new Dictionary<string, double>();
                Rhino.Geometry.Plane plane = kvp.Value.Item3 != null ? kvp.Value.Item3 : new Rhino.Geometry.Plane();


                FabTaskFrame fabTaskFrame = new FabTaskFrame(travel_Name);
                fabTaskFrame.AddMainFrames(plane);

                foreach (var kvpExtValue in extValues)
                {
                    if (!fabTaskFrame.Main_ExtValues.ContainsKey(kvpExtValue.Key))
                    {
                        // If the key doesn't exist in the destination dictionary, add it.
                        fabTaskFrame.Main_ExtValues.Add(kvpExtValue.Key, new List<double> { kvpExtValue.Value });
                    }
                    else
                    {
                        // If the key already exists, append the value to the existing list.
                        fabTaskFrame.Main_ExtValues[kvpExtValue.Key].Add(kvpExtValue.Value);
                    }
                }

                fabTaskFrame.Actors.Add(iFabActor.Name, iFabActor);

                StaticEnv static_travel_PT_VGrip = iStaticEnvs.Find(staticEnv => staticEnv.Name == static_travel_Name);
                fabTaskFrame.StaticEnvs.Add(static_travel_Name, static_travel_PT_VGrip);

                string actionName = "Travel_AXIS";

                foreach (var kvpAction in iFabActor.Actions)
                {
                    if (kvpAction.Key == actionName)
                    {
                        // Get the existing action from iFabActor.Actions
                        Core.FabEnvironment.Action action_travel_PT_VGrip = kvpAction.Value;

                        // Add the existing action to travel_PT_VGrip.Actions.Action
                        fabTaskFrame.Action.Add(actionName, action_travel_PT_VGrip);
                    }
                }

                // Check if the tool exists in iFabActor.Tools
                if (iFabActor.Tools.ContainsKey(tool_travel_Name))
                {
                    Core.FabEnvironment.Tool tool_travel_PT_VGrip = iFabActor.Tools[tool_travel_Name];

                    // Add the action to travel_PT_VGrip.Actions.Action
                    fabTaskFrame.Tools.Add(tool_travel_Name, tool_travel_PT_VGrip);
                }

                fabTaskFrames.Add(fabTaskFrame);
            }

            return fabTaskFrames;
        }


       
    }

}

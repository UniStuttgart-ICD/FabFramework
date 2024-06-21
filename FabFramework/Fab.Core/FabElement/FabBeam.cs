using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Fab.Core;
using Fab.Core.DesignElement;
using Fab.Core.FabEnvironment;
using Fab.Core.FabUtilities;

namespace Fab.Core.FabElement
{


    public class FabBeam : FabElement
    {
        //Properties
        FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

        public string DesignBeamName
        {
            get { return designBeamName; }
            set { designBeamName = value; }
        }

        private string designBeamName;


        public FabBeam() : base() { }

        public FabBeam(string name, FabComponent parentComponent, DesignElement.DesignBeam designElement, StaticEnv envMag)
            : base(name, parentComponent, designElement, envMag)
        {
            this.DesignBeamName = designElement.Name;
            parentComponent.AddFabBeam(this);

            this.RefPln_SituBox = GetRefPlnSituBox();
            this.RefPln_Situ = GetRefPlnSitu();
            this.RefPln_Mag = FabUtilities.FabUtilities.OrientPlane(this.RefPln_Situ, this.RefPln_SituBox, envMag.RefPln[0]);
            this.RefPln_Fab = FabUtilities.FabUtilities.OrientPlane(this.RefPln_Situ, fabCollection.fabComponentCollection[this.ParentComponentName].RefPln_Situ, fabCollection.fabComponentCollection[this.ParentComponentName].RefPln_Fab);
            this.RefPln_FabOut = this.RefPln_Fab;
            this.Angle_FabOut = 0;


        }

        public Plane FabBeam_SetRefPlnSituBox_old()
        {
            //RefPln_SituBox only semi-correct calculation of frame

            //get start frame from beam at lower side of beam
            Plane beamstartFrame =  fabCollection.designBeamCollection[designBeamName].FrameLower.Clone();
            Point3d beamStartOrigin = beamstartFrame.ClosestPoint(fabCollection.designBeamCollection[designBeamName].StartPoint);
            beamstartFrame.Origin = beamStartOrigin;
            //orient refPln_SituBox by 180 to align x and y axis better
            Plane unorientedFrame = FabUtilities.FabUtilities.GetBBoxBasePlane(fabCollection.designBeamCollection[designBeamName].BoundingBox, beamstartFrame, false);
            unorientedFrame.Rotate(FabUtilities.FabUtilities.DegreeToRadian(180), unorientedFrame.ZAxis, unorientedFrame.Origin);

            return unorientedFrame;
        }

        public Plane FabBeam_SetRefPlnSitu_old()
        {
            //orient  refPln_Situ by 180 to align x and y axis better
            Plane beamRefPln_Situ = FabUtilitiesElement.GetElementCenterPlane(fabCollection.designBeamCollection[designBeamName].Geometry, fabCollection.designBeamCollection[designBeamName].FrameLower);
            beamRefPln_Situ.Rotate(FabUtilities.FabUtilities.DegreeToRadian(180), beamRefPln_Situ.ZAxis, beamRefPln_Situ.Origin);
            return beamRefPln_Situ;
        }

        public DesignBeam GetDesignBeam()
        {
            if (fabCollection.designBeamCollection.ContainsKey(designBeamName))
            {
                return fabCollection.designBeamCollection[designBeamName];
            }
            else
            {
                throw new KeyNotFoundException($"Key '{designBeamName}' not found in the designBeamCollection dictionary.");
            }
        }
    }
}

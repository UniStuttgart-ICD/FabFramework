using Fab.Core.DesignElement;
using Fab.Core.FabEnvironment;
using System.Collections.Generic;

namespace Fab.Core.FabElement
{
    public class FabPlate : FabElement
    {

        //properties
        FabCollection.FabCollection fabCollection = FabCollection.FabCollection.GetFabCollection();

        public string DesignPlateName
        {
            get { return designPlateName; }
            set { designPlateName = value; }
        }

        private string designPlateName;


        public FabPlate() : base() { }

        public FabPlate(string name, FabComponent parentComponent, DesignElement.DesignPlate designElement, StaticEnv envMag)
            : base(name, parentComponent, designElement, envMag)
        {
            this.DesignPlateName = designElement.Name;
            parentComponent.AddFabPlate(this);

            this.RefPln_SituBox = GetRefPlnSituBox();
            this.RefPln_Situ = GetRefPlnSitu();
            this.RefPln_Mag = FabUtilities.FabUtilities.OrientPlane(this.RefPln_Situ, this.RefPln_SituBox, envMag.RefPln[0]);
            this.RefPln_Fab = FabUtilities.FabUtilities.OrientPlane(this.RefPln_Situ, fabCollection.fabComponentCollection[this.ParentComponentName].RefPln_Situ, fabCollection.fabComponentCollection[this.ParentComponentName].RefPln_Fab);
            this.RefPln_FabOut = this.RefPln_Fab;
            this.Angle_FabOut = 0;
        }

        public DesignPlate GetDesignPlate()
        {
            if (fabCollection.designPlateCollection.ContainsKey(designPlateName))
            {
                return fabCollection.designPlateCollection[designPlateName];
            }
            else
            {
                throw new KeyNotFoundException($"Key '{designPlateName}' not found in the designPlateCollection dictionary.");
            }
        }

    }
}

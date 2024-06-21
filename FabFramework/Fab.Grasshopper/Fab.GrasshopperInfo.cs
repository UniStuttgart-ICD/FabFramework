using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Fab.Grasshopper
{
    public class Fab_GrasshopperInfo : GH_AssemblyInfo
    {
        public override string Name => "Fab.Grasshopper";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("87b0a073-bb71-4b32-ba05-aa3d46393196");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}
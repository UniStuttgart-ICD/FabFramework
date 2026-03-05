using Fab.Core.FabCollection;
using Fab.Grasshopper.Properties;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Fab.Grasshopper.GhComponents.GhcUtilities
{
    public class GhcGetGetObjectsByName : GH_Component
    {


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcGetGetObjectsByName()
          : base("Get Objects",
                "ObjectsByName",
                "Search FabCollection by Names to receive objects.",
                "Fab",
                "Utilities")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Names to look for.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the object", GH_ParamAccess.list);
            pManager.AddTextParameter("ObjectType", "OT", "Type of the object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Objects", "O", "Objects from FabCollectionData", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> iName = new List<string>();
            DA.GetDataList("Name", iName);

            // Initialize FabCollection
            FabCollection fabCollection = FabCollection.GetFabCollection();


            // Create a list to store the matching objects
            List<object> matchingObjects = new List<object>();

            if (iName != null && iName.Count > 0)
            {
                foreach (var name in iName)
                {
                    if (name != null)
                    {
                        bool nameFound = false;
                        if (fabCollection.designElementCollection.TryGetValue(name, out var designElement))
                        {
                            matchingObjects.Add(designElement);
                            nameFound = true;
                        }
                        if (fabCollection.fabElementCollection.TryGetValue(name, out var fabElement))
                        {
                            matchingObjects.Add(fabElement);
                            nameFound = true;
                        }
                        if (fabCollection.fabTaskCollection.TryGetValue(name, out var fabTask))
                        {
                            matchingObjects.Add(fabTask);
                            nameFound = true;
                        }
                        if (nameFound == false)
                        { throw new Exception($"Name not found: {name}"); }
                    }
                    else
                    {
                        throw new Exception($"Name: {name} is null");
                    }
                }
            }
            //else
            //{
            //    throw new Exception("iName is null or empty");
            //}


            List<string> matchingObjectsName = new List<string>();
            List<string> matchingObjectsType = new List<string>(); // New list for class names


            for (int i = 0; i < matchingObjects.Count; i++)
            {
                var nameProperty = matchingObjects[i].GetType().GetProperty("Name");
                if (nameProperty != null)
                {
                    var nameValue = nameProperty.GetValue(matchingObjects[i]);
                    matchingObjectsType.Add(matchingObjects[i].GetType().Name); // Add class name
                    matchingObjectsName.Add(nameValue?.ToString() ?? "No Name specified");
                }
                else
                {
                    matchingObjectsType.Add(matchingObjects[i].GetType().Name); // Add class name
                    matchingObjectsName.Add("No Name specified");
                }
            }


            //-----------
            //OUTPUTS
            //-----------
            DA.SetDataList("Name", matchingObjectsName);
            DA.SetDataList("ObjectType", matchingObjectsType);
            DA.SetDataList("Objects", matchingObjects);
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
                return Resources.FabFramework_Icon_CollectionID;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9862028d-dda3-4e32-a46a-4a45849b2ab9"); }
        }
    }
}
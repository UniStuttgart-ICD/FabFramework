using System.Collections.Generic;
using System.Linq;

namespace Fab.Core.FabCollection
{
    public class FabCollection
    {
        //DesignElement
        public Dictionary<string, DesignElement.DesignElement> designElementCollection;
        public Dictionary<string, DesignElement.DesignPlate> designPlateCollection;
        public Dictionary<string, DesignElement.DesignBeam> designBeamCollection;
        public Dictionary<string, DesignElement.DesignComponent> designComponentCollection;

        //FabElement
        public Dictionary<string, FabElement.FabElement> fabElementCollection;
        public Dictionary<string, FabElement.FabPlate> fabPlateCollection;
        public Dictionary<string, FabElement.FabBeam> fabBeamCollection;
        public Dictionary<string, FabElement.FabComponent> fabComponentCollection;

        //FabTask
        public Dictionary<string, FabTask.FabTask> fabTaskCollection;
        public Dictionary<string, List<string>> fabTaskSequenceSchema;



        public FabCollection()
        {
            designElementCollection = new Dictionary<string, DesignElement.DesignElement>();
            designPlateCollection = new Dictionary<string, DesignElement.DesignPlate>();
            designBeamCollection = new Dictionary<string, DesignElement.DesignBeam>();
            designComponentCollection = new Dictionary<string, DesignElement.DesignComponent>();

            fabElementCollection = new Dictionary<string, FabElement.FabElement>();
            fabPlateCollection = new Dictionary<string, FabElement.FabPlate>();
            fabBeamCollection = new Dictionary<string, FabElement.FabBeam>();
            fabComponentCollection = new Dictionary<string, FabElement.FabComponent>();

            fabTaskCollection = new Dictionary<string, FabTask.FabTask>();
            fabTaskSequenceSchema = new Dictionary<string, List<string>>();
        }


        public static FabCollection GetFabCollection()
        {
            FabCollection fabCollection = null;

            if (Rhino.RhinoDoc.ActiveDoc.RuntimeData.TryGetValue("fabCollection", out object fabCollectionObj))
            {
                fabCollection = (FabCollection)fabCollectionObj;
            }
            else
            {
                // If the key doesn't exist, create and initialize the FabCollection.
                fabCollection = InitializeFabCollection();
            }

            return fabCollection;
        }

        public static FabCollection InitializeFabCollection()
        {
            FabCollection fabCollection = new FabCollection();

            if (!Rhino.RhinoDoc.ActiveDoc.RuntimeData.ContainsKey("fabCollection"))
            {
                Rhino.RhinoDoc.ActiveDoc.RuntimeData.Add("fabCollection", fabCollection);
            }
            else
            {
                Rhino.RhinoDoc.ActiveDoc.RuntimeData["fabCollection"] = fabCollection;
            }

            return fabCollection;
        }


        public List<FabTask.FabTask> GetFabTaskAccordingToSequenceSchema()
        {
            List<FabTask.FabTask> sortedFabTasks = new List<FabTask.FabTask>();

            foreach (var pair in fabTaskSequenceSchema)
            {
                string fabElementName = pair.Key;
                List<string> actionList = pair.Value;
                FabElement.FabElement fabElement = fabElementCollection[fabElementName];


                //iterate through all actions in fabElementTask
                foreach (string action in actionList)
                {

                    //iterate through all associated FabTasks in fabElement
                    foreach (string fabTaskName in fabElement.FabTasksName)
                    {
                        FabTask.FabTask fabElementTask = fabTaskCollection[fabTaskName];


                        //iterate through all actions in actionList
                        foreach (var fabElementAction in fabElementTask.Action)
                        {

                            if (fabElementAction.Key == action)
                            {
                                sortedFabTasks.Add(fabElementTask);

                            }

                        }

                    }

                }

            }

            return sortedFabTasks;

        }



        public void AddToFabTaskSequence(string componentName, params string[] tasks)
        {
            fabTaskSequenceSchema[componentName] = new List<string>(tasks);
        }


        public void AddDesignElement(DesignElement.DesignElement designElement)
        {
            if (!designElementCollection.ContainsKey(designElement.Name))
            {
                designElementCollection.Add(designElement.Name, designElement);

                // Check the type of designElement and add it to the appropriate collection
                if (designElement is DesignElement.DesignPlate designPlate)
                {
                    AddDesignPlate(designPlate);
                }
                else if (designElement is DesignElement.DesignBeam designBeam)
                {
                    AddDesignBeam(designBeam);
                }
                else if (designElement is DesignElement.DesignComponent designComponent)
                {
                    AddDesignComponent(designComponent);
                }
            }
            else
            {
                designElementCollection[designElement.Name] = designElement;

                //The designElement already exists in designElementCollection, so check its type and update it in the appropriate collection
                if (designElementCollection[designElement.Name] is DesignElement.DesignPlate existingDesignPlate && designElement is DesignElement.DesignPlate newDesignPlate)
                {
                    AddDesignPlate(newDesignPlate);
                }
                else if (designElementCollection[designElement.Name] is DesignElement.DesignBeam existingDesignBeam && designElement is DesignElement.DesignBeam newDesignBeam)
                {
                    AddDesignBeam(newDesignBeam);
                }
                else if (designElementCollection[designElement.Name] is DesignElement.DesignComponent existingDesignComponent && designElement is DesignElement.DesignComponent newDesignComponent)
                {
                    AddDesignComponent(newDesignComponent);
                }
            }
        }

        public void AddDesignPlate(DesignElement.DesignPlate designPlate)
        {
            if (!designPlateCollection.ContainsKey(designPlate.Name))
            { designPlateCollection.Add(designPlate.Name, designPlate); }
            else { designPlateCollection[designPlate.Name] = designPlate; }
        }

        public void AddDesignBeam(DesignElement.DesignBeam designBeam)
        {
            if (!designBeamCollection.ContainsKey(designBeam.Name))
            { designBeamCollection.Add(designBeam.Name, designBeam); }
            else { designBeamCollection[designBeam.Name] = designBeam; }
        }

        public void AddDesignComponent(DesignElement.DesignComponent designComponent)
        {
            if (!designComponentCollection.ContainsKey(designComponent.Name))
            { designComponentCollection.Add(designComponent.Name, designComponent); }
            else { designComponentCollection[designComponent.Name] = designComponent; }
        }


        public void AddFabElement(FabElement.FabElement fabElement)
        {
            if (!fabElementCollection.ContainsKey(fabElement.Name))
            {
                fabElementCollection.Add(fabElement.Name, fabElement);

                // Check the type of fabElement and add it to the appropriate collection
                if (fabElement is FabElement.FabPlate fabPlate)
                {
                    AddFabPlate(fabPlate);
                }
                else if (fabElement is FabElement.FabBeam fabBeam)
                {
                    AddFabBeam(fabBeam);
                }
                else if (fabElement is FabElement.FabComponent fabComponent)
                {
                    AddFabComponent(fabComponent);
                }

            }
            else
            {

                fabElementCollection[fabElement.Name] = fabElement;

                // The fabElement already exists in fabElementCollection, so check its type and update it in the appropriate collection
                if (fabElementCollection[fabElement.Name] is FabElement.FabPlate existingFabPlate && fabElement is FabElement.FabPlate newFabPlate)
                {
                    AddFabPlate(newFabPlate);
                }
                else if (fabElementCollection[fabElement.Name] is FabElement.FabBeam existingFabBeam && fabElement is FabElement.FabBeam newFabBeam)
                {
                    AddFabBeam(newFabBeam);
                }
                else if (fabElementCollection[fabElement.Name] is FabElement.FabComponent existingFabComponent && fabElement is FabElement.FabComponent newFabComponent)
                {
                    AddFabComponent(newFabComponent);
                }
            }
        }

        public void AddFabPlate(FabElement.FabPlate fabPlate)
        {
            if (!fabPlateCollection.ContainsKey(fabPlate.Name))
            { fabPlateCollection.Add(fabPlate.Name, fabPlate); }
            else { fabPlateCollection[fabPlate.Name] = fabPlate; }
        }

        public void AddFabBeam(FabElement.FabBeam fabBeam)
        {
            if (!fabBeamCollection.ContainsKey(fabBeam.Name))
            { fabBeamCollection.Add(fabBeam.Name, fabBeam); }
            else { fabBeamCollection[fabBeam.Name] = fabBeam; }
        }

        public void AddFabComponent(FabElement.FabComponent fabComponent)
        {
            if (!fabComponentCollection.ContainsKey(fabComponent.Name))
            { fabComponentCollection.Add(fabComponent.Name, fabComponent); }
            else { fabComponentCollection[fabComponent.Name] = fabComponent; }
        }

        public void AddFabTask(FabTask.FabTask fabTask)
        {
            if (!fabTaskCollection.ContainsKey(fabTask.Name))
            { fabTaskCollection.Add(fabTask.Name, fabTask); }
            else { fabTaskCollection[fabTask.Name] = fabTask; }
        }

        public List<T> GetDictionaryValues<T>(Dictionary<string, T> dictionary)
        {
            return dictionary.Values.ToList();
        }

    }
}

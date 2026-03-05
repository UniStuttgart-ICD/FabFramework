using Fab.Grasshopper.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//This code contains parts from the repository for building Speckle Clients for Rhino and Grasshopper.
//https://github.com/speckleworks/SpeckleRhino/tree/master


namespace Fab.Grasshopper.GhComponents.GhcUtilities
{
    public class ExpandObject : GH_Component, IGH_VariableParameterComponent
    {
        Action ExpireComponent, setInputsAndExpireComponent;

        List<object> Objects = new List<object>();
        List<string> Properties = new List<string>();

        Dictionary<string, List<object>> Global;

        public ExpandObject() : base("Deconstruct Object", "EO", "Expands an object's properties. With magic.", "Fab", "Utilities") { }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("object", "O", "Object to expand.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);

            ExpireComponent = () => this.ExpireSolution(true);
            setInputsAndExpireComponent = () =>
            {
                for (int i = Params.Output.Count - 1; i >= 0; i--)
                {
                    var myParam = Params.Output[i];
                    if ((!Global.Keys.Contains(myParam.Name)) || (!Global.Keys.Contains(myParam.NickName)))
                    {
                        Params.UnregisterOutputParameter(myParam, true);
                    }
                }

                Params.OnParametersChanged();
                foreach (var key in Global.Keys)
                {
                    var myparam = Params.Output.FirstOrDefault(q => q.Name == key);
                    if (myparam == null)
                    {
                        Param_GenericObject newParam = getGhParameter(key);
                        Params.RegisterOutputParam(newParam);
                    }
                }

                Params.OnParametersChanged();
                //end
                this.ExpireSolution(true);
            };
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Objects = new List<object>();
            DA.GetDataList(0, Objects);

            // Check for null objects
            if (Objects.Any(obj => obj == null))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input contains null objects.");
                return;
            }

            List<Dictionary<string, object>> DictList = new List<Dictionary<string, object>>();

            foreach (var obj in Objects)
            {
                object FO = obj.GetType().GetProperty("Value").GetValue(obj, null);

                DictList.Add(FO.ToDictionary());
            }

            bool first = true;

            Global = new Dictionary<string, List<object>>();

            foreach (var dict in DictList)
            {
                foreach (var key in dict.Keys)
                {
                    if (first)
                    {
                        Global.Add(key, new List<object>());
                        Global[key].Add(dict[key]);
                    }
                    else if (!Global.Keys.Contains(key))
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Object dictionaries do not match.");
                        return;
                    }
                    else
                        Global[key].Add(dict[key]);
                }
                first = false;
            }

            if (Global.Keys.Count == 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Empty dictionary.");
                return;
            }

            if (Params.Output.Count != Global.Keys.Count)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Rhino.RhinoApp.MainApplicationWindow.Invoke(setInputsAndExpireComponent);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                int k = 0;
                foreach (var key in Global.Keys)
                {
                    Params.Output[k].Name = Params.Output[k].NickName = key;
                    if (Global[key] is IEnumerable)
                    {
                        DataTree<object> myTree = new DataTree<object>();
                        ToDataTree(Global[key], ref myTree, new List<int> { 0 });
                        var x = myTree;
                        DA.SetDataTree(k++, myTree);
                    }
                    else
                        DA.SetDataList(k++, Global[key]);

                }
            }

        }

        void ToDataTree(IEnumerable list, ref DataTree<object> Tree, List<int> path)
        {
            int k = 0;
            int b = 0;
            bool addedRecurse = false;
            foreach (var item in list)
            {
                if ((item is IEnumerable) && !(item is string))
                {
                    if (!addedRecurse)
                    {
                        path.Add(b);
                        addedRecurse = true;
                    }
                    else
                        path[path.Count - 1]++;

                    ToDataTree(item as IEnumerable, ref Tree, path);
                }
                else
                {
                    GH_Path Path = new GH_Path(path.ToArray());
                    Tree.Insert(item, Path, k++);
                }
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return null;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public void VariableParameterMaintenance()
        {
        }

        private Param_GenericObject getGhParameter(string key)
        {
            Param_GenericObject newParam = new Param_GenericObject();
            newParam.Name = (string)key;
            newParam.NickName = (string)key;
            newParam.MutableNickName = false;
            newParam.Access = GH_ParamAccess.tree;
            return newParam;
        }

        protected override System.Drawing.Bitmap Icon => Resources.Deconstruct_Icon;

        public override Guid ComponentGuid
        {
            get { return new Guid("{36922092-a365-4056-8f96-68bb49aeadf6}"); }
        }
    }

    public static class ToDictExtension
    {
        public static Dictionary<string, object> ToDictionary(this object myObj)
        {
            Dictionary<string, object> objDict = new Dictionary<string, object>();

            var type = myObj.GetType();
            var props = type.GetRuntimeProperties();

            var flds = myObj.GetType().GetProperties();

            foreach (var f in flds)
            {
                try
                {
                    objDict[f.Name] = f.GetValue(myObj);
                }
                catch { }
            }

            return objDict;
        }
    }
}
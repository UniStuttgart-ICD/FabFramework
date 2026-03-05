namespace Fab.Core.FabEnvironment
{
    public class Action
    {
        public int ActionID
        {
            get { return actionID; }
            set { actionID = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string ProgramName
        {
            get { return programName; }
            set { programName = value; }
        }

        private int actionID;
        private string name;
        private string type;
        private string programName;

        public Action() { }

        public Action(int actionID, string name, string type)
        {
            this.actionID = actionID;
            this.name = name;
            this.type = type;
        }
    }
}

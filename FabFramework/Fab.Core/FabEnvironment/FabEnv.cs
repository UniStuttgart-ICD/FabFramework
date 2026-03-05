using System.Collections.Generic;

namespace Fab.Core.FabEnvironment
{
    public class FabEnv
    {
        public Dictionary<string, Actor> Actors
        {
            get { return actors; }
            set { actors = value; }
        }

        public Dictionary<string, StaticEnv> StaticEnvs
        {
            get { return staticEnvs; }
            set { staticEnvs = value; }
        }

        public Dictionary<string, Tool> Tools
        {
            get { return tools; }
            set { tools = value; }
        }

        private Dictionary<string, Actor> actors;
        private Dictionary<string, StaticEnv> staticEnvs;
        private Dictionary<string, Tool> tools;

        public FabEnv() { }
        public FabEnv(Dictionary<string, Actor> Actors, Dictionary<string, StaticEnv> StaticEnvs, Dictionary<string, Tool> Tools)
        {

            this.actors = Actors;
            this.staticEnvs = StaticEnvs;
            this.tools = Tools;
        }

    }

}

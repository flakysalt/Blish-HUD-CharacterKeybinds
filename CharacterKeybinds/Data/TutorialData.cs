using System.Collections.Generic;

namespace flakysalt.CharacterKeybinds.Data
{
    public class TutorialPanel
    {
        public string ImagePath;
        public string Description;
    }
    
    public abstract class TutorialData
    {
        public abstract string Header { get;}
        public abstract List<TutorialPanel> Panels { get;}
    }
}

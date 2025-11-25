using System.Collections.Generic;
using flakysalt.CharacterKeybinds.Resources;

namespace flakysalt.CharacterKeybinds.Data.Tutorial
{
    public class SetupTutorial : TutorialData
    {
        public override string Header => TutorialLoca.initialSetupHeader;

        public override List<TutorialPanel> Panels => new List<TutorialPanel>()
        {
            new TutorialPanel()
            {
                ImagePath = @"images/tutorial/setup_1.png",
                Description = TutorialLoca.initialSetupPanel1
            }
        };
    }
}

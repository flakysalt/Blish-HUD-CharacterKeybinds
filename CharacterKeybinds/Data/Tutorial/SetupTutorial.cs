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
                ImagePath = "images/tutorial/setup_1.png",
                Description = TutorialLoca.initialSetupPanel1
            },
            new TutorialPanel()
            {
                ImagePath = "images/tutorial/setup_2.png",
                Description = TutorialLoca.initialSetupPanel2
            },
            new TutorialPanel()
            {
                ImagePath = "images/tutorial/setup_2.png",
                Description = TutorialLoca.initialSetupPanel3
            },
            new TutorialPanel()
            {
                ImagePath = "images/tutorial/setup_3.png",
                Description = TutorialLoca.initialSetupPanel4
            },
            new TutorialPanel()
            {
                ImagePath = "images/tutorial/setup_4.png",
                Description = TutorialLoca.initialSetupPanel5
            }
        };
    }
}

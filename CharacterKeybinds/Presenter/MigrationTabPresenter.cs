using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using flakysalt.CharacterKeybinds.Views;
using flakysalt.CharacterKeybinds.Model;
using flakysalt.CharacterKeybinds.Util;

namespace flakysalt.CharacterKeybinds.Presenter
{
    public class MigrationTabPresenter : Presenter<KeybindMigrationTab, MigrationTabModel>, IDisposable
    {
        
        public MigrationTabPresenter(KeybindMigrationTab view, MigrationTabModel model) : base(view, model)
        {
            View.OnDeleteClicked += View_OnDeleteClicked;
            View.OnMigrateClicked += (e,s) => _ = View_OnMigrateClicked(); 
        }

        private async Task View_OnMigrateClicked()
        {
            var migrationTaskResult = await Model.MigrateKeybindings();
            View.SetMigrationResult(migrationTaskResult);
        }

        private void View_OnDeleteClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            View.OnDeleteClicked -= View_OnDeleteClicked;
            //View.OnMigrateClicked -= View_OnMigrateClicked;
        }
    }
}
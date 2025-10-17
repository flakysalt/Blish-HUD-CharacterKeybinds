using System;
using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using flakysalt.CharacterKeybinds.Model;
using flakysalt.CharacterKeybinds.Services;
using flakysalt.CharacterKeybinds.Views;
using Microsoft.Xna.Framework;

namespace flakysalt.CharacterKeybinds.Presenter
{
    public class MainWindowPresenter : Presenter<MainWindowView,MainWindowModel>
    {
        
        Dictionary<IView,IPresenter> subPresenters;
        
        private CharacterKeybindsTabPresenter keybindsTabPresenter;
        private MigrationTabPresenter migrationTabPresenter;
        private readonly Gw2ApiService _apiService;
        private readonly CharacterKeybindsSettings _settingsModel;

        public MainWindowPresenter(Gw2ApiService apiService,CharacterKeybindsSettings settingsModel,MainWindowView view, MainWindowModel model) :
            base(view, model)
        {
            subPresenters = new Dictionary<IView, IPresenter>();
            _settingsModel = settingsModel;
            _apiService = apiService;
            CreateSubPresenters();

            View.TabChanged += OnTabChanged;
            View.WindowShown += OnWindowShown;

        }

        private void OnWindowShown(object sender, EventArgs e)
        {
            if (subPresenters.TryGetValue(View.SelectedTab.View.Invoke(), out IPresenter value))
            {
                value.DoUpdateView();
            }
        }

        public void Update(GameTime gameTime)
        {
            keybindsTabPresenter.Update(gameTime);
        }

        private void OnTabChanged(object sender, ValueChangedEventArgs<Tab> e)
        {
            if (subPresenters.TryGetValue(e.NewValue.View.Invoke(), out IPresenter value))
            {
                value.DoUpdateView();
            }
        }

        void CreateSubPresenters()
        {
            keybindsTabPresenter =
                new CharacterKeybindsTabPresenter(View.KeybindsTab,
                    new CharacterKeybindsModel(_settingsModel,_apiService),
                    _apiService,
                    _settingsModel,
                    AutoClickerView.Instance);
            
            migrationTabPresenter =
                new MigrationTabPresenter(View.MigrationTab,
                    new MigrationTabModel(_settingsModel,_apiService));
            
            subPresenters.Add(View.KeybindsTab,keybindsTabPresenter);
            subPresenters.Add(View.MigrationTab,migrationTabPresenter);


        }
    }
}
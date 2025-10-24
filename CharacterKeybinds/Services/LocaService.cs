using System;
using System.Globalization;
using Blish_HUD;
using flakysalt.CharacterKeybinds.Resources;

namespace flakysalt.CharacterKeybinds.Services
{
    public class LocaService : IDisposable
    {
        
        public event EventHandler<EventArgs> LocaleChanged;

        public static LocaService Instance;
        
        public LocaService()
        {
            Instance = this;
            GameService.Overlay.UserLocaleChanged+= OnLocaleChanged;

        }
        private void OnLocaleChanged(object sender, ValueEventArgs<CultureInfo> e)
        {
            Loca.Culture = e.Value;
            LocaleChanged?.Invoke(this, EventArgs.Empty);

        }

        public void Dispose()
        {
            GameService.Overlay.UserLocaleChanged -= OnLocaleChanged;
        }
    }
}
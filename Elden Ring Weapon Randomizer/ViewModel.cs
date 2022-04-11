using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Elden_Ring_Weapon_Randomizer
{
    internal class ViewModel : ObservableObject
    {
        public ERHook Hook { get; private set; }
        public bool GameLoaded { get; set; }
        public bool Loaded => Hook.Loaded;
        public ViewModel()
        {
            Hook = new ERHook(5000, 5000, p => p.MainWindowTitle == "ELDEN RING™");
            Hook.Start();
        }

        public void Update()
        {
            OnPropertyChanged(nameof(ContentLoaded));
            OnPropertyChanged(nameof(ForegroundLoaded));
            OnPropertyChanged(nameof(Loaded));
        }
        
        public string ContentLoaded
        {
            get
            {
                if (Hook.Loaded)
                    return "Yes";
                return "No";
            }
        }
        public Brush ForegroundLoaded
        {
            get
            {
                if (Hook.Loaded)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }

    }
}

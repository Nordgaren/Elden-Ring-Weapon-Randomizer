using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Elden_Ring_Weapon_Randomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ERItemCategory.GetItemCategories();
            ERGem.GetGems();
        }
        Timer UpdateTimer = new Timer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTimer.Interval = 16;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Enabled = true;
        }
        ERHook Hook => VModel.Hook;
        bool FormLoaded
        {
            get => VModel.GameLoaded;
            set => VModel.GameLoaded = value;
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                VModel.Update();
            }));

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateTimer.Stop();
            if (Hook.RHandRandom)
                Hook.RHandRandom = false;
            if (Hook.LHandRandom)
                Hook.LHandRandom = false;
            Hook.RestoreParams();
            //Hook.InfDurability = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var box = sender as CheckBox;

            cbxRight.IsEnabled = cbxLeft.IsEnabled = !box.IsChecked.Value;
            cbxRight.IsChecked = Hook.RHandRandom = box.IsChecked.Value;
            cbxLeft.IsChecked = Hook.LHandRandom = box.IsChecked.Value;
        }
    }
}

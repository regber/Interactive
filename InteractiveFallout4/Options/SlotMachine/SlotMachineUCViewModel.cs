using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace InteractiveFallout4.Options.SlotMachine
{
    class SlotMachineUCViewModel
    {
        public static List<string> EnableList { get; set; } = new List<string> { "Включено", "Отключено" };
        public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.SlotMachine).GetProperty("Enable"));
        public static ObservableCollection<Options.SlotMachine.Barrel> BarrelsTreeViewData { get; set; } = Options.SlotMachine.Barrels;
        private static int _BarrelNumeration=0;
        public static int BarrelNumeration
        {
            get
            {
                _BarrelNumeration++;
                return _BarrelNumeration;
            }
            set
            {
                _BarrelNumeration = value;
            }

        }

        public static OptionsCommand SlotMachineColorWindowButtonClick
        {
            get
            {
                return new OptionsCommand((Object) =>
                {
                    System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog() { FullOpen = true, Color = System.Drawing.Color.FromArgb(Options.SlotMachine.SlotMachineBackgroundColor.Red, Options.SlotMachine.SlotMachineBackgroundColor.Green, Options.SlotMachine.SlotMachineBackgroundColor.Blue) };

                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Options.SlotMachine.SlotMachineBackgroundColor.Red = colorDialog.Color.R;
                        Options.SlotMachine.SlotMachineBackgroundColor.Green = colorDialog.Color.G;
                        Options.SlotMachine.SlotMachineBackgroundColor.Blue = colorDialog.Color.B;

                        InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(()=>
                        {
                            Options.SlotMachine.BackgroundColor = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                            ((InteractiveRoulette.InteractiveRouletteWindowViewModel)InteractiveRoulette.InteractiveRouletteWindow.GetWindow().DataContext).SetBackgroundColor(Options.SlotMachine.BackgroundColor);
                        });
                    }
                });
            }
        }


    }
}

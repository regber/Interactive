using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows;

namespace InteractiveFallout4.Options.DonateBar
{
    class DonateBarUCViewModel
    {

        public static OptionsCommand DonateBarColorWindowButtonClick
        {


            get
            {
                return new OptionsCommand((Object) =>
                {
                    System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog() { FullOpen = true, Color = System.Drawing.Color.FromArgb(Options.SlotMachine.SlotMachineBackgroundColor.Red, Options.SlotMachine.SlotMachineBackgroundColor.Green, Options.SlotMachine.SlotMachineBackgroundColor.Blue) };

                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {

                        Options.DonateBar.DonateBarBackgroundColor.Red = colorDialog.Color.R;
                        Options.DonateBar.DonateBarBackgroundColor.Green = colorDialog.Color.G;
                        Options.DonateBar.DonateBarBackgroundColor.Blue = colorDialog.Color.B;

                        InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Dispatcher.Invoke(() =>
                        {
                            Options.DonateBar.BackgroundColor = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                            ((InteractiveDonateBar.InteractiveDonateBarWindowViewModel)InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().DataContext).SetBackgroundColor(Options.DonateBar.BackgroundColor);
                        });

                    }
                });
            }
        }
    }
}

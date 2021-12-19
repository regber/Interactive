using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.IO;
using System.Drawing;

namespace InteractiveFallout4.Options
{
    class OptionsViewModel
    {
        public static UserControl OptionsUC { get; set; } = new UserControl();

        public OptionsCommand AcceptButtonClick
        {
            get
            {
                return new OptionsCommand((Object) =>
              {
                  Options.OptionsSave();
                  ((Window)Object).Close();
              }
              );
            }
        }
        public OptionsCommand CancelButtonClick
        {
            get
            {
                return new OptionsCommand((Object) =>
                {
                    Options.OptionsLoad();
                    ((Window)Object).Close();
                }
                );
            }
        }
    }
}

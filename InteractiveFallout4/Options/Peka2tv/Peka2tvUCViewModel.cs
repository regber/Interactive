using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace InteractiveFallout4.Options.Peka2tv
{
    class Peka2tvUCViewModel
    {
        public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.API.Peka2tv).GetProperty("Enable"));
        public static PropertyPath Channel { get; set; } = new PropertyPath(typeof(Options.API.Peka2tv).GetProperty("Channel"));
    }
}

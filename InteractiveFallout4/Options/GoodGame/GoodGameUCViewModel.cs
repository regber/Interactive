using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;


namespace InteractiveFallout4.Options.GoodGame
{
    class GoodGameUCViewModel
    {
        public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.API.GoodGame).GetProperty("Enable"));
        public static PropertyPath Channel { get; set; } = new PropertyPath(typeof(Options.API.GoodGame).GetProperty("Channel"));
        public static PropertyPath Login { get; set; } = new PropertyPath(typeof(Options.API.GoodGame).GetProperty("Login"));
        public static PropertyPath Password { get; set; } = new PropertyPath(typeof(Options.API.GoodGame).GetProperty("Password"));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace InteractiveFallout4.Options.Twitch
{
    class TwitchUCViewModel
    {
        public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.API.Twitch).GetProperty("Enable"));
        public static PropertyPath Channel { get; set; } = new PropertyPath(typeof(Options.API.Twitch).GetProperty("Channel"));
        public static PropertyPath Login { get; set; } = new PropertyPath(typeof(Options.API.Twitch).GetProperty("Login"));
        public static PropertyPath OAuth { get; set; } = new PropertyPath(typeof(Options.API.Twitch).GetProperty("OAuth"));
    }
}

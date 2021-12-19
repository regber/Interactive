using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace InteractiveFallout4.Options.Alerts
{
    class AlertsUCViewModel
    {
        public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.Alerts).GetProperty("Enable"));
        public static List<string> EnableList { get; set; } = new List<string> { "Включено", "Отключено" };
        public static List<string> AlertTypeList { get; set; } = new List<string> { "Donate", "Premium", "Follow" };
        public static List<string> AlertVoiceList { get; set; } = new List<string> { "off", "oksana", "jane", "omazh", "zahar", "ermil" };
        public static ObservableCollection<Options.Alerts.Alert> AlertsTreeViewData { get; set; } =Options.Alerts.AlertsList;


        public static OptionsCommand AddAlertButtonClick
        {
            get
            {
                return new OptionsCommand((Object) =>
                {
                    Options.Alerts.AlertsList.Add(new Options.Alerts.Alert());
                });

            }
        }
        public static OptionsCommand AlertColorWindowButtonClick
        {
            get
            {
                return new OptionsCommand((Object) =>
                {
                    System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog() { FullOpen = true, Color = System.Drawing.Color.FromArgb(Options.Alerts.AlertsBackgroundColor.Red, Options.Alerts.AlertsBackgroundColor.Green, Options.Alerts.AlertsBackgroundColor.Blue) };

                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Options.Alerts.AlertsBackgroundColor.Red = colorDialog.Color.R;
                        Options.Alerts.AlertsBackgroundColor.Green = colorDialog.Color.G;
                        Options.Alerts.AlertsBackgroundColor.Blue = colorDialog.Color.B;

                        OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke(()=>
                        {
                            Options.Alerts.BackgroundColor = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                            OldAlertWindow.AlertWindow.GetWindow().SetBackgroundColor(Options.Alerts.BackgroundColor);
                        });
                    }
                });
            }
        }


        public static OptionsCommand Delete
        {
            get
            {
                return  new OptionsCommand(obj =>
                    {
                        Options.Alerts.Alert currentAlert = ((Options.Alerts.Alert)(((ContextMenu)((MenuItem)obj).Parent).DataContext));

                        Options.Alerts.AlertsList.Remove(currentAlert);

                    });
            }

        }
        public static OptionsCommand ImageUriBrowser
        {

            get
            {
                return new OptionsCommand(obj =>
                {
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.FileName = "image"; // Default file name
                                            //dlg.DefaultExt = ".gif"; // Default file extension
                    dlg.Filter = "Image files (*.png;*.gif;*.jpeg;*.jpg)|*.png;*.gif;*.jpeg;*.jpg|All files (*.*)|*.*"; // Filter files by extension

                    // Show open file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    //Options.Alerts.Alert currentAlert = ((Options.Alerts.Alert)(((StackPanel)((Button)obj).Parent).DataContext));

                    //Если Alert
                    if (obj is Options.Alerts.Alert Alert)
                    {
                        string RelativeImagePath = @"\AlertImages\";

                        if (result == true)
                        {
                            //если файл с таким именем уже существует
                            if (File.Exists(Directory.GetCurrentDirectory() + RelativeImagePath + Path.GetFileName(dlg.FileName)))
                            {
                                Alert.ImageUri = RelativeImagePath + Path.GetFileName(dlg.FileName);
                            }
                            else
                            {
                                File.Copy(dlg.FileName, Directory.GetCurrentDirectory() + RelativeImagePath + Path.GetFileName(dlg.FileName));
                                Alert.ImageUri = RelativeImagePath + Path.GetFileName(dlg.FileName);
                            }
                        }
                        else
                        {
                            Alert.ImageUri = "";
                        }
                    }

                });
            }

        }
        public static OptionsCommand MusicUriBrowser
        {

            get
            {
                return new OptionsCommand(obj =>
                {
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.FileName = "Music";

                    dlg.Filter = "Music files (*.WAVE;*.WAV;*.MP3)|*.WAVE;*.WAV;*.MP3|All files (*.*)|*.*";

                    // Show open file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    //Options.Alerts.Alert currentAlert = ((Options.Alerts.Alert)(((StackPanel)((Button)obj).Parent).DataContext));

                    //Если Alert
                    if (obj is Options.Alerts.Alert Alert)
                    {
                        string RelativeSoundPath = @"\AlertSounds\";

                        if (result == true)
                        {
                            //если файл с таким именем уже существует
                            if (File.Exists(Directory.GetCurrentDirectory() + RelativeSoundPath + Path.GetFileName(dlg.FileName)))
                            {
                                Alert.MusicUri = RelativeSoundPath + Path.GetFileName(dlg.FileName);
                            }
                            else
                            {
                                File.Copy(dlg.FileName, Directory.GetCurrentDirectory() + RelativeSoundPath + Path.GetFileName(dlg.FileName));
                                Alert.MusicUri = RelativeSoundPath + Path.GetFileName(dlg.FileName);
                            }
                        }
                        else
                        {
                            Alert.MusicUri = "";
                        }
                    }

                });
            }

        }

        public static OptionsCommand ShowHelp
        {
            get
            {
                return new OptionsCommand(obj =>
                {
                    MessageBox.Show("В сообщение донатного алерта можно добавлять следующие теги: \n" +
                                    "[donateSource]-Источник доната(сайт) \n" +
                                    "[donaterName]-Ник донатера \n" +
                                    "[donateAmount]-Сумма доната \n" +
                                    "[donateText]-текст донатного сообщения");
                });
            }
        }
    }
}

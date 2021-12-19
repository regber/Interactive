using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using InteractiveFallout4.Common.Crypting;
using System.Windows.Media;




namespace InteractiveFallout4.Options
{
    public static class Options
    {
        static bool GetOptions { get; set; }
        static Options()
        {
            OptionsLoad();
        }
        public static class API
        {
            public static class GoodGame
            {
                public static bool Enable { get; set; }
                public static string Channel { get; set; }
                public static string Login { get; set; }
                public static string Password { get; set; }
                static GoodGame()
                {
                    Options.GetOptions = true;
                }
            }
            public static class Twitch
            {
                public static bool Enable { get; set; }
                public static string Channel { get; set; }
                public static string Login { get; set; }
                public static string OAuth { get; set; }

                static Twitch()
                {
                    Options.GetOptions = true;
                }
            }
            public static class Peka2tv
            {
                public static bool Enable { get; set; }
                public static string Channel { get; set; }

                static Peka2tv()
                {
                    Options.GetOptions = true;
                }
            }
        }

        public static class Rutony
        {
            public static bool Enable { get; set; }
            public static string MD5Key { get; set; }

            static Rutony()
            {
                Options.GetOptions = true;
            }
        }

        public static class Donate
        {
            public static bool DonatTestEnabled { get; set; }

            static Donate()
            {
                Options.GetOptions = true;
            }
            public static class DonatePay
            {
                public static bool Enable { get; set; }
                public static string AccessToken { get; set; }

                static DonatePay()
                {
                    Options.GetOptions = true;
                }
            }
            public static class DonationAlerts
            {
                public static bool Enable { get; set; }
                public static string AccessToken { get; set; }
                public static string RefreshToken { get; set; }

                static DonationAlerts()
                {
                    Options.GetOptions = true;
                }
            }

        }
        public static class Alerts
        {
            public static Color BackgroundColor { get; set; }
            public static class AlertsBackgroundColor
            {
                public static byte Red { get; set; }
                public static byte Green { get; set; }
                public static byte Blue { get; set; }

                static AlertsBackgroundColor()
                {
                    Options.GetOptions = true;
                }
            }
            public static bool Enable { get; set; }
            public static ObservableCollection<Alert> AlertsList { get; set; }

            static Alerts()
            {
                Options.GetOptions = true;
            }

            public class Alert : ViewModel.BaseViewModel
            {
                private string _ImageUri;
                private string _MusicUri;
                public bool Enable { get; set; }
                public string Type { get; set; }
                public string ImageUri
                {
                    get => _ImageUri;
                    set
                    {
                        _ImageUri = value;
                        OnPropertyChanged("ImageUri");
                    }
                }
                public string Voice { get; set; }
                public int VoiceVolume { get; set; }
                public string Text { get; set; }
                public string MusicUri
                {
                    get => _MusicUri;
                    set
                    {
                        _MusicUri = value;
                        OnPropertyChanged("MusicUri");
                    }
                }
                public int MusicVolume { get; set; }
                public int Price { get; set; }

                public Alert()
                {
                    Type = "Donate";
                    Voice = "off";
                }

                public Alert(bool _enable, string _type, string _imageUri, string _voice, int _voiceVolume, string _text, string _musicUri, int _musicVolume, int _price)
                {
                    Enable = _enable;
                    Type = _type;
                    ImageUri = _imageUri;
                    Voice = _voice;
                    VoiceVolume = _voiceVolume;
                    Text = _text;
                    MusicUri = _musicUri;
                    MusicVolume = _musicVolume;
                    Price = _price;
                }
            }
        }

        public static class SlotMachine
        {
            public static bool Enable { get; set; }

            public static ObservableCollection<Barrel> Barrels { get; set; }
           
            public static Color BackgroundColor { get; set; }

            public static class SlotMachineBackgroundColor
            {
                
                public static byte Red { get; set; }
                public static byte Green { get; set; }
                public static byte Blue { get; set; }

                static SlotMachineBackgroundColor()
                {
                    Options.GetOptions = true;
                }
            }

            //static bool GetOptions { get; set; }
            static SlotMachine()
            {
                Options.GetOptions = true;
            }

            public class Barrel : ViewModel.BaseViewModel, IDataErrorInfo
            {
                private int _Price;

                public int Price
                {
                    get
                    {
                        return _Price;
                    }
                    set
                    {
                        _Price = value;
                        OnPropertyChanged(nameof(Price));
                    }
                }

                public string this[string columnName]
                {
                    get
                    {
                        string error = String.Empty;
                        switch (columnName)
                        {
                            case "Price":
                                for (int i = 1; i < Barrels.Count; i++)
                                {
                                    if (Barrels[i - 1].Price >= Barrels[i].Price)
                                    {
                                        error = "Числа должны идти по возврастанию";
                                        Barrels[i].Price = Barrels[i - 1].Price + 1;
                                    }
                                    else if (Barrels[0].Price != 0)
                                    {
                                        Barrels[0].Price = 0;
                                    }
                                    else if (Barrels[i - 1].Price < 0)
                                    {
                                        error = "Число должно быть положительным";
                                        Barrels[i].Price = Barrels[i - 1].Price + 1;
                                    }
                                }
                                break;
                        }
                        return error;
                    }
                }
                public string Error
                {
                    get { throw new NotImplementedException(); }
                }

                public Barrel(int price)
                {
                    Price = price;
                }
            }
        }

        
        public static class DonateBar
        {
            public static Color BackgroundColor { get; set; }

            public static class DonateBarBackgroundColor
            {
                public static byte Red { get; set; }
                public static byte Green { get; set; }
                public static byte Blue { get; set; }

                static DonateBarBackgroundColor()
                {
                    Options.GetOptions = true;
                }

            }

            static DonateBar()
            {
                Options.GetOptions = true;
            }
        }

        public static void OptionsLoad()
        {
            OptionsSerialize OpSerialize = new OptionsSerialize();

            //Deserialize
            XmlSerializer formatter = new XmlSerializer(typeof(OptionsSerialize));
            using (FileStream fs = new FileStream("Options.xml", FileMode.Open))
            {
                OpSerialize = (OptionsSerialize)formatter.Deserialize(fs);
            }

            //Goodgame
            Options.API.GoodGame.Enable = OpSerialize.APIObject.GoodGameObject.Enable;
            Options.API.GoodGame.Channel = OpSerialize.APIObject.GoodGameObject.Channel;
            Options.API.GoodGame.Login = OpSerialize.APIObject.GoodGameObject.Login;
            Options.API.GoodGame.Password = Crypting.WriteDecryptString(OpSerialize.APIObject.GoodGameObject.Password);

            //Twitch
            Options.API.Twitch.Enable = OpSerialize.APIObject.TwitchObject.Enable;
            Options.API.Twitch.Channel = OpSerialize.APIObject.TwitchObject.Channel;
            Options.API.Twitch.Login = OpSerialize.APIObject.TwitchObject.Login;
            Options.API.Twitch.OAuth = Crypting.WriteDecryptString(OpSerialize.APIObject.TwitchObject.OAuth);

            //Peka2tv
            Options.API.Peka2tv.Enable = OpSerialize.APIObject.Peka2tvObject.Enable;
            Options.API.Peka2tv.Channel = OpSerialize.APIObject.Peka2tvObject.Channel;

            //Rutony
            Options.Rutony.Enable = OpSerialize.RutonyObject.Enable;
            Options.Rutony.MD5Key = OpSerialize.RutonyObject.MD5Key;

            //Donat
            Options.Donate.DonatTestEnabled = OpSerialize.DonateObject.DonatTestEnabled;

            //DonatPay
            Options.Donate.DonatePay.Enable = OpSerialize.DonateObject.DonatePayObject.Enable;
            Options.Donate.DonatePay.AccessToken = Crypting.WriteDecryptString(OpSerialize.DonateObject.DonatePayObject.AccessToken);

            //DonatAlerts
            Options.Donate.DonationAlerts.Enable = OpSerialize.DonateObject.DonationAlertsObject.Enable;
            Options.Donate.DonationAlerts.AccessToken = Crypting.WriteDecryptString(OpSerialize.DonateObject.DonationAlertsObject.AccessToken);
            Options.Donate.DonationAlerts.RefreshToken = Crypting.WriteDecryptString(OpSerialize.DonateObject.DonationAlertsObject.RefreshToken);

            //Alerts
            Options.Alerts.AlertsBackgroundColor.Red = OpSerialize.AlertsObject.BackgroundColorObject.Red;
            Options.Alerts.AlertsBackgroundColor.Green = OpSerialize.AlertsObject.BackgroundColorObject.Green;
            Options.Alerts.AlertsBackgroundColor.Blue = OpSerialize.AlertsObject.BackgroundColorObject.Blue;

            Options.Alerts.BackgroundColor= Color.FromRgb(Options.Alerts.AlertsBackgroundColor.Red, Options.Alerts.AlertsBackgroundColor.Green, Options.Alerts.AlertsBackgroundColor.Blue);

            Options.Alerts.Enable = OpSerialize.AlertsObject.Enable;

            Options.Alerts.AlertsList = new ObservableCollection<Alerts.Alert>();

            foreach (var a in OpSerialize.AlertsObject.AlertsList)
            {
                Options.Alerts.AlertsList.Add(new Alerts.Alert(a.Enable, a.Type, a.ImageUri, a.Voice, a.VoiceVolume, a.Text, a.MusicUri, a.MusicVolume, a.Price));
            }

            //SlotMachine
            Options.SlotMachine.Enable = OpSerialize.SlotMachineObject.Enable;

            Options.SlotMachine.SlotMachineBackgroundColor.Red = OpSerialize.SlotMachineObject.BackgroundColorObject.Red;
            Options.SlotMachine.SlotMachineBackgroundColor.Green = OpSerialize.SlotMachineObject.BackgroundColorObject.Green;
            Options.SlotMachine.SlotMachineBackgroundColor.Blue = OpSerialize.SlotMachineObject.BackgroundColorObject.Blue;

            Options.SlotMachine.BackgroundColor = Color.FromRgb(Options.SlotMachine.SlotMachineBackgroundColor.Red, Options.SlotMachine.SlotMachineBackgroundColor.Green, Options.SlotMachine.SlotMachineBackgroundColor.Blue);

            Options.SlotMachine.Barrels = new ObservableCollection<SlotMachine.Barrel>();

            //Заполняем стоимость барабанов, в случае если барабаны отсутствуют в Options.xml или их кол-во меньше 7 заполняем с шагом в 1000 руб.
            if (OpSerialize.SlotMachineObject.Barrels.Count < 7)
            {
                for (int i = 0; i < 7; i++)
                {
                    Options.SlotMachine.Barrels.Add(new SlotMachine.Barrel(i * 1000));
                }
            }
            else
            {
                foreach (var b in OpSerialize.SlotMachineObject.Barrels)
                {
                    Options.SlotMachine.Barrels.Add(new SlotMachine.Barrel(b.Price));
                }
            }

            //DonateBar
            Options.DonateBar.DonateBarBackgroundColor.Red = OpSerialize.DonateBarObject.BackgroundColorObject.Red;
            Options.DonateBar.DonateBarBackgroundColor.Green = OpSerialize.DonateBarObject.BackgroundColorObject.Green;
            Options.DonateBar.DonateBarBackgroundColor.Blue = OpSerialize.DonateBarObject.BackgroundColorObject.Blue;

            Options.DonateBar.BackgroundColor= Color.FromRgb(Options.DonateBar.DonateBarBackgroundColor.Red, Options.DonateBar.DonateBarBackgroundColor.Green, Options.DonateBar.DonateBarBackgroundColor.Blue);

        }

        public static void OptionsSave()
        {
            OptionsSerialize OpSerialize = new OptionsSerialize();

            //GoodGame
            OpSerialize.APIObject.GoodGameObject.Enable = Options.API.GoodGame.Enable;
            OpSerialize.APIObject.GoodGameObject.Channel = Options.API.GoodGame.Channel;
            OpSerialize.APIObject.GoodGameObject.Login = Options.API.GoodGame.Login;
            OpSerialize.APIObject.GoodGameObject.Password = Crypting.WriteEncryptString(Options.API.GoodGame.Password);

            //Twitch
            OpSerialize.APIObject.TwitchObject.Enable = Options.API.Twitch.Enable;
            OpSerialize.APIObject.TwitchObject.Channel = Options.API.Twitch.Channel;
            OpSerialize.APIObject.TwitchObject.Login = Options.API.Twitch.Login;
            OpSerialize.APIObject.TwitchObject.OAuth = Crypting.WriteEncryptString(Options.API.Twitch.OAuth);

            //Peka2tv
            OpSerialize.APIObject.Peka2tvObject.Enable = Options.API.Peka2tv.Enable;
            OpSerialize.APIObject.Peka2tvObject.Channel = Options.API.Peka2tv.Channel;

            //Rutony
            OpSerialize.RutonyObject.Enable = Options.Rutony.Enable;
            OpSerialize.RutonyObject.MD5Key = Options.Rutony.MD5Key;

            //DonateProperty
            OpSerialize.DonateObject.DonatTestEnabled = Options.Donate.DonatTestEnabled;

            //DonatePay
            OpSerialize.DonateObject.DonatePayObject.Enable = Options.Donate.DonatePay.Enable;
            OpSerialize.DonateObject.DonatePayObject.AccessToken = Crypting.WriteEncryptString(Options.Donate.DonatePay.AccessToken);

            //DonateAlerts
            OpSerialize.DonateObject.DonationAlertsObject.Enable = Options.Donate.DonationAlerts.Enable;
            OpSerialize.DonateObject.DonationAlertsObject.AccessToken = Crypting.WriteEncryptString(Options.Donate.DonationAlerts.AccessToken);
            OpSerialize.DonateObject.DonationAlertsObject.RefreshToken = Crypting.WriteEncryptString(Options.Donate.DonationAlerts.RefreshToken);

            //Alerts
            OpSerialize.AlertsObject.BackgroundColorObject.Red = Options.Alerts.AlertsBackgroundColor.Red;
            OpSerialize.AlertsObject.BackgroundColorObject.Green = Options.Alerts.AlertsBackgroundColor.Green;
            OpSerialize.AlertsObject.BackgroundColorObject.Blue = Options.Alerts.AlertsBackgroundColor.Blue;

            OpSerialize.AlertsObject.Enable = Options.Alerts.Enable;
            OpSerialize.AlertsObject.AlertsList = new ObservableCollection<OptionsSerialize.Alerts.Alert>();

            foreach (var a in Options.Alerts.AlertsList)
            {
                OpSerialize.AlertsObject.AlertsList.Add(new OptionsSerialize.Alerts.Alert(a.Enable, a.Type, a.ImageUri, a.Voice, a.VoiceVolume, a.Text, a.MusicUri, a.MusicVolume, a.Price));
            }

            //SlotMachine
            OpSerialize.SlotMachineObject.Enable = Options.SlotMachine.Enable;

            OpSerialize.SlotMachineObject.BackgroundColorObject.Red = Options.SlotMachine.SlotMachineBackgroundColor.Red;
            OpSerialize.SlotMachineObject.BackgroundColorObject.Green = Options.SlotMachine.SlotMachineBackgroundColor.Green;
            OpSerialize.SlotMachineObject.BackgroundColorObject.Blue = Options.SlotMachine.SlotMachineBackgroundColor.Blue;



            OpSerialize.SlotMachineObject.Barrels = new ObservableCollection<OptionsSerialize.SlotMachine.Barrel>();

            foreach (var b in Options.SlotMachine.Barrels)
            {

                OpSerialize.SlotMachineObject.Barrels.Add(new OptionsSerialize.SlotMachine.Barrel(b.Price));
            }

            //DonateBar
            OpSerialize.DonateBarObject.BackgroundColorObject.Red = Options.DonateBar.DonateBarBackgroundColor.Red;
            OpSerialize.DonateBarObject.BackgroundColorObject.Green = Options.DonateBar.DonateBarBackgroundColor.Green;
            OpSerialize.DonateBarObject.BackgroundColorObject.Blue = Options.DonateBar.DonateBarBackgroundColor.Blue;

            //Serialize
            XmlSerializer formatter = new XmlSerializer(typeof(OptionsSerialize));
            using (FileStream fs = new FileStream("Options.xml", FileMode.Create))
            {
                formatter.Serialize(fs, OpSerialize);
            }
        }
    }

    [Serializable]
    [XmlRoot("Options")]
    public class OptionsSerialize
    {

        [XmlElement("API")]
        public API APIObject { get; set; } = new API();
        public class API
        {
            [XmlElement("GoodGame")]
            public GoodGame GoodGameObject { get; set; } = new GoodGame();
            [XmlElement("Twitch")]
            public Twitch TwitchObject { get; set; } = new Twitch();
            [XmlElement("Peka2tv")]
            public Peka2tv Peka2tvObject { get; set; } = new Peka2tv();
            public class GoodGame
            {
                public bool Enable;
                public string Channel;
                public string Login;
                public string Password;
            }
            public class Twitch
            {
                public bool Enable;
                public string Channel;
                public string Login;
                public string OAuth;
            }

            public class Peka2tv
            {
                public bool Enable;
                public string Channel;
            }
        }
        [XmlElement("Rutony")]
        public Rutony RutonyObject { get; set; } = new Rutony();

        public class Rutony
        {
            public bool Enable;
            public string MD5Key;
        }
        [XmlElement("DonatProperty")]
        public Donate DonateObject { get; set; } = new Donate();
        public class Donate
        {
            public bool DonatTestEnabled;
            [XmlElement("DonatePay")]
            public DonatePay DonatePayObject { get; set; } = new DonatePay();
            [XmlElement("DonationAlerts")]
            public DonationAlerts DonationAlertsObject { get; set; } = new DonationAlerts();


            public class DonatePay
            {
                public bool Enable;
                public string AccessToken;
            }

            public class DonationAlerts
            {
                public bool Enable;
                public string AccessToken;
                public string RefreshToken;
            }
        }
        [XmlElement("Alerts")]
        public Alerts AlertsObject { get; set; } = new Alerts();

        public class Alerts
        {
            [XmlElement("BackgroundColor")]
            public AlertsBackgroundColor BackgroundColorObject { get; set; } = new AlertsBackgroundColor();
            public class AlertsBackgroundColor
            {
                [XmlAttribute]
                public byte Red { get; set; }
                [XmlAttribute]
                public byte Green { get; set; }
                [XmlAttribute]
                public byte Blue { get; set; }
            }

            [XmlAttribute("Enable")]
            public bool Enable;

            [XmlArrayItem("Alert")]
            public ObservableCollection<Alert> AlertsList { get; set; }


            public class Alert
            {
                [XmlAttribute]
                public bool Enable { get; set; }
                [XmlAttribute]
                public string Type { get; set; }
                [XmlAttribute]
                public string ImageUri { get; set; }
                [XmlAttribute]
                public string Voice { get; set; }
                [XmlAttribute]
                public int VoiceVolume { get; set; }
                [XmlAttribute]
                public string Text { get; set; }
                [XmlAttribute]
                public string MusicUri { get; set; }
                [XmlAttribute]
                public int MusicVolume { get; set; }
                [XmlText]
                public int Price { get; set; }
                public Alert()
                {

                }
                public Alert(bool _enable, string _type, string _imageUri, string _voice, int _voiceVolume, string _text, string _musicUri, int _musicVolume, int _price)
                {
                    Enable = _enable;
                    Type = _type;
                    ImageUri = _imageUri;
                    Voice = _voice;
                    VoiceVolume = _voiceVolume;
                    Text = _text;
                    MusicUri = _musicUri;
                    MusicVolume = _musicVolume;
                    Price = _price;
                }
            }
        }

        [XmlElement("SlotMachine")]
        public SlotMachine SlotMachineObject { get; set; } = new SlotMachine();
        public class SlotMachine
        {
            [XmlAttribute("Enable")]
            public bool Enable { get; set; }

            [XmlArrayItem("Barrel")]
            public ObservableCollection<Barrel> Barrels { get; set; }

            [XmlElement("BackgroundColor")]
            public SlotMachineBackgroundColor BackgroundColorObject { get; set; } = new SlotMachineBackgroundColor();
            public class SlotMachineBackgroundColor
            {
                [XmlAttribute]
                public byte Red { get; set; }
                [XmlAttribute]
                public byte Green { get; set; }
                [XmlAttribute]
                public byte Blue { get; set; }
            }

            public class Barrel
            {
                [XmlText]
                public int Price { get; set; }
                public Barrel()
                {

                }
                public Barrel(int _price)
                {
                    Price = _price;
                }
            }
        }

        [XmlElement("DonateBar")]
        public DonateBar DonateBarObject { get; set; } = new DonateBar();
        public class DonateBar
        {
            [XmlElement("BackgroundColor")]
            public DonateBarBackgroundColor BackgroundColorObject { get; set; } = new DonateBarBackgroundColor();
            public class DonateBarBackgroundColor
            {
                [XmlAttribute]
                public byte Red { get; set; }
                [XmlAttribute]
                public byte Green { get; set; }
                [XmlAttribute]
                public byte Blue { get; set; }
            }
        }


    }

}


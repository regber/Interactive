using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media.Animation;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using System.ComponentModel;


namespace OldAlertWindow
{
    /// <summary>
    /// Логика взаимодействия для AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window, INotifyPropertyChanged
    {
        private static AlertWindow ThisWindow;
        private Color _BackgroundColor = InteractiveFallout4.Options.Options.Alerts.BackgroundColor;
        public Color BackgroundColor
        {
            get
            {
                return _BackgroundColor;
            }
            set
            {
                _BackgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }
        public void SetBackgroundColor(Color color)
        {
            BackgroundColor = color;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        protected static void OnStaticPropertyChanged(string propertyName = "")
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        private static System.Windows.Controls.Image playAlertsImage;
        private static string pathImages = Directory.GetCurrentDirectory() + @"\AlertImages\";
        private static string pathSounds = Directory.GetCurrentDirectory() + @"\AlertSounds\";
        private static int counter = 0;
        private static Grid primaryGrid;
        private static System.Windows.Controls.TextBlock messageTextBlock;
        private static System.Windows.Controls.DockPanel resizeDockPanelCode;
        public static int AlertFontSize = 24;
        private static ScaleTransform scaleProperty;
        public static int timeLifeAlertImage = 3000;
        public static bool waitAlertVoice = true;
        private static double scaleSize = 1;
        public static event EventHandler AlertComplited;//Событие при завершении алерта
        [ThreadStatic]
        public static bool AlertRuningNow = false;
        //Переменные яндексспича
        private static MemoryStream memoryStream;
        private static HttpWebRequest httpWebRequest;
        private static HttpWebResponse httpResponse;
        private static string folderId = "b1garhk7jhpef7gonv7c";// ID каталога облака яндекса принимаем из https://console.cloud.yandex.ru/
        private static string requestContent = "{\"yandexPassportOauthToken\":\"AQAAAABDeVMHAATuwRxHhcacIU9QlPh_nQJCOj0\"}";//Заполнение запроса данными в нашем случае пройдя по ссылке https://oauth.yandex.ru/authorize?response_type=token&client_id=1a6990aa636648e9b2ef855fa7bec2fb получаем "AgAAAABDeVMHAATuwSE_BmU44kHVjZjG5loI9u4", вроде вечный
        private static Newtonsoft.Json.Linq.JObject tokenObject;//Данные по полученному на запрос токену
        private static string iamToken = null;//Сам полученный токен
        private static DateTime iamExpiresAt;//Срок действия текущего полученного токена до

        //Проигрыватель сообщения
        public static MediaElement mediaPlayer = new MediaElement();

        //Переменные метода
        private static string uriImageArg;
        //private static string userNameArg;
        //private static string textAlertArg;
        //private static string textPaymentArg;
        private static string voiceArg;
        private static int voiceVolumeArg;
        private static string uriMusicArg;
        private static int musicVolumeArg;

        //Объект для проверки создания speech файлов
        private static System.IO.FileSystemWatcher fileWatcher = new FileSystemWatcher();


        //Удалить нижерасположенные переменные 
        private static double gridWidth;
        private static double gridHeight;
        private static double intermediateValue;

        //Парметры окна
        //XDocument xDocument;
        //internal static string XMLWindowsOptions = Directory.GetCurrentDirectory() + @"\" + "XMLWindowsOptions.xml";

        public AlertWindow()
        {
            //SecurityProtocolType.Tls12 сделан для запросов на яндекс
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            this.Topmost = true;
            InitializeComponent();
            RemoveOldWavFiles();
            //LoadWindowParametrs();//Загружаем сохраненные параметры окна
            //mainWindow.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
            playAlertsImage = mainImage;
            primaryGrid = mainGrid;
            messageTextBlock = myTextBlock;
            resizeDockPanelCode = dockPanelResizeXML;
            scaleProperty = rectScale;
            //primaryGrid.Opacity = 0; пока пусть будет закоменчено
            fileWatcher.Created -= FileSpeech_Changed;/////////////////////////костыль///////////////////////////////
            fileWatcher.Created += FileSpeech_Changed;/////////////////////////проверка///////////////////////////////
            //this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Width = 600;


            gridWidth = primaryGrid.ActualWidth;
            gridHeight = primaryGrid.ActualHeight;

        }

        public static AlertWindow GetWindow()
        {
            if (ThisWindow == null)
            {
                ThisWindow = new AlertWindow();
            }
            return ThisWindow;
        }

        //При закрытии окна оно скрывается
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Пока нет намеренья закрыть окно скрываем его, ManualWindowClosing переходит в true при зыакрытии основного окна
            if (!ManualWindowClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public static bool ManualWindowClosing { get; set; } = false;


        //Новые метод по вызову алерта
        //public static void StartAlert(string uriImage, string userName, string textAlert, string textPayment, string donateAmount, string voice, int voiceVolume, string uriMusic, int musicVolume)
        public static void StartAlert(InteractiveFallout4.InteractiveBuilder.Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert alert, Newtonsoft.Json.Linq.JObject donateMessage)
        {
            StartAlert(new InteractiveFallout4.Options.Options.Alerts.Alert() { Enable= alert.Enable, ImageUri= alert.ImageUri, MusicUri= alert.MusicUri, MusicVolume= alert.MusicVolume, Price= 0, Text= alert.Text, Type="", Voice=alert.Voice, VoiceVolume=alert.VoiceVolume }, donateMessage);
        }
        public static void StartAlert(InteractiveFallout4.Options.Options.Alerts.Alert alert, Newtonsoft.Json.Linq.JObject donateMessage)
        {
            if (OldAlertWindow.AlertWindow.GetWindow().Visibility == Visibility.Visible)
            {
                AlertRuningNow = true;

                //MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + rawJObject["nick"] + "\"" + "," + "\"amount\":" + "\"" + float.Parse(rawJObject["summ"].ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                string donateSource = donateMessage["MessageSource"].ToString();
                string donaterName = donateMessage["username"].ToString();
                string donateAmount = donateMessage["amount"].ToString();
                string donateText = donateMessage["text"].ToString();

                /*
                uriImageArg = "";
                userNameArg = "";[DonaterName]
                textAlertArg = "";[AlertText]
                textPaymentArg = "";[PaymentText]
                                    [DonateAmount] 
                voiceArg = "off";
                voiceVolumeArg = 0;
                uriMusicArg = "";
                musicVolumeArg = 0;*/

                uriImageArg = alert.ImageUri;
                //userNameArg = userName;
                //textAlertArg = textAlert;//Текст из алерта
                //textPaymentArg = textPayment;//Текст из пеймента
                voiceArg = alert.Voice;
                voiceVolumeArg = alert.VoiceVolume;
                uriMusicArg = alert.MusicUri;
                musicVolumeArg = alert.MusicVolume;


                if (System.IO.File.Exists(Directory.GetCurrentDirectory() + uriImageArg))
                {
                    System.Windows.Media.Imaging.BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(Directory.GetCurrentDirectory() + uriImageArg);
                    image.EndInit();
                    WpfAnimatedGif.ImageBehavior.SetRepeatBehavior(playAlertsImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                    WpfAnimatedGif.ImageBehavior.SetAnimatedSource(playAlertsImage, image);
                }
                else
                {
                    //Очищать параметры WpfAnimatedGif в случае если нет картинки
                    WpfAnimatedGif.ImageBehavior.SetRepeatBehavior(playAlertsImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                    WpfAnimatedGif.ImageBehavior.SetAnimatedSource(playAlertsImage, null);
                }



                //--------------------------------------текст к картинке----------------------------
                string messageText = alert.Text;
                messageText = messageText.Replace("[donateSource]", donateSource);
                messageText = messageText.Replace("[donaterName]", donaterName);
                messageText = messageText.Replace("[donateAmount]", donateAmount);
                messageText = messageText.Replace("[donateText]", donateText);

                /*if (userName != "")
                {
                    messageText += userName + " ";
                }
                if (textAlert != "")
                {
                    messageText += " " + textAlert;
                }
                if (textPayment != "")
                {
                    messageText += " И сказал, ";
                    messageText += textPayment;
                }*/

                //Запихнуть в текст блок текст из сообщения
                messageTextBlock.Text = messageText;
                //messageTextBlock.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                messageTextBlock.FontSize = AlertFontSize;
                messageTextBlock.TextWrapping = TextWrapping.Wrap;
                messageTextBlock.TextTrimming = TextTrimming.WordEllipsis;
                messageTextBlock.Height = 150;
                //messageTextBlock.Width = 600;
                //messageTextBlock.Height = 300;
                /////////////

                //Анимация увеличения картинки
                DoubleAnimation alertAnimation = new DoubleAnimation();
                alertAnimation.From = 0.1;
                alertAnimation.To = scaleSize;
                alertAnimation.Duration = TimeSpan.FromMilliseconds(1500);// Время анимации
                System.Windows.Media.Animation.ElasticEase EE = new System.Windows.Media.Animation.ElasticEase();//настройка эластичности появления
                EE.EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut;
                EE.Springiness = 4;//4
                EE.Oscillations = 3;//3
                alertAnimation.EasingFunction = EE;
                //--------------------------------
                //Анимация появления картинки
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 0;
                opacityAnimation.To = 1;
                opacityAnimation.Duration = TimeSpan.FromMilliseconds(500);// Время анимации

                DoubleAnimation widthAnimation = new DoubleAnimation();//Мусорная анимация без которой не работает отображение текста
                widthAnimation.From = 500;
                widthAnimation.To = 500;
                widthAnimation.Duration = TimeSpan.FromMilliseconds(500);// Время анимации
                                                                         //Проигрываем настроенные анимации(увеличение и проявление)
                resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, opacityAnimation);
                //resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.WidthProperty, widthAnimation);//??????
                scaleProperty.BeginAnimation(ScaleTransform.ScaleXProperty, alertAnimation);
                scaleProperty.BeginAnimation(ScaleTransform.ScaleYProperty, alertAnimation);



                /////////////////////////////////////////////////////////////////////////////////////////
                DoubleAnimation alertAnimationXPos = new DoubleAnimation();
                alertAnimationXPos.From = gridWidth / 2;
                alertAnimationXPos.To = gridWidth / 2;
                alertAnimationXPos.Duration = TimeSpan.FromMilliseconds(1);// Время анимации
                scaleProperty.BeginAnimation(ScaleTransform.CenterXProperty, alertAnimationXPos);

                DoubleAnimation alertAnimationYPos = new DoubleAnimation();
                alertAnimationYPos.From = gridHeight / 2;
                alertAnimationYPos.To = gridHeight / 2;
                alertAnimationYPos.Duration = TimeSpan.FromMilliseconds(1);// Время анимации
                scaleProperty.BeginAnimation(ScaleTransform.CenterYProperty, alertAnimationYPos);
                /////////////////////////////////////////////////////////////////////////////////////////




                /////////////////////Создаем объект отслеживающий создание вав файла и запускающий по появлению файла воспроизведение файла//////////////////////////////////////////////
                fileWatcher.Path = Directory.GetCurrentDirectory();
                fileWatcher.Filter = System.IO.Path.GetFileName("/speech" + counter + ".wav");
                fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.LastAccess | NotifyFilters.FileName;
                fileWatcher.EnableRaisingEvents = true;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Формируем wav файл с сообщением который будем проигрывать
                if (voiceArg != "off")//Если текстовое сообщение не равно пустому сообщению
                {
                    getSpeech(messageText, voiceArg, "neutral", "1");
                }
                else
                {
                    if (waitAlertVoice == true)
                    {
                        if (uriMusicArg != "" && voiceArg != "off")
                        {
                            mediaPlayer = new MediaElement();
                            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                            mediaPlayer.Visibility = Visibility.Collapsed;
                            mediaPlayer.LoadedBehavior = MediaState.Manual;
                            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_MVW;
                            primaryGrid.Children.Add(mediaPlayer);
                            mediaPlayer.Volume = musicVolumeArg;
                            mediaPlayer.Play();
                        }
                        if (uriMusicArg != "" && voiceArg == "off")
                        {
                            mediaPlayer = new MediaElement();
                            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                            mediaPlayer.Visibility = Visibility.Collapsed;
                            mediaPlayer.LoadedBehavior = MediaState.Manual;
                            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_MvW;
                            primaryGrid.Children.Add(mediaPlayer);
                            mediaPlayer.Volume = musicVolumeArg;
                            mediaPlayer.Play();
                        }
                        if (uriMusicArg == "" && voiceArg != "off")
                        {
                            mediaPlayer = new MediaElement();
                            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + @"/speech" + counter + ".wav", UriKind.Relative);
                            mediaPlayer.Visibility = Visibility.Collapsed;
                            mediaPlayer.LoadedBehavior = MediaState.Manual;
                            primaryGrid.Children.Add(mediaPlayer);
                            mediaPlayer.MediaEnded += MediaPlayer_MediaEndedVoice_MVW;
                            mediaPlayer.Volume = voiceVolumeArg;
                            mediaPlayer.Play();
                        }
                    }
                    if (waitAlertVoice == false)
                    {
                        if (uriMusicArg != "" && voiceArg != "off")
                        {
                            mediaPlayer = new MediaElement();
                            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                            mediaPlayer.Visibility = Visibility.Collapsed;
                            mediaPlayer.LoadedBehavior = MediaState.Manual;
                            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_MVw;
                            primaryGrid.Children.Add(mediaPlayer);
                            mediaPlayer.Volume = musicVolumeArg;
                            mediaPlayer.Play();
                        }
                        if (uriMusicArg != "" && voiceArg == "off")
                        {
                            mediaPlayer = new MediaElement();
                            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                            mediaPlayer.Visibility = Visibility.Collapsed;
                            mediaPlayer.LoadedBehavior = MediaState.Manual;
                            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_Mvw;
                            primaryGrid.Children.Add(mediaPlayer);
                            mediaPlayer.Volume = musicVolumeArg;
                            mediaPlayer.Play();
                        }
                        if (uriMusicArg == "" && voiceArg != "off")
                        {
                            mediaPlayer = new MediaElement();
                            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + "/speech" + counter + ".wav", UriKind.Relative);
                            mediaPlayer.Visibility = Visibility.Collapsed;
                            mediaPlayer.LoadedBehavior = MediaState.Manual;
                            primaryGrid.Children.Add(mediaPlayer);
                            mediaPlayer.MediaEnded += Alert_Completed;
                            mediaPlayer.Volume = voiceVolumeArg;
                            mediaPlayer.Play();
                            //Анимация для осуществления задержки перед исчезновением
                            DoubleAnimation alertAnimationWait = new DoubleAnimation();
                            alertAnimationWait.From = 1;
                            alertAnimationWait.To = 1;
                            alertAnimationWait.Duration = TimeSpan.FromMilliseconds(timeLifeAlertImage);// Время анимации
                            alertAnimationWait.Completed += AlertAnimationUnwait_Completed;
                            resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationWait);
                        }
                    }
                }
                ///////////////////////////////////////////////////////////////////////////
                if (voiceArg == "off" && uriMusicArg == "")
                {
                    //Анимация для осуществления задержки перед исчезновением
                    DoubleAnimation alertAnimationWait = new DoubleAnimation();
                    alertAnimationWait.From = 1;
                    alertAnimationWait.To = 1;
                    alertAnimationWait.Duration = TimeSpan.FromMilliseconds(timeLifeAlertImage);// Время анимации
                    alertAnimationWait.Completed += AlertAnimationWait_Completed;
                    resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationWait);
                }
            }
            /*else
            {
                if (AlertComplited != null)
                {
                    AlertComplited(null, new EventArgs());
                }
            }*/
        }


        private static void waitLoadedVoiceMethod()
        {
            if (waitAlertVoice == true)
            {
                if (uriMusicArg != "" && voiceArg != "off")
                {
                    mediaPlayer = new MediaElement();
                    mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    mediaPlayer.LoadedBehavior = MediaState.Manual;
                    mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_MVW;
                    primaryGrid.Children.Add(mediaPlayer);
                    mediaPlayer.Volume = musicVolumeArg;
                    mediaPlayer.Play();
                }
                if (uriMusicArg != "" && voiceArg == "off")
                {
                    mediaPlayer = new MediaElement();
                    mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    mediaPlayer.LoadedBehavior = MediaState.Manual;
                    mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_MvW;
                    primaryGrid.Children.Add(mediaPlayer);
                    mediaPlayer.Volume = musicVolumeArg;
                    mediaPlayer.Play();
                }
                if (uriMusicArg == "" && voiceArg != "off")
                {
                    mediaPlayer = new MediaElement();
                    mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + @"/speech" + counter + ".wav", UriKind.Relative);
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    mediaPlayer.LoadedBehavior = MediaState.Manual;
                    primaryGrid.Children.Add(mediaPlayer);
                    mediaPlayer.MediaEnded += MediaPlayer_MediaEndedVoice_MVW;
                    mediaPlayer.Volume = voiceVolumeArg;
                    mediaPlayer.Play();
                }
            }
            if (waitAlertVoice == false)
            {
                if (uriMusicArg != "" && voiceArg != "off")
                {
                    mediaPlayer = new MediaElement();
                    mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    mediaPlayer.LoadedBehavior = MediaState.Manual;
                    mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_MVw;
                    primaryGrid.Children.Add(mediaPlayer);
                    mediaPlayer.Volume = musicVolumeArg;
                    mediaPlayer.Play();
                }
                if (uriMusicArg != "" && voiceArg == "off")
                {
                    mediaPlayer = new MediaElement();
                    mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + uriMusicArg);
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    mediaPlayer.LoadedBehavior = MediaState.Manual;
                    mediaPlayer.MediaEnded += MediaPlayer_MediaEnded_Mvw;
                    primaryGrid.Children.Add(mediaPlayer);
                    mediaPlayer.Volume = musicVolumeArg;
                    mediaPlayer.Play();
                }
                if (uriMusicArg == "" && voiceArg != "off")
                {
                    mediaPlayer = new MediaElement();
                    mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + "/speech" + counter + ".wav", UriKind.Relative);
                    mediaPlayer.Visibility = Visibility.Collapsed;
                    mediaPlayer.LoadedBehavior = MediaState.Manual;
                    primaryGrid.Children.Add(mediaPlayer);
                    mediaPlayer.MediaEnded += Alert_Completed;
                    mediaPlayer.Volume = voiceVolumeArg;
                    mediaPlayer.Play();
                    //Анимация для осуществления задержки перед исчезновением
                    DoubleAnimation alertAnimationWait = new DoubleAnimation();
                    alertAnimationWait.From = 1;
                    alertAnimationWait.To = 1;
                    alertAnimationWait.Duration = TimeSpan.FromMilliseconds(timeLifeAlertImage);// Время анимации
                    alertAnimationWait.Completed += AlertAnimationUnwait_Completed;
                    resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationWait);
                }
            }
        }
        private static void FileSpeech_Changed(object sender, FileSystemEventArgs e)
        {
            //System.Windows.MessageBox.Show("Создан новый файл speech");

            ThisWindow.Dispatcher.Invoke(()=>
            {
                OldAlertWindow.AlertWindow.waitLoadedVoiceMethod();
            });
            //MainForm1.alertWindow.Dispatcher.Invoke(new Action(() => { AlertWindow.waitLoadedVoiceMethod(); }));

        }

        private static void MediaPlayer_MediaEnded_MVW(object sender, RoutedEventArgs e)//По окончанию музыки проигрываем голос
        {
            mediaPlayer = new MediaElement();
            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + "/speech" + counter + ".wav", UriKind.Relative);
            mediaPlayer.Visibility = Visibility.Collapsed;
            mediaPlayer.LoadedBehavior = MediaState.Manual;
            primaryGrid.Children.Add(mediaPlayer);
            mediaPlayer.MediaEnded += MediaPlayer_MediaEndedVoice_MVW;
            mediaPlayer.Volume = voiceVolumeArg;
            mediaPlayer.Play();
        }
        private static void MediaPlayer_MediaEndedVoice_MVW(object sender, RoutedEventArgs e)//По окончанию голоса проигрываем затухание
        {
            //Анимация исчезновения картинки алерта
            DoubleAnimation alertAnimation = new DoubleAnimation();
            alertAnimation.From = 1;
            alertAnimation.To = 0;
            alertAnimation.Duration = TimeSpan.FromMilliseconds(500);// Время анимации
            alertAnimation.Completed += Alert_Completed;
            resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimation);
        }

        private static void MediaPlayer_MediaEnded_MvW(object sender, RoutedEventArgs e)//По окончанию музыки при MvW проигрываем анимацию задержки и вызываем затухание
        {
            //Анимация для осуществления задержки перед исчезновением
            DoubleAnimation alertAnimationWait = new DoubleAnimation();
            alertAnimationWait.From = 1;
            alertAnimationWait.To = 1;
            alertAnimationWait.Duration = TimeSpan.FromMilliseconds(timeLifeAlertImage);// Время анимации
            alertAnimationWait.Completed += AlertAnimationWait_Completed;
            resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationWait);
        }

        private static void AlertAnimationWait_Completed(object sender, EventArgs e)//Анимация затухания и вызов метода окончания
        {
            //Анимация исчезновения картинки алерта
            DoubleAnimation alertAnimationErase = new DoubleAnimation();
            alertAnimationErase.From = 1;
            alertAnimationErase.To = 0;
            alertAnimationErase.Duration = TimeSpan.FromMilliseconds(500);// Время анимации
            alertAnimationErase.Completed += Alert_Completed;
            resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationErase);
        }

        private static void Alert_Completed(object sender, EventArgs e)
        {
            AlertEndAction();
        }

        // unwait
        //----
        private static void MediaPlayer_MediaEnded_MVw(object sender, RoutedEventArgs e)
        {
            mediaPlayer = new MediaElement();
            mediaPlayer.Source = new Uri(Directory.GetCurrentDirectory() + @"/speech" + counter + ".wav", UriKind.Relative);
            mediaPlayer.Visibility = Visibility.Collapsed;
            mediaPlayer.LoadedBehavior = MediaState.Manual;
            primaryGrid.Children.Add(mediaPlayer);
            mediaPlayer.Volume = voiceVolumeArg;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEndedVoice_MVw;
            mediaPlayer.Play();
            //Анимация для осуществления задержки перед исчезновением
            DoubleAnimation alertAnimationWait = new DoubleAnimation();
            alertAnimationWait.From = 1;
            alertAnimationWait.To = 1;
            alertAnimationWait.Duration = TimeSpan.FromMilliseconds(timeLifeAlertImage);// Время анимации
            alertAnimationWait.Completed += AlertAnimationUnwait_Completed;
            resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationWait);
        }

        private static void MediaPlayer_MediaEndedVoice_MVw(object sender, RoutedEventArgs e)//По окончанию голоса проигрываем затухание
        {
            AlertEndAction();
        }

        private static void AlertAnimationUnwait_Completed(object sender, EventArgs e)//Анимация затухания и вызов метода окончания
        {
            //Анимация исчезновения картинки алерта
            DoubleAnimation alertAnimationErase = new DoubleAnimation();
            alertAnimationErase.From = 1;
            alertAnimationErase.To = 0;
            alertAnimationErase.Duration = TimeSpan.FromMilliseconds(500);// Время анимации
            resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationErase);
        }
        //---
        private static void MediaPlayer_MediaEnded_Mvw(object sender, RoutedEventArgs e)//По окончанию музыки при MvW проигрываем анимацию задержки и вызываем затухание
        {
            //Анимация для осуществления задержки перед исчезновением
            DoubleAnimation alertAnimationWait = new DoubleAnimation();
            alertAnimationWait.From = 1;
            alertAnimationWait.To = 1;
            alertAnimationWait.Duration = TimeSpan.FromMilliseconds(timeLifeAlertImage);// Время анимации
            alertAnimationWait.Completed += AlertAnimationWait_Completed;
            resizeDockPanelCode.BeginAnimation(System.Windows.Controls.DockPanel.OpacityProperty, alertAnimationWait);
        }
        //----

        private static async void getSpeech(string text, string voiceName, string voiceEmotion, string voiceSpeed)
        {
            //Параметры голоса
            try
            {
                memoryStream = new MemoryStream();

                //Получаем id канала
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize?text=" + text + "&lang=ru-RU&folderId=" + folderId + "&format=oggopus&sampleRateHertz=48000&voice=" + voiceName + "&emotion=" + voiceEmotion + "&speed=" + voiceSpeed);
                httpWebRequest.Headers.Add("Authorization", "Bearer " + iamToken);
                httpWebRequest.Method = "GET";

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream());

                httpResponse.GetResponseStream().CopyTo(memoryStream);
                File.WriteAllBytes("speech.ogg", memoryStream.ToArray());//Формируем ogg файл и конвертируем его в wav

                var startInfo = new ProcessStartInfo();
                startInfo.FileName = @"opusdec.exe";
                startInfo.Arguments = @"speech.ogg speech" + counter + ".wav";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                using (Process soxProc = Process.Start(startInfo))
                {
                    soxProc.WaitForExit();
                }

            }
            catch
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://iam.api.cloud.yandex.net/iam/v1/tokens"))
                    {
                        memoryStream = new MemoryStream();
                        request.Content = new StringContent(requestContent);
                        request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");//задаем параметры ContentType запроса

                        //Получаем ответ на запрос
                        var response = await httpClient.SendAsync(request);

                        //var responseBytes = response.Content.ReadAsByteArrayAsync();
                        await response.Content.CopyToAsync(memoryStream);

                        //File.WriteAllBytes("token.txt", ms.ToArray());
                        //var reader = new StreamReader(await responseBytes.ReadAsStreamAsync());

                        //MessageBox.Show(Newtonsoft.Json.Linq.JObject.Parse(Encoding.UTF8.GetString(ms.ToArray(), 0, ms.ToArray().Length).Trim()).ToString());
                        //Сохраняем полученный ответ в JSON объект, а так же сохраняем его отдельные значения в переменные iamToken(сам полученный токен) и iamExpiresAt(окончание срока действия полученного токена)
                        tokenObject = Newtonsoft.Json.Linq.JObject.Parse(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, memoryStream.ToArray().Length).Trim());
                        iamToken = tokenObject["iamToken"].ToString();
                        iamExpiresAt = Convert.ToDateTime(tokenObject["expiresAt"].ToString());
                        //MessageBox.Show(iamExpiresAt.ToLongTimeString());

                        //Получаем id канала
                        httpWebRequest = (HttpWebRequest)WebRequest.Create("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize?text=" + text + "&lang=ru-RU&folderId=" + folderId + "&format=oggopus&sampleRateHertz=48000&voice=" + voiceName + "&emotion=" + voiceEmotion + "&speed=" + voiceSpeed);
                        httpWebRequest.Headers.Add("Authorization", "Bearer " + iamToken);
                        httpWebRequest.Method = "GET";

                        httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        //streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream());

                        httpResponse.GetResponseStream().CopyTo(memoryStream);
                        File.WriteAllBytes("speech.ogg", memoryStream.ToArray());//Формируем ogg файл и конвертируем его в wav

                        var startInfo = new ProcessStartInfo();
                        startInfo.FileName = @"opusdec.exe";
                        startInfo.Arguments = @"speech.ogg speech" + counter + ".wav";
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = true;
                        startInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        using (Process soxProc = Process.Start(startInfo))
                        {
                            soxProc.WaitForExit();
                        }

                    }
                }
            }
            finally
            {
                httpWebRequest?.Abort();
            }
        }

        private static void AlertEndAction()
        {
            //File.Delete(Directory.GetCurrentDirectory() + @"/speech.ogg");
            mediaPlayer.Source = null;
            File.Delete(Directory.GetCurrentDirectory() + "/speech" + counter + ".wav");
            counter++;

            AlertRuningNow = false;

            if (AlertComplited != null)
            {
                AlertComplited(null, new EventArgs());
            }

            //System.Windows.MessageBox.Show("Вызываем рулетку и посос");
        }

        private static void RemoveOldWavFiles()
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());

            Regex regex = new Regex(@"speech.*.wav");
            foreach (string file in files)
            {
                MatchCollection matches = regex.Matches(file);

                if (matches.Count > 0)
                {
                    File.Delete(file);
                }
            }
        }


        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //mainWindow.Width = mainWindow.Width-1;
            //System.Windows.MessageBox.Show(subStringPanelCode.Width.ToString());
            gridWidth = primaryGrid.ActualWidth;
            gridHeight = primaryGrid.ActualHeight;
            if (gridWidth < intermediateValue)
            {
                scaleSize = scaleSize - 0.01;
            }
            if (gridWidth > intermediateValue)
            {
                scaleSize = scaleSize + 0.01;
            }

            DoubleAnimation alertAnimation = new DoubleAnimation();
            alertAnimation.From = scaleSize;
            alertAnimation.To = scaleSize;
            alertAnimation.Duration = TimeSpan.FromMilliseconds(1);// Время анимации
            scaleProperty.BeginAnimation(ScaleTransform.ScaleXProperty, alertAnimation);
            scaleProperty.BeginAnimation(ScaleTransform.ScaleYProperty, alertAnimation);
            /////////////////////////////////////////////////////////////////////////////////////////
            DoubleAnimation alertAnimationXPos = new DoubleAnimation();
            alertAnimationXPos.From = gridWidth / 2;
            alertAnimationXPos.To = gridWidth / 2;
            alertAnimationXPos.Duration = TimeSpan.FromMilliseconds(1);// Время анимации
            scaleProperty.BeginAnimation(ScaleTransform.CenterXProperty, alertAnimationXPos);

            DoubleAnimation alertAnimationYPos = new DoubleAnimation();
            alertAnimationYPos.From = gridHeight / 2;
            alertAnimationYPos.To = gridHeight / 2;
            alertAnimationYPos.Duration = TimeSpan.FromMilliseconds(1);// Время анимации
            scaleProperty.BeginAnimation(ScaleTransform.CenterYProperty, alertAnimationYPos);
            /////////////////////////////////////////////////////////////////////////////////////////
            intermediateValue = gridWidth;
        }

        /*
        private void LoadWindowParametrs()
        {
            //mainWindow.Dispatcher.Invoke(()=>
            //{
                //mainWindow.Background = InteractiveFallout4.Options.Options.Alerts.BackgroundColor;
            //});
            
        }*/
    }
}

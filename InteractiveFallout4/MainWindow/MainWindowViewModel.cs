using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Collections.ObjectModel;
using InteractiveFallout4.InteractiveBuilder;
using System.Windows;
using System.Xml.Linq;

using System.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Diagnostics;

using messageProcessor = InteractiveFallout4.MainWindow.MessageProcessor.MessageProcessor;
using InteractiveFallout4.Common.Injectors;

namespace InteractiveFallout4.MainWindow
{
    class Connected
    {
        private static int _Finish = 0;
        public static int Finish
        {
            get
            {
                return _Finish;
            }
            set
            {
                _Finish = value;
                OnStaticPropertyChanged(nameof(Finish));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }


        public static void Start()
        {
            Finish++;
        }
        public static void Done()
        {
            Finish--;
        }
    }
    class MainWindowViewModel : ViewModel.BaseViewModel
    {
        //Платформы
        ChatSource.GoodGameChat GoodGameAPI = new ChatSource.GoodGameChat();
        ChatSource.Peka2tvChat Peka2tvAPI = new ChatSource.Peka2tvChat();
        ChatSource.TwitchChat TwitchAPI = new ChatSource.TwitchChat();
        ChatSource.RutonyChat RutonyAPI = new ChatSource.RutonyChat();

        //Платформы доната
        ChatSource.DonatePay DonatePayAPI = new ChatSource.DonatePay();
        ChatSource.DonationAlerts DonationAlertsAPI = new ChatSource.DonationAlerts();

        //потоки окон
        public static Thread RouletteWindowThread;
        public static Thread DonateBarWindowThread;
        public static Thread AlertsWindowThread;

        //потоки платформ
        public static Thread GoodGameAPIThread;
        public static Thread Peka2tvAPIThread;
        public static Thread TwitchAPIThread;
        public static Thread RutonyAPIThread;

        //потоки платформ доната
        public static Thread DonatePayAPIThread;
        public static Thread DonationAlertsAPIThread;

        //поток для процессора
        public static Thread MessageProcessorThread;


        //Переменная указывающая в каком положении находится программа в подключенном или отключенном
        private bool _ApplicationIsConnectedToGame = false;
        public bool ApplicationIsConnectedToGame
        {
            get
            {
                return _ApplicationIsConnectedToGame;
            }
            set
            {
                _ApplicationIsConnectedToGame = value;
                OnPropertyChanged(nameof(ApplicationIsConnectedToGame));
            }
        }

        //Свойство для переключения консоли в рабочее или не рабочее состояние
        private static bool _enableCommandConsole = false;
        public static bool enableCommandConsole
        {
            get
            {
                return _enableCommandConsole;
            }
            set
            {
                _enableCommandConsole = value;
                OnStaticPropertyChanged(nameof(enableCommandConsole));
            }
        }

        //Текст боксы для отображения информации из чатов, донатов и т.д.
        public RichTextBox ChatMessageRichTextBox { get; set; } = new RichTextBox();
        public RichTextBox InteractiveCommandMessageRichTextBox { get; set; } = new RichTextBox();
        public RichTextBox DonateMessageRichTextBox { get; set; } = new RichTextBox();
        public RichTextBox RawMessageRichTextBox { get; set; } = new RichTextBox();

        static System.Windows.Controls.MenuItem ConnectMenuItem;


        //Костыль для обновления списка команд в основном окне при изменении выбора активного набора
        public static ObservableCollection<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand> CurrentActiveSetCommandList = new ObservableCollection<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand>();
        static MainWindowViewModel()
        {
            //Заполняем список командами из текущего активного набора
            CurrentActiveSetCommandList.Clear();
            foreach (var command in Interactive.InteractiveSets.InteractiveSetList.Where(obj => obj.Title == Interactive.InteractiveSets.ActiveSet).First().InteractiveSetCommandsList)
            {
                CurrentActiveSetCommandList.Add(command);
            }
        }

        public ViewModel.Command AddCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    var SelectedCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;

                    messageProcessor.GetCommand(SelectedCommand);

                    GetCommandMessage(SelectedCommand);
                    //System.Windows.Forms.MessageBox.Show(SelectedCommand.Title);
                });
            }
        }
        public ViewModel.Command ConnectCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    ConnectMenuItem = (System.Windows.Controls.MenuItem)obj;

                    //Подключаем или отключаем программу к игре в зависимосте от ее текущего состояния
                    if (ApplicationIsConnectedToGame == false)
                    {
                        //Проверяем запущена игра или нет и в зависимости от этого продолжаем или прерываем процесс подключения интерактива к игре
                        if (!CheckGameIsEnable())
                            return;

                        ApplicationIsConnectedToGame = true;
                        ConnectMenuItem.Header = "Отключить";

                        //Включаем консоль с командами
                        enableCommandConsole = true;

                        //Запускаем обработчик сообщений
                        ConnectMessageProcessor();
                        //Если чат ГГ подключен
                        ConnectGoodGame();
                        //если пекач подключен
                        ConnectPeka2tv();
                        //Если твич подключен
                        ConnectTwitch();
                        //Если рутони подключен
                        ConnectRutony();
                        //Если DonatePay подключен
                        ConnectDonatePay();
                        //Если DonationAlerts подключен
                        ConnectDonationAlerts();
                    }
                    else
                    {
                        ApplicationIsConnectedToGame = false;
                        ConnectMenuItem.Header = "Подключить";

                        //Отключаем консоль с командами
                        enableCommandConsole = false;

                        //Останавливаем обработчик событий
                        DisconnectMessageProcessor();
                        //Если чат ГГ подключен
                        DisconnectGoodGame();
                        //если пекач подключен
                        DisconnectPeka2tv();
                        //Если твич подключен
                        DisconnectTwitch();
                        //Если рутони подключен
                        DisconnectRutony();
                        //Если DonatePay подключен
                        DisconnectDonatePay();
                        //Если DonationAlerts подключен
                        DisconnectDonationAlerts();

                    }

                });
            }
        }

        /*
        public void DisconnectAll()
        {
            ApplicationIsConnectedToGame = false;
            ConnectMenuItem.Header = "Подключить";

            //Отключаем консоль с командами
            enableCommandConsole = false;


            //Если чат ГГ подключен
            DisconnectGoodGame();
            //если пекач подключен
            DisconnectPeka2tv();
            //Если твич подключен
            DisconnectTwitch();
            //Если рутони подключен
            DisconnectRutony();
            //Если DonatePay подключен
            DisconnectDonatePay();
            //Если DonationAlerts подключен
            DisconnectDonationAlerts();
            //Останавливаем обработчик событий
            DisconnectMessageProcessor();
        }*/

        private void DisconnectDonationAlerts()
        {
            if (Options.Options.Donate.DonationAlerts.Enable)
            {

                System.Windows.Threading.Dispatcher.FromThread(DonationAlertsAPIThread).Invoke(() =>
                {
                    Connected.Start();
                    DonationAlertsAPI.Disconnect();
                    DonationAlertsAPI.MessageEvent -= this.Get_MessageEvent;
                    Connected.Done();
                });
                DonationAlertsAPIThread?.Abort();
            }
        }
        private void DisconnectDonatePay()
        {
            if (Options.Options.Donate.DonatePay.Enable)
            {
                System.Windows.Threading.Dispatcher.FromThread(DonatePayAPIThread).Invoke(() =>
                {
                    Connected.Start();
                    DonatePayAPI.Disconnect();
                    DonatePayAPI.MessageEvent -= this.Get_MessageEvent;
                    Connected.Done();
                });
                DonatePayAPIThread?.Abort();
            }
        }
        private void DisconnectRutony()
        {
            if (Options.Options.Rutony.Enable)
            {

                System.Windows.Threading.Dispatcher.FromThread(RutonyAPIThread).Invoke(() =>
                {
                    Connected.Start();
                    RutonyAPI.Disconnect();
                    RutonyAPI.MessageEvent -= this.Get_MessageEvent;
                    Connected.Done();
                });
                RutonyAPIThread?.Abort();
            }
        }
        private void DisconnectTwitch()
        {
            if (Options.Options.API.Twitch.Enable)
            {

                System.Windows.Threading.Dispatcher.FromThread(TwitchAPIThread).Invoke(() =>
                {
                    Connected.Start();
                    TwitchAPI.Disconnect();
                    TwitchAPI.MessageEvent -= this.Get_MessageEvent;
                    Connected.Done();
                });
                TwitchAPIThread?.Abort();
            }
        }
        private void DisconnectPeka2tv()
        {
            if (Options.Options.API.Peka2tv.Enable)
            {

                System.Windows.Threading.Dispatcher.FromThread(Peka2tvAPIThread).Invoke(() =>
                {
                    Connected.Start();
                    Peka2tvAPI.Disconnect();
                    Peka2tvAPI.MessageEvent -= this.Get_MessageEvent;
                    Connected.Done();
                });
                Peka2tvAPIThread?.Abort();
            }
        }
        private void DisconnectGoodGame()
        {
            if (Options.Options.API.GoodGame.Enable)
            {
                System.Windows.Threading.Dispatcher.FromThread(GoodGameAPIThread).Invoke(() =>
                {
                    Connected.Start();
                    GoodGameAPI.Disconnect();
                    GoodGameAPI.MessageEvent -= this.Get_MessageEvent;
                    Connected.Done();
                });

                GoodGameAPIThread?.Abort();
            }
        }
        private void DisconnectMessageProcessor()
        {
            System.Windows.Threading.Dispatcher.FromThread(MessageProcessorThread).Invoke(() =>
            {
                Connected.Start();
                messageProcessor.Stop();
                Connected.Done();
            });
            //MessageProcessorThread?.Abort();//убрал т.к. пытаемся очистить поток в котором останавливается калибровка
        }

        private void ConnectDonationAlerts()
        {
            if (Options.Options.Donate.DonationAlerts.Enable)
            {
                DonationAlertsAPIThread?.Abort();
                DonationAlertsAPIThread = new Thread(() =>
                {
                    Connected.Start();
                    DonationAlertsAPI.MessageEvent += this.Get_MessageEvent;
                    DonationAlertsAPI.Connect(Options.Options.Donate.DonationAlerts.AccessToken, Options.Options.Donate.DonationAlerts.RefreshToken);
                    Connected.Done();
                    System.Windows.Threading.Dispatcher.Run();//запускаем диспетчер для дальнейшего взаимодействия с потоком

                });
                DonationAlertsAPIThread.IsBackground = true;
                DonationAlertsAPIThread.Start();
            }
        }
        private void ConnectDonatePay()
        {
            if (Options.Options.Donate.DonatePay.Enable)
            {
                DonatePayAPIThread?.Abort();
                DonatePayAPIThread = new Thread(() =>
                {
                    Connected.Start();
                    DonatePayAPI.MessageEvent += this.Get_MessageEvent;
                    DonatePayAPI.Connect(Options.Options.Donate.DonatePay.AccessToken);
                    Connected.Done();
                    System.Windows.Threading.Dispatcher.Run();//запускаем диспетчер для дальнейшего взаимодействия с потоком

                });
                DonatePayAPIThread.IsBackground = true;
                DonatePayAPIThread.Start();
            }
        }
        private void ConnectRutony()
        {
            if (Options.Options.Rutony.Enable)
            {
                RutonyAPIThread?.Abort();
                RutonyAPIThread = new Thread(() =>
                {
                    Connected.Start();
                    RutonyAPI.MessageEvent += this.Get_MessageEvent;
                    RutonyAPI.Connect();
                    Connected.Done();
                    System.Windows.Threading.Dispatcher.Run();//запускаем диспетчер для дальнейшего взаимодействия с потоком

                });
                RutonyAPIThread.IsBackground = true;
                RutonyAPIThread.Start();
            }
        }
        private void ConnectTwitch()
        {
            if (Options.Options.API.Twitch.Enable)
            {
                TwitchAPIThread?.Abort();
                TwitchAPIThread = new Thread(() =>
                {
                    Connected.Start();
                    TwitchAPI.MessageEvent += this.Get_MessageEvent;
                    TwitchAPI.Connect(Options.Options.API.Twitch.Channel, Options.Options.API.Twitch.Login, Options.Options.API.Twitch.OAuth);
                    Connected.Done();
                    System.Windows.Threading.Dispatcher.Run();//запускаем диспетчер для дальнейшего взаимодействия с потоком

                });
                TwitchAPIThread.IsBackground = true;
                TwitchAPIThread.Start();
            }
        }
        private void ConnectPeka2tv()
        {
            if (Options.Options.API.Peka2tv.Enable)
            {
                Peka2tvAPIThread?.Abort();
                Peka2tvAPIThread = new Thread(() =>
                {
                    Connected.Start();
                    Peka2tvAPI.MessageEvent += this.Get_MessageEvent;
                    Peka2tvAPI.Connect(Options.Options.API.Peka2tv.Channel);
                    Connected.Done();
                    System.Windows.Threading.Dispatcher.Run();//запускаем диспетчер для дальнейшего взаимодействия с потоком

                });
                Peka2tvAPIThread.IsBackground = true;
                Peka2tvAPIThread.Start();
            }
        }
        private void ConnectGoodGame()
        {
            if (Options.Options.API.GoodGame.Enable)
            {
                GoodGameAPIThread?.Abort();
                GoodGameAPIThread = new Thread(() =>
                {
                    Connected.Start();
                    GoodGameAPI.MessageEvent += this.Get_MessageEvent;
                    GoodGameAPI.Connect(Options.Options.API.GoodGame.Channel, Options.Options.API.GoodGame.Login, Options.Options.API.GoodGame.Password);
                    Connected.Done();
                    System.Windows.Threading.Dispatcher.Run();//запускаем диспетчер для дальнейшего взаимодействия с потоком
                });
                GoodGameAPIThread.IsBackground = true;
                GoodGameAPIThread.Start();

            }
        }
        private void ConnectMessageProcessor()
        {
            MessageProcessorThread?.Abort();
            MessageProcessorThread = new Thread(() =>
            {
                Connected.Start();
                messageProcessor.Start();
                DllInjector.Inject();//сделать проверку на инжектирование библотека, что бы избежать повторной инжекции
                Connected.Done();
                /*
                //Если калибровка прервана то дисконектим все подключения к площадкам
                if (messageProcessor.IsCalibrationAborted)
                {
                    DisconnectDonationAlerts();
                    DisconnectDonatePay();
                    DisconnectRutony();
                    DisconnectTwitch();
                    DisconnectPeka2tv();
                    DisconnectGoodGame();
                    DisconnectMessageProcessor();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ApplicationIsConnectedToGame = false;

                        ConnectMenuItem.Header = "Подключить";

                        //Отключаем консоль с командами
                        enableCommandConsole = false;

                        //System.Windows.MessageBox.Show("калибровка прервана");

                    });
                }*/

                System.Windows.Threading.Dispatcher.Run();//запускаем диспетчер для дальнейшего взаимодействия с потоком
            });
            MessageProcessorThread.SetApartmentState(ApartmentState.STA);
            MessageProcessorThread.IsBackground = true;
            MessageProcessorThread.Start();
        }

        public void SendMessageToChatRichTextBox(string message)
        {
            ChatMessageRichTextBox.Text += message;
            OnPropertyChanged(nameof(ChatMessageRichTextBox));
        }
        private void GetTestDonateMessage(Newtonsoft.Json.Linq.JObject message)
        {
            if (Options.Options.Donate.DonatTestEnabled)
            {
                Regex regex = new Regex(@"!\d\d*");
                Match match = regex.Match(message["text"].ToString());

                if (match.Value != string.Empty)
                {
                    //Конвертируем сообщение в донатное сообщение
                    Newtonsoft.Json.Linq.JObject testDonateMessage = Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + message["MessageSource"] + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + "TestDonation" + "\"" + "," + "\"amount\":" + "\"" + float.Parse(match.Value.Trim('!').Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + message["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}");

                    //Передаем сконвертированное сообщение в обработку донатных сообщений
                    GetDonateMessage(testDonateMessage);
                }
            }
        }
        private void GetCommandMessage(Newtonsoft.Json.Linq.JObject message)
        {
            Interactive.InteractiveSets.InteractiveSet.InteractiveCommand currentCommand = null;

            var currentInteractiveSet = Interactive.InteractiveSets.InteractiveSetList.Where(set => set.Title == Interactive.InteractiveSets.ActiveSet).First();

            if ((string)message["type"] == "payment")
            {
                //System.Windows.Forms.MessageBox.Show(message.ToString());

                //Проверяем есть ли в текущем активном наборе команда за сумму текущего доната, если есть то отправляем в CommandProcessor, если нет то отправляем этот донат в AlertsProcessor
                if (currentInteractiveSet.InteractiveSetCommandsList.Any(command => command.CommPayment.Type == "Donate" && command.CommPayment.Price == (int)message["amount"]))
                {
                    currentCommand = currentInteractiveSet.InteractiveSetCommandsList.Where(command => command.CommPayment.Price == (int)message["amount"]).First();
                }
            }
            if ((string)message["type"] == "new_subscriber")
            {
                //Проверяем есть ли в текущем активном наборе команда за подписку, если есть то отправляем в CommandProcessor, если нет то отправляем эту подписку в AlertsProcessor
                if (currentInteractiveSet.InteractiveSetCommandsList.Any(command => command.CommPayment.Type == "Premium"))
                {
                    currentCommand = currentInteractiveSet.InteractiveSetCommandsList.Where(command => command.CommPayment.Type == "Premium").First();
                }

            }
            if ((string)message["type"] == "new_follower")
            {
                //Проверяем есть ли в текущем активном наборе команда за фолов, если есть то отправляем в CommandProcessor, если нет то отправляем этот фолов в AlertsProcessor
                if (currentInteractiveSet.InteractiveSetCommandsList.Any(command => command.CommPayment.Type == "Follow"))
                {
                    currentCommand = currentInteractiveSet.InteractiveSetCommandsList.Where(command => command.CommPayment.Type == "Follow").First();
                }

            }

            //Сообщения в окно на вкладке "Интерактив"

            if (currentCommand != null)
            {
                InteractiveCommandMessageRichTextBox.Text += message["username"] + " воспользовавшись площадкой: " + message["MessageSource"] + " вызвал команду " + currentCommand.Title + " заплатив " + currentCommand.CommPayment.Price + " время вызова: " + DateTime.Now.ToShortTimeString() + Environment.NewLine;
            }


            if (InteractiveCommandMessageRichTextBox.Lines.Count() > 50)
            {
                InteractiveCommandMessageRichTextBox.Text = InteractiveCommandMessageRichTextBox.Text.Remove(0, InteractiveCommandMessageRichTextBox.Lines.First().Length + 2);
            }
            OnPropertyChanged(nameof(InteractiveCommandMessageRichTextBox));
        }
        private void GetCommandMessage(Interactive.InteractiveSets.InteractiveSet.InteractiveCommand command)
        {
            //Сообщения в окно на вкладке "Интерактив"
            InteractiveCommandMessageRichTextBox.Text += "Из консоли вызвана команда " + command.Title + " время вызова: " + DateTime.Now.ToShortTimeString() + Environment.NewLine;

            if (InteractiveCommandMessageRichTextBox.Lines.Count() > 50)
            {
                InteractiveCommandMessageRichTextBox.Text = InteractiveCommandMessageRichTextBox.Text.Remove(0, InteractiveCommandMessageRichTextBox.Lines.First().Length + 2);
            }
            OnPropertyChanged(nameof(InteractiveCommandMessageRichTextBox));
        }
        private void GetRawMessage(Newtonsoft.Json.Linq.JObject message)
        {
            //Сообщения в окно на вкладке "Raw-сообщения"
            RawMessageRichTextBox.Text += message.ToString() + Environment.NewLine;

            if (RawMessageRichTextBox.Lines.Count() > 50)
            {
                RawMessageRichTextBox.Text = RawMessageRichTextBox.Text.Remove(0, RawMessageRichTextBox.Lines.First().Length + 2);
            }
            OnPropertyChanged(nameof(RawMessageRichTextBox));
        }
        private void GetMessageMessage(Newtonsoft.Json.Linq.JObject message)
        {
            if ((string)message["type"] == "message")
            {
                ChatMessageRichTextBox.Text += message["username"] + ": " + message["text"] + Environment.NewLine;

                GetTestDonateMessage(message);
            }
            if ((string)message["type"] == "private_message")
            {
                ChatMessageRichTextBox.Text += "ЛС от " + message["username"] + ": " + message["text"] + Environment.NewLine;

                GetTestDonateMessage(message);
            }
            if ((string)message["type"] == "channel_message")
            {
                ChatMessageRichTextBox.Text += message["text"] + " " + message["channel_name"] + Environment.NewLine;
            }
            if ((string)message["type"] == "new_subscriber")
            {
                ChatMessageRichTextBox.Text += message["text"] + " :" + message["username"] + Environment.NewLine;

                //Передаем сообщение для дальнейшей обработки в MessageProcessor
                SendMessageToMessageProcessor(message);

                //Сообщение в окно на вкладке Интерактив
                GetCommandMessage(message);
            }
            if ((string)message["type"] == "new_follower")
            {
                ChatMessageRichTextBox.Text += message["text"] + " :" + message["username"] + Environment.NewLine;

                //Передаем сообщение для дальнейшей обработки в MessageProcessor
                SendMessageToMessageProcessor(message);

                //Сообщение в окно на вкладке Интерактив
                GetCommandMessage(message);
            }

            if (ChatMessageRichTextBox.Lines.Count() > 50)
            {
                ChatMessageRichTextBox.Text = ChatMessageRichTextBox.Text.Remove(0, ChatMessageRichTextBox.Lines.First().Length + 2);
            }

            OnPropertyChanged(nameof(ChatMessageRichTextBox));
        }
        private void GetDonateMessage(Newtonsoft.Json.Linq.JObject message)
        {
            if ((string)message["type"] == "payment")
            {
                DonateMessageRichTextBox.Text += message["username"] + " задонатил: " + message["amount"] + " и сообщил: " + message["text"] + Environment.NewLine;

                //Передаем сообщение для дальнейшей обработки в MessageProcessor
                SendMessageToMessageProcessor(message);

                //Сообщение в окно на вкладке Интерактив
                GetCommandMessage(message);

                //передаем информацию в статистику
                DonateStatistics.DonateStatistics.GetDonateMessage(message);

                //передаем информацию в донат бар
                InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Dispatcher.Invoke(() =>
                {
                    if (InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().IsLoaded)
                    {
                        InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().GetDonate((double)message["amount"]);
                    }
                });
            }

            if (DonateMessageRichTextBox.Lines.Count() > 50)
            {
                DonateMessageRichTextBox.Text = DonateMessageRichTextBox.Text.Remove(0, DonateMessageRichTextBox.Lines.First().Length + 2);
            }

            OnPropertyChanged(nameof(DonateMessageRichTextBox));


        }
        private void GetApplicationMessage(Newtonsoft.Json.Linq.JObject message)
        {
            if ((string)message["type"] == "application")
            {
                Options.Options.Donate.DonationAlerts.AccessToken = (string)message["access_token"].ToString();
                Options.Options.Donate.DonationAlerts.RefreshToken = (string)message["refresh_token"].ToString();

                Options.Options.OptionsSave();
            }
        }
        private void SendMessageToMessageProcessor(Newtonsoft.Json.Linq.JObject message)
        {
            //Передаем сообщение для дальнейшей обработки в MessageProcessor
            System.Windows.Threading.Dispatcher.FromThread(MessageProcessorThread).Invoke(() =>
            {
                messageProcessor.GetMessage(message);
            });
        }
        private void Get_MessageEvent(object sender, Newtonsoft.Json.Linq.JObject message)
        {
            //Сообщения в окно на вкладке "Raw-сообщения"
            GetRawMessage(message);

            //Сообщения в окно на вкладке "Сообщения"
            GetMessageMessage(message);

            //Сообщения в окно на вкладке "Донаты"
            GetDonateMessage(message);

            //Системное сообщение самого приложения(костыль) было сделано если вдруг в DonationAlerts отвалится access_token и потребуется его обновление на ходу
            GetApplicationMessage(message);
        }

        public ViewModel.Command ShowOptionsCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    Options.OptionsWindow OptionsWindows = new Options.OptionsWindow();
                    OptionsWindows.ShowDialog();
                });
            }
        }
        public ViewModel.Command ShowDonateStatisticsCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    DonateStatistics.DonateStatisticsWindow DonateStatistics = new DonateStatistics.DonateStatisticsWindow();
                    DonateStatistics.ShowDialog();
                });
            }
        }
        public ViewModel.Command ShowHelpCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    //симуляция доната
                    /*
                    InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Dispatcher.Invoke(() =>
                    {
                        if (InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().IsLoaded)
                        {
                            InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().GetDonate(100);
                        }
                    });
                    DonateStatistics.DonateStatistics.GetDonate(100);
                    */
                    //симуляция алерта
                    /*OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke(() =>
                    {
                        OldAlertWindow.AlertWindow.StartAlert(Options.Options.Alerts.AlertsList[0], Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + "GoodGame" + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + "Regber" + "\"" + "," + "\"amount\":" + "\"" + "111" + "\"" + "," + "\"text\":" + "\"" + "Так так" + "\"" + "}"));
                    });*/
                    System.Windows.Forms.MessageBox.Show("1. Запускаем игру от администратора и остаемся в главном меню" + "\n" +
                                                         "2.Запускаем программу от администратора и жмем кнопку \"Подключить игру к чату\"" + "\n" +
                                                         "3.Загружаем игру или начинаем новую игру(в зависимости от этого нам надо или не надо запустить стартовый квест консольной командой \"setstage 07006481 10\")" + "\n" +
                                                         "4.Загрузив игру и имея включенный стартовый квест(\"setstage 07006481 10\") запускаем калибровку значений консольной командой \"setstage 07075AF1 10\"" + "\n" +
                                                         "5.ждем окончания калибровки(о чем будет сообщено в левом верхнем углу экрана в игре)" + "\n" +
                                                         "6.Все программа подключена" + "\n" +
                                                         "7. ....." + "\n" +
                                                         "8.Profit!!11!11");
                });
            }
        }
        public ViewModel.Command ShowInteractiveBuilderCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    InteractiveBuilder.InteractiveBuilderWindow InteractiveBuilderWindow = new InteractiveBuilder.InteractiveBuilderWindow();
                    InteractiveBuilderWindow.ShowDialog();
                });
            }
        }
        public ViewModel.Command ShowInteractiveRouletteCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    if (RouletteWindowThread == null)
                    {
                        //Запускаем окно в другом потоке, в дальнейшем обращаемся к нему через диспетчер
                        RouletteWindowThread = new Thread(() =>
                        {
                            InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(() =>
                            {
                                InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Show();
                            });
                            System.Windows.Threading.Dispatcher.Run();
                        });
                        RouletteWindowThread.SetApartmentState(ApartmentState.STA);
                        RouletteWindowThread.IsBackground = true;
                        RouletteWindowThread.Start();
                    }
                    else
                    {
                        InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(() =>
                        {
                            InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Show();
                        });
                    }

                });
            }
        }
        public ViewModel.Command ShowInteractiveDonateBarCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    //Если поток пуст то заполняем его новым потоком и открываем в новом потоке окно, иначе просто открываем окно в другом потоке
                    if (DonateBarWindowThread == null)
                    {
                        //Запускаем окно в другом потоке, в дальнейшем обращаемся к нему через диспетчер
                        DonateBarWindowThread = new Thread(() =>
                        {
                            InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Dispatcher.Invoke(() =>
                            {
                                InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Show();
                            });

                            System.Windows.Threading.Dispatcher.Run();
                        });
                        DonateBarWindowThread.SetApartmentState(ApartmentState.STA);
                        DonateBarWindowThread.IsBackground = true;
                        DonateBarWindowThread.Start();
                    }
                    else
                    {
                        InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Dispatcher.Invoke(() =>
                        {
                            InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Show();
                        });
                    }


                });
            }
        }
        public ViewModel.Command ShowAlertWindowCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    //Если поток пуст то заполняем его новым потоком и открываем в новом потоке окно, иначе просто открываем окно в другом потоке
                    if (AlertsWindowThread == null)
                    {
                        //Запускаем окно в другом потоке, в дальнейшем обращаемся к нему через диспетчер
                        AlertsWindowThread = new Thread(() =>
                        {
                            OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke(() =>
                            {
                                OldAlertWindow.AlertWindow.GetWindow().Show();
                            });

                            System.Windows.Threading.Dispatcher.Run();
                        });
                        AlertsWindowThread.SetApartmentState(ApartmentState.STA);
                        AlertsWindowThread.IsBackground = true;
                        AlertsWindowThread.Start();
                    }
                    else
                    {
                        OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke(() =>
                        {
                            OldAlertWindow.AlertWindow.GetWindow().Show();
                        });
                    }

                });
            }
        }
        public ViewModel.Command ExitCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    var MainWindow = (MainWindow)obj;
                    MainWindow.Close();
                });
            }
        }

        public new static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private new static void OnStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
            {
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
            }
        }
        private bool CheckGameIsEnable()
        {
            bool gameEnable = false;
            //Игра для которой меняются переменные
            string GameName = "Fallout4";
            //Очищаем процесс перед его перезаписью
            Process gameProcess = null;
            //Поиск процесса и присвоение его объекту "process"
            gameProcess = Process.GetProcessesByName(GameName).FirstOrDefault();

            if (gameProcess != null)
            {
                gameEnable = true;
            }
            else
            {
                gameEnable = false;
            }

            if (gameEnable == false)
            {
                var result = System.Windows.Forms.MessageBox.Show("Перед подключением интерактива запустите игру!", "Важно", MessageBoxButtons.OKCancel);

                if (result == DialogResult.Cancel)
                {
                    gameEnable = false;
                }
                if(result == DialogResult.OK)
                {
                    gameEnable=CheckGameIsEnable();
                }
            }

            return gameEnable;
        }
    }
}

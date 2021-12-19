using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Xml.Linq;

using Interactive = InteractiveFallout4.InteractiveBuilder.Interactive;
using CommandProc = InteractiveFallout4.MainWindow.CommandProcessor.CommandProcessor;
using AlertsProc = InteractiveFallout4.MainWindow.AlertsProcessor.AlertsProcessor;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace InteractiveFallout4.MainWindow.MessageProcessor
{

    class MessageProcessor
    {
        //Свойство указывающее был ли процес калибровки прерван
        public static bool IsCalibrationAborted = false;

        //проверка включен ли алерт
        static bool AlertRuningNow = false;

        //Процессор для обработки комманд
        private static CommandProc commandProcessor = new CommandProc();
        //Процессор для отработки алертов
        private static AlertsProc alertsProcessor = new AlertsProc();

        //Лист содержащий очередь команд
        private static List<Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>> commandsList = new List<Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>>();
        //Лист содержащий очередь алертов
        private static List<Tuple<Options.Options.Alerts.Alert, Newtonsoft.Json.Linq.JObject>> alertsList = new List<Tuple<Options.Options.Alerts.Alert, Newtonsoft.Json.Linq.JObject>>();

        private static TimerCallback timerCallback;
        private static System.Threading.Timer timer;

        public static void Start()
        {
            IsCalibrationAborted = !CalibrationProcessor.CalibrationProcessor.CheckGameVariableValue();

            //Если калибровка оборвана, то отменяем старт MessageProcessor
            if (IsCalibrationAborted)
                return;

            // т.к. списки пусты то указываем что обработка алертов и команд завершена
            AlertRuningNow = false;
            CommandProc.commandInAction = false;

            int num = 0;
            // устанавливаем метод обратного вызова
            timerCallback = new TimerCallback(MessageProcessing);
            // создаем таймер
            timer = new System.Threading.Timer(timerCallback, num, 0, 5000);
        }
        public static void Stop()
        {
            //Очищаем очереди команд и алертов и останавливаем таймер
            commandsList.Clear();
            alertsList.Clear();

            // т.к. списки очищены то указываем что обработка алертов и команд завершена
            AlertRuningNow = false;
            CommandProc.commandInAction = false;

            timer?.Dispose();
        }

        public static void GetMessage(Newtonsoft.Json.Linq.JObject message)
        {
            var currentInteractiveSet = Interactive.InteractiveSets.InteractiveSetList.Where(set => set.Title == Interactive.InteractiveSets.ActiveSet).First();

            if ((string)message["type"] == "payment")
            {
                //System.Windows.Forms.MessageBox.Show(message.ToString());

                //Проверяем есть ли в текущем активном наборе команда за сумму текущего доната, если есть то отправляем в CommandProcessor, если нет то отправляем этот донат в AlertsProcessor
                if (currentInteractiveSet.InteractiveSetCommandsList.Any(command => command.CommPayment.Type == "Donate" && command.CommPayment.Price == (int)message["amount"]))
                {
                    var currentCommand = currentInteractiveSet.InteractiveSetCommandsList.Where(command => command.CommPayment.Price == (int)message["amount"]).First();
                    
                    //Добавляем первую найденную команду в очередь
                    commandsList.Add(new Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>(currentCommand, message));
                }
                //Проверяем есть ли за указанную цену алерт
                else if (AlertsProcessor.AlertsProcessor.GetAlertFromDonateAmount((int)message["amount"]) != null)
                {
                    //добавляем найденный алерт в очередь
                    alertsList.Add(new Tuple<Options.Options.Alerts.Alert, Newtonsoft.Json.Linq.JObject>(AlertsProcessor.AlertsProcessor.GetAlertFromDonateAmount((int)message["amount"]), message));
                }
            }
            if ((string)message["type"] == "new_subscriber")
            {
                //Проверяем есть ли в текущем активном наборе команда за подписку, если есть то отправляем в CommandProcessor, если нет то отправляем эту подписку в AlertsProcessor
                if (currentInteractiveSet.InteractiveSetCommandsList.Any(command => command.CommPayment.Type == "Premium"))
                {
                    var currentCommand = currentInteractiveSet.InteractiveSetCommandsList.Where(command => command.CommPayment.Type == "Premium").First();

                    //Добавляем первую найденную команду в очередь
                    commandsList.Add(new Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>(currentCommand, message));
                    
                }
                //Проверяем есть ли за премиум подписку алерт
                else if (Options.Options.Alerts.AlertsList.Where(alert => alert.Type == "Premium").Count() > 0)
                {
                    //добавляем найденный алерт в очередь
                    alertsList.Add(new Tuple<Options.Options.Alerts.Alert, Newtonsoft.Json.Linq.JObject>(Options.Options.Alerts.AlertsList.Where(alert => alert.Type == "Premium").First(), message));
                }
            }
            if ((string)message["type"] == "new_follower")
            {
                //Проверяем есть ли в текущем активном наборе команда за фолов, если есть то отправляем в CommandProcessor, если нет то отправляем этот фолов в AlertsProcessor
                if (currentInteractiveSet.InteractiveSetCommandsList.Any(command => command.CommPayment.Type == "Follow"))
                {
                    var currentCommand = currentInteractiveSet.InteractiveSetCommandsList.Where(command => command.CommPayment.Type == "Follow").First();

                    //Добавляем первую найденную команду в очередь
                    commandsList.Add(new Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>(currentCommand, message));
                }
                //Проверяем есть ли за фоллов алерт
                else if (Options.Options.Alerts.AlertsList.Where(alert => alert.Type == "Follow").Count() > 0)
                {
                    //добавляем найденный алерт в очередь
                    alertsList.Add(new Tuple<Options.Options.Alerts.Alert, Newtonsoft.Json.Linq.JObject>(Options.Options.Alerts.AlertsList.Where(alert => alert.Type == "Follow").First(), message));
                }
            }
        }
        public static void GetCommand(Interactive.InteractiveSets.InteractiveSet.InteractiveCommand command)
        {
            //ВЫзов команды из консоли отправляем в обработку БЕЗ сообщения(null)
            commandsList.Add(new Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>(command, null));
        }
        private static void MessageProcessing(object obj)
        {
            //Проверка окна алерта, если оно запущено то возвращаем фактическое состояние AlertRuningNow,
            //если нет то возвращаем что алерт не проигрывается, т.е. AlertRuningNow=false
            AlertRuningNow = false;
            try
            {
                if (MainWindowViewModel.AlertsWindowThread != null)
                {
                    AlertRuningNow = OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke<bool>(() =>
                    {
                        return OldAlertWindow.AlertWindow.AlertRuningNow;
                    });
                }
            }
            catch
            {
                AlertRuningNow = false;
            }


            //Если ни команда, ни алерт не запущены то запускаем команду
            if (!AlertRuningNow && !CommandProc.commandInAction)
            {
                commandProcessor.StartCommand(commandsList);
            }
            //Если алерт не запущен то запускаем его
            if (!AlertRuningNow)
            {
                alertsProcessor.StartAlert(alertsList);
            }
        }

    }
}

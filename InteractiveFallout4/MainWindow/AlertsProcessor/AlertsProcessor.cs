using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveFallout4.MainWindow.AlertsProcessor
{
    class AlertsProcessor
    {


        public void StartAlert(List<Tuple<Options.Options.Alerts.Alert, Newtonsoft.Json.Linq.JObject>> alertsList)
        {

            if(alertsList.Count()>0)
            {
                var alert = alertsList.First();

                //Если окно хоть раз было запущено то пытаемся воспроизвести алерт
                if (MainWindowViewModel.AlertsWindowThread != null)
                {
                    OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke(() =>
                    {
                        OldAlertWindow.AlertWindow.StartAlert(alert.Item1, alert.Item2);
                    });
                }

                alertsList.Remove(alert);
            }
        }

        public static Options.Options.Alerts.Alert GetAlertFromDonateAmount(int price)
        {
            Options.Options.Alerts.Alert currentAlert = null;

            if (Options.Options.Alerts.AlertsList.Where(alert => alert.Enable == true && alert.Type == "Donate" && alert.Price <= price).Count() > 0)
            {
                currentAlert = Options.Options.Alerts.AlertsList.Where(alert => alert.Enable == true && alert.Type == "Donate" && alert.Price <= price).Last();
                return currentAlert;
            }

            return currentAlert;
        }
    }
}

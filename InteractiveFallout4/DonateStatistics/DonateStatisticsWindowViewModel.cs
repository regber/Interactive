using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace InteractiveFallout4.DonateStatistics
{
    class DonateStatisticsWindowViewModel:ViewModel.BaseViewModel
    {
        public static PropertyPath CurrentDonateAmount { get; set; } = new PropertyPath(typeof(DonateStatistics).GetProperty("CurrentDonateAmount"));

        public UserControl StatisticsWindow { get; set; } = new InteractiveFallout4.DonatStatistic.DonatStatisticWindow();

        public ViewModel.Command AcceptButtonClick
        {
            get
            {
                return new ViewModel.Command((Object) =>
                {
                    DonateStatistics.StatisticsSave();


                        InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Dispatcher.Invoke(()=>
                        {
                            //обновляем текущее положение уровня донатов если донат бар загружен
                            if (InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().IsLoaded)
                            {
                                ((InteractiveDonateBar.InteractiveDonateBarWindowViewModel)InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().DataContext).CurrentDonateValue = InteractiveFallout4.DonateStatistics.DonateStatistics.CurrentDonateAmount;
                                ((InteractiveDonateBar.InteractiveDonateBarWindowViewModel)InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().DataContext).RefreshCurrentDonateValue();
                            }
                        }
                        );
                        
                    

                    ((Window)Object).Close();
                }
              );
            }
        }
        public ViewModel.Command CancelButtonClick
        {
            get
            {
                return new ViewModel.Command((Object) =>
                {
                    DonateStatistics.StatisticsLoad();
                    ((Window)Object).Close();
                }
                );
            }
        }
    }
}

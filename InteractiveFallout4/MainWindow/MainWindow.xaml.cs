using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InteractiveFallout4.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //При закрытии окна оно скрывается
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {



            //Костыльное завершение программы, без отключения и освобождения потоков
            MainWindowViewModel.MessageProcessorThread?.Abort();
            return;

            //if(CalibrationProcessor.CalibrationProcessor.CalibrationOnAction)
            //{
            //Костыль принудительно закрывающий приложение при условии что подключения не завершены
            //CalibrationProcessor.CalibrationProcessor.IsCalibrationAborted = true;
            //MessageProcessor.MessageProcessor.IsCalibrationAborted = true;

            /*
            System.Windows.Threading.Dispatcher.FromThread(MainWindowViewModel.MessageProcessorThread).Invoke(() =>
            {
                CalibrationProcessor.CalibrationProcessor.IsCalibrationAborted = true;
                MessageProcessor.MessageProcessor.IsCalibrationAborted = true;
                InteractiveFallout4.MainWindow.MessageProcessor.MessageProcessor.Stop();
            });*/



            //DisposeThreads();
            /*}
            else
            {
                if(((MainWindowViewModel)this.DataContext).ApplicationIsConnectedToGame == true)
                {
                    e.Cancel = true;
                }
                else
                {
                    //
                    /*
                    if (((MainWindowViewModel)this.DataContext).ApplicationIsConnectedToGame==true)
                    {
                        ((MainWindowViewModel)this.DataContext).DisconnectAll();
                    }*/


            //if(Connected.Finish!=0)
            //{
            //Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
            //}
            //DisposeThreads();

            //}

            //}



        }

        private void DisposeThreads()
        {
            //Завершение потока рулетки
            InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(() =>
            {
                InteractiveRoulette.InteractiveRouletteWindow.ManualWindowClosing = true;
                InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Close();
            });

            MainWindowViewModel.RouletteWindowThread?.Abort();


            //Завершение потока донат бара
            InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Dispatcher.Invoke(() =>
            {
                InteractiveDonateBar.InteractiveDonateBarWindow.ManualWindowClosing = true;
                InteractiveDonateBar.InteractiveDonateBarWindow.GetWindow().Close();

            });

            MainWindowViewModel.DonateBarWindowThread?.Abort();


            //Завершение потока окна алертов
            OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke(() =>
            {
                OldAlertWindow.AlertWindow.ManualWindowClosing = true;
                OldAlertWindow.AlertWindow.GetWindow().Close();
            });

            MainWindowViewModel.AlertsWindowThread?.Abort();
        }
    }
}

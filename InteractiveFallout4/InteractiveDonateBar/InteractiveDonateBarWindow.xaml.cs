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

namespace InteractiveFallout4.InteractiveDonateBar
{
    /// <summary>
    /// Логика взаимодействия для InteractiveDonateBarWindow.xaml
    /// </summary>
    public partial class InteractiveDonateBarWindow : Window
    {
        private static InteractiveDonateBarWindow ThisWindow;

        public static InteractiveDonateBarWindow GetWindow()
        {
            if(ThisWindow==null)
            {
                ThisWindow = new InteractiveDonateBarWindow();
            }
            return ThisWindow;
        }


        public InteractiveDonateBarWindow()
        {
           //System.Windows.Forms.MessageBox.Show("InteractiveDonateBarWindow " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            InitializeComponent();
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
        public void GetDonate(double donateAmount)
        {
            ((InteractiveDonateBarWindowViewModel)this.DataContext).CurrentDonateValue += donateAmount;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((InteractiveDonateBarWindowViewModel)this.DataContext).CurrentDonateValue = DonateStatistics.DonateStatistics.CurrentDonateAmount;
            ((InteractiveDonateBarWindowViewModel)this.DataContext).RefreshCurrentDonateValue();
        }

    }
}

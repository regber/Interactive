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

using System.Windows.Forms;

namespace InteractiveFallout4.InteractiveRoulette
{
    /// <summary>
    /// Логика взаимодействия для InteractiveRouletteWindow.xaml
    /// </summary>
    public partial class InteractiveRouletteWindow : Window
    {

        public delegate void EndRotateAnimation(object sender, List<int> argum);
        public event EndRotateAnimation RouletteAnimationEnd;

        private static InteractiveRouletteWindow ThisWindow;
        InteractiveRouletteWindowViewModel CurrentViewModel;

        public InteractiveRouletteWindow()
        {
            //System.Windows.Forms.MessageBox.Show("InteractiveRouletteWindow " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            InitializeComponent();
            CurrentViewModel = (InteractiveRouletteWindowViewModel)this.DataContext;
        }



        public static InteractiveRouletteWindow GetWindow()
        {
            if (ThisWindow == null)
            {
                ThisWindow = new InteractiveRouletteWindow();
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

        //Меняем переменную в датаконтексте окна для запуска анимации появления рулетки в нем
        private void RiseAnimation_Completed(object sender, EventArgs e)
        {

            CurrentViewModel.BarrelOnRotation = true;
        }

        //Меняем переменную в датаконтексте окна для запуска анимации исчезновения рулетки в нем
        private void DownAnimation_Completed(object sender, EventArgs e)
        {
            //InteractiveRouletteWindowViewModel vm = (InteractiveRouletteWindowViewModel)this.DataContext;
            CurrentViewModel.BarrelOnRotation = false;
            
        }

        //Задаем выйгрышные значения рулетки и вызываем событие RouletteAnimation_End
        public void SetRouletteResult(List<int> RouletteResult)
        {
            RouletteAnimationEnd?.Invoke(ThisWindow, RouletteResult);
        }

        public void StartRoulette(InteractiveFallout4.InteractiveBuilder.Interactive.InteractiveSets.InteractiveSet.InteractiveCommand Command, int BarrelCount)
        {
            CurrentViewModel.StartRoulette(Command, BarrelCount);
        }
    }
}

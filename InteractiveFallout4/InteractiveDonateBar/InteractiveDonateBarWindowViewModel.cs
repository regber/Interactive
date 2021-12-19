using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace InteractiveFallout4.InteractiveDonateBar
{
    class InteractiveDonateBarWindowViewModel : ViewModel.BaseViewModel
    {

        //public static PropertyPath CurrentDonateAmount { get; set; } = new PropertyPath(typeof(DonateStatistics.DonateStatistics).GetProperty("CurrentDonateAmount"));

        private double _CurrentDonateValue = InteractiveFallout4.DonateStatistics.DonateStatistics.CurrentDonateAmount;
        public double CurrentDonateValue
        {
            get
            {
                //_CurrentDonateValue = InteractiveFallout4.DonateStatistics.DonateStatistics.CurrentDonateAmount;
                return _CurrentDonateValue;
            }
            set
            {
                //Очередной костыль когда вместо связывания используется жесткое присвоение значения переменной CurrentDonateAmount
                //InteractiveFallout4.DonateStatistics.DonateStatistics.CurrentDonateAmount = value;
                _CurrentDonateValue = value;
                OnPropertyChanged(nameof(CurrentDonateValue));
            }
        }

        public double CurrentDonateValueAnimationCrutch { get; set; }

        private Color _BackgroundColor = Options.Options.DonateBar.BackgroundColor;
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

        public static ObservableCollection<Options.Options.SlotMachine.Barrel> Barrels = Options.Options.SlotMachine.Barrels;

        public Options.Options.SlotMachine.Barrel[] BarrelsForPrice { get; set; } = { Barrels[1], Barrels[2], Barrels[3], Barrels[4], Barrels[5] };
        public Options.Options.SlotMachine.Barrel[] BarrelsForLines { get; set; } = {  Barrels[1], Barrels[2], Barrels[3], Barrels[4], Barrels[5] };
        public Options.Options.SlotMachine.Barrel LastBarrel { get; set; } = Barrels[6];




        //Импортируем метод для получения состояния клавиши
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        public const int VK_SHIFT = 0x10;

        private ObservableCollection<object> _Parts = new ObservableCollection<object>();
        public ObservableCollection<object> Parts
        {
            get
            {
                return _Parts;
            }
            set
            {
                _Parts = value;
                OnPropertyChanged(nameof(Parts));
            }
        }

        public InteractiveDonateBarWindowViewModel()
        {
            //MessageBox.Show("InteractiveDonateBarWindowViewModel: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            Parts.Add(new object());
            Parts.Add(new object());
            Parts.Add(new object());
            Parts.Add(new object());
        }

        public ViewModel.Command RefreshParts
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    //Костыль для обновления донат бара при изменении предельной суммы доната
                    Parts.Add(new object());
                    Parts.RemoveAt(0);

                    //Костыль для обновления реального положения урорвня донатов после ресайза
                    OnPropertyChanged(nameof(CurrentDonateValue));
                });
            }
        }
        public void RefreshCurrentDonateValue()
        {
            //Костыль для обновления реального положения уровня донатов
            OnPropertyChanged(nameof(CurrentDonateValueAnimationCrutch));
        }


        public ViewModel.CommandWithMultParam ResizeCanvasCommand
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    var values = (object[])obj;

                    var CurrentWindow = (InteractiveDonateBarWindow)values[0];
                    var CurrentViewBox = (Viewbox)values[1];

                    bool ShiftIsDown = GetAsyncKeyState(VK_SHIFT) < 0;
                    //Если левая кнопка мыши не зажата скрываем задник рулетки
                    if (ShiftIsDown == true)
                    {
                        if (CurrentViewBox.ActualWidth < CurrentWindow.ActualWidth && CurrentViewBox.ActualHeight > 0.55 * CurrentWindow.ActualHeight)
                        {
                            Parts.Add(new object());
                        }
                        else
                        {
                            if (CurrentViewBox.ActualHeight < 0.45 * CurrentWindow.ActualHeight)
                            {

                                if (Parts.Count > 2)
                                {
                                    Parts.RemoveAt(0);
                                }
                            }
                        }
                    }

                    //Костыль для обновления реального положения урорвня донатов после ресайза
                    OnPropertyChanged(nameof(CurrentDonateValueAnimationCrutch));

                });
            }
        }

        public ViewModel.CommandWithMultParam DonateBarDonateAnimationCommand
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    var values = (object[])obj;
                    var DataContextCurrentWindow = ((InteractiveDonateBarWindowViewModel)((InteractiveDonateBarWindow)values[0]).DataContext);//костыль
                    var CentrePanel = (Image)values[1];
                    var CurrentPartsCount = (int)values[2];
                    var CurrentCanvas = (Canvas)values[3];

                    double CurrentActualWidth;


                    if ((double)DataContextCurrentWindow.CurrentDonateValue >= (double)DataContextCurrentWindow.LastBarrel.Price)
                    {
                        CurrentActualWidth = CurrentPartsCount * (double)CentrePanel.ActualWidth;
                    }
                    else
                    {
                        CurrentActualWidth = ((CurrentPartsCount * (double)CentrePanel.ActualWidth) * ((double)DataContextCurrentWindow.CurrentDonateValue / (double)DataContextCurrentWindow.LastBarrel.Price));
                    }

                    System.Windows.Media.Animation.DoubleAnimation DonateBarAnimation = new System.Windows.Media.Animation.DoubleAnimation();
                    DonateBarAnimation.From = CurrentCanvas.ActualWidth;
                    DonateBarAnimation.To = CurrentActualWidth;
                    DonateBarAnimation.FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd;
                    DonateBarAnimation.Duration = TimeSpan.FromMilliseconds(2000);// Время анимации
                    CurrentCanvas.BeginAnimation(System.Windows.Shapes.Rectangle.WidthProperty, DonateBarAnimation);
                });
            }
        }
        public ViewModel.CommandWithMultParam DonateBarAnimationResizeCruntchCommand
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    var values = (object[])obj;
                    var DataContextCurrentWindow = ((InteractiveDonateBarWindowViewModel)((InteractiveDonateBarWindow)values[0]).DataContext);//костыль
                    var CentrePanel = (Image)values[1];
                    var CurrentPartsCount = (int)values[2];
                    var currentCanvas = (Canvas)values[3];

                    double CurrentActualWidth;

                    if ((double)DataContextCurrentWindow.CurrentDonateValue >= (double)DataContextCurrentWindow.LastBarrel.Price)
                    {
                        CurrentActualWidth = CurrentPartsCount * (double)CentrePanel.ActualWidth;
                    }
                    else
                    {
                        CurrentActualWidth = ((CurrentPartsCount * (double)CentrePanel.ActualWidth) * ((double)DataContextCurrentWindow.CurrentDonateValue / (double)DataContextCurrentWindow.LastBarrel.Price));
                    }

                    System.Windows.Media.Animation.DoubleAnimation progressBarAnimation = new System.Windows.Media.Animation.DoubleAnimation();
                    progressBarAnimation.From = currentCanvas.ActualWidth;
                    progressBarAnimation.To = CurrentActualWidth;
                    progressBarAnimation.FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd;
                    progressBarAnimation.Duration = TimeSpan.FromMilliseconds(0);// Время анимации
                    //progressBarAnimation.Completed += ProgressBarAnimation_Completed;
                    currentCanvas.BeginAnimation(System.Windows.Shapes.Rectangle.WidthProperty, progressBarAnimation);
                    //currentCanvas.Width = RightMarginValue;
                });
            }
        }
    }
    class WidthDonateBarConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int PartsCount = (int)values[0];


            //Умножаем ширину картинки барабана на кол-во барабанов чем задаем общую ширину канваса центральной части рулетки
            return (double)values[1] * PartsCount;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class HeightDonateBarConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            //Умножаем ширину картинки барабана на кол-во барабанов чем задаем общую ширину канваса центральной части рулетки
            return (double)values[0] + (double)values[1] + (double)values[2];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /*
    class DonateBarBackgroundMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var CurrentWindow = (InteractiveDonateBarWindow)values[0];//костыль
            var CentrePanel = (Image)values[1];
            var RightPanel = (Image)values[2];
            var CurrentPartsCount = (int)values[3];

            double LeftMarginValue = -(CurrentPartsCount * (double)CentrePanel.ActualWidth + (double)RightPanel.ActualWidth);
            double RightMarginValue = RightPanel.ActualWidth;

            return new Thickness(LeftMarginValue, 0, RightMarginValue, 0);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class DonateBarMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var DataContextCurrentWindow = ((InteractiveDonateBarWindowViewModel)((InteractiveDonateBarWindow)values[0]).DataContext);//костыль
            var CentrePanel = (Image)values[1];
            var RightPanel = (Image)values[2];
            var CurrentPartsCount = (int)values[3];

            //int BarrelsCount = (int)values[0];
            double LeftMarginValue = (CurrentPartsCount * (double)CentrePanel.ActualWidth + (double)RightPanel.ActualWidth);
            double RightMarginValue = ((CurrentPartsCount * (double)CentrePanel.ActualWidth) * ((double)DataContextCurrentWindow.CurrentDonateValue / (double)DataContextCurrentWindow.LastBarrel.Price));

            return new Thickness(-LeftMarginValue, 0, LeftMarginValue - RightMarginValue, 0);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    */
    class DonateBarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var DataContextCurrentWindow = ((InteractiveDonateBarWindowViewModel)((InteractiveDonateBarWindow)values[0]).DataContext);//костыль
            var CentrePanel = (Image)values[1];
            var RightPanel = (Image)values[2];
            var CurrentPartsCount = (int)values[3];

            double CurrentActualWidth = ((CurrentPartsCount * (double)CentrePanel.ActualWidth) * ((double)DataContextCurrentWindow.CurrentDonateValue / (double)DataContextCurrentWindow.LastBarrel.Price));

            return CurrentActualWidth;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class DonateBarPriceMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var DataContextCurrentWindow = ((InteractiveDonateBarWindowViewModel)((InteractiveDonateBarWindow)values[0]).DataContext);//костыль
            var CentrePanel = (Image)values[1];
            var RightPanel = (Image)values[2];
            var CurrentPartsCount = (int)values[3];
            var CurrentDonateValue = (double)((int)values[4]);

            //int BarrelsCount = (int)values[0];
            double LeftMarginValue = -(CurrentPartsCount * (double)CentrePanel.ActualWidth + (double)RightPanel.ActualWidth) + ((CurrentPartsCount * (double)CentrePanel.ActualWidth) * (CurrentDonateValue / (double)DataContextCurrentWindow.LastBarrel.Price));


            return new Thickness(LeftMarginValue, 0, 0, 0);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class DonateBarLinesMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var DataContextCurrentWindow = ((InteractiveDonateBarWindowViewModel)((InteractiveDonateBarWindow)values[0]).DataContext);//костыль
            var CentrePanel = (Image)values[1];
            var RightPanel = (Image)values[2];
            var CurrentPartsCount = (int)values[3];
            var CurrentDonateValue = (double)((int)values[4]);

            //int BarrelsCount = (int)values[0];
            double LeftMarginValue = -(CurrentPartsCount * (double)CentrePanel.ActualWidth + (double)RightPanel.ActualWidth) + ((CurrentPartsCount * (double)CentrePanel.ActualWidth) * (CurrentDonateValue / (double)DataContextCurrentWindow.LastBarrel.Price));


            return new Thickness(LeftMarginValue, 0, 0, 0);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class DonateBarMaxPriceMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var DataContextCurrentWindow = ((InteractiveDonateBarWindowViewModel)((InteractiveDonateBarWindow)values[0]).DataContext);//костыль
            var CentrePanel = (Image)values[1];
            var RightPanel = (Image)values[2];
            var CurrentPartsCount = (int)values[3];
            var CurrentDonateValue = (double)((int)values[4]);
            var CurrentLabel = (Label)values[5];

            //int BarrelsCount = (int)values[0];
            double LeftMarginValue = -(CurrentPartsCount * (double)CentrePanel.ActualWidth + (double)RightPanel.ActualWidth) + ((CurrentPartsCount * (double)CentrePanel.ActualWidth) * (CurrentDonateValue / (double)DataContextCurrentWindow.LastBarrel.Price)) - System.Windows.Forms.TextRenderer.MeasureText(CurrentLabel.Content.ToString(), new System.Drawing.Font("Arial", 40)).Width;


            return new Thickness(LeftMarginValue, 0, 0, 0);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

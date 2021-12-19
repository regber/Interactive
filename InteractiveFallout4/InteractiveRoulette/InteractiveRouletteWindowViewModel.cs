using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using InteractiveFallout4.InteractiveBuilder;
using InteractiveFallout4.Options;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;




namespace InteractiveFallout4.InteractiveRoulette
{

    class InteractiveRouletteWindowViewModel : DependencyObject, INotifyPropertyChanged
    {
        // рандомайзер для определения стартовых позиций барабанов рулетки используемый в конвертере
        public static Random Randomizer = new Random();

        //RouletteShowUpProperty=true -ShowUp
        //RouletteShowUpProperty=false - vanished
        //BarrelOnRotationProperty=true - in rotation
        //BarrelOnRotationProperty=false - rotate end
        public static readonly DependencyProperty RouletteShowUpProperty;
        public static readonly DependencyProperty BarrelOnRotationProperty;
        public bool RouletteShowUp
        {
            get { return (bool)GetValue(RouletteShowUpProperty); }
            set { SetValue(RouletteShowUpProperty, value); }
        }
        public bool BarrelOnRotation
        {
            get { return (bool)GetValue(BarrelOnRotationProperty); }
            set { SetValue(BarrelOnRotationProperty, value); }
        }

        static InteractiveRouletteWindowViewModel()
        {

            //MessageBox.Show("InteractiveRouletteWindowViewModel: " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            //регистрируем свойство RouletteShowUp и BarrelOnRotation
            RouletteShowUpProperty = DependencyProperty.Register("RouletteShowUp", typeof(bool), typeof(InteractiveRouletteWindowViewModel));
            BarrelOnRotationProperty = DependencyProperty.Register("BarrelOnRotation", typeof(bool), typeof(InteractiveRouletteWindowViewModel));

            //Таймер для переодического получения состояния левой кнопки мыши
            DispatcherTimer dispTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal,
            delegate
            {
                bool mouseIsDown = GetAsyncKeyState(VK_LBUTTON) < 0;

                //Если левая кнопка мыши не зажата скрываем задник рулетки
                if (mouseIsDown == false)
                {
                    //resizingNow = false;
                    InteractiveRouletteWindow.GetWindow().BackgroundCanvas.Visibility = Visibility.Hidden;
                }
            }, InteractiveRouletteWindow.GetWindow().Dispatcher);
        }


        //Импортируем метод для получения состояния клавиши
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        public const int VK_LBUTTON = 0x01;


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private ObservableCollection<Uri[]> _Barrels = new ObservableCollection<Uri[]>();
        public ObservableCollection<Uri[]> Barrels
        {
            get
            {
                return _Barrels;
            }
            set
            {
                _Barrels = value;
                OnPropertyChanged(nameof(Barrels));
            }
        }

        //Необходимо только для отрисовки задника с необходимым размером(размером 7-ми рисунков составляющих рулетку)
        public object[] MaxCountBarrels { get; set; } = new object[7];
        private Color _BackgroundColor = Options.Options.SlotMachine.BackgroundColor;
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


        //Очередь из кортежей со смещениями и канвасами текущих барабанов
        public static Queue<Tuple<double, Canvas>> MarginOffsetAndCanvasCurrentBarrels = new Queue<Tuple<double, Canvas>>();
        
        //Выйгрышные позиции рулетки
        List<int> WinPositionInBarrels = new List<int>();

        //Проверяем наличие картинок по указанной ссылке и при их отсутствии вставляем заглушку
        public static Uri GetImageUri(string UriString)
        {
            if (File.Exists(UriString))
            {
                return new Uri(UriString);
            }
            else
            {
                return new Uri(Directory.GetCurrentDirectory() + @"\BarrelsImage\CapChoice.png");
            }

        }

        public void StartRoulette(Interactive.InteractiveSets.InteractiveSet.InteractiveCommand Command, int BarrelCount)
        {
            /*
            if(!Options.Options.SlotMachine.Enable)
            {
                BarrelCount = Command.CommBarrels.Count;
            }*/

            //Массив массивов ссылок на картинки барабанов, т.е. создается массив барабанов, каждый элемент которого заполняется массивом картинок этого барабана
            Uri[][] UriCommandArray;

            //Рандомайзер
            System.Random Random = new System.Random();
            List<Uri> ListRandomUri = new List<Uri>();

            //Массив Uri для команды
            UriCommandArray = new Uri[BarrelCount][];


            //очищаем выйгрышные позиции рулетки
            WinPositionInBarrels.Clear();

            //Заполняем барабаны рулетки
            for (int i = 0; i < BarrelCount; i++)
            {
                //Если в текущем барабане выбранной команды есть варианты
                if (Command.CommBarrels[i].BarrelChoices.Count != 0)
                {
                    //Если в текущем барабане выбранной команды только один вариант
                    if (Command.CommBarrels[i].BarrelChoices.Count == 1)
                    {
                        WinPositionInBarrels.Add(0);//указываем -1 если барабан сформирован с ошибкой

                        UriCommandArray[i] = new Uri[1];
                        Uri[] UriBarrelArray = new Uri[2];

                        //Заполняем барабан минимум двумя вариантами имеющимся в единственном варианте изображением
                        UriBarrelArray[0] = GetImageUri(Directory.GetCurrentDirectory() + Command.CommBarrels[i].BarrelChoices[0].ChoiceImageUri);
                        UriBarrelArray[1] = GetImageUri(Directory.GetCurrentDirectory() + Command.CommBarrels[i].BarrelChoices[0].ChoiceImageUri);

                        //Заполняем UriCommandArray сформированным UriBarrelArray
                        UriCommandArray[i] = UriBarrelArray;
                    }
                    else//Когда в барабане все хорошо
                    {
                        //Определение кол-ва ячеек в данном барабане
                        int ChoicesCountInCurrentBarrel = Command.CommBarrels[i].BarrelChoices.Count;
                        UriCommandArray[i] = new Uri[ChoicesCountInCurrentBarrel];

                        int RandomNumber = Random.Next(1, 100);
                        double SearchArea = 0;

                        for (int j = 0; j < ChoicesCountInCurrentBarrel; j++)//последовательное определение в какую зону попал выигрышный номер и выцеживание победителя из xml и составление ури массива для передачи в рулетку
                        {
                            if (RandomNumber > SearchArea)//Если значение не попало в searchArea прибавляем к searchArea это значение
                            {
                                double ChanceValue = Command.CommBarrels[i].BarrelChoices[j].Chance;
                                SearchArea += ChanceValue;

                            }
                            if (RandomNumber <= SearchArea)//Если значение попало в searchArea
                            {
                                //Заполняем массив с выйгрышными позициями
                                WinPositionInBarrels.Add(j);

                                Uri[] UriBarrelArray = new Uri[ChoicesCountInCurrentBarrel];//Переставляем выигравший элемент на 0 позицию а с 0 позици элемент перекидываем на место выигрывшего
                                                                                            //Заполняем массив всеми элементами барабана в том порядке в котором они изначально идут
                                for (int k = 0; k < ChoicesCountInCurrentBarrel; k++)
                                {
                                    UriBarrelArray[k] = GetImageUri(Directory.GetCurrentDirectory() + Command.CommBarrels[i].BarrelChoices[k].ChoiceImageUri);
                                }
                                //Сохраняем в лист всех позиций кроме той что выйграла
                                for (int k = 0; k < ChoicesCountInCurrentBarrel; k++)
                                {
                                    if (j != k)
                                    {
                                        ListRandomUri.Add(GetImageUri(Directory.GetCurrentDirectory() + Command.CommBarrels[i].BarrelChoices[k].ChoiceImageUri));
                                    }
                                }

                                //индекс начинает перебиратся с 1 т.к. в 0 находится выйгрышная позиция
                                int index = 1;
                                //Перебираем лист с Невыйгрышными позициями заполняя промежуточный массив пока они не закончатся
                                while (ListRandomUri.Count != 0)
                                {
                                    //Рандомим значения от нуля до количества элементов в листе
                                    int RandomNumberInList = Random.Next(0, ListRandomUri.Count);

                                    //Записываем полученное значение из листа в промежуточный массив
                                    UriBarrelArray[index] = ListRandomUri[RandomNumberInList];
                                    //Удаляем использованное значение из листа
                                    ListRandomUri.RemoveAt(RandomNumberInList);

                                    index++;
                                }

                                //Заполняем элемент winUri выйгрывшим вариантом барабана
                                Uri WinUri = GetImageUri(Directory.GetCurrentDirectory() + Command.CommBarrels[i].BarrelChoices[j].ChoiceImageUri);

                                //Устанавливаем в позицию выйгрышного элемента нулевой элемент рулетки, а в нулевой элемент рулетки устанавливаем выйгрышный, т.к. именно он будет в итоге выпадать на рулетке
                                UriBarrelArray[0] = WinUri;

                                UriCommandArray[i] = UriBarrelArray;
                                break;
                            }
                        }
                    }

                }
                else
                {
                    WinPositionInBarrels.Add(-1);//указываем -1 если барабан сформирован с ошибкой

                    UriCommandArray[i] = new Uri[1];
                    Uri[] UriBarrelArray = new Uri[2];

                    //Заполняем барабан минимум двумя вариантами с изображением заглушки
                    UriBarrelArray[0] = new Uri(Directory.GetCurrentDirectory() + @"\BarrelsImage\CapBarrel.png");
                    UriBarrelArray[1] = new Uri(Directory.GetCurrentDirectory() + @"\BarrelsImage\CapBarrel.png");

                    //Заполняем UriCommandArray сформированным UriBarrelArray
                    UriCommandArray[i] = UriBarrelArray;
                }
            }

            //Если окно рулетки видимо то запускаем анимацию и по ее окончанию вызываем событие RouletteAnimation_End через метод SetRouletteResult, иначе сразу передаем выйгрышные значения рулетки через метод SetRouletteResult
            if (InteractiveRouletteWindow.GetWindow().Visibility == Visibility.Visible && BarrelOnRotation == false)
            {
                StartRouletteAnimation(UriCommandArray);
            }
            else
            {
                InteractiveRouletteWindow.GetWindow().SetRouletteResult(WinPositionInBarrels);
            }
            

            //return WinPositionInBarrels;
        }

        private void StartRouletteAnimation(Uri[][] UriCommandArray)
        {

            //очищаем барабаны рулетки от предыдущих картинок
            Barrels.Clear();
            
            int BarrelsCount = UriCommandArray.Count();

            //Заполняем барабаны картинкамим в х3 для заполнения сверху и снизу дополнительно, что бы исключить проскок не заполненной картинками зоны
            for (int i = 0; i < BarrelsCount; i++)
            {
                List<Uri> MultipleBarrelArray = new List<Uri>();

                for (int j = 0; j < 3; j++)
                {
                    foreach (var u in UriCommandArray[i])
                    {
                        MultipleBarrelArray.Add(u);
                    }
                }
                Barrels.Add(MultipleBarrelArray.ToArray());
            }

            //Запускаем звук вращения барабанов
            InteractiveRouletteWindow.GetWindow().RotationPlayer.Play();

             //запускаем анимацию появления рулетки
            RouletteShowUp = true;
        }



        public ViewModel.Command ResizeCanvasCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    InteractiveRouletteWindow.GetWindow().BackgroundCanvas.Visibility = Visibility.Visible;
                });
            }
        }

        public static ViewModel.CommandWithMultParam StartBarrelAnimationCommand
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    
                    Canvas CurrentBarrelCanvas = (Canvas)((object[])obj)[0];
                    int CurrentBarrelIndex = (int)((object[])obj)[1];
                    var InteractionBarrelTriggers = (Microsoft.Xaml.Behaviors.InvokeCommandAction)((object[])obj)[2];
                    
                    int SlowingDownAnimation = CurrentBarrelIndex * 2;

                    System.Windows.Media.Animation.ThicknessAnimation BarrelAnimation = new System.Windows.Media.Animation.ThicknessAnimation();
                    BarrelAnimation.From = CurrentBarrelCanvas.Margin;
                    BarrelAnimation.To = new Thickness(0, CurrentBarrelCanvas.Margin.Top * 2, 0, 0);
                    BarrelAnimation.Duration = TimeSpan.FromMilliseconds(50);// Время анимации
                    BarrelAnimation.BeginTime = TimeSpan.FromSeconds(2);
                    BarrelAnimation.AutoReverse = true;
                    BarrelAnimation.RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(9 * (CurrentBarrelIndex + SlowingDownAnimation));// Повторы анимации. отрегулировать повторения так чтобы барабаны останавливались по очереди или одновременно

                    BarrelAnimation.Completed += BarrelAnimation_Completed;
                    CurrentBarrelCanvas.BeginAnimation(Canvas.MarginProperty, BarrelAnimation);

                    InteractionBarrelTriggers.Detach();

                });
            }
        }

        public static ViewModel.Command DetachBarrelTriggersCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {

                    var InteractionBarrelTriggers = (Microsoft.Xaml.Behaviors.InvokeCommandAction)obj;

                    InteractionBarrelTriggers.Detach();
                });
            }
        }

        private static void BarrelAnimation_Completed(object sender, EventArgs e)
        {
            //Поочередно выбрасываем канвасы с картинками барабанов из очереди вызывая в них анимацию качения(завершения вращения)
            Tuple<double, Canvas> CurrentMarginOffsetAndCanvasBarrel = InteractiveRouletteWindowViewModel.MarginOffsetAndCanvasCurrentBarrels.Dequeue();
            
            //Анимация окончания вращения барабана
            System.Windows.Media.Animation.ThicknessAnimation barrelEndAnimation = new System.Windows.Media.Animation.ThicknessAnimation();        //может быть придется заменить двойку снизу   //Тут|изменять
            barrelEndAnimation.From = new Thickness(0, CurrentMarginOffsetAndCanvasBarrel.Item1/2, 0, 0);
            barrelEndAnimation.To = new Thickness(0, CurrentMarginOffsetAndCanvasBarrel.Item1, 0, 0);//исправить конечную точку
            barrelEndAnimation.Duration = TimeSpan.FromMilliseconds(2000);
            barrelEndAnimation.RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(1);

            //barrelEndAnimation.Completed += BarrelEndAnimation_Completed;

            System.Windows.Media.Animation.ElasticEase EE = new System.Windows.Media.Animation.ElasticEase();
            EE.EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut;
            EE.Springiness = 4;//4
            EE.Oscillations = 2;//2
            barrelEndAnimation.EasingFunction = EE;
            CurrentMarginOffsetAndCanvasBarrel.Item2.BeginAnimation(Canvas.MarginProperty, barrelEndAnimation);

            InteractiveRouletteWindow.GetWindow().StopRotationPlayer.Stop();
            InteractiveRouletteWindow.GetWindow().StopRotationPlayer.Play();

            //Если барабаны закончились
            if (InteractiveRouletteWindowViewModel.MarginOffsetAndCanvasCurrentBarrels.Count == 0)
            {

                //Останавливаем воспроизведение звука вращения(задержка нужна что бы звук прекращался в соответствии с окончанием анимации вращения)
                Task.Run(()=> 
                {
                    System.Threading.Thread.Sleep(500);
                    InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(() => { InteractiveRouletteWindow.GetWindow().RotationPlayer.Stop(); });
                });
                
                //запускаем анимаию исчезновения рулетки
                ((InteractiveRouletteWindowViewModel)InteractiveRouletteWindow.GetWindow().DataContext).RouletteShowUp = false;

                //Вызываем через метод SetRouletteResult событие и передаем значения выйгрышных позиций рулетки
                InteractiveRouletteWindow.GetWindow().SetRouletteResult(((InteractiveRouletteWindowViewModel)InteractiveRouletteWindow.GetWindow().DataContext).WinPositionInBarrels);
            }
        }
    }

    class WidthRouletteConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int BarrelsCount = (int)values[0];

            //Умножаем ширину картинки барабана на кол-во барабанов чем задаем общую ширину канваса центральной части рулетки
            return (double)values[1] * BarrelsCount;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Заглушка закрывающая периодически возникающую прореху при ресайзе рулетки
    class CrutchCapRouletteConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int BarrelsCount = (int)values[0];
            double MarginValue = (double)values[1] * BarrelsCount / 2 - (double)values[1] / 2;
            return new Thickness(MarginValue, 0, 0, 0);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Конвертер рендомно располагающий стартовое положение барабана
    class StartBarrelPositonConverter : IMultiValueConverter//, INotifyPropertyChanged
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (DependencyProperty.UnsetValue == values[0])
            {
                return new Thickness(0, 0, 0, 0);
            }
            else
            {
                int FullBarrelItems = (int)values[0];
                int RealBarrelItems = FullBarrelItems / 3;
                int RandomOffsetPosition = InteractiveRouletteWindowViewModel.Randomizer.Next(RealBarrelItems);
                Canvas CurrentCanvas = (Canvas)values[4];

                double ImageHeight = (double)values[1] + (double)values[2] + (double)values[3];

                
                double MarginValue = -1 * ((double)values[2] + (double)values[3]) - (RealBarrelItems * (double)values[2]) - (RandomOffsetPosition * (double)values[2]);

                //Записываем канвас и его смещение
                InteractiveRouletteWindowViewModel.MarginOffsetAndCanvasCurrentBarrels.Enqueue(new Tuple<double, Canvas>((MarginValue+(RandomOffsetPosition * (double)values[2])), CurrentCanvas));
                
                return new Thickness(0, MarginValue, 0, 0);
            }

        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}

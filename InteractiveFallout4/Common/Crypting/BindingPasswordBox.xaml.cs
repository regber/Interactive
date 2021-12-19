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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InteractiveFallout4.Common.Crypting
{
    /// <summary>
    /// Логика взаимодействия для BindingPasswordBox.xaml
    /// </summary>
    public partial class BindingPasswordBox : UserControl
    {
        private bool IsPasswordChanged;

        public static readonly DependencyProperty PasswordProperty;

        public string Password
        {
            get { return (string)GetValue(PasswordProperty);}
            set { SetValue(PasswordProperty, value); }
        }

        static BindingPasswordBox()
        {
            //регистрируем свойство PasswordProperty
            PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(BindingPasswordBox),
                                                            new PropertyMetadata(string.Empty,PasswordPropertyChanged));
            
        }

        private static void PasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is BindingPasswordBox PasswordBox)
            {
                PasswordBox.UpdatePassword();
            }
        }



        public BindingPasswordBox()
        {
            InitializeComponent();
        }

        private void MyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            IsPasswordChanged = true;
            Password = MyPasswordBox.Password;
            IsPasswordChanged = false;
        }
        private void UpdatePassword()
        {
            if(!IsPasswordChanged)
            {
                MyPasswordBox.Password = Password;
            }
        }
    }
}

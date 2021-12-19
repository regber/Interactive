using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InteractiveFallout4.ViewModel
{
    class MainWindowViewModel:BaseViewModel
    {
        private string _name;

        public string Name
        {
            get => _name;
            set 
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public Command Click
        {
            get 
            {
                return new Command((Object)=>new Options.OptionsWindow().ShowDialog(),(Object) =>Name.Length<100);
            }
        }

    }
}

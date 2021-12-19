using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Windows;
using System;

namespace InteractiveFallout4.Options.Rutony
{
    class RutonyUCViewModel:ViewModel.BaseViewModel
    {
        public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.Rutony).GetProperty("Enable"));
        public static PropertyPath MD5Key { get; set; } = new PropertyPath(typeof(Options.Rutony).GetProperty("MD5Key"));

        private string _PopUpMessage;
        public string PopUpMessage
        {
            get
            {
                return _PopUpMessage;
            }
            set
            {
                _PopUpMessage = value;
                OnPropertyChanged(nameof(PopUpMessage));
            }
        }


        private bool _IsPopUpShow;

        public bool IsPopUpShow
        {
            get { return _IsPopUpShow; }

            set
            {
                if (_IsPopUpShow == value)
                {
                    return;
                }

                _IsPopUpShow = value;
                OnPropertyChanged(nameof(IsPopUpShow));
            }
        }

        public OptionsCommand CheckRutonyKeyButtonClick
        {
            get
            {
                return new OptionsCommand((Object) =>
                {
                    Task.Run(()=> 
                    {
                        int days = -1;
                        DateTime activeForDate = new DateTime();
                        bool chekMD5 = AuthMD5.CheckMD5Code(Object.ToString(), ref days, ref activeForDate);

                        if (chekMD5)
                            PopUpMessage = $"{days} дневный ключ, действует до {activeForDate.ToShortDateString()}";
                        else
                            PopUpMessage = $"Ключ еще или уже не активен";

                        IsPopUpShow = true;
                    });

                });
            }
        }
    }
}


using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace InteractiveFallout4.Options
{
    class TreeViewModel
    {

        public ObservableCollection<Item> TreeViewData { get; set; }
        public TreeViewModel()
        {

            TreeViewData = new ObservableCollection<Item>();


            var item = new Item();
            var child = new Item();

            //Добавляем пункт меню API чатов и его дочерние элементы
            item.Title = "API чатов";

            child = new Item();
            child.Title = "GoodGame";
            child.Control= new GoodGame.GoodGameUC();
            item.SubItems.Add(child);

            child = new Item();
            child.Title = "Twitch";
            child.Control = new Twitch.TwitchUC();
            item.SubItems.Add(child);

            child = new Item();
            child.Title = "Peka2tv";
            child.Control = new Peka2tv.Peka2tvUC();
            item.SubItems.Add(child);

            TreeViewData.Add(item);

            //Добавляем пункт меню Rutony
            item = new Item();
            item.Title = "Rutony";
            item.Control = new Rutony.RutonyUC();
            TreeViewData.Add(item);

            //Добавляем пункт меню Донаты
            item = new Item();
            item.Title = "Донаты";
            item.Control =new DonateProperty.DonatePropertyUC();
            TreeViewData.Add(item);

            //Добавляем пункт меню Алерты
            item = new Item();
            item.Title = "Алерты";
            item.Control = new Alerts.AlertsUC();
            TreeViewData.Add(item);

            //Добавляем пункт меню Рулетка
            item = new Item();
            item.Title = "Рулетка";
            item.Control = new SlotMachine.SlotMachineUC();
            TreeViewData.Add(item);

            //Добавляем пункт меню Рулетка
            item = new Item();
            item.Title = "Donate Bar";
            item.Control = new DonateBar.DonateBarUC();
            TreeViewData.Add(item);
        }
    }

    class Item
    {
        public string Title { get; set; }
        public UserControl Control = new UserControl();

        public ObservableCollection<Item> SubItems { get; set; }

        public Item()
        {
            SubItems = new ObservableCollection<Item>();
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;

                if (_isSelected == true)
                {
                    OptionsViewModel.OptionsUC.Content = Control;
                }
            }
        }

    }
}

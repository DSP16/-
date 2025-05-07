using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Xml.Linq;
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Winds;

namespace ИП_Хевеши.UI.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ProductCard.xaml
    /// </summary>
    public partial class ProductCard : UserControl, INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private decimal _price;
        private int _quantity;
        private string _manufacturer;
        private string _actuality;
        private bool _isCheckedActuality;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public  string ComponentName
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string Manufacturer
        {
            get => _manufacturer;
            set
            {
                _manufacturer = value;
                OnPropertyChanged();
            }
        }
        public string Actuality
        {
            get => _actuality;
            set
            {
                _actuality = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedActuality
        {
            get => _isCheckedActuality;
            set
            {
                _isCheckedActuality = value;
                OnPropertyChanged();
            }
        }
        public ProductCard()
        {
            InitializeComponent();
            DataContext = this;
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ActualityCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                using (var context = new ИП_ХевешиEntities())
                {
                    var componentToUpdate = context.Components.SingleOrDefault(c => c.ID == ID);
                    if (componentToUpdate != null)
                    {
                        componentToUpdate.Actuality = checkBox.IsChecked == true ? " Актуально" : " Не актуально";
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}

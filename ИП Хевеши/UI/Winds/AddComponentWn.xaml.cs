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
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.Classes;
using System.Windows.Navigation;

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для AddComponentWn.xaml
    /// </summary>
    public partial class AddComponentWn : Window
    {
        private readonly ИП_ХевешиEntities _context;
        public AddComponentWn()
        {
            InitializeComponent();
            _context = new ИП_ХевешиEntities();
            LoadManufacturers();
            LoadZones();
        }
        private void LoadManufacturers()
        {
            var manufacturers = _context.Manufacturers.ToList();
            cbManufacturer.ItemsSource = manufacturers;
            cbManufacturer.DisplayMemberPath = "Name";
            cbManufacturer.SelectedValuePath = "ID";
        }

        public void LoadZones()
        {
            var zones = _context.Zones.ToList();
            cbZone.ItemsSource = zones;
            cbZone.DisplayMemberPath = "Zone";
            cbZone.SelectedValuePath = "ID";
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var context = new ИП_ХевешиEntities();
            AddComponentBack addComponentBack = new AddComponentBack();
            addComponentBack.AddComponent(tbMinQuantity, cbActuality, tbName, tbPrice, cbManufacturer, cbZone, tbQuantity, tbRowCell, tbType);
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
           this.Close();
        }
    }
}

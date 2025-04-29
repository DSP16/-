using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ИП_Хевеши.Classes;
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.UserControls;
using ИП_Хевеши.UI.Winds;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для ArrivalsPage.xaml
    /// </summary>
    public partial class ArrivalsPage : Page
    {
      
        public ArrivalsPage()
        {
            InitializeComponent();
            LoadArrivals();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Если нет, можно показать сообщение об ошибке или выполнить другое действие
                MessageBox.Show("Нет предыдущей страницы.");
            }
        }

        public void LoadArrivals()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var arrivals = context.Arrivals.Include("Component").Include("User").Include("Provider")
                    .Select(a => new ArrivalViewModel
                    {
                        
                        Name = a.Components.Name,
                        ArrivalDate = a.ArrivalDate,
                        Quantity = a.Quantity,
                        PurchasePrice = a.PurchasePrice,
                        UserName = a.Users.UserName + " " + a.Users.UserSurname,
                        ProviderName = a.Providers.Name
                    }).ToList();

                DGridArrivals.ItemsSource = arrivals;
            }
        }
        public class ArrivalViewModel
        {

            public string Name { get; set; }
            public DateTime? ArrivalDate { get; set; }
            public int? Quantity { get; set; }
            public decimal? PurchasePrice { get; set; }
            public string UserName { get; set; }
            public string ProviderName { get; set; }
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            if (searchBox.Text == "Поиск")
            {
                searchBox.Text = "";
                searchBox.Foreground = Brushes.Black; // Измените цвет текста на стандартный
            }
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            if (String.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "Поиск";
                searchBox.Foreground = Brushes.Gray; // Измените цвет текста на серый
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = tbSearch.Text.ToLower();

            if (DGridArrivals.ItemsSource is List<ArrivalViewModel> currentData)
            {
                var filteredData = currentData
                    .Where(a => a.Name.ToLower().Contains(searchTerm) ||
                                a.UserName.ToLower().Contains(searchTerm) ||
                                (a.ArrivalDate.HasValue && a.ArrivalDate.Value.ToString("yyyy-MM-dd").Contains(searchTerm)) ||
                                (a.Quantity.HasValue && a.Quantity.Value.ToString().Contains(searchTerm)) ||
                                (a.PurchasePrice.HasValue && a.PurchasePrice.Value.ToString("C").Contains(searchTerm)))
                    .ToList();

                if (filteredData.Any())
                {
                    DGridArrivals.ItemsSource = filteredData;
                    NoResultsTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DGridArrivals.ItemsSource = null;
                    NoResultsTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text == "")
            {
                LoadArrivals();
                NoResultsTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void btnAddArrivals_Click(object sender, RoutedEventArgs e)
        {
            AddArrivalWn addArrivalWn = new AddArrivalWn();
            addArrivalWn.Show();
        }

        private void btnUpdateArrivalsList_Click(object sender, RoutedEventArgs e)
        {
            LoadArrivals();
        }

        private void btnAddProvider_Click(object sender, RoutedEventArgs e)
        {
            AddProviderWn addProviderWn = new AddProviderWn();
            addProviderWn.Show();
        }
    }    
}

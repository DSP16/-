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
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Winds;
using static ИП_Хевеши.UI.Pages.ArrivalsPage;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для IssuencesPage.xaml
    /// </summary>
    public partial class IssuancesPage : Page
    {
        public IssuancesPage()
        {
            InitializeComponent();
            LoadIssuances();
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

            if (DGridIssuences.ItemsSource is List<IssuanceViewModel> currentData)
            {
                var filteredData = currentData
                    .Where(a => a.Name.ToLower().Contains(searchTerm) ||
                                a.UserName.ToLower().Contains(searchTerm) ||
                                (a.IssuanceDate.HasValue && a.IssuanceDate.Value.ToString("yyyy-MM-dd").Contains(searchTerm)) ||
                                (a.Quantity.HasValue && a.Quantity.Value.ToString().Contains(searchTerm)) ||
                                (a.BuyerName.ToLower().Contains(searchTerm)))
                    .ToList();

                if (filteredData.Any())
                {
                    DGridIssuences.ItemsSource = filteredData;
                    NoResultsTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DGridIssuences.ItemsSource = null;
                    NoResultsTextBlock.Visibility = Visibility.Visible;
                }
            }
        }
        public void LoadIssuances()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var issuences = context.Issuance.Include("Component").Include("User").Include("Buyer")
                    .Select(i => new IssuanceViewModel
                    {
                        Name = i.Components.Name,
                        IssuanceDate = i.IssuanceDate,
                        Quantity = i.Quantity,
                        UserName = i.Users.UserName + " " + i.Users.UserSurname,
                        BuyerName = i.Buyers.Name
                    }).ToList();

                DGridIssuences.ItemsSource = issuences;
            }
        }
        public class IssuanceViewModel
        {

            public string Name { get; set; }
            public DateTime? IssuanceDate { get; set; }
            public int? Quantity { get; set; }
            public string UserName { get; set; }
            public string BuyerName { get; set; }
        }

        private void btnUpdateIssuanceList_Click(object sender, RoutedEventArgs e)
        {
            LoadIssuances();
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

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text == "")
            {
                LoadIssuances();
                NoResultsTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void btnAddIssuance_Click(object sender, RoutedEventArgs e)
        {
            AddIssuanceWn addIssuanceWn = new AddIssuanceWn();
            addIssuanceWn.Show();
        }
    }
}

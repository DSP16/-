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
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.Data;
using ИП_Хевеши.Classes;
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.UI.UserControls;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainContentPage.xaml
    /// </summary>
    public partial class MainContentPage : Page
    {
        public string name;
        public static ИП_ХевешиEntities context = new ИП_ХевешиEntities();
        public static List<Components> components;
        public MainContentPage(string name)
        {
            InitializeComponent();
            LoadCards();
            tbCurrentUser.Text = name;
        }

        private void LoadRemainsCard()
        {
            var loadRemainsProductCard = new LoadRemainsCardBack();
            var components = loadRemainsProductCard.GetComponentsList();

            if (components.Any())
            {
                ProductContainer.Children.Clear();
                NoResultsTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProductContainer.Children.Clear();
                NoResultsTextBlock.Visibility = Visibility.Visible;

            }

            foreach (var component in components)
            {
                var productCard = new ProductCard
                {
                    ComponentName = component.Name,
                    Price = component.Price,
                    Quantity = component.Quantity ?? 0,
                    Manufacturer = component.Manufacturers.Name, // Выводим имя производителя
                    Actuality = component.Actuality,
                    IsCheckedActuality = component.Actuality == "Актуально"
                };
                ProductContainer.Children.Add(productCard);
            }
            NoResultsTextBlock.Visibility = components.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadCards()
        {
            var loadProductCards = new LoadProductCards();
            var components = loadProductCards.GetComponentsList();
            if (components.Any())
            {
                ProductContainer.Children.Clear();
                NoResultsTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProductContainer.Children.Clear();
                NoResultsTextBlock.Visibility = Visibility.Visible;

            }
            foreach (var component in components)
            {
                var productCard = new ProductCard
                {
                    ComponentName = component.Name,
                    Price = component.Price,
                    Quantity = component.Quantity ?? 0,
                    Manufacturer = component.Manufacturers.Name, // Выводим имя производителя
                    Actuality = component.Actuality,
                    IsCheckedActuality = component.Actuality == "Актуально"


                };
                ProductContainer.Children.Add(productCard);
            }
            NoResultsTextBlock.Visibility = components.Any() ? Visibility.Collapsed : Visibility.Visible;
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

        private void btnOpenAddComponent_Click(object sender, RoutedEventArgs e)
        {
            AddComponentWn addComponentWn = new AddComponentWn();
            addComponentWn.Show();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
           
             SearchComponentsBack searchComponentsBack = new SearchComponentsBack();
             var searchTerm = tbSearch.Text;
             var components = searchComponentsBack.GetComponentsByName(searchTerm);
             if (components.Any())
             {
                ProductContainer.Children.Clear();
                NoResultsTextBlock.Visibility = Visibility.Collapsed;
             }
             else
             {
                ProductContainer.Children.Clear();
                NoResultsTextBlock.Visibility = Visibility.Visible;
                 
             }
            foreach (var component in components)
            {
                var productCard = new ProductCard
                {
                    ComponentName = component.Name,
                    Price = component.Price,
                    Quantity = component.Quantity ?? 0,
                    Manufacturer = component.Manufacturers?.Name,
                     Actuality = component.Actuality,
                    IsCheckedActuality = component.Actuality == "Актуально"
                };
                NoResultsTextBlock.Visibility = Visibility.Collapsed;
                ProductContainer.Children.Add(productCard);
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text == "")
            {
                LoadCards();
            }
        }

        private void btnOpenArrivals_Click(object sender, RoutedEventArgs e)
        {
            ArrivalsPage arrivalsPage = new ArrivalsPage();

            Manager.Frame.Navigate(arrivalsPage);
        }

        private void btnUpdateComponentList_Click(object sender, RoutedEventArgs e)
        {
            LoadCards();
            btnAllList.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#249ad7"));
            btnRemains.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#249ad7"));
        }

        private void btnRemains_Click(object sender, RoutedEventArgs e)
        {
            LoadRemainsCard();
            btnRemains.Background = new SolidColorBrush(Colors.Gray);
            btnAllList.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#249ad7"));
        }

        private void btnAllList_Click(object sender, RoutedEventArgs e)
        {

            LoadCards();
            btnAllList.Background = new SolidColorBrush(Colors.Gray);
            btnRemains.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#249ad7"));
        }

        private void btnOpenIssuences_Click(object sender, RoutedEventArgs e)
        {
            
            Manager.Frame.Navigate(new IssuancesPage());
        }

        private void tbBackward_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) 
            {
                AuthorizeWn authorizeWn = new AuthorizeWn();
                authorizeWn.Show();
                Window.GetWindow(this).Close();
               
            }
            
        }

        private void btnOpenReports_Click(object sender, RoutedEventArgs e)
        {
            ReportsWn reportsWn = new ReportsWn();
            reportsWn.Show();
        }

        private void btnOpenDiagrams_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadCards();
                e.Handled = true;
            }


        }
    }
}

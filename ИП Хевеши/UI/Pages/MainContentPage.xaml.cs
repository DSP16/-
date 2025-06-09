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
 
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using ИП_Хевеши.UI.Winds.TipWinds;
using ИП_Хевеши.Views;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainContentPage.xaml
    /// </summary>
    public partial class MainContentPage : Page
    {

        private ИП_ХевешиEntities context = new ИП_ХевешиEntities();
        private List<ComponentViewModel> allComponents;

        public MainContentPage()
        {
            InitializeComponent();
            LoadData();
            SetupFilters();
            ApplyFilters();
        }

        private void LoadData()
        {
            allComponents = context.Components.Select(c => new ComponentViewModel
            {
                ID = c.ID,
                Name = c.Name,
                Type = c.Type,
                Price = c.Price,
                SKU = c.SKU,
                Quantity = c.Quantity ?? 0,
                MinQuantity = c.MinQuantity ?? 0,
                ProviderName = c.Arrivals
                                 .OrderByDescending(a => a.ArrivalDate)
                                 .Select(a => a.Providers.Name)
                                 .FirstOrDefault() ?? "Не было поставок",
                LastArrivalDate = c.Arrivals
                                    .OrderByDescending(a => a.ArrivalDate)
                                    .Select(a => a.ArrivalDate)
                                    .FirstOrDefault()
            }).ToList();
        }

        private void SetupFilters()
        {
            var categories = allComponents.Select(c => c.Type).Distinct().ToList();
            categories.Insert(0, "Все");
            CategoryFilter.ItemsSource = categories;

            var suppliers = allComponents.Select(c => c.ProviderName).Distinct().ToList();
             
            suppliers.Insert(0, "Все");
            SupplierFilter.ItemsSource = suppliers;

            CategoryFilter.SelectedIndex = 0;
            SupplierFilter.SelectedIndex = 0;
            StockLevelFilter.SelectedIndex = 1;
            AvailabilityFilter.SelectedIndex = 0;
            TurnoverFilter.SelectedIndex = 0;
            ArrivalDateFilter.SelectedIndex = 0;
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = allComponents.AsEnumerable();

            if (CategoryFilter.SelectedItem is string category && category != "Все")
                filtered = filtered.Where(c => c.Type == category);

            if (StockLevelFilter.SelectedIndex == 1)
                filtered = filtered.Where(c => c.Quantity < c.MinQuantity);

            if (AvailabilityFilter.SelectedIndex == 1)
                filtered = filtered.Where(c => c.Quantity > 0);

            if (TurnoverFilter.SelectedIndex == 1)
            {
                var topSellingIds = context.Issuance
                    .GroupBy(i => i.ComponentID)
                    .Select(g => new { ComponentID = g.Key, SoldQty = g.Sum(i => i.Quantity) })
                    .OrderByDescending(g => g.SoldQty)
                    .Take(10)
                    .Select(g => g.ComponentID)
                    .ToList();

                filtered = filtered.Where(c => topSellingIds.Contains(c.ID));
            }

            if (ArrivalDateFilter.SelectedIndex == 1)
            {
                var threshold = DateTime.Now.AddDays(-30);
                filtered = filtered.Where(c => c.LastArrivalDate >= threshold);
            }
            else if (ArrivalDateFilter.SelectedIndex == 2)
            {
                var threshold = DateTime.Now.AddYears(-1);
                filtered = filtered.Where(c => c.LastArrivalDate >= threshold);
            }

            if (SupplierFilter.SelectedItem is string supplier && supplier != "Все")
                filtered = filtered.Where(c => c.ProviderName == supplier);

            ProductsList.ItemsSource = filtered.ToList();
            var filteredList = filtered.ToList();
            ProductsList.ItemsSource = filteredList;

            // Проверяем пустоту и управляем отображением
            if (filteredList.Count == 0)
            {
                EmptyTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        public class ComponentViewModel
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public int MinQuantity { get; set; }
            public string ProviderName { get; set; }
            public string SKU { get; set; }
            public string ProviderText
            {
                get
                {
                    if (LastArrivalDate == null || string.IsNullOrWhiteSpace(ProviderName) || ProviderName == "Не было поставок")
                        return "Не было поставок";
                    return $"Поставщик: {ProviderName}";
                }
            }
            public DateTime? LastArrivalDate { get; set; }
            public string ActualityText =>
       (Quantity == 0 || LastArrivalDate == null || ProviderName == "Не было поставок") ? "Не актуально" : "Актуально";

            public string ActualityColor =>
                (Quantity == 0 || LastArrivalDate == null || ProviderName == "Не было поставок") ? "Red" : "Green";
        }
        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StockLevelFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void AvailabilityFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TurnoverFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ArrivalDateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SupplierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectedIndex = 0;
            SupplierFilter.SelectedIndex = 0;
            StockLevelFilter.SelectedIndex = 0;
            AvailabilityFilter.SelectedIndex = 0;
            TurnoverFilter.SelectedIndex = 0;
            ArrivalDateFilter.SelectedIndex = 0;

            ApplyFilters();
        }
    }
}

    


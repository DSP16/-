using DocumentFormat.OpenXml.Drawing.Charts;
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
using ИП_Хевеши.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveCharts.Wpf;
using LiveCharts;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для AnalyticsPage.xaml
    /// </summary>
    public partial class AnalyticsPage : Page
    {
        private List<InventoryStockGroup> stockByDatesFull;
        public class InventoryStockGroup
        {
            public DateTime Date { get; set; }
            public int TotalStock { get; set; }
            public List<CategoryStock> Categories { get; set; }
        }

        public class CategoryStock
        {
            public string Type { get; set; }
            public int Quantity { get; set; }
        }

        public AnalyticsPage()
        {
            InitializeComponent();
            LoadAnalytics();
            LoadTypes();
        }
        public void LoadAnalytics()
        {
            try
            {
                using (var context = new ИП_ХевешиEntities())
                {
                    // --- Определяем фильтр времени ---
                    DateTime? filterDate = null;

                    if (DateFilter.SelectedIndex == 1) // последние 30 дней
                        filterDate = DateTime.Now.AddDays(-30);
                    else if (DateFilter.SelectedIndex == 2) // последний год
                        filterDate = DateTime.Now.AddYears(-1);

                    // --- Загружаем продажи (issuance) ---
                    var issuanceQuery = context.Issuance
                        .Join(context.IssuanceReceipts, i => i.ReceiptID, ir => ir.ID, (i, ir) => new { i, ir })
                        .Join(context.Components, x => x.i.ComponentID, c => c.ID, (x, c) => new { x.i, x.ir, c });

                    if (filterDate.HasValue)
                        issuanceQuery = issuanceQuery.Where(x => x.ir.Date >= filterDate.Value);

                    var issuance = issuanceQuery.ToList();

                    decimal totalRevenue = issuance.Sum(x => (x.i.Quantity ?? 0) * x.c.Price);

                    // --- Анализ по категориям ---
                    var groupAnalysis = issuance
                        .GroupBy(x => x.c.Type)
                        .Select(g => new GroupAnalysis
                        {
                            Category = g.Key,
                            SoldQuantity = g.Sum(x => x.i.Quantity ?? 0),
                            Revenue = g.Sum(x => (x.i.Quantity ?? 0) * x.c.Price),
                            StockQuantity = context.Components.Where(c => c.Type == g.Key).Sum(c => c.Quantity ?? 0),
                            SalesShare = (double)(g.Sum(x => (x.i.Quantity ?? 0) * x.c.Price) / (totalRevenue == 0 ? 1 : totalRevenue)) * 100
                        })
                        .ToList();

                    CategoryAnalysisPanel.Children.Clear();

                    foreach (var item in groupAnalysis)
                    {
                        var border = new Border
                        {
                            BorderBrush = Brushes.LightGray,
                            BorderThickness = new System.Windows.Thickness(1),
                            CornerRadius = new CornerRadius(10),
                            Margin = new System.Windows.Thickness(5),
                            Padding = new System.Windows.Thickness(10),
                            Width = 220,
                            Background = Brushes.White
                        };

                        var stack = new StackPanel();
                        stack.Children.Add(new TextBlock { Text = item.Category, FontWeight = FontWeights.Bold, FontSize = 16 });
                        stack.Children.Add(new TextBlock { Text = $"Выручка: {item.Revenue:N2} руб.", FontWeight = FontWeights.Bold, FontSize = 12 });
                        stack.Children.Add(new TextBlock { Text = $"Продано: {item.SoldQuantity} шт." });
                        stack.Children.Add(new TextBlock { Text = $"Остаток: {item.StockQuantity} шт." });
                        stack.Children.Add(new TextBlock { Text = $"Доля продаж: {item.SalesShare:F1}%" });

                        border.Child = stack;
                        CategoryAnalysisPanel.Children.Add(border);
                    }

                    // --- Оборачиваемость ---
                    string selectedType = TypeFilter.SelectedItem?.ToString();

                    var filteredIssuance = issuance.AsEnumerable();

                    if (selectedType != null && selectedType != "Все")
                    {
                        filteredIssuance = filteredIssuance.Where(x => x.c.Type == selectedType);
                    }

                    var turnover = filteredIssuance
                        .GroupBy(x => x.c.Name)
                        .Select(g => new TurnoverAnalysis
                        {
                            Product = g.Key,
                            SoldQuantity = g.Sum(x => x.i.Quantity ?? 0)
                        })
                        .OrderByDescending(t => t.SoldQuantity)
                        .ToList();

                    TurnoverPanel.Children.Clear();

                    foreach (var item in turnover)
                    {
                        var border = new Border
                        {
                            BorderBrush = Brushes.LightGray,
                            BorderThickness = new System.Windows.Thickness(1),
                            CornerRadius = new CornerRadius(10),
                            Margin = new System.Windows.Thickness(5),
                            Padding = new System.Windows.Thickness(10),
                            Width = 220,
                            Background = Brushes.White
                        };

                        var stack = new StackPanel();
                        stack.Children.Add(new TextBlock { Text = item.Product, FontWeight = FontWeights.Bold, FontSize = 16 });
                        stack.Children.Add(new TextBlock { Text = $"Продано: {item.SoldQuantity} шт." , FontWeight = FontWeights.Bold});

                        border.Child = stack;
                        TurnoverPanel.Children.Add(border);
                    }

                    // --- Динамика остатков ---
                    var inventoryQuery = context.InventoryChecks
                        .Where(c => c.InventoryDate.HasValue);

                    if (filterDate.HasValue)
                        inventoryQuery = inventoryQuery.Where(c => c.InventoryDate >= filterDate.Value);

                    var rawInventory =  context.InventoryChecks.Include("Components").Where(c => c.InventoryDate.HasValue)
    .ToList();

                    stockByDatesFull = rawInventory
     .GroupBy(c => c.InventoryDate.Value.Date)
     .Select(g => new InventoryStockGroup
     {
         Date = g.Key,
         TotalStock = g.Sum(c => c.ActualQuantity ?? 0),
         Categories = g.GroupBy(x => x.Components.Type)
                       .Select(cg => new CategoryStock
                       {
                           Type = cg.Key,
                           Quantity = cg.Sum(x => x.ActualQuantity ?? 0)
                       })
                       .ToList()
     })
     .OrderBy(g => g.Date)
     .ToList();
                    if (stockByDatesFull.Count == 0)
                    {
                        StockChart.Visibility = Visibility.Collapsed;
                        NoStockDataTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StockChart.Visibility = Visibility.Visible;
                        NoStockDataTextBlock.Visibility = Visibility.Collapsed;

                        var values = stockByDatesFull.Select(x => (double)x.TotalStock).ToArray();
                        var labels = stockByDatesFull.Select(x => x.Date.ToString("dd.MM")).ToArray();

                        StockChart.Series = new SeriesCollection
{
    new ColumnSeries
    {
        Title = "Остатки",
        Values = new ChartValues<double>(values),
        DataLabels = false,
       LabelPoint = point =>
{
    int index = (int)point.X;
    var stockData = stockByDatesFull[index];

    // Защита — если вдруг categories нет (чисто на всякий случай)
    if (stockData.Categories == null || stockData.Categories.Count == 0)
        return $"{stockData.Date:dd.MM.yyyy}\nНет данных по категориям";

    // Формируем текст по всем категориям:
    var categoriesInfo = string.Join(Environment.NewLine,
        stockData.Categories.Select(c => $"{c.Type}: {c.Quantity}"));

    return $"{stockData.Date:dd.MM.yyyy}\n{categoriesInfo}";
}
    }
};
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        private void LoadTypes()
        {
            try
            {
                using (var context = new ИП_ХевешиEntities())
                {
                    var types = context.Components
                        .Select(c => c.Type)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToList();

                    TypeFilter.Items.Add("Все"); // универсальный пункт
                    foreach (var type in types)
                    {
                        TypeFilter.Items.Add(type);
                    }
                    TypeFilter.SelectedIndex = 0; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        private void DateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAnalytics();
        }

        private void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAnalytics();
        }
    }

    public class GroupAnalysis
    {
        public string Category { get; set; }
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
        public int StockQuantity { get; set; }
        public double SalesShare { get; set; }
    }
    public class TurnoverAnalysis
    {
        public string Product { get; set; }
        public int SoldQuantity { get; set; }
    }
    public class StockAnalysis
    {
        public DateTime Date { get; set; }
        public int TotalStock { get; set; }
    }

}

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
using System.Xml.Linq;
using ИП_Хевеши.Data;
using ИП_Хевеши.Views;
using System.Data.Entity;
using System.Text.RegularExpressions;
using ИП_Хевеши.Winds;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.Classes;
using ClosedXML.Excel;
using System.IO;
using System.Globalization;


namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReceiptPage.xaml
    /// </summary>
    public partial class ReceiptPage : Page
    {
        public ObservableCollection<ReceiptViewModel> Receipts { get; set; }

        public ReceiptPage()
        {
            InitializeComponent();
            LoadReceipts();
            DataContext = this;
        }
        private void ReceiptCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            int receiptId = (int)border.Tag;
            bool IsNew = false;
            NavigationService?.Navigate(new ReceiptDetailsPage(receiptId, IsNew));
        }
        private void LoadReceipts()
        {
            using (var db = new ИП_ХевешиEntities())
            {
                // Загружаем только накладные, у которых назначен номер
                var receiptList = db.Receipts
                    .Where(r => r.ReceiptNumber != null)
                    .Include(r => r.Providers)
                    .ToList()
                    .Select(r => new ReceiptViewModel
                    {
                        ID = r.ID,
                        ReceiptNumber = r.ReceiptNumber,
                        Date = r.Date,
                        ProviderName = r.Providers.Name,
                        UserName = r.Users.UserName // если хочешь вывести также имя пользователя
                    });
               
                Receipts = new ObservableCollection<ReceiptViewModel>(receiptList);
               
            }

            // Обновление привязки
            DataContext = null;
            
            DataContext = this;
          
        }


        private void ImportXml_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "XLSX файлы|*.xlsx" };
            if (dialog.ShowDialog() == true)
            {
                ImportReceiptsFromExcel(dialog.FileName);
                LoadReceipts();
            }
        }

        public void ImportReceiptsFromExcel(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропустить заголовок

                    using (var db = new ИП_ХевешиEntities())
                    {
                        // Получение последнего номера накладной
                        string lastReceiptNumber = db.Receipts
                            .OrderByDescending(r => r.ID)
                            .Select(r => r.ReceiptNumber)
                            .FirstOrDefault();

                        int lastNumber = 0;
                        if (!string.IsNullOrEmpty(lastReceiptNumber))
                        {
                            var match = Regex.Match(lastReceiptNumber, @"\d+");
                            if (match.Success) lastNumber = int.Parse(match.Value);
                        }

                        Receipts currentReceipt = null;
                        DateTime currentDate;
                        int? currentProviderId = null;
                        int? currentUserId = null;

                        foreach (var row in rows)
                        {
                            // Проверка на полностью пустую строку (разделитель)
                            if (row.IsEmpty())
                            {
                                currentReceipt = null;
                                continue;
                            }

                            // Проверка начала новой накладной (A, B, C заполнены)
                            bool isNewReceipt = !row.Cell(1).IsEmpty() &&
                                              !row.Cell(2).IsEmpty() &&
                                              !row.Cell(3).IsEmpty();

                            if (isNewReceipt)
                            {
                                // Генерация номера
                                string number = "HK-" + (++lastNumber).ToString("D3");

                                // Парсинг данных накладной
                                string dateString = row.Cell(1).GetString();
                                if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate))
                                {
                                    MessageBox.Show($"Ошибка в строке {row.RowNumber()}: неверный формат даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    continue;
                                }

                                string providerName = row.Cell(2).GetValue<string>();
                                string userName = row.Cell(3).GetValue<string>();

                                var provider = db.Providers.FirstOrDefault(p => p.Name == providerName);
                                var user = db.Users.FirstOrDefault(u => u.UserName == userName);

                                if (provider == null || user == null)
                                {
                                    MessageBox.Show($"Не найдены данные в строке {row.RowNumber()}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    continue;
                                }

                                // Создание накладной
                                currentReceipt = new Receipts
                                {
                                    ReceiptNumber = number,
                                    Date = currentDate,
                                    ProviderID = provider.ID,
                                    UserID = user.ID
                                };

                                db.Receipts.Add(currentReceipt);
                                db.SaveChanges();

                                currentProviderId = provider.ID;
                                currentUserId = user.ID;
                            }

                            // Добавление компонента (D, E, F заполнены)
                            if (currentReceipt != null && !row.Cell(4).IsEmpty())
                            {
                                string componentName = row.Cell(4).GetValue<string>();
                                int quantity = row.Cell(5).IsEmpty() ? 0 : row.Cell(5).GetValue<int>();
                                decimal price = row.Cell(6).IsEmpty() ? 0 : row.Cell(6).GetValue<decimal>();

                                var component = db.Components.FirstOrDefault(c => c.Name == componentName);
                                if (component == null)
                                {
                                    MessageBox.Show($"Компонент '{componentName}' не найден (строка {row.RowNumber()})", "Ошибка поиска", MessageBoxButton.OK, MessageBoxImage.Hand);
                                    continue;
                                }

                                var arrival = new Arrivals
                                {
                                    ComponentID = component.ID,
                                    Quantity = quantity,
                                    PurchasePrice = price,
                                    ArrivalDate = currentReceipt.Date,
                                    ReceiptID = currentReceipt.ID,
                                    ProviderID = currentProviderId.Value,
                                    UserID = currentUserId.Value
                                };

                                db.Arrivals.Add(arrival);
                                component.Quantity += quantity;
                            }
                        }

                        db.SaveChanges();
                        MessageBox.Show("Импорт завершен успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddReceipt_Click(object sender, RoutedEventArgs e)
        {
           
                

                var dialog = new CreateArrivalReceiptWindow();
                if (dialog.ShowDialog() == true)
                {
                    NavigationService?.Navigate(new ReceiptDetailsPage(dialog.CreatedReceiptId, isNew: true));
                }
            
        }

        private void FilterByDate_Click(object sender, RoutedEventArgs e)
        {
            if (dpFilterDate.SelectedDate is DateTime selectedDate)
            {
                var firstDayOfSelectedMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                var firstDayOfNextMonth = firstDayOfSelectedMonth.AddMonths(1);

                
                FutureDateTextBlock.Visibility = Visibility.Collapsed;
                NoDataTextBlock.Visibility = Visibility.Collapsed;
                int month = selectedDate.Month;
                int year = selectedDate.Year;

                using (var db = new ИП_ХевешиEntities())
                {
                    var filtered = db.Receipts
                        .Where(r => r.ReceiptNumber != null &&
                                    r.Date.Month == month &&
                                    r.Date.Year == year)
                        .Include(r => r.Providers)
                        .Include(r => r.Users)
                        .ToList()
                        .Select(r => new ReceiptViewModel
                        {
                            ID = r.ID,
                            ReceiptNumber = r.ReceiptNumber,
                            Date = r.Date,
                            ProviderName = r.Providers.Name,
                            UserName = r.Users.UserName
                        });
                  
                    Receipts = new ObservableCollection<ReceiptViewModel>(filtered);
                }
                DataContext = null;
                DataContext = this;
                if (firstDayOfNextMonth > DateTime.Now.AddMonths(1))
                {
                    FutureDateTextBlock.Visibility = Visibility.Visible;
                    NoDataTextBlock.Visibility = Visibility.Collapsed;
                   
                    return;
                }
                if (Receipts.Count == 0)
                {
                    NoDataTextBlock.Visibility = Visibility.Visible;
                    FutureDateTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoDataTextBlock.Visibility = Visibility.Collapsed;
                    FutureDateTextBlock.Visibility = Visibility.Collapsed;
                }
               
            }
            else
            {
                MessageBox.Show("Выберите дату для фильтрации.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void btnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            Classes.Manager.Frame.Navigate(new ReceiptPage());
        }

        private void btnAddProvider_Click(object sender, RoutedEventArgs e)
        {
            AddProviderWn addProviderWn = new AddProviderWn();
            addProviderWn.Show();
        }
    }
}

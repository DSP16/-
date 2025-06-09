using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Data.Entity;
using ИП_Хевеши.Views;
using ИП_Хевеши.Classes;
using ИП_Хевеши.Winds;
using ClosedXML.Excel;
using System.Globalization;
using ИП_Хевеши.UI.Winds;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для IssuanceReceiptsPage.xaml
    /// </summary>
    public partial class IssuanceReceiptsPage : Page
    {
        public ObservableCollection<IssuanceReceiptsViewModel> Receipts { get; set; }

        public IssuanceReceiptsPage()
        {
            InitializeComponent();
            LoadReceipts();
            DataContext = this;
        }

        private void LoadReceipts()
        {
            using (var db = new ИП_ХевешиEntities())
            {
                var list = db.IssuanceReceipts
                             .Where(r => r.ReceiptNumber != null)
                             .Include(r => r.Buyers)
                             .ToList()
                             .Select(r => new IssuanceReceiptsViewModel
                             {
                                 ID = r.ID,
                                 ReceiptNumber = r.ReceiptNumber,
                                 Date = r.Date,
                                 BuyerName = r.Buyers.Name,
                                 UserName = r.Users.UserName
                                 
                             });
                
               
                Receipts = new ObservableCollection<IssuanceReceiptsViewModel>(list);
            }

            DataContext = null;
            DataContext = this;
        }

        private void OpenReceipt_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            int id = (int)border.Tag;
            NavigationService?.Navigate(new IssuanceReceiptDetailsPage(id));
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateIssuanceReceiptWindow();
            if (dialog.ShowDialog() == true)
            {
                NavigationService?.Navigate(new IssuanceReceiptDetailsPage(dialog.CreatedReceiptId, isNew: true));
            }
        }

     

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "XLSX файлы|*.xlsx" };
            if (dialog.ShowDialog() == true)
            {
                ImportIssuanceReceiptsFromExcel(dialog.FileName);
                LoadReceipts();
            }
        }

        public void ImportIssuanceReceiptsFromExcel(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропустить заголовок

                    using (var db = new ИП_ХевешиEntities())
                    {
                        // Получение последнего номера расходной накладной
                        string lastIssuanceNumber = db.IssuanceReceipts
                            .OrderByDescending(r => r.ID)
                            .Select(r => r.ReceiptNumber)
                            .FirstOrDefault();

                        int lastNumber = 0;
                        if (!string.IsNullOrEmpty(lastIssuanceNumber))
                        {
                            var match = Regex.Match(lastIssuanceNumber, @"\d+");
                            if (match.Success) lastNumber = int.Parse(match.Value);
                        }

                        IssuanceReceipts currentReceipt = null;
                        DateTime currentDate;
                        int? currentBuyerId = null;
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
                                string number = "PAC-" + (++lastNumber).ToString("D3");

                                // Парсинг даты (с поддержкой времени)
                                var cellValue = row.Cell(1).GetValue<DateTime>();
                                currentDate = cellValue.Date; // Обрезаем время

                                string buyerName = row.Cell(2).GetValue<string>();
                                string userName = row.Cell(3).GetValue<string>();

                                var buyer = db.Buyers.FirstOrDefault(b => b.Name == buyerName);
                                var user = db.Users.FirstOrDefault(u => u.UserName == userName);

                                if (buyer == null || user == null)
                                {
                                    MessageBox.Show($"Не найдены данные в строке {row.RowNumber()}", "Ошибка поиска", MessageBoxButton.OK, MessageBoxImage.Error);
                                    continue;
                                }

                                // Создание расходной накладной
                                currentReceipt = new IssuanceReceipts
                                {
                                    ReceiptNumber = number,
                                    Date = currentDate, // Сохраняем только дату
                                    BuyerID = buyer.ID,
                                    UserID = user.ID
                                };

                                db.IssuanceReceipts.Add(currentReceipt);
                                db.SaveChanges();

                                currentBuyerId = buyer.ID;
                                currentUserId = user.ID;
                            }

                            // Добавление расхода (D, E заполнены)
                            if (currentReceipt != null && !row.Cell(4).IsEmpty())
                            {
                                string componentName = row.Cell(4).GetValue<string>();
                                int quantity = row.Cell(5).IsEmpty() ? 0 : row.Cell(5).GetValue<int>();

                                var component = db.Components.FirstOrDefault(c => c.Name == componentName);
                                if (component == null)
                                {
                                    MessageBox.Show($"Компонент '{componentName}' не найден (строка {row.RowNumber()})", "Ошибка поиска", MessageBoxButton.OK, MessageBoxImage.Hand);
                                    continue;
                                }

                                if (component.Quantity < quantity)
                                {
                                    MessageBox.Show($"Недостаточно компонента '{componentName}' (строка {row.RowNumber()})", "Ошибка формирования расхода", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    continue;
                                }

                                var issuance = new Issuance
                                {
                                    ComponentID = component.ID,
                                    Quantity = quantity,
                                    IssuanceDate = currentReceipt.Date, // Используем обрезанную дату
                                    BuyerID = currentBuyerId.Value,
                                    UserID = currentUserId.Value,
                                    ReceiptID = currentReceipt.ID
                                };

                                db.Issuance.Add(issuance);
                                component.Quantity -= quantity;
                            }
                        }

                        db.SaveChanges();
                        MessageBox.Show("Импорт завершен успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterByDate_Click(object sender, RoutedEventArgs e)
        {
            if (dpFilterDate.SelectedDate is DateTime selectedDate)
            {
                var firstDayOfSelectedMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                var firstDayOfNextMonth = firstDayOfSelectedMonth.AddMonths(1);

                if (firstDayOfNextMonth > DateTime.Now.AddDays(1))
                {
                    FutureDateTextBlock.Visibility = Visibility.Visible;
                    NoDataTextBlock.Visibility = Visibility.Collapsed;
                    return;
                }
                int month = selectedDate.Month;
                int year = selectedDate.Year;

                using (var db = new ИП_ХевешиEntities())
                {
                    var filtered = db.IssuanceReceipts
                        .Where(r => r.ReceiptNumber != null &&
                                    r.Date.Month == month &&
                                    r.Date.Year == year)
                        .Include(r => r.Buyers)
                        .Include(r => r.Users)
                        .ToList()
                        .Select(r => new IssuanceReceiptsViewModel
                        {
                            ID = r.ID,
                            ReceiptNumber = r.ReceiptNumber,
                            Date = r.Date,
                            BuyerName = r.Buyers.Name,
                            UserName = r.Users.UserName
                        });

                    Receipts = new ObservableCollection<IssuanceReceiptsViewModel>(filtered);
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
                DataContext = null;
                DataContext = this;
            }
            else
            {
                MessageBox.Show("Выберите дату для фильтрации.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void btnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            Classes.Manager.Frame.Navigate(new IssuanceReceiptsPage());
        }

        private void btnAddBuyer_Click(object sender, RoutedEventArgs e)
        {
            AddBuyerWn addBuyerWn = new AddBuyerWn();
            addBuyerWn.Show();
        }
    }
}

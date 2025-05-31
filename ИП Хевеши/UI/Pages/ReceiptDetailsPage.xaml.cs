using Microsoft.Win32;
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
using System.Xml.Linq;
using ИП_Хевеши.Data;
using System.Data.Entity;
using DocumentFormat.OpenXml.Vml.Office;
using ИП_Хевеши.UI.Winds;
using DocumentFormat.OpenXml.ExtendedProperties;
using ИП_Хевеши.Classes;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReceiptDetailsPage.xaml
    /// </summary>
    public partial class ReceiptDetailsPage : Page
    {
        private readonly bool isNewReceipt;
        public List<Arrivals> Arrivals { get; set; }
        private int receiptId;
        private bool hasAddedArrivals = false;
        public ReceiptDetailsPage(int receiptId, bool isNew )
        {
            InitializeComponent();
            this.receiptId = receiptId;
            this.isNewReceipt = isNew;
            this.Unloaded += ReceiptDetailsPage_Unloaded;
            System.Windows.Application.Current.Exit += App_Exit;
            using (var db = new ИП_ХевешиEntities())
            {
                var receiptNumber = db.Receipts.FirstOrDefault(r => r.ID == receiptId);
                tbReceipt.Text = $"Товары по приходной накладной\t–  {receiptNumber.ReceiptNumber}";
            }
            LoadData();
        }

        private void TryRemoveReceiptIfEmpty()
        {
            if (hasAddedArrivals) return;

            using (var db = new ИП_ХевешиEntities())
            {
                var arrivals = db.Arrivals.Where(a => a.ReceiptID == receiptId).ToList();
                if (!arrivals.Any())
                {
                    var receipt = db.Receipts.Find(receiptId);
                    if (receipt != null)
                    {
                        db.Receipts.Remove(receipt);
                        db.SaveChanges();
                    }
                }
            }
        }
        private void ReceiptDetailsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            TryRemoveReceiptIfEmpty();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            TryRemoveReceiptIfEmpty();
        }   
        
        private void LoadData()
        {
            using (var db = new ИП_ХевешиEntities())
            {
                Arrivals = db.Arrivals
                    .Include(a => a.Components)
                    .Where(a => a.ReceiptID == receiptId)
                    .ToList();
                DataContext = null;
                DataContext = this;
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ИП_ХевешиEntities())
            {
                var receipt = db.Receipts
                    .Include(r => r.Providers)
                    .Include(r => r.Users)
                    .First(r => r.ID == receiptId);

                var arrivals = db.Arrivals
                    .Include(a => a.Components)
                    .Where(a => a.ReceiptID == receiptId)
                    .ToList();

                // Создание Excel-файла
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Поступление");

                    // Заголовок
                    worksheet.Cell(1, 1).Value = "Накладная №";
                    worksheet.Cell(1, 2).Value = receipt.ReceiptNumber;
                    worksheet.Cell(2, 1).Value = "Дата:";
                    worksheet.Cell(2, 2).Value = receipt.Date.ToShortDateString();
                    worksheet.Cell(3, 1).Value = "Поставщик:";
                    worksheet.Cell(3, 2).Value = receipt.Providers.Name;

                    // Заголовки таблицы
                    worksheet.Cell(5, 1).Value = "Комплектующее";
                    worksheet.Cell(5, 2).Value = "Количество";
                    worksheet.Cell(5, 3).Value = "Цена закупки";

                    // Данные
                    int row = 6;
                    foreach (var a in arrivals)
                    {
                        worksheet.Cell(row, 1).Value = a.Components.Name;
                        worksheet.Cell(row, 2).Value = a.Quantity;
                        worksheet.Cell(row, 3).Value = a.PurchasePrice;
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    // Сохранение файла
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        FileName = $"Накладная_{receipt.ReceiptNumber}_{receipt.Date:yyyyMMdd}.xlsx"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        workbook.SaveAs(dialog.FileName);
                        MessageBox.Show("Отчет успешно сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
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
                MessageBox.Show("Нет предыдущей страницы.", "Ошибка навигации", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isNewReceipt)
                btnAddArrivedComponent.Visibility = Visibility.Collapsed;
        }

        private void AddComponentToReceipt_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddArrivalToReceiptWindow(receiptId); // ← проверь что тут реально ID
            if (window.ShowDialog() == true)
            {
                hasAddedArrivals = true;
                LoadData();
            }
        }

        private void btnbtnSave_Click(object sender, RoutedEventArgs e)
        {
            Classes.Manager.Frame.Navigate(new ReceiptPage());
        }
    }
}

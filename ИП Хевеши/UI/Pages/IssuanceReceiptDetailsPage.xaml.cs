using ClosedXML.Excel;
using Microsoft.Win32;
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
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Winds;
using System.Data.Entity;
using ИП_Хевеши.Classes;
using ИП_Хевеши.Views;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для IssuanceReceiptDetailsPage.xaml
    /// </summary>
    
        public partial class IssuanceReceiptDetailsPage : Page
        {
            private readonly int receiptId;
            private readonly bool isNew;
            private bool hasAddedIssuance = false;

            public ObservableCollection<Issuance> IssuanceItems { get; set; }

            public IssuanceReceiptDetailsPage(int receiptId, bool isNew = false)
            {
                InitializeComponent();
                this.receiptId = receiptId;
                this.isNew = isNew;

                LoadData();
                DataContext = this;
                using (var db = new ИП_ХевешиEntities())
                {
                    var receiptNumber = db.IssuanceReceipts.FirstOrDefault(r => r.ID == receiptId);
                    tbIssuanceReceipt.Text = $"Товары по расходной накладной\t–  {receiptNumber.ReceiptNumber}";
                }
            if (!isNew)
                    btnAdd.Visibility = Visibility.Collapsed;

                Unloaded += OnPageUnloaded;
                Application.Current.Exit += OnAppExit;
            }

            private void LoadData()
            {
                using (var db = new ИП_ХевешиEntities())
                {
                    var list = db.Issuance
                                 .Include(i => i.Components)
                                 .Where(i => i.ReceiptID == receiptId)
                                 .ToList();

                    IssuanceItems = new ObservableCollection<Issuance>(list);
                    DataContext = null;
                    DataContext = this;
                }
            }

            private void AddIssuance_Click(object sender, RoutedEventArgs e)
            {
                var win = new AddIssuanceToReceiptWindow(receiptId);
                if (win.ShowDialog() == true)
                {
                    hasAddedIssuance = true;
                    LoadData();
                }
            }

            private void Export_Click(object sender, RoutedEventArgs e)
            {
                using (var db = new ИП_ХевешиEntities())
                {
                    var receipt = db.IssuanceReceipts
                                    .Include(r => r.Buyers)
                                    .First(r => r.ID == receiptId);

                    var items = db.Issuance
                                  .Include(i => i.Components)
                                  .Where(i => i.ReceiptID == receiptId)
                                  .ToList();

                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Расход");

                        ws.Cell(1, 1).Value = "Накладная №";
                        ws.Cell(1, 2).Value = receipt.ReceiptNumber;
                        ws.Cell(2, 1).Value = "Дата:";
                        ws.Cell(2, 2).Value = receipt.Date.ToShortDateString();
                        ws.Cell(3, 1).Value = "Покупатель:";
                        ws.Cell(3, 2).Value = receipt.Buyers.Name;

                        ws.Cell(5, 1).Value = "Комплектующее";
                        ws.Cell(5, 2).Value = "Количество";

                        int row = 6;
                        foreach (var i in items)
                        {
                            ws.Cell(row, 1).Value = i.Components.Name;
                            ws.Cell(row, 2).Value = i.Quantity;
                            row++;
                        }

                        ws.Columns().AdjustToContents();

                        var dialog = new SaveFileDialog
                        {
                            FileName = $"Расход_{receipt.ReceiptNumber}_{receipt.Date:yyyyMMdd}.xlsx",
                            Filter = "Excel файлы (*.xlsx)|*.xlsx"
                        };

                        if (dialog.ShowDialog() == true)
                        {
                            workbook.SaveAs(dialog.FileName);
                            MessageBox.Show("Файл сохранён.");
                        }
                    }
                }
            }

            private void OnPageUnloaded(object sender, RoutedEventArgs e)
            {
                CleanupIfEmpty();
            }

            private void OnAppExit(object sender, ExitEventArgs e)
            {
                CleanupIfEmpty();
            }

        private void CleanupIfEmpty()
        {
            if (hasAddedIssuance) return;

            using (var db = new ИП_ХевешиEntities())
            {
                var items = db.Issuance.Where(i => i.ReceiptID == receiptId).ToList();
                if (!items.Any())
                {
                    var receipt = db.IssuanceReceipts.Find(receiptId);
                    if (receipt != null)
                    {
                        db.IssuanceReceipts.Remove(receipt);
                        db.SaveChanges();
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
                MessageBox.Show("Нет предыдущей страницы.");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new IssuanceReceiptsPage());
        }
    }
}

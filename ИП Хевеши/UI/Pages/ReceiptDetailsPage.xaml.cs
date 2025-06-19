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
using Xceed.Words.NET;
using System.IO;
using Xceed.Document.NET;
using ИП_Хевеши.Views;


namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReceiptDetailsPage.xaml
    /// </summary>
    public partial class ReceiptDetailsPage : Page
    {
        public string tempPath;
        private readonly bool isNewReceipt;
        public List<Arrivals> Arrivals { get; set; }
        private int receiptId;
        private bool hasAddedArrivals = false;
        public ReceiptDetailsPage(int receiptId, bool isNew)
        {
            InitializeComponent();
            this.receiptId = receiptId;
            this.isNewReceipt = isNew;
            if (isNew == true)
            {
                btnSave.Visibility = Visibility.Visible;
            }
            else
            {
                btnSave.Visibility = Visibility.Collapsed;
            }
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
            try
            {
              
                using (var context = new ИП_ХевешиEntities())
                {
                    var receipt = context.Receipts.Find(receiptId);
                    var provider = context.Providers.Find(receipt.ProviderID);
                    var user = context.Users.Find(receipt.UserID);
                    tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Приходная накладная_{receipt.ReceiptNumber}.docx");

                    var arrivals = context.Arrivals
                        .Where(a => a.ReceiptID == receiptId)
                        .ToList();

                    var dialog = new SaveFileDialog();
                    dialog.Title = "Сохранить накладную";
                    dialog.Filter = "Word Document (*.docx)|*.docx";
                    dialog.FileName = $"Накладная_{receipt.ReceiptNumber}.docx";

                    if (dialog.ShowDialog() != true)
                        return;

                    string filePath = dialog.FileName;

                    using (DocX document = DocX.Create(filePath))
                    {
                        document.PageLayout.Orientation = Xceed.Document.NET.Orientation.Portrait;

                        // Шапка
                        document.InsertParagraph("ИП Хевеши").FontSize(14).Bold().Alignment = Alignment.center;
                        document.InsertParagraph("ПРИХОДНАЯ НАКЛАДНАЯ").FontSize(14).Bold().Alignment = Alignment.center;
                        document.InsertParagraph();

                        document.InsertParagraph($"Документ № {receipt.ReceiptNumber} от {receipt.Date:dd.MM.yyyy}").FontSize(12);
                        document.InsertParagraph($"Поставщик: {provider.Name} ({provider.Country})").FontSize(12);
                        document.InsertParagraph($"Принял поставку: {user.UserSurname} {user.UserName}").FontSize(12);
                        document.InsertParagraph();

                        // Таблица
                        var table = document.AddTable(arrivals.Count + 2, 7);
                        table.Design = TableDesign.TableGrid;

                        // Заголовок
                        table.Rows[0].Cells[0].Paragraphs[0].Append("№");
                        table.Rows[0].Cells[1].Paragraphs[0].Append("Товар");
                        table.Rows[0].Cells[2].Paragraphs[0].Append("Тип");
                        table.Rows[0].Cells[3].Paragraphs[0].Append("Артикул");
                        table.Rows[0].Cells[4].Paragraphs[0].Append("Цена (руб.)");
                        table.Rows[0].Cells[5].Paragraphs[0].Append("Кол-во");
                        table.Rows[0].Cells[6].Paragraphs[0].Append("Сумма");

                        decimal totalSum = 0;
                        int totalQuantity = 0;

                        for (int i = 0; i < arrivals.Count; i++)
                        {
                            var item = arrivals[i];
                            var component = context.Components.Find(item.ComponentID);
                            decimal sum = (item.Quantity ?? 0) * (item.PurchasePrice ?? 0);

                            table.Rows[i + 1].Cells[0].Paragraphs[0].Append((i + 1).ToString());
                            table.Rows[i + 1].Cells[1].Paragraphs[0].Append(component.Name);
                            table.Rows[i + 1].Cells[2].Paragraphs[0].Append(component.Type);
                            table.Rows[i + 1].Cells[3].Paragraphs[0].Append(component.SKU);
                            table.Rows[i + 1].Cells[4].Paragraphs[0].Append($"{item.PurchasePrice:0.00}");
                            table.Rows[i + 1].Cells[5].Paragraphs[0].Append($"{item.Quantity}");
                            table.Rows[i + 1].Cells[6].Paragraphs[0].Append($"{sum:0.00}");

                            totalSum += sum;
                            totalQuantity += item.Quantity ?? 0;
                        }

                        // Итоговая строка
                       
                        table.Rows[arrivals.Count + 1].Cells[0].Paragraphs[0].Append("ИТОГО").Bold();
                        table.Rows[arrivals.Count + 1].Cells[5].Paragraphs[0].Append($"{totalQuantity}").Bold();
                        table.Rows[arrivals.Count + 1].Cells[6].Paragraphs[0].Append($"{totalSum:0.00}").Bold();

                        document.InsertTable(table);
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph();

                        // Блок для подписи
                        document.InsertParagraph("Принял: ____________________  (" + user.UserSurname + " " + user.UserName + ")").FontSize(12);
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph("М.П.").FontSize(12);

                        document.Save();


                    }
                    MessageBox.Show("Документ успешно сохранён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var window = new AddArrivalToReceiptWindow(receiptId); 
            if (window.ShowDialog() == true)
            {
                hasAddedArrivals = true;
                LoadData();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (hasAddedArrivals)
            {
                Classes.Manager.Frame.Navigate(new ReceiptPage());
            }
            else
            {
                MessageBox.Show("Добавьте как минимум одно комплектующее.", "Пустой список комплектующих", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
            
        }
    }
}

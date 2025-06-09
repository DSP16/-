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
using Xceed.Words.NET;
using Xceed.Document.NET;

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
                try
                {
                using (var context = new ИП_ХевешиEntities())
                {
                    var issuanceReceipt = context.IssuanceReceipts.Find(receiptId);
                    var buyer = context.Buyers.Find(issuanceReceipt.BuyerID);
                    var user = context.Users.Find(issuanceReceipt.UserID);

                    var issuances = context.Issuance
                        .Where(i => i.ReceiptID == receiptId)
                        .ToList();

                    
                    var dialog = new SaveFileDialog();
                    dialog.Title = "Сохранить расходную накладную";
                    dialog.Filter = "Word Document (*.docx)|*.docx";
                    dialog.FileName = $"Расходная накладная_{issuanceReceipt.ReceiptNumber}.docx";

                    if (dialog.ShowDialog() != true)
                        return;

                    string filePath = dialog.FileName;

                    
                    using (DocX document = DocX.Create(filePath))
                    {
                        document.PageLayout.Orientation = Xceed.Document.NET.Orientation.Portrait;

                        document.InsertParagraph("ИП Хевеши").FontSize(14).Bold().Alignment = Alignment.center;
                        document.InsertParagraph("Расходная накладная").FontSize(14).Bold().Alignment = Alignment.center;
                        document.InsertParagraph();

                        document.InsertParagraph($"Документ № {issuanceReceipt.ReceiptNumber} от {issuanceReceipt.Date:dd.MM.yyyy}").FontSize(12);
                        document.InsertParagraph($"Покупатель: {buyer.Name} ({buyer.Country})").FontSize(12);
                        document.InsertParagraph($"Отпустил: {user.UserSurname} {user.UserName}").FontSize(12);
                        document.InsertParagraph();

                        var table = document.AddTable(issuances.Count + 2, 7);
                        table.Design = TableDesign.TableGrid;

                        table.Rows[0].Cells[0].Paragraphs[0].Append("№");
                        table.Rows[0].Cells[1].Paragraphs[0].Append("Товар");
                        table.Rows[0].Cells[2].Paragraphs[0].Append("Тип");
                        table.Rows[0].Cells[3].Paragraphs[0].Append("Артикул");
                        table.Rows[0].Cells[4].Paragraphs[0].Append("Цена (руб.)");
                        table.Rows[0].Cells[5].Paragraphs[0].Append("Кол-во");
                        table.Rows[0].Cells[6].Paragraphs[0].Append("Сумма");

                        decimal totalSum = 0;
                        int totalQuantity = 0;

                        for (int i = 0; i < issuances.Count; i++)
                        {
                            var item = issuances[i];
                            var component = context.Components.Find(item.ComponentID);
                            decimal sum = (item.Quantity ?? 0) * component.Price;

                            table.Rows[i + 1].Cells[0].Paragraphs[0].Append((i + 1).ToString());
                            table.Rows[i + 1].Cells[1].Paragraphs[0].Append(component.Name);
                            table.Rows[i + 1].Cells[2].Paragraphs[0].Append(component.Type);
                            table.Rows[i + 1].Cells[3].Paragraphs[0].Append(component.SKU);
                            table.Rows[i + 1].Cells[4].Paragraphs[0].Append($"{component.Price:0.00}");
                            table.Rows[i + 1].Cells[5].Paragraphs[0].Append($"{item.Quantity}");
                            table.Rows[i + 1].Cells[6].Paragraphs[0].Append($"{sum:0.00}");

                            totalSum += sum;
                            totalQuantity += item.Quantity ?? 0;
                        }

                        table.Rows[issuances.Count + 1].Cells[0].Paragraphs[0].Append("ИТОГО").Bold();
                        table.Rows[issuances.Count + 1].Cells[5].Paragraphs[0].Append($"{totalQuantity}").Bold();
                        table.Rows[issuances.Count + 1].Cells[6].Paragraphs[0].Append($"{totalSum:0.00}").Bold();

                        document.InsertTable(table);
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph();
                        // Блок для подписи
                        document.InsertParagraph("Отпустил: ____________________(" + user.UserSurname + " " + user.UserName + ")").FontSize(12);
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph("М.П.").FontSize(12);
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph();
                        document.InsertParagraph("Принял: ____________________(" + buyer.Name+ ")").FontSize(12);
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
                MessageBox.Show("Нет предыдущей страницы.", "Ошибка навигации", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new IssuanceReceiptsPage());
        }
    }
}

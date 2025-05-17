using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ИП_Хевеши.Data;
using ClosedXML;
using ИП_Хевеши.UI.Winds;
using OfficeOpenXml;
using ИП_Хевеши.Views;
using DocumentFormat.OpenXml.Bibliography;

namespace ИП_Хевеши.Classes
{
    public class ReportsBack : ReportsWn
    {
        private static List<ProviderComponentsData> Providerdata;

        public static void GenerateSupplierComponentReport(string filePath)
        {

            try
            {
                // 1. Получаем данные
                
                using (var db = new ИП_ХевешиEntities()) // Замените на ваш контекст данных
                {
                    Providerdata = (from a in db.Arrivals
                            select new ProviderComponentsData
                            {
                                ProviderName = a.Providers.Name,
                                ComponentName = a.Components.Name,
                                Quantity = (int)a.Quantity,
                                PurchasePrice = (decimal)a.PurchasePrice,
                                ArrivalDate = a.ArrivalDate
                            }).ToList();
                }

                // 2. Создаем Excel файл
                using (XLWorkbook workbook = new XLWorkbook()) //  XLWorkbook вместо ExcelPackage
                {
                    // Добавляем лист
                    IXLWorksheet worksheet = workbook.Worksheets.Add("Поставщики и комплектующие"); //  IXLWorksheet

                    // Заголовки столбцов
                    worksheet.Cell(1, 1).Value = "Поставщик";  // Cell(row, column)
                    worksheet.Cell(1, 2).Value = "Комплектующее";
                    worksheet.Cell(1, 3).Value = "Количество";
                    worksheet.Cell(1, 4).Value = "Цена закупки";
                    worksheet.Cell(1, 5).Value = "Дата поступления";

                    // Записываем данные
                    int row = 2;
                    foreach (var item in Providerdata)
                    {
                        worksheet.Cell(row, 1).Value = item.ProviderName;
                        worksheet.Cell(row, 2).Value = item.ComponentName;
                        worksheet.Cell(row, 3).Value = item.Quantity;
                        worksheet.Cell(row, 4).Value = item.PurchasePrice;
                        worksheet.Cell(row, 5).Value = item.ArrivalDate;
                        row++;
                    }

                    // Сохраняем файл
                    workbook.SaveAs(filePath);  // SaveAs(string filePath)

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}");
            }
        }
        public void GenerateExcelReport(int selectedMonth, int enteredYear, string reportType)
        {

            DataTable dataTable = new DataTable();

            try
            {
                using (var db = new ИП_ХевешиEntities())
                {
                    IQueryable<object> query = null;

                    switch (reportType)
                    {
                        case "Поступления":
                            query = from a in db.Arrivals
                                    join c in db.Components on a.ComponentID equals c.ID
                                    join p in db.Providers on a.ProviderID equals p.ID
                                    join u in db.Users on a.UserID equals u.ID
                                    where a.ArrivalDate != null && a.ArrivalDate.Year == enteredYear && a.ArrivalDate.Month == selectedMonth
                                    orderby a.ArrivalDate
                                    select new
                                    {
                                        Комплектующее = c.Name,
                                        Тип_Комплектующего = c.Type,
                                        Дата_Поставки = a.ArrivalDate,
                                        Количество = a.Quantity,
                                        Цена_Покупки = a.PurchasePrice,
                                        Имя_Поставщика = p.Name,
                                        Принявший_Кладовщик = u.UserName,
                                        Сумма_Поставки = (decimal)(a.Quantity * a.PurchasePrice) // Вычисляем сумму для поступлений
                                    };
                            break;

                        case "Расходы":
                            query = from i in db.Issuance
                                    join c in db.Components on i.ComponentID equals c.ID
                                    join b in db.Buyers on i.BuyerID equals b.ID
                                    join u in db.Users on i.UserID equals u.ID
                                    where i.IssuanceDate != null && i.IssuanceDate.Value.Year == enteredYear && i.IssuanceDate.Value.Month == selectedMonth
                                    orderby i.IssuanceDate
                                    select new
                                    {
                                        Комплектующее = c.Name,
                                        Тип_Комплектующего = c.Type,
                                        Дата_Расхода = i.IssuanceDate,
                                        Количество = i.Quantity,
                                        Цена = c.Price,
                                        Имя_Покупателя = b.Name,
                                        Отпустивший_Кладовщик = u.UserName,
                                        Сумма_Расхода = (decimal)(i.Quantity * c.Price) // Вычисляем сумму для расходов
                                    };
                            break;

                        case "По поставщикам":
                            query = from a in db.Arrivals
                                    orderby a.Providers.Name
                                    select new
                                    {
                                        Имя_Поставщика = a.Providers.Name,
                                        Комплектующее = a.Components.Name,
                                        Количество = a.Quantity,
                                        Цена_Поставки = a.PurchasePrice,
                                        Дата_Поставки = a.ArrivalDate,
                                        Сумма_Поставки = (decimal)(a.Quantity * a.PurchasePrice)

                                    };
                            break; 

                        default:
                            MessageBox.Show("Выберите тип отчета: Поступления, Расходы или По поставщикам.");
                            return;
                    }

                    if (query != null)
                    {
                        var results = query.ToList();

                        if (results.Count == 0)
                        {
                            MessageBox.Show("Нет данных для отчета за выбранный период.");
                            return;
                        }

                        // 1. Создание столбцов DataTable
                        var firstResult = results.FirstOrDefault();
                        if (firstResult != null)
                        {
                            foreach (var property in firstResult.GetType().GetProperties())
                            {
                                Type columnType = property.PropertyType;
                                if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    columnType = Nullable.GetUnderlyingType(columnType) ?? property.PropertyType;
                                }
                                DataColumn column = new DataColumn(property.Name, columnType);
                                column.AllowDBNull = true;
                                dataTable.Columns.Add(column);
                            }

                            // 2. Заполнение строк данными
                            foreach (var item in results)
                            {
                                DataRow row = dataTable.NewRow();
                                foreach (var property in item.GetType().GetProperties())
                                {
                                    object value = property.GetValue(item, null);
                                    row[property.Name] = value ?? DBNull.Value;
                                }
                                dataTable.Rows.Add(row);
                            }
                        }
                    }
                }

                // 3. Создание Excel файла и запись данных
                Application.Current.Dispatcher.Invoke(() => // Убеждаемся, что работаем в UI потоке
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = $"Отчет_{reportType}_{enteredYear}_{selectedMonth}.xlsx";
                    saveFileDialog.DefaultExt = ".xlsx";
                    saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        FileInfo fileInfo = new FileInfo(saveFileDialog.FileName);

                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add(reportType);

                            // Запись заголовков
                            for (int i = 0; i < dataTable.Columns.Count; i++)
                            {
                                worksheet.Cell(1, i + 1).Value = dataTable.Columns[i].ColumnName;
                                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                            }

                            // Запись данных с приведением типов
                            for (int row = 0; row < dataTable.Rows.Count; row++)
                            {
                                for (int col = 0; col < dataTable.Columns.Count; col++)
                                {
                                    object cellValue = dataTable.Rows[row][col];

                                    if (cellValue == DBNull.Value || cellValue == null)
                                    {
                                        worksheet.Cell(row + 2, col + 1).Value = "";
                                    }
                                    else if (cellValue is DateTime)
                                    {
                                        worksheet.Cell(row + 2, col + 1).Value = (DateTime)cellValue;
                                        worksheet.Cell(row + 2, col + 1).Style.NumberFormat.Format = "yyyy-MM-dd";
                                    }
                                    else if (cellValue is decimal)
                                    {
                                        worksheet.Cell(row + 2, col + 1).Value = (decimal)cellValue;
                                    }
                                    else if (cellValue is int)
                                    {
                                        worksheet.Cell(row + 2, col + 1).Value = (int)cellValue;
                                    }
                                    else
                                    {
                                        worksheet.Cell(row + 2, col + 1).Value = cellValue.ToString();
                                    }
                                }
                            }

                            worksheet.Columns().AdjustToContents();
                            workbook.SaveAs(saveFileDialog.FileName);
                        }
                        MessageBox.Show($"Отчет успешно создан: {saveFileDialog.FileName}");
                        ReportProgressBar.Visibility = Visibility.Hidden;
                        btnGenerateReport.IsEnabled = true;
                        StatusTextBlock.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}");
            }
            
        }
    }
}

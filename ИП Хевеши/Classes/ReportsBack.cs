using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ИП_Хевеши.Data;


namespace ИП_Хевеши.Classes
{
    public class ReportsBack
    {
        public void GenerateExcelReport(int selectedMonth, int enteredYear, string reportType)
        {

            DataTable dataTable = new DataTable(); // Создаем DataTable для передачи в Excel

            try
            {
                using (var db = new ИП_ХевешиEntities()) // Замените YourDbContext
                {
                    // LINQ запрос
                    IQueryable<object> query = null; // IQueryable позволяет динамически строить запрос

                    switch (reportType)
                    {
                        case "Поступления":
                            query = from a in db.Arrivals
                                    join c in db.Components on a.ComponentID equals c.ID
                                    join p in db.Providers on a.ProviderID equals p.ID
                                    join u in db.Users on a.UserID equals u.ID
                                    where a.ArrivalDate != null && a.ArrivalDate. Year == enteredYear && a.ArrivalDate.Month == selectedMonth
                                    orderby a.ArrivalDate
                                    select new
                                    {
                                        ComponentName = c.Name,
                                        ComponentType = c.Type,
                                        ArrivalDate = a.ArrivalDate,
                                        Quantity = a.Quantity,
                                        PurchasePrice = a.PurchasePrice,
                                        ProviderName = p.Name,
                                        UserName = u.UserName
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
                                        ComponentName = c.Name,
                                        ComponentType = c.Type,
                                        IssuanceDate = i.IssuanceDate,
                                        Quantity = i.Quantity,
                                        Price = c.Price, // Цена продажи
                                        BuyerName = b.Name,
                                        UserName = u.UserName
                                    };
                            break;

                        default:
                            MessageBox.Show("Выберите тип отчета: Поступления или Расходы.");
                            return;
                    }

                    // Преобразуем результаты LINQ запроса в DataTable
                    if (query != null) // Проверка, что запрос был сформирован
                    {
                        var results = query.ToList();

                        if (results.Count == 0)
                        {
                            MessageBox.Show("Нет данных для отчета за выбранный период.");
                            return;
                        }

                        // Создаем DataTable dynamically на основе анонимного типа
                        var firstResult = results.FirstOrDefault();
                        if (firstResult != null)
                        {
                            foreach (var property in firstResult.GetType().GetProperties())
                            {
                                dataTable.Columns.Add(property.Name, property.PropertyType);
                            };

                            foreach (var item in results)
                            {
                                DataRow row = dataTable.NewRow();
                                foreach (var property in item.GetType().GetProperties())
                                {
                                    row[property.Name] = property.GetValue(item, null) ?? DBNull.Value; // Handle null values
                                }
                                dataTable.Rows.Add(row);
                            }
                        }
                    }
                } 

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = $"Отчет_{reportType}_{enteredYear}_{selectedMonth}.xlsx";
                saveFileDialog.DefaultExt = ".xlsx";
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    FileInfo fileInfo = new FileInfo(saveFileDialog.FileName);
                    using (ExcelPackage package = new ExcelPackage(fileInfo))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(reportType);

                        // Запись заголовков
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = dataTable.Columns[i].ColumnName;
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        }

                        // Запись данных
                        for (int row = 0; row < dataTable.Rows.Count; row++)
                        {
                            for (int col = 0; col < dataTable.Columns.Count; col++)
                            {
                                worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col];
                            }
                        }

                        worksheet.Cells.AutoFitColumns();
                        package.Save();
                    }

                    MessageBox.Show($"Отчет успешно создан: {saveFileDialog.FileName}");
                }
            }
                catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}");
            }
        }
    }
}

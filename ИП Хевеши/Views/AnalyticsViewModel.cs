

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ИП_Хевеши.Data;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Data.Entity;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace ИП_Хевеши.Views
{
    public class AnalyticsViewModel
    {
        public ObservableCollection<DeadProductEntry> DeadProducts { get; set; }

        public ObservableCollection<TopProductEntry> TopProducts { get; set; }
        public PlotModel RevenueForecastModel { get; set; }

        public PlotModel RevenuePlotModel { get; private set; }
        public PlotModel ForecastPlotModel { get; private set; }

        public AnalyticsViewModel()
        {
            LoadRevenueChart();
            LoadTopProducts();
            LoadDeadProducts();
            LoadRevenueChart();
            LoadRevenueForecast();
        }
        private void LoadRevenueForecast()
        {
            RevenueForecastModel = new PlotModel { Title = "Прогноз выручки" };

            // Оси
            RevenueForecastModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MMM yyyy",
                Title = "Месяц",
                IntervalType = DateTimeIntervalType.Months,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            RevenueForecastModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Выручка",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            List<RevenueDataPoint> revenueByMonth;

            using (var db = new ИП_ХевешиEntities())
            {
                var raw = db.Issuance
                    .Where(i => i.IssuanceDate != null && i.Components != null)
                    .ToList();

                revenueByMonth = raw
                    .GroupBy(i => new { i.IssuanceDate.Value.Year, i.IssuanceDate.Value.Month })
                    .Select(g => new RevenueDataPoint
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Revenue = (decimal)g.Sum(x => x.Quantity * x.Components.Price)
                    })
                    .OrderBy(dp => dp.Date)
                    .ToList();
            }

            // Серия точек
            var series = new LineSeries
            {
                Title = "Факт + Прогноз",
                MarkerType = MarkerType.Circle,
                Color = OxyColors.Green
            };

            // Добавляем факт
            foreach (var item in revenueByMonth)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), (double)item.Revenue));
            }

            // Прогноз
            if (revenueByMonth.Count > 1)
            {
                var x = Enumerable.Range(0, revenueByMonth.Count).Select(i => (double)i).ToArray();
                var y = revenueByMonth.Select(p => (double)p.Revenue).ToArray();

                double avgX = x.Average();
                double avgY = y.Average();

                double k = x.Zip(y, (xi, yi) => (xi - avgX) * (yi - avgY)).Sum()
                            / x.Sum(xi => Math.Pow(xi - avgX, 2));

                double b = avgY - k * avgX;

                var nextMonth = revenueByMonth.Last().Date.AddMonths(1);
                double forecastY = k * x.Length + b;

                // Точка прогноза
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(nextMonth), Math.Round(forecastY, 2)));
            }

            RevenueForecastModel.Series.Add(series);
        }

        private void LoadDeadProducts()
        {
            using (var db = new ИП_ХевешиEntities())
            {
                var cutoffDate = DateTime.Now.AddDays(-60);

                // Последние даты продаж по каждому компоненту
                var lastSales = db.Issuance
                    .Where(i => i.IssuanceDate != null)
                    .GroupBy(i => i.ComponentID)
                    .Select(g => new
                    {
                        ComponentID = g.Key,
                        LastSoldDate = g.Max(i => i.IssuanceDate)
                    })
                    .ToList();

                // Выбираем только тех, у кого дата последней продажи > 60 дней назад
                var dead = lastSales
                    .Where(x => x.LastSoldDate < cutoffDate)
                    .Join(db.Components, x => x.ComponentID, c => c.ID, (x, c) => new DeadProductEntry
                    {
                        Name = c.Name,
                        LastSoldDate = x.LastSoldDate
                    })
                    .OrderBy(x => x.LastSoldDate)
                    .Take(5) // топ-5
                    .ToList();

                DeadProducts = new ObservableCollection<DeadProductEntry>(dead);
            }
        }
        private void LoadRevenueChart()
        {
            RevenuePlotModel = new PlotModel { Title = "Выручка по месяцам" };

            // Оси
            RevenuePlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MMM yyyy",
                Title = "Месяц",
                IntervalType = DateTimeIntervalType.Months,
                MinorIntervalType = DateTimeIntervalType.Months,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            RevenuePlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Выручка",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            // Данные
            List<RevenueDataPoint> monthlyRevenue;
            using (var db = new ИП_ХевешиEntities())
            {
                var grouped = db.Issuance
                    .Where(i => i.IssuanceDate != null)
                    .ToList() // переводим в память
                    .GroupBy(i => new { i.IssuanceDate.Value.Year, i.IssuanceDate.Value.Month })
                    .Select(g => new RevenueDataPoint
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Revenue = g.Sum(x => (decimal)(x.Quantity * x.Components.Price))
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                monthlyRevenue = grouped;
            }

            // Добавляем точки на график
            var lineSeries = new LineSeries
            {
                Title = "Выручка",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.Red,
                MarkerFill = OxyColors.LightPink
            };

            foreach (var item in monthlyRevenue)
            {
                lineSeries.Points.Add(DateTimeAxis.CreateDataPoint(item.Date, (double)item.Revenue));
            }

            RevenuePlotModel.Series.Add(lineSeries);
        }
        private void LoadTopProducts()
        {
            using (var db = new ИП_ХевешиEntities())
            {
                var top = db.Issuance
                    .Where(i => i.ComponentID != null)
                    .GroupBy(i => i.Components.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Total = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(5)
                    .ToList();

                TopProducts = new ObservableCollection<TopProductEntry>(
                    top.Select(x => new TopProductEntry
                    {
                        Name = x.Name,
                        Quantity = x.Total ?? 0
                    })
                );
            }
        }
        public void ExportAnalyticsToExcel()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Аналитика - {DateTime.Today}.xlsx",
                    DefaultExt = ".xlsx",
                    Filter = "Excel файлы (*.xlsx)|*.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        // 1. Выручка по месяцам
                        var revenueSheet = workbook.Worksheets.Add("Выручка");
                        revenueSheet.Cell(1, 1).Value = "Месяц";
                        revenueSheet.Cell(1, 2).Value = "Выручка";

                        var lineSeries = RevenueForecastModel.Series.OfType<LineSeries>().FirstOrDefault();
                        var points = lineSeries?.Points ?? new List<DataPoint>();
                        int row = 2;
                        foreach (var point in points)
                        {
                            var date = DateTimeAxis.ToDateTime(point.X);
                            var revenue = point.Y;
                            revenueSheet.Cell(row, 1).Value = date.ToString("MMMM yyyy");
                            revenueSheet.Cell(row, 2).Value = revenue;
                            row++;
                        }

                        // 2. Топ-5 товаров
                        var topSheet = workbook.Worksheets.Add("Топ товары");
                        topSheet.Cell(1, 1).Value = "Наименование";
                        topSheet.Cell(1, 2).Value = "Продано (шт)";
                        for (int i = 0; i < TopProducts.Count; i++)
                        {
                            topSheet.Cell(i + 2, 1).Value = TopProducts[i].Name;
                            topSheet.Cell(i + 2, 2).Value = TopProducts[i].Quantity;
                        }

                        // 3. Неликвиды
                        var deadSheet = workbook.Worksheets.Add("Неликвиды");
                        deadSheet.Cell(1, 1).Value = "Наименование";
                        deadSheet.Cell(1, 2).Value = "Дата последней продажи";
                        for (int i = 0; i < DeadProducts.Count; i++)
                        {
                            deadSheet.Cell(i + 2, 1).Value = DeadProducts[i].Name;
                            deadSheet.Cell(i + 2, 2).Value = DeadProducts[i].LastSoldDate;
                        }

                        // 4. Прогноз
                        var forecastSheet = workbook.Worksheets.Add("Прогноз");
                        forecastSheet.Cell(1, 1).Value = "Месяц";
                        forecastSheet.Cell(1, 2).Value = "Выручка";
                        var forecastSeries = RevenueForecastModel.Series.OfType<LineSeries>().FirstOrDefault();
                        var forecastPoints = forecastSeries?.Points ?? new List<DataPoint>();
                        for (int i = 0; i < forecastPoints.Count; i++)
                        {
                            var point = forecastPoints[i];
                            forecastSheet.Cell(i + 2, 1).Value = DateTimeAxis.ToDateTime(point.X).ToString("MMMM yyyy");
                            forecastSheet.Cell(i + 2, 2).Value = point.Y;
                        }

                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show("Отчет успешно сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public class TopProductEntry
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public string Display => $"шт";
        }
        private class RevenueDataPoint
        {
            public DateTime Date { get; set; }
            public decimal Revenue { get; set; }

        }
        public class DeadProductEntry
        {
            public string Name { get; set; }
            public DateTime? LastSoldDate { get; set; }
        }
    }
}
    

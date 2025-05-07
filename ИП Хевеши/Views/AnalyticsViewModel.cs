using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ИП_Хевеши.Data;

namespace ИП_Хевеши.Views
{
    public class DataPoint
    {
        public string Category { get; set; }
        public decimal Value { get; set; }
    }

    public class AnalyticsViewModel : INotifyPropertyChanged
    {
        // 1. Свойства для привязки к UI
        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; OnPropertyChanged(nameof(StartDate)); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
        }

        private string _selectedComponentName;
        public string SelectedComponentName
        {
            get { return _selectedComponentName; }
            set { _selectedComponentName = value; OnPropertyChanged(nameof(SelectedComponentName)); }
        }

        private List<string> _componentNames;
        public List<string> ComponentNames
        {
            get { return _componentNames; }
            set { _componentNames = value; OnPropertyChanged(nameof(ComponentNames)); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        private bool _hasError;
        public bool HasError
        {
            get { return _hasError; }
            set { _hasError = value; OnPropertyChanged(nameof(HasError)); }
        }

        // Данные для графика поступлений и расходов
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        // Данные для круговой диаграммы
        public SeriesCollection PieChartSeries { get; set; }

        // 2. Конструктор
        public AnalyticsViewModel()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            ComponentNames = new List<string>();
            SeriesCollection = new SeriesCollection();
            Labels = new List<string>();
            YFormatter = value => value.ToString("C");
            PieChartSeries = new SeriesCollection();

            Task.Run(() => LoadComponentNames());
        }

        // 3. Методы для загрузки данных и применения фильтров
        public  void LoadComponentNames()
        {
            try
            {
                using (var db = new ИП_ХевешиEntities())
                {
                    ComponentNames = db.Components.Select(c => c.Name).Distinct().ToList();
                    OnPropertyChanged(nameof(ComponentNames)); //  Уведомляем об изменении
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => // Error Display on UI Thread
                {
                    ErrorMessage = $"Ошибка при загрузке названий комплектующих: {ex.Message}";
                    HasError = true;
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });

            }
        }

        public async void ApplyFilters()
        {
            try
            {
                ErrorMessage = null;
                HasError = false;

                List<DataPoint> stockByComponentType = null;
                List<IncomingOutgoingData> chartData = null;

                using (var db = new ИП_ХевешиEntities())
                {
                    // Фильтруем поступления и расходы
                    var incoming = db.Arrivals.Where(a => a.ArrivalDate >= StartDate && a.ArrivalDate <= EndDate);
                    var outgoing = db.Issuance.Where(i => i.IssuanceDate >= StartDate && i.IssuanceDate <= EndDate);

                    if (!string.IsNullOrEmpty(SelectedComponentName))
                    {
                        incoming = incoming.Where(a => a.Components.Name == SelectedComponentName);
                        outgoing = outgoing.Where(i => i.Components.Name == SelectedComponentName);
                    }

                    // Анализ поступлений и расходов по месяцам
                    var incomingGrouped = incoming
                        .GroupBy(a => a.ArrivalDate.Month)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Month = g.Key,
                            Total = g.Sum(a => a.Quantity * a.PurchasePrice)
                        }).ToList();

                    var outgoingGrouped = outgoing
                        .GroupBy(i => i.IssuanceDate.Value.Month)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Month = g.Key,
                            Total = g.Sum(i => i.Quantity * i.Components.Price)
                        }).ToList();

                    chartData = new List<IncomingOutgoingData>();
                    if (StartDate.HasValue && EndDate.HasValue)
                    {
                        for (int i = StartDate.Value.Month; i <= EndDate.Value.Month; i++)
                        {
                            string monthName = DateTimeFormatInfo.CurrentInfo.GetMonthName(i);
                            decimal incomingTotal = incomingGrouped.Where(x => x.Month == i).Sum(x => x.Total ?? 0);
                            decimal outgoingTotal = outgoingGrouped.Where(x => x.Month == i).Sum(x => x.Total ?? 0);

                            chartData.Add(new IncomingOutgoingData { Month = monthName, Incoming = incomingTotal, Outgoing = outgoingTotal });
                        }
                    }
                    if (incomingGrouped.Count == 0 && outgoingGrouped.Count == 0)
                    {
                        Application.Current.Dispatcher.Invoke(() => // Error Display on UI Thread
                        {

                            ErrorMessage = "Нет данных для выбранного периода.";
                            HasError = true;
                            Labels.Clear();
                            SeriesCollection.Clear();
                            PieChartSeries.Clear();
                        });
                        return;
                    }

                    stockByComponentType = db.Components
                  .GroupBy(c => c.Type) //  <-  Используйте поле, которое содержит тип
                  .Select(g => new DataPoint
                  {
                      Category = g.Key,
                      Value = g.Sum(c => c.Quantity * c.Price ?? 0)
                  })
                  .ToList();

                }
                               
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Labels.Clear();
                    SeriesCollection.Clear();
                    PieChartSeries.Clear();

                  
                    var incomingSeries = new LineSeries
                    {
                        Title = "Поступления",
                        Values = new ChartValues<decimal>(),
                        LineSmoothness = 0
                    };

                    var outgoingSeries = new LineSeries
                    {
                        Title = "Расходы",
                        Values = new ChartValues<decimal>(),
                        LineSmoothness = 0
                    };

                    if (chartData != null) 
                    {
                        foreach (var item in chartData)
                        {
                            Labels.Add(item.Month);
                            incomingSeries.Values.Add(item.Incoming);
                            outgoingSeries.Values.Add(item.Outgoing);
                        }
                    }


                    SeriesCollection.Add(incomingSeries);
                    SeriesCollection.Add(outgoingSeries);
                    if (stockByComponentType != null) // Check if component type data exists
                    {
                        foreach (var dataPoint in stockByComponentType)
                        {
                            PieChartSeries.Add(new PieSeries
                            {
                                Title = dataPoint.Category, // Тип комплектующего
                                Values = new ChartValues<decimal> { dataPoint.Value },
                                DataLabels = true, // Отображать подписи
                                LabelPoint = point => $"{dataPoint.Category} ({point.Y:C0})" // Формат подписи
                            });
                        }
                    }

                    OnPropertyChanged(nameof(Labels)); // Update Labels for Chart
                    OnPropertyChanged(nameof(SeriesCollection)); // Update Chart
                    OnPropertyChanged(nameof(PieChartSeries)); // Update Pie Chart
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => // Display Exception on UI Thread
                {
                    ErrorMessage = $"Ошибка при применении фильтров: {ex.Message}";
                    HasError = true;
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });

            }
        }

        public void ResetFilters()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            SelectedComponentName = null;
            ApplyFilters();
        }

        // 4. INotifyPropertyChanged реализация
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class IncomingOutgoingData
    {
        public string Month { get; set; }
        public decimal Incoming { get; set; }
        public decimal Outgoing { get; set; }
    }
}

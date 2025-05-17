using LiveCharts.Wpf;
using LiveCharts;
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

namespace ИП_Хевеши.Views
{
    public class DataPoint
    {
        public string Category { get; set; }
        public decimal Value { get; set; }
    }

    public class ProviderComponentData
    {
        public string ProviderName { get; set; }
        public string ComponentName { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime ArrivalDate { get; set; }
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
        private ObservableCollection<ProviderComponentData> _providerComponentData;
        public ObservableCollection<ProviderComponentData> ProviderComponentData
        {
            get => _providerComponentData;
            set { _providerComponentData = value; OnPropertyChanged(nameof(ProviderComponentData)); }
        }
     

        // Для принудительного обновления (добавьте метод)
        public void RefreshCharts()
        {
            OnPropertyChanged(nameof(ProvidersChartSeries));
            OnPropertyChanged(nameof(ProvidersLabels));
        }
        // Данные для графиков
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; } = value => value.ToString("N0");
        public SeriesCollection PieChartSeries { get; set; }
        public SeriesCollection ProvidersChartSeries { get;  } = new SeriesCollection();
        public List<string> ProvidersLabels { get;  } = new List<string>();

        // 2. Конструктор
        public AnalyticsViewModel()
        {
            InitializeProperties();
            Task.Run(LoadComponentNames);

            ProvidersLabels = new List<string>();
 
        }

        private void InitializeProperties()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            ComponentNames = new List<string>();
            ProviderComponentData = new ObservableCollection<ProviderComponentData>();

            SeriesCollection = new SeriesCollection();
            Labels = new List<string>();
           

            PieChartSeries = new SeriesCollection();

          
        }

        // 3. Методы для загрузки данных
        public async Task LoadComponentNames()
        {
            await SafeExecuteAsync(async () =>
            {
                using (var db = new ИП_ХевешиEntities())
                {
                    ComponentNames = await Task.Run(() =>
                        db.Components.Select(c => c.Name).Distinct().ToList());
                }
            }, "Ошибка при загрузке названий комплектующих");
        }

        public async void ApplyFilters()
        {
            try
            {
                ErrorMessage = null;
                HasError = false;

                using (var db = new ИП_ХевешиEntities()) // Замените YourDbContext на ваш контекст
                {
                    // 1. Фильтрация данных
                    var incoming = db.Arrivals
                        .Where(a => a.ArrivalDate >= StartDate && a.ArrivalDate <= EndDate);

                    var outgoing = db.Issuance
                        .Where(i => i.IssuanceDate >= StartDate && i.IssuanceDate <= EndDate);

                    if (!string.IsNullOrEmpty(SelectedComponentName))
                    {
                        incoming = incoming.Where(a => a.Components.Name == SelectedComponentName);
                        outgoing = outgoing.Where(i => i.Components.Name == SelectedComponentName);
                    }

                    // 2. Получаем данные для всех диаграмм
                    var (chartData, pieData, providerData) = await Task.Run(() =>
                    {
                        // Данные для первой диаграммы (поступления/расходы)
                        var incomingGrouped = incoming
                            .GroupBy(a => a.ArrivalDate.Month)
                            .Select(g => new
                            {
                                Month = g.Key,
                                Total = g.Sum(a => a.Quantity * a.PurchasePrice ?? 0)
                            })
                            .ToList();

                        var outgoingGrouped = outgoing
                            .GroupBy(i => i.IssuanceDate.Value.Month)
                            .Select(g => new
                            {
                                Month = g.Key,
                                Total = g.Sum(i => i.Quantity * i.Components.Price ?? 0)
                            })
                            .ToList();

                        var chartResult = new List<IncomingOutgoingData>();
                        if (StartDate.HasValue && EndDate.HasValue)
                        {
                            for (int i = StartDate.Value.Month; i <= EndDate.Value.Month; i++)
                            {
                                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);
                                decimal incomingTotal = incomingGrouped.Where(x => x.Month == i).Sum(x => x.Total);
                                decimal outgoingTotal = outgoingGrouped.Where(x => x.Month == i).Sum(x => x.Total);

                                chartResult.Add(new IncomingOutgoingData
                                {
                                    Month = monthName,
                                    Incoming = incomingTotal,
                                    Outgoing = outgoingTotal
                                });
                            }
                        }

                        // Данные для второй диаграммы (круговая по типам компонентов)
                        var pieResult = db.Components
                            .GroupBy(c => c.Type)
                            .Select(g => new DataPoint
                            {
                                Category = g.Key,
                                Value = g.Sum(c => c.Quantity * c.Price ?? 0)
                            })
                            .ToList();

                        // Данные для третьей диаграммы (поставщики - количество)
                        var providerResult = incoming
                            .GroupBy(a => a.Providers.Name) // Убедитесь, что имя свойства верное
                            .Select(g => new DataPoint
                            {
                                Category = g.Key,
                                Value = (decimal)g.Sum(s => s.Quantity) // Только количество!
                            })
                            .ToList();

                        return (chartResult, pieResult, providerResult);
                    });

                    if (chartData == null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "Нет данных для выбранного периода.";
                            HasError = true;
                            ClearAllCharts();
                        });
                        return;
                    }

                    // 3. Обновляем UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Очищаем предыдущие данные
                        Labels.Clear();
                        SeriesCollection.Clear();
                        PieChartSeries.Clear();
                        ProvidersChartSeries.Clear();
                        ProvidersLabels.Clear();

                        // Проверка на пустые данные
                        if (!chartData.Any() && !pieData.Any() && !providerData.Any())
                        {
                            ErrorMessage = "Нет данных для выбранного периода.";
                            HasError = true;
                            return;
                        }

                        // 1. Первая диаграмма (поступления/расходы)
                        if (chartData.Any())
                        {
                            var incomingSeries = new LineSeries
                            {
                                Title = "Поступления",
                                Values = new ChartValues<decimal>(chartData.Select(d => d.Incoming)),
                                LineSmoothness = 0
                            };

                            var outgoingSeries = new LineSeries
                            {
                                Title = "Расходы",
                                Values = new ChartValues<decimal>(chartData.Select(d => d.Outgoing)),
                                LineSmoothness = 0
                            };

                            Labels.AddRange(chartData.Select(d => d.Month));
                            SeriesCollection.Add(incomingSeries);
                            SeriesCollection.Add(outgoingSeries);
                        }

                        // 2. Вторая диаграмма (круговая по типам компонентов)
                        if (pieData.Any())
                        {
                            foreach (var item in pieData)
                            {
                                PieChartSeries.Add(new PieSeries
                                {
                                    Title = item.Category,
                                    Values = new ChartValues<decimal> { item.Value },
                                    DataLabels = true,
                                    LabelPoint = point => $"{item.Category}: {point.Y:C0}"
                                });
                            }
                        }

                        // 3. Третья диаграмма (поставщики - количество)
                        if (providerData.Any())
                        {
                            ProvidersChartSeries.Clear();
                            ProvidersLabels.Clear();

                            var providerComponentDataList = (from a in db.Arrivals
                                                             select new ProviderComponentData
                                                             {
                                                                 ProviderName = a.Providers.Name,
                                                                 ComponentName = a.Components.Name,
                                                                 Quantity = (int)a.Quantity,
                                                                 PurchasePrice = (decimal)a.PurchasePrice,
                                                                 ArrivalDate = a.ArrivalDate
                                                             }).ToList();
                            
                            var diagramProvierData = providerComponentDataList
                                .GroupBy(s => s.ProviderName)
                                .Select(g => new DataPoint
                                {
                                    Category = g.Key,
                                    Value = g.Sum(s => s.Quantity )
                                })
                                .ToList();

                            
                            if (providerData.Any() == true)
                            {
                                
                                var providerSeries = new ColumnSeries
                                {
                                    Title = "Поставщики",
                                    Values = new ChartValues<decimal>(diagramProvierData.Select(p => p.Value)),
                                    DataLabels = true
                                };

                                
                                ProvidersChartSeries.Add(providerSeries);
                                ProvidersLabels.AddRange(diagramProvierData.Select(p => p.Category));

                                ProvidersChartSeries.Add(providerSeries);
                                ProvidersLabels.AddRange(providerData.Select(p => p.Category));

                                // Принудительное обновление
                                OnPropertyChanged(nameof(ProvidersChartSeries));
                                OnPropertyChanged(nameof(ProvidersLabels));

                            }
                            if (providerData == null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ErrorMessage = "Нет данных о поставщиках за выбранный период.";
                                    HasError = true;
                                    ClearAllCharts();
                                });
                                return;
                            }

                            // Уведомляем об изменениях
                            OnPropertyChanged(nameof(SeriesCollection));
                            OnPropertyChanged(nameof(Labels));
                            OnPropertyChanged(nameof(PieChartSeries));
                            OnPropertyChanged(nameof(ProvidersChartSeries));
                            OnPropertyChanged(nameof(ProvidersLabels));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Ошибка при применении фильтров: {ex.Message}";
                    HasError = true;
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        private void ClearAllCharts()
        {
            SeriesCollection.Clear();
            Labels.Clear();
            PieChartSeries.Clear();
            ProvidersChartSeries.Clear();
            ProvidersLabels.Clear();
        }

        private List<IncomingOutgoingData> GetIncomingOutgoingData(
            IQueryable<Arrivals> incoming,
            IQueryable<Issuance> outgoing)
        {
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

            var result = new List<IncomingOutgoingData>();
            if (StartDate.HasValue && EndDate.HasValue)
            {
                for (int i = StartDate.Value.Month; i <= EndDate.Value.Month; i++)
                {
                    string monthName = DateTimeFormatInfo.CurrentInfo.GetMonthName(i);
                    decimal incomingTotal = incomingGrouped.Where(x => x.Month == i).Sum(x => x.Total ?? 0);
                    decimal outgoingTotal = outgoingGrouped.Where(x => x.Month == i).Sum(x => x.Total ?? 0);

                    result.Add(new IncomingOutgoingData
                    {
                        Month = monthName,
                        Incoming = incomingTotal,
                        Outgoing = outgoingTotal
                    });
                }
            }
            return result;
        }

        private void UpdateChartsAndGrid(
            List<IncomingOutgoingData> chartData,
            List<DataPoint> stockByComponentType,
            List<DataPoint> supplierData,
            List<ProviderComponentData> supplierComponentData)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Очистка старых данных
                Labels.Clear();
                SeriesCollection.Clear();
                PieChartSeries.Clear();
                ProvidersLabels.Clear();
                ProvidersChartSeries.Clear();
                ProviderComponentData.Clear();

                // 1. Обновление DataGrid
                if (supplierComponentData != null)
                {
                    foreach (var item in supplierComponentData)
                    {
                        ProviderComponentData.Add(item);
                    }
                }

                // 2. Обновление первой диаграммы (поступления/расходы)
                if (chartData != null && chartData.Any())
                {
                    var incomingSeries = new LineSeries
                    {
                        Title = "Поступления",
                        Values = new ChartValues<decimal>(chartData.Select(d => d.Incoming)),
                        LineSmoothness = 0
                    };

                    var outgoingSeries = new LineSeries
                    {
                        Title = "Расходы",
                        Values = new ChartValues<decimal>(chartData.Select(d => d.Outgoing)),
                        LineSmoothness = 0
                    };

                    Labels.AddRange(chartData.Select(d => d.Month));
                    SeriesCollection.Add(incomingSeries);
                    SeriesCollection.Add(outgoingSeries);
                }

                // 3. Обновление круговой диаграммы (по типам)
                if (stockByComponentType != null && stockByComponentType.Any())
                {
                    foreach (var dataPoint in stockByComponentType)
                    {
                        PieChartSeries.Add(new PieSeries
                        {
                            Title = dataPoint.Category,
                            Values = new ChartValues<decimal> { dataPoint.Value },
                            DataLabels = true,
                            LabelPoint = point => $"{dataPoint.Category} ({point.Y:C0})"
                        });
                    }
                }

                // 4. Обновление третьей диаграммы (по поставщикам)
                if (supplierData != null && supplierData.Any())
                {
                    var columnSeries = new ColumnSeries
                    {
                        Title = "Сумма закупок",
                        Values = new ChartValues<decimal>(supplierData.Select(d => d.Value)),
                        DataLabels = true
                    };

                    ProvidersLabels.AddRange(supplierData.Select(d => d.Category));
                    ProvidersChartSeries.Add(columnSeries);
                }

                // Уведомление об изменениях
                OnPropertyChanged(nameof(Labels));
                OnPropertyChanged(nameof(SeriesCollection));
                OnPropertyChanged(nameof(PieChartSeries));
                OnPropertyChanged(nameof(ProvidersLabels));
                OnPropertyChanged(nameof(ProvidersChartSeries));
                OnPropertyChanged(nameof(ProviderComponentData));
            });
        }

        public void ResetFilters()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            SelectedComponentName = null;
            ClearAllCharts();
        }

        // 4. Вспомогательные методы
        private async Task SafeExecuteAsync(Func<Task> action, string errorPrefix)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"{errorPrefix}: {ex.Message}";
                    HasError = true;
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        // 5. INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class IncomingOutgoingData
    {
        public string Month { get; set; }
        public decimal Incoming { get; set; }
        public decimal Outgoing { get; set; }
        public int MonthNumber { get; internal set; }
    }
}

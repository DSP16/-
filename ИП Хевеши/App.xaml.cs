using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ИП_Хевеши
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            ClearCache();
        }
        private void ClearCache()
        {
            string cachePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");

            if (Directory.Exists(cachePath))
            {
                try
                {
                    Directory.Delete(cachePath, true); // Удалить полностью
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при очистке кэша: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // можно заново создать, если нужно
            Directory.CreateDirectory(cachePath);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace ИП_Хевеши.Classes
{
    internal class CaptchaGenerate
    {
        public string GenerateCaptcha(out Bitmap bitmap)
        {
            string captchaText = GenerateRandomText(6);
            bitmap = new Bitmap(150, 50);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.White);
                using (Font font = new Font("Arial", 16, System.Drawing.FontStyle.Bold))
                {
                    // Измеряем размер текста
                    SizeF textSize = g.MeasureString(captchaText, font);

                    // Вычисляем координаты для центровки текста
                    float x = (bitmap.Width - textSize.Width) / 2;
                    float y = (bitmap.Height - textSize.Height) / 2;

                    g.DrawString(captchaText, font, System.Drawing.Brushes.Black, new PointF(x, y));

                    // Добавление шума
                    Random rand = new Random();
                    for (int i = 0; i < 100; i++)
                    {
                        int xNoise = rand.Next(bitmap.Width);
                        int yNoise = rand.Next(bitmap.Height);
                        bitmap.SetPixel(xNoise, yNoise, System.Drawing.Color.Gray);
                    }
                }
                return captchaText;
            }
        }

        private string GenerateRandomText(int length)
        {
            const string chars = "abcdefghigkklmopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random rand = new Random();
            return new string(Enumerable.Range(1, length)
                .Select(_ => chars[rand.Next(chars.Length)]).ToArray());
        }
    }
}

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DowngradePicture
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private string _imagePath = null;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoadClicked(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            // Get the current window's HWND by passing in the Window object
            // https://github.com/microsoft/WindowsAppSDK/issues/1188
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            // Use file picker like normal!
            filePicker.FileTypeFilter.Add(".bmp");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".jpg");
            var file = filePicker.PickSingleFileAsync().GetAwaiter().GetResult();
            if (file == null)
                return;

            _imagePath = file.Path;
            txtPath.Text = _imagePath;
            string fqn = file.Path.Replace("\\", "/");
            originalImage.Source = new BitmapImage(new Uri("file:///" + fqn));
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            reduceButton.IsEnabled = true;
        }


        private void OnReduceClicked(object sender, RoutedEventArgs e)
        {
            int step = int.Parse(pixelSize.Text);
            Bitmap bitmap = new(_imagePath);

            DateTime start = DateTime.Now;
            Bitmap reduced = ReduceBitmap(bitmap, step);
            TimeSpan duration = DateTime.Now - start;
            txtDuration.Text = duration.ToString();

            string folder = Path.GetDirectoryName(_imagePath);
            string name = Path.GetFileNameWithoutExtension(_imagePath) + "-reduced";
            string ext = Path.GetExtension(_imagePath);
            string reducedPath = Path.Combine(folder, name) + ext;
            reduced.Save(reducedPath);
            txtReducedPath.Text = reducedPath;
            string fqn = reducedPath.Replace("\\", "/");
            reducedImage.Source = new BitmapImage(new Uri("file:///" + fqn));
        }

        private static Bitmap ReduceBitmap(Bitmap bitmap, int step)
        {
            for (int x = 0; x < bitmap.Width; x += step)
            {
                for (int y = 0; y < bitmap.Height; y += step)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;

                    for (int i = 0; i < step && x + i < bitmap.Width; i++)
                    {
                        for (int j = 0; j < step && y + j < bitmap.Height; j++)
                        {
                            Color c = bitmap.GetPixel(x + i, y + j);
                            r += c.R;
                            g += c.G;
                            b += c.B;
                        }
                    }
                    Color avg = Color.FromArgb(r / (step * step), g / (step * step), b / (step * step));
                    
                    for (int i = 0; i < step && x + i < bitmap.Width; i++)
                    {
                        for (int j = 0; j < step && y + j < bitmap.Height; j++)
                        {
                            bitmap.SetPixel(x + i, y + j, avg);
                        }
                    }
                }
            }

            return bitmap;
        }

        // Simple way to enforce numeric input
        private void PixelSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string txt = textBox.Text;
            string filtered = new(txt.Where(c => char.IsDigit(c)).ToArray());
            textBox.Text = filtered;
        }

        #region Low Level stuff


        #endregion Low Level stuff

    }
}

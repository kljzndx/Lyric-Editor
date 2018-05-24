using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Toolkit.Uwp;
using SimpleLyricsEditor.Events;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class BlurBackgroundImage : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(IRandomAccessStream), typeof(BlurBackgroundImage), new PropertyMetadata(null));

        public static readonly DependencyProperty ImageOpacityProperty = DependencyProperty.Register(
            nameof(ImageOpacity), typeof(double), typeof(BlurBackgroundImage), new PropertyMetadata(0.3D));

        public static readonly DependencyProperty MaskOpacityProperty = DependencyProperty.Register(
            nameof(MaskOpacity), typeof(double), typeof(BlurBackgroundImage), new PropertyMetadata(0.3D));

        public static readonly DependencyProperty BlurDegreeProperty = DependencyProperty.Register(
            "BlurDegree", typeof(double), typeof(BlurBackgroundImage), new PropertyMetadata(5D));

        private readonly CanvasDevice device = new CanvasDevice();

        public BlurBackgroundImage()
        {
            InitializeComponent();
        }

        public IRandomAccessStream Source
        {
            get => (IRandomAccessStream) GetValue(SourceProperty);
            set
            {
                SetValue(SourceProperty, value);

                FadeOut.Begin();
            } 
        }

        public double ImageOpacity
        {
            get => (double) GetValue(ImageOpacityProperty);
            set => SetValue(ImageOpacityProperty, value);
        }

        public double MaskOpacity
        {
            get => (double) GetValue(MaskOpacityProperty);
            set => SetValue(MaskOpacityProperty, value);
        }

        public double BlurDegree
        {
            get => (double) GetValue(BlurDegreeProperty);
            set => SetValue(BlurDegreeProperty, value);
        }

        private async Task<Color> GetDominantColor(IRandomAccessStream bitmap)
        {
            //色调的总和
            var sum_hue = 0d;
            //色差的阈值
            var threshold = 30;
            
            CanvasBitmap canvasBitmap = await CanvasBitmap.LoadAsync(device, bitmap);
            var colors = canvasBitmap.GetPixelColors();

            var avg_hue = sum_hue / (canvasBitmap.Size.Width * canvasBitmap.Size.Height);

            //色差大于阈值的颜色值
            var rgbs = new List<Color>();

            foreach (var color in colors)
            {
                var hue = color.ToHsv().H;
                //如果色差大于阈值，则加入列表
                if (Math.Abs(hue - avg_hue) > threshold)
                    rgbs.Add(color);
            }

            if (rgbs.Count == 0)
                return Colors.Black;

            int sumR = 0, sumG = 0, sumB = 0;
            foreach (var rgb in rgbs)
            {
                sumR += rgb.R;
                sumG += rgb.G;
                sumB += rgb.B;
            }

            return Color.FromArgb(255, (byte)(sumR / rgbs.Count), (byte)(sumG / rgbs.Count), (byte)(sumB / rgbs.Count));
        }

        private async void FadeOut_Completed(object sender, object e)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(Source);

            Image.Source = bitmap;
            Image_FadeIn.Begin();

            SolidColorBrush brush = new SolidColorBrush(await GetDominantColor(Source));
            Mask_Rectangle.Fill = brush;
            Mask_FadeIn.Begin();
        }
    }
}
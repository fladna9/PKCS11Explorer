using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PKCS11Explorer.Tools;

namespace PKCS11Explorer.Views
{
    public class LoadingBox : Window
    {
        public LoadingBox(string title, string description, string imageURI)
        {
            this.InitializeComponent();
            this.SetUI(title, description, imageURI);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SetUI(string title, string description, string imageURI)
        {
            TextBlock text = this.FindControl<TextBlock>("Text");
            Image image = this.FindControl<Image>("Image");
            Button button = this.FindControl<Button>("Button");
            this.Title = title;
            text.Text = description;
            image.Source = (Bitmap)BitmapValueConverter.Instance.Convert((object)imageURI, typeof(IBitmap), null, null);
            SizeToContent = SizeToContent.WidthAndHeight;
            Icon = new WindowIcon((Bitmap)BitmapValueConverter.Instance.Convert((object)imageURI, typeof(IBitmap), null, null));
            CanResize = false;

        }
    }
}

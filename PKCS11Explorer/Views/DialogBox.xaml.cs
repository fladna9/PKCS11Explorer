using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PKCS11Explorer.Tools;

namespace PKCS11Explorer.Views
{
    public class DialogBox : Window
    {
        public DialogBox(string title, string description, string imageURI, string buttonString)
        {
            this.InitializeComponent();
            this.SetUI(title, description, imageURI, buttonString);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SetUI(string title, string description, string imageURI, string buttonString)
        {
            TextBlock text = this.FindControl<TextBlock>("Text");
            Image image = this.FindControl<Image>("Image");
            Button button = this.FindControl<Button>("Button");
            this.Title = title;
            text.Text = description;
            image.Source = (Bitmap)BitmapValueConverter.Instance.Convert((object)imageURI, typeof(IBitmap), null, null);
            button.Content = buttonString;
            button.Click += Button_Click;
            SizeToContent = SizeToContent.WidthAndHeight;
            Icon = new WindowIcon((Bitmap)BitmapValueConverter.Instance.Convert((object)imageURI, typeof(IBitmap), null, null));
            CanResize = false;

        }

        private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

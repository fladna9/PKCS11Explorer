using Avalonia;
using Avalonia.Markup.Xaml;

namespace PKCS11Test
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

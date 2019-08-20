﻿using Avalonia;
using Avalonia.Markup.Xaml;

namespace PKCS11Explorer
{
    public class App : Application
    {
        public string Version
        {
            get
            {
                return "0.3";
            }
        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

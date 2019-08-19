using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Threading.Tasks;

namespace PKCS11Test
{
    public class MainWindow : Window
    {
        TextBlock Label;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Label = this.FindControl<TextBlock>("Label");
        }

        private async void ButtonHandler_LoadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AllowMultiple = false;
            List<String> allowedExtensions = new List<String>();
            allowedExtensions.Add("so");
            allowedExtensions.Add("dll");
            allowedExtensions.Add("dylib");
            FileDialogFilter filter = new FileDialogFilter();
            filter.Extensions = allowedExtensions;
            filter.Name = "PKCS11 Middleware";
            List<FileDialogFilter> filters = new List<FileDialogFilter>();
            filters.Add(filter);
            openFileDialog.Filters = new List<FileDialogFilter>(filters);
            string[] fileSelected = await openFileDialog.ShowAsync(this);
            if (fileSelected.Length == 0)
                Console.WriteLine("Canceled file selection");
            else
            {
                Console.WriteLine("Selected file: " + fileSelected[0]);
                TestPKCS11(fileSelected[0]);
            }
        }

        private async void TestPKCS11(string libraryFilePath)
        {
            // Specify the path to unmanaged PKCS#11 library provided by the cryptographic device vendor
            string pkcs11LibraryPath = libraryFilePath;

            // Create factories used by Pkcs11Interop library
            Pkcs11InteropFactories factories = new Pkcs11InteropFactories();

            try
            {
                // Load unmanaged PKCS#11 library
                using (IPkcs11Library pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, pkcs11LibraryPath, AppType.SingleThreaded))
                {
                    // Show general information about loaded library
                    ILibraryInfo libraryInfo = pkcs11Library.GetInfo();

                    Console.WriteLine("Library");
                    Console.WriteLine("  Manufacturer:       " + libraryInfo.ManufacturerId);
                    Console.WriteLine("  Description:        " + libraryInfo.LibraryDescription);
                    Console.WriteLine("  Version:            " + libraryInfo.LibraryVersion);

                    // Get list of all available slots
                    foreach (ISlot slot in pkcs11Library.GetSlotList(SlotsType.WithOrWithoutTokenPresent))
                    {
                        // Show basic information about slot
                        ISlotInfo slotInfo = slot.GetSlotInfo();

                        Console.WriteLine();
                        Console.WriteLine("Slot");
                        Console.WriteLine("  Manufacturer:       " + slotInfo.ManufacturerId);
                        Console.WriteLine("  Description:        " + slotInfo.SlotDescription);
                        Console.WriteLine("  Token present:      " + slotInfo.SlotFlags.TokenPresent);

                        if (slotInfo.SlotFlags.TokenPresent)
                        {
                            // Show basic information about token present in the slot
                            ITokenInfo tokenInfo = slot.GetTokenInfo();

                            Console.WriteLine("Token");
                            Console.WriteLine("  Manufacturer:       " + tokenInfo.ManufacturerId);
                            Console.WriteLine("  Model:              " + tokenInfo.Model);
                            Console.WriteLine("  Serial number:      " + tokenInfo.SerialNumber);
                            Console.WriteLine("  Label:              " + tokenInfo.Label);

                            // Show list of mechanisms (algorithms) supported by the token
                            Console.WriteLine("Supported mechanisms: ");
                            foreach (CKM mechanism in slot.GetMechanismList())
                                Console.WriteLine("  " + mechanism);
                        }
                    }
                }
            }
            catch(Net.Pkcs11Interop.Common.UnmanagedException exception)
            {
                Label.Text = exception.Message;
                Console.WriteLine("Not a PKCS11 middleware: " + exception.Message);
            }
            catch(Net.Pkcs11Interop.Common.Pkcs11Exception exception)
            {
                Label.Text = exception.Message;
                Console.WriteLine("Not a PKCS11 middleware: " + exception.Message);
            }
        }
    }
}

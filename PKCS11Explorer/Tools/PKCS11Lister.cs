using Avalonia;
using Avalonia.Platform;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static PKCS11Explorer.Views.MainWindow;

namespace PKCS11Explorer.Tools
{
    static class PKCS11Lister
    {
        public async static Task<Node> ListForTreeview(string PKCS11LibraryFilePath) 
        {

            // Create factories used by Pkcs11Interop library
            Pkcs11InteropFactories factories = new Pkcs11InteropFactories();

            Node Tree = new Node();

            // Load unmanaged PKCS#11 library
            using (IPkcs11Library pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, PKCS11LibraryFilePath, AppType.SingleThreaded))
            {
                // Show general information about loaded library
                ILibraryInfo libraryInfo = pkcs11Library.GetInfo();
                Node Library = new Node() { Header = "PKCS11 Library " + libraryInfo.LibraryDescription, IconURI = "resm:PKCS11Explorer.Assets.baseline_layers_black_18dp.png" };
                Library.Children.Add(new Node() { Header = "Filepath: " + PKCS11LibraryFilePath, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                Library.Children.Add(new Node() { Header = "Manufacturer: " + libraryInfo.ManufacturerId, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                Library.Children.Add(new Node() { Header = "Description: " + libraryInfo.LibraryDescription, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                Library.Children.Add(new Node() { Header = "Version: " + libraryInfo.LibraryVersion, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                Tree.Children.Add(Library);
                // Get list of all available slots
                foreach (ISlot slot in pkcs11Library.GetSlotList(SlotsType.WithOrWithoutTokenPresent))
                {
                    // Show basic information about slot
                    ISlotInfo slotInfo = slot.GetSlotInfo();
                    Node Slot = new Node() { Header = "Slot " + slot.GetSlotInfo().SlotId, IconURI = "resm:PKCS11Explorer.Assets.baseline_scanner_black_18dp.png" };
                    Slot.Children.Add(new Node() { Header = "Manufacturer: " + slotInfo.ManufacturerId, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                    Slot.Children.Add(new Node() { Header = "Description: " + slotInfo.SlotDescription, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                    Slot.Children.Add(new Node() { Header = "Token present: " + slotInfo.SlotFlags.TokenPresent, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });

                    if (slotInfo.SlotFlags.TokenPresent)
                    {
                        // Show basic information about token present in the slot
                        ITokenInfo tokenInfo = slot.GetTokenInfo();

                        Node Token = new Node() { Header = "Token " + tokenInfo.SerialNumber, IconURI = "resm:PKCS11Explorer.Assets.baseline_sim_card_black_18dp.png" };
                        Token.Children.Add(new Node() { Header = "Manufacturer: " + tokenInfo.ManufacturerId, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                        Token.Children.Add(new Node() { Header = "Model: " + tokenInfo.Model, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                        Token.Children.Add(new Node() { Header = "Serial number: " + tokenInfo.SerialNumber, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                        Token.Children.Add(new Node() { Header = "Label: " + tokenInfo.Label, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png" });
                        Node SupportedMechanisms = new Node() { Header = "Supported mechanisms", IconURI = "resm:PKCS11Explorer.Assets.baseline_check_circle_black_18dp.png" };

                        // Show list of mechanisms (algorithms) supported by the token
                        foreach (CKM mechanism in slot.GetMechanismList())
                        {
                            SupportedMechanisms.Children.Add(new Node() { Header = mechanism.ToString(), IconURI = "resm:PKCS11Explorer.Assets.baseline_check_circle_black_18dp.png" });
                        }
                        Token.Children.Add(SupportedMechanisms);
                        Slot.Children.Add(Token);
                    }

                    Tree.Children.Add(Slot);
                }
            }
            return Tree;
        }
    }
}

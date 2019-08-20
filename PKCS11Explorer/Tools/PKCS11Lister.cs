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
        public static event EventHandler<ListForTreeviewEventArgs> ListForTreeviewFinished;
        public async static Task ListForTreeview(string PKCS11LibraryFilePath)
        {
            try
            {

                // Create factories used by Pkcs11Interop library
                Pkcs11InteropFactories factories = new Pkcs11InteropFactories();

                Node Tree = new Node();

                // Load unmanaged PKCS#11 library
                using (IPkcs11Library pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, PKCS11LibraryFilePath, AppType.MultiThreaded))
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
                            if(tokenInfo.FreePublicMemory != uint.MaxValue)
                                Token.Children.Add(new Node() { Header = "Available public objects memory: " + tokenInfo.FreePublicMemory + " / " + tokenInfo.TotalPublicMemory, IconURI = "resm:PKCS11Explorer.Assets.baseline_memory_black_18dp.png" });
                            if (tokenInfo.TotalPrivateMemory != uint.MaxValue)
                                Token.Children.Add(new Node() { Header = "Available private objects memory: " + tokenInfo.FreePrivateMemory + " / " + tokenInfo.TotalPrivateMemory, IconURI = "resm:PKCS11Explorer.Assets.baseline_memory_black_18dp.png" });

                            // Show public keys name
                            Node pubkeyNode = new Node() { Header = "Public keys", IconURI = "resm:PKCS11Explorer.Assets.baseline_search_black_18dp.png" };
                            using (ISession session = slot.OpenSession(SessionType.ReadOnly))
                            {
                                // Do something interesting in RO session
                                var attrList = new List<IObjectAttribute>();
                                attrList.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY));
                                attrList.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true));
                                var objects = session.FindAllObjects(attrList);
                                foreach(var obj in objects)
                                {
                                    if(obj.ObjectId != CK.CK_INVALID_HANDLE)
                                    {
                                        var objectHandle = factories.ObjectHandleFactory.Create(obj.ObjectId);
                                        List<CKA> attributes = new List<CKA>();
                                        attributes.Add(CKA.CKA_LABEL);
                                        List<IObjectAttribute> objectAttributes = session.GetAttributeValue(objectHandle, attributes);
                                        pubkeyNode.Children.Add(new Node() { Header = objectAttributes[0].GetValueAsString(), IconURI = "resm:PKCS11Explorer.Assets.baseline_vpn_key_black_18dp.png" });
                                        Console.WriteLine("Found: " + objectAttributes[0].GetValueAsString());
                                    }
                                }
                                session.CloseSession();
                            }
                            Token.Children.Add(pubkeyNode);


                            // Show private keys name
                            /*Node privkeyNode = new Node() { Header = "Private keys", IconURI = "resm:PKCS11Explorer.Assets.baseline_search_black_18dp.png" };
                            using (ISession session = slot.OpenSession(SessionType.ReadOnly))
                            {
                                session.Login(CKU.CKU_USER, "");
                                // Do something interesting in RO session
                                var attrList = new List<IObjectAttribute>();
                                attrList.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY));
                                attrList.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true));
                                var objects = session.FindAllObjects(attrList);
                                foreach (var obj in objects)
                                {
                                    if (obj.ObjectId != CK.CK_INVALID_HANDLE)
                                    {
                                        var objectHandle = factories.ObjectHandleFactory.Create(obj.ObjectId);
                                        List<CKA> attributes = new List<CKA>();
                                        attributes.Add(CKA.CKA_LABEL);
                                        List<IObjectAttribute> objectAttributes = session.GetAttributeValue(objectHandle, attributes);
                                        privkeyNode.Children.Add(new Node() { Header = objectAttributes[0].GetValueAsString(), IconURI = "resm:PKCS11Explorer.Assets.baseline_vpn_key_black_18dp.png" });
                                        Console.WriteLine("Found: " + objectAttributes[0].GetValueAsString());
                                    }
                                }
                                session.Logout();
                                session.CloseSession();
                            }
                            Token.Children.Add(privkeyNode);
                            */

                            // Show certs name
                            Node certsNode = new Node() { Header = "Certificates", IconURI = "resm:PKCS11Explorer.Assets.baseline_search_black_18dp.png" };
                            using (ISession session = slot.OpenSession(SessionType.ReadOnly))
                            {
                                // Do something interesting in RO session
                                var attrList = new List<IObjectAttribute>();
                                attrList.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE));
                                attrList.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true));
                                var objects = session.FindAllObjects(attrList);
                                foreach (var obj in objects)
                                {
                                    if (obj.ObjectId != CK.CK_INVALID_HANDLE)
                                    {
                                        var objectHandle = factories.ObjectHandleFactory.Create(obj.ObjectId);
                                        List<CKA> attributes = new List<CKA>();
                                        attributes.Add(CKA.CKA_LABEL);
                                        List<IObjectAttribute> objectAttributes = session.GetAttributeValue(objectHandle, attributes);
                                        certsNode.Children.Add(new Node() { Header = objectAttributes[0].GetValueAsString(), IconURI = "resm:PKCS11Explorer.Assets.baseline_list_alt_black_18dp.png" });
                                        Console.WriteLine("Found: " + objectAttributes[0].GetValueAsString());
                                    }
                                }
                                session.CloseSession();
                            }
                            Token.Children.Add(certsNode);


                            // Show list of mechanisms (algorithms) supported by the token
                            Node SupportedMechanisms = new Node() { Header = "Supported mechanisms", IconURI = "resm:PKCS11Explorer.Assets.baseline_check_circle_black_18dp.png" };
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
                ListForTreeviewFinished?.Invoke(Tree, new ListForTreeviewEventArgs() { MainNode = Tree, Success=true });
            }
            catch (Net.Pkcs11Interop.Common.UnmanagedException exception)
            {
                ListForTreeviewFinished?.Invoke(exception, new ListForTreeviewEventArgs() { UnmanagedException = exception, Success=false });
            }
            catch (Net.Pkcs11Interop.Common.Pkcs11Exception exception)
            {
                ListForTreeviewFinished?.Invoke(exception, new ListForTreeviewEventArgs() { Pkcs11Exception = exception, Success = false });
            }
            return;
        }
    }

    public class ListForTreeviewEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public Node MainNode { get; set; }
        public Net.Pkcs11Interop.Common.UnmanagedException UnmanagedException { get; set; }
        public Net.Pkcs11Interop.Common.Pkcs11Exception Pkcs11Exception { get; set; }
    }
}

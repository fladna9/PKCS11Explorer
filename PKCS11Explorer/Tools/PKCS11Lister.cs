using Avalonia;
using Avalonia.Platform;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.LowLevelAPI80;
using PKCS11Explorer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static PKCS11Explorer.Views.MainWindow;

namespace PKCS11Explorer.Tools
{
    public sealed class PKCS11Lister
    {
        #region singleton creation
        private static readonly Lazy<PKCS11Lister> lazyInstance = new Lazy<PKCS11Lister>(() => new PKCS11Lister());
        public static PKCS11Lister Instance
        {
            get { return lazyInstance.Value; }
        }
        private PKCS11Lister() { }
        #endregion

        #region Public fields and properties
        private string _libraryFilePath;
        public string LibraryFilePath { get { return _libraryFilePath; } }
        #endregion

        #region Public fields and properties
        private Pkcs11InteropFactories Factories;
        private IPkcs11Library PKCS11Library;
        #endregion

        #region Events
        public static event EventHandler<EventArgs> Initialized;
        public static event EventHandler<ListForTreeviewEventArgs> ListForTreeviewFinished;
        #endregion

        #region Low level methods
        public void Initialize(string libraryFilePath)
        {
            if (Factories == null)
                Factories = new Pkcs11InteropFactories();

            if (PKCS11Library != null)
                PKCS11Library.Dispose();

            _libraryFilePath = libraryFilePath;
            PKCS11Library = Factories.Pkcs11LibraryFactory.LoadPkcs11Library(Factories, LibraryFilePath, AppType.MultiThreaded);

            Initialized?.Invoke(this, new EventArgs());
        }
        #endregion

        #region High level methods
        #endregion
        public void ListForTreeview()
        {
            try
            {
                Node Tree = new Node();

                // Show general information about loaded library
                ILibraryInfo libraryInfo = PKCS11Library.GetInfo();
                Node Library = new Node() { Header = "PKCS11 Library " + libraryInfo.LibraryDescription, IconURI = "resm:PKCS11Explorer.Assets.baseline_layers_grey_18dp.png" };
                Library.Children.Add(new Node() { Header = "Filepath: " + LibraryFilePath, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                Library.Children.Add(new Node() { Header = "Manufacturer: " + libraryInfo.ManufacturerId, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                Library.Children.Add(new Node() { Header = "Description: " + libraryInfo.LibraryDescription, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                Library.Children.Add(new Node() { Header = "Version: " + libraryInfo.LibraryVersion, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                Tree.Children.Add(Library);
                // Get list of all available slots
                foreach (ISlot slot in PKCS11Library.GetSlotList(SlotsType.WithOrWithoutTokenPresent))
                {
                    // Show basic information about slot
                    ISlotInfo slotInfo = slot.GetSlotInfo();
                    Node Slot;
                    if (slotInfo.SlotFlags.TokenPresent)
                        Slot = new Node() { Header = "Slot " + slot.GetSlotInfo().SlotId, IconURI = "resm:PKCS11Explorer.Assets.baseline_scanner_green_18dp.png" };
                    else
                        Slot = new Node() { Header = "Slot " + slot.GetSlotInfo().SlotId, IconURI = "resm:PKCS11Explorer.Assets.baseline_scanner_red_18dp.png" };
                    Slot.Children.Add(new Node() { Header = "Manufacturer: " + slotInfo.ManufacturerId, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                    Slot.Children.Add(new Node() { Header = "Description: " + slotInfo.SlotDescription, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                    Slot.Children.Add(new Node() { Header = "Token present: " + slotInfo.SlotFlags.TokenPresent, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });

                    if (slotInfo.SlotFlags.TokenPresent)
                    {
                        // Show basic information about token present in the slot
                        ITokenInfo tokenInfo = slot.GetTokenInfo();

                        Node Token = new Node() { Header = "Token " + tokenInfo.SerialNumber, IconURI = "resm:PKCS11Explorer.Assets.baseline_sim_card_green_18dp.png" };
                        Token.Children.Add(new Node() { Header = "Manufacturer: " + tokenInfo.ManufacturerId, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                        Token.Children.Add(new Node() { Header = "Model: " + tokenInfo.Model, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                        Token.Children.Add(new Node() { Header = "Serial number: " + tokenInfo.SerialNumber, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                        Token.Children.Add(new Node() { Header = "Label: " + tokenInfo.Label, IconURI = "resm:PKCS11Explorer.Assets.baseline_info_blue_18dp.png" });
                        /*
                        if (tokenInfo.FreePublicMemory != uint.MaxValue)
                            Token.Children.Add(new Node() { Header = "Available public objects memory: " + tokenInfo.FreePublicMemory + " / " + tokenInfo.TotalPublicMemory, IconURI = "resm:PKCS11Explorer.Assets.baseline_memory_black_18dp.png" });
                        if (tokenInfo.TotalPrivateMemory != uint.MaxValue)
                            Token.Children.Add(new Node() { Header = "Available private objects memory: " + tokenInfo.FreePrivateMemory + " / " + tokenInfo.TotalPrivateMemory, IconURI = "resm:PKCS11Explorer.Assets.baseline_memory_black_18dp.png" });
                        */

                        // Show public keys name
                        Node pubkeyNode = new Node() { Header = "Public keys", IconURI = "resm:PKCS11Explorer.Assets.baseline_search_grey_18dp.png" };
                        using (ISession session = slot.OpenSession(SessionType.ReadOnly))
                        {
                            // Do something interesting in RO session
                            var attrList = new List<IObjectAttribute>();
                            attrList.Add(Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY));
                            attrList.Add(Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true));
                            var objects = session.FindAllObjects(attrList);
                            foreach (var obj in objects)
                            {
                                if (obj.ObjectId != CK.CK_INVALID_HANDLE)
                                {
                                    var objectHandle = Factories.ObjectHandleFactory.Create(obj.ObjectId);
                                    List<CKA> attributes = new List<CKA>();
                                    attributes.Add(CKA.CKA_LABEL);
                                    List<IObjectAttribute> objectAttributes = session.GetAttributeValue(objectHandle, attributes);
                                    pubkeyNode.Children.Add(new Node() { Header = objectAttributes[0].GetValueAsString(), IconURI = "resm:PKCS11Explorer.Assets.baseline_vpn_key_green_18dp.png" });
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
                        Node certsNode = new Node() { Header = "Certificates", IconURI = "resm:PKCS11Explorer.Assets.baseline_search_grey_18dp.png" };
                        using (ISession session = slot.OpenSession(SessionType.ReadOnly))
                        {
                            // Do something interesting in RO session
                            var attrList = new List<IObjectAttribute>();
                            attrList.Add(Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE));
                            attrList.Add(Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true));
                            var objects = session.FindAllObjects(attrList);
                            foreach (var obj in objects)
                            {
                                if (obj.ObjectId != CK.CK_INVALID_HANDLE)
                                {
                                    var objectHandle = Factories.ObjectHandleFactory.Create(obj.ObjectId);
                                    List<CKA> attributes = new List<CKA>();
                                    attributes.Add(CKA.CKA_LABEL);
                                    List<IObjectAttribute> objectAttributes = session.GetAttributeValue(objectHandle, attributes);
                                    certsNode.Children.Add(new Node() { Header = objectAttributes[0].GetValueAsString(), IconURI = "resm:PKCS11Explorer.Assets.baseline_list_alt_green_18dp.png" });
                                    Console.WriteLine("Found: " + objectAttributes[0].GetValueAsString());
                                }
                            }
                            session.CloseSession();
                        }
                        Token.Children.Add(certsNode);


                        // Show list of mechanisms (algorithms) supported by the token
                        Node SupportedMechanisms = new Node() { Header = "Supported mechanisms", IconURI = "resm:PKCS11Explorer.Assets.baseline_memory_grey_18dp.png" };
                        foreach (CKM mechanism in slot.GetMechanismList())
                        {
                            SupportedMechanisms.Children.Add(new Node() { Header = mechanism.ToString(), IconURI = "resm:PKCS11Explorer.Assets.baseline_check_circle_green_18dp.png" });
                        }
                        Token.Children.Add(SupportedMechanisms);
                        Slot.Children.Add(Token);
                    }
                    Assert.AssertTrue(slot.GetTokenInfo().RwSessionCount == 0);
                    Assert.AssertTrue(slot.GetTokenInfo().SessionCount == 0);
                    Tree.Children.Add(Slot);
                }
                ListForTreeviewFinished?.Invoke(Tree, new ListForTreeviewEventArgs() { MainNode = Tree, Success = true });
            }
            catch (Net.Pkcs11Interop.Common.UnmanagedException exception)
            {
                ListForTreeviewFinished?.Invoke(exception, new ListForTreeviewEventArgs() { UnmanagedException = exception, Success = false });
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

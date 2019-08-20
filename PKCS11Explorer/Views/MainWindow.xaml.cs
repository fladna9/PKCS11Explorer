using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using System.Reflection;
using PKCS11Explorer.Tools;
using PKCS11Explorer.Views;

namespace PKCS11Explorer.Views
{
    public class MainWindow : Window
    {
        TreeView MyTreeView;
        Node Tree;
        LoadingBox LoadingBox;

        public MainWindow()
        {
            var output = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            InitializeComponent();
            Tree = new Node();
            DataContext = Tree.Children;
#if DEBUG
            this.AttachDevTools();
#endif

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            MyTreeView = this.FindControl<TreeView>("MyTreeView");
            SetGUI();
        }

        private void SetGUI()
        {
            Icon = new WindowIcon((Bitmap)BitmapValueConverter.Instance.Convert((object)"resm:PKCS11Explorer.Assets.baseline_sim_card_black_18dp.png", typeof(IBitmap), null, null));
            Title = "PKCS11 Explorer";
            Console.WriteLine("PKCS11 Explorer version " + ((App)App.Current).Version);
            Console.WriteLine("");
            Console.WriteLine("Debug console");
            Console.WriteLine("---------------------");
            MyTreeView.Width = Double.NaN;
            MyTreeView.Height = Double.NaN;
            MyTreeView.IsVisible = false;
            LoadingBox = new LoadingBox("Please wait", "Loading PKCS11 middleware and looking for devices...", "resm:PKCS11Explorer.Assets.baseline_hourglass_empty_black_18dp.png");
        }

        private async void ButtonHandler_LoadFile(object sender, RoutedEventArgs e)
        {
            // Open file dialog and allow only middleware libraries extension to be loaded.
            // That is: DLL on Win, SO on Linux and DYLIB on macOS
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
                LoadingBox.ShowDialog(this);

                Console.WriteLine("Selected file: " + fileSelected[0]);
                Console.WriteLine("Loading informations, please wait.");

                try
                {
                    Tree = await PKCS11Lister.ListForTreeview(fileSelected[0]);
                    Console.WriteLine("Loading done. refreshing UI.");
                    DataContext = Tree.Children;
                    MyTreeView.IsVisible = true;
                }
                catch (Net.Pkcs11Interop.Common.UnmanagedException exception)
                {
                    if (exception.Message.Contains("Unable to get pointer for C_GetFunctionList function. Error code: 0x0000007F."))
                    {
                        Console.WriteLine("Not a PKCS11 middleware, aborting.");
                        var dialog = new DialogBox("Error", "The loaded library is not a valid PKCS11 middleware.", "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png", "Ok");
                        await dialog.ShowDialog(this);
                    }
                    else
                    {
                        Console.WriteLine("Problem with provided middleware: " + exception.Message);
                    }
                }
                catch (Net.Pkcs11Interop.Common.Pkcs11Exception exception)
                {
                    Console.WriteLine("Not a PKCS11 middleware: " + exception.Message);
                }
                LoadingBox.Close();
            }
        }

        public class Node
        {
            private ObservableCollection<Node> _children;
            public string IconURI { get; set; }
            public string Header { get; set; }
            public bool IsVisible { get
                {
                    return ! string.IsNullOrWhiteSpace(IconURI);
                }
            }
            public IBitmap Icon { get
                {
                    if (IsVisible)
                        return (Bitmap)BitmapValueConverter.Instance.Convert((object)IconURI, typeof(IBitmap), null, null);
                    else
                        return null;
                }
            }

            public ObservableCollection<Node> Children
            {
                get
                {
                    if (_children == null)
                        _children = new ObservableCollection<Node>();
                    return _children;
                }
                set
                {
                    _children = value;
                }
            }
        }
    }
}

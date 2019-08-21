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
using Avalonia.Threading;
using PKCS11Explorer.Models;

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
            PKCS11Lister.ListForTreeviewFinished += OnListForTreeviewFinished;
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
                try
                {
                    var loadingTask = Task.Run(() => PKCS11Lister.Instance.Initialize(fileSelected[0]));
                    LoadingBox = new LoadingBox("Please wait", "Loading PKCS11 middleware...", "resm:PKCS11Explorer.Assets.baseline_hourglass_orange_black_18dp.png");
                    LoadingBox.ShowDialog(this);

                    Console.WriteLine("Selected file: " + fileSelected[0]);
                    Console.WriteLine("Loading informations, please wait.");
                    await loadingTask;
                    LoadingBox.Close();
                    Console.WriteLine("Loaded PKCS11 middleware.");
                    Task.Run(() => { PKCS11Lister.Instance.ListForTreeview(); });
                }
                catch(Exception exception)
                {
                    LoadingBox.Close();
                    Console.WriteLine("Problem with PKCS11: " + exception.Message);
                    Console.WriteLine("Stacktrace: " + exception.StackTrace);
                    if (Tree.Children?.Count > 0)
                        Tree.Children?.Clear();
                    var dialog = new DialogBox("Error", exception.Message + "\nDid you select a valid PKCS11 library?" , "resm:PKCS11Explorer.Assets.baseline_highlight_off_red_18dp.png", "Ok");
                    await dialog.ShowDialog(this);
                }
            }
        }

        private void OnListForTreeviewFinished(object sender, ListForTreeviewEventArgs eventArgs)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                LoadingBox.Close();
                
                if(eventArgs.Success)
                {
                    Console.WriteLine("Loading done. refreshing UI.");
                    Tree = eventArgs.MainNode;
                    DataContext = Tree.Children;
                    MyTreeView.IsVisible = true;
                }
                else
                {
                    if (eventArgs.UnmanagedException != null && eventArgs.UnmanagedException.Message.Contains("Unable to get pointer for C_GetFunctionList function. Error code: 0x0000007F."))
                    {
                        Console.WriteLine("Not a PKCS11 middleware, aborting.");
                        var dialog = new DialogBox("Error", "The loaded library is not a valid PKCS11 middleware.", "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png", "Ok");
                        dialog.ShowDialog(this);
                    }
                    else if (eventArgs.UnmanagedException != null)
                    {
                        Console.WriteLine("Problem with provided middleware: " + eventArgs.UnmanagedException.Message);
                        var dialog = new DialogBox("Error", eventArgs.UnmanagedException.Message, "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png", "Ok");
                        dialog.ShowDialog(this);
                    }
                    else if (eventArgs.Pkcs11Exception != null)
                    {
                        Console.WriteLine("Problem with PKCS11: " + eventArgs.Pkcs11Exception.Message);
                        var dialog = new DialogBox("Error", eventArgs.Pkcs11Exception.Message, "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png", "Ok");
                        dialog.ShowDialog(this);
                    }
                    else
                    {
                        Console.WriteLine("Unmanaged exception");
                        var dialog = new DialogBox("Error", "Unmanaged exception", "resm:PKCS11Explorer.Assets.baseline_info_black_18dp.png", "Ok");
                        dialog.ShowDialog(this);
                    }
                }
            });
        }
    }
}

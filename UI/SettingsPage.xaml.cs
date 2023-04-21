// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.WebUI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            //set wabt directory from futureaccesslist if it exists
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("WabtDirectory"))
            {
                StorageFolder wabtFolder = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("WabtDirectory").GetAwaiter().GetResult();
                WabtDirectory.Text = wabtFolder.Path;
            }
        }

        private async void WabtButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a folder picker.
            var folderPicker = new FolderPicker();


            var window = (Application.Current as App)?.Window as MainWindow;
            
            // 1. Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // 2. Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

            // Use the folder picker as usual.
            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync();

            //updatethe textblock
            WabtDirectory.Text = folder.Path;

            //save folder path
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("WabtDirectory", folder);

        }


    }
}

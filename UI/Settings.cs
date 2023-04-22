using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;

namespace UI;

public class Settings
{
    public static string WabtDirectory 
    { 
        get
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("WabtDirectory"))
            {
                StorageFolder wabtFolder = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("WabtDirectory").GetAwaiter().GetResult();
                return wabtFolder.Path;
            }

            return "WABT Directory";
        }
    }

    public static string InputPath
    {
        get
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("InputPath"))
            {
                var inputFile = StorageApplicationPermissions.FutureAccessList.GetFileAsync("InputPath").GetAwaiter().GetResult();
                return inputFile.Path;
            }

            return "Input Path";
        }
    }
    
    public static void UpdateWabtDirectory(IStorageFolder folder)
    {
        StorageApplicationPermissions.FutureAccessList.AddOrReplace("WabtDirectory", folder);        
    }
    
    public static void UpdateInputPath(IStorageFile file)
    {
        StorageApplicationPermissions.FutureAccessList.AddOrReplace("InputPath", file);
    }
    
}
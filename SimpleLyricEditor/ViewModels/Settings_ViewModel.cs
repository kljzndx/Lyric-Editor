using GalaSoft.MvvmLight;
using SimpleLyricEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricEditor.ViewModels
{
    public class Settings_ViewModel:ViewModelBase
    {
        public Settings Settings { get; } = Settings.GetSettingsObject();

        public async Task PickerImageFile()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file is null)
                return;

            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("BackgroundImage", file);
            Settings.LocalBackgroundImagePath = file.Path;
        }

        public void SwitchSourceType_AlbumImage()
        {
            Settings.BackgroundSourceType = BackgroundSourceTypeEnum.AlbumImage;
        }

        public async void SwitchSourceType_LocalImage()
        {
            if (Settings.LocalBackgroundImagePath == String.Empty)
                await PickerImageFile();
            Settings.BackgroundSourceType = BackgroundSourceTypeEnum.LocalImage;
        }
    }
}

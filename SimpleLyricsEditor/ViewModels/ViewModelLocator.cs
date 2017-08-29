using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace SimpleLyricsEditor.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<Main_ViewModel>();
        }

        public Main_ViewModel Main
        {
            get => SimpleIoc.Default.GetInstance<Main_ViewModel>();
        }
    }
}
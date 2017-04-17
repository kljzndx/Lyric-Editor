using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<Main_ViewModel>();
            SimpleIoc.Default.Register<Settings_ViewModel>();
        }

        public Main_ViewModel Main { get => ServiceLocator.Current.GetInstance<Main_ViewModel>(); }

        public Settings_ViewModel Settings { get => ServiceLocator.Current.GetInstance<Settings_ViewModel>(); }
    }
}

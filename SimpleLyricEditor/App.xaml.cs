using SimpleLyricEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
using Windows.ApplicationModel.Resources;
using WinRTExceptions;
using SimpleLyricEditor.Models;
using Microsoft.HockeyApp;

namespace SimpleLyricEditor
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        private Settings settings = Settings.GetSettingsObject();
        private CoreWindow window;

        public static ViewModelLocator Locator = new ViewModelLocator();
        public static bool IsPressCtrl { get; private set; }
        public static bool IsPressShift { get; private set; }

        //指示输入框是否已获得焦点
        public static bool IsInputBoxGotFocus { get; set; }
        public static bool IsSuspend { get; private set; }


        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            this.UnhandledException += OnUnhandledException;
            HockeyClient.Current.Configure("69cf42d198694fe3b7a5ab0d7eae6370");
        }

        private void CreateKeySubscription()
        {
            if (window != null)
                return;
            window = CoreWindow.GetForCurrentThread();
            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
        }

        public async void ShowErrorDialog(Exception ex)
        {
            Error error = new Error(ex);

            var buttons = new Dictionary<string, Windows.UI.Popups.UICommandInvokedHandler>();
            buttons.Add(CharacterLibrary.ErrorDialog.GetString("EmailErrorReport"),
                async c => await EmailEx.SendAsync("kljzndx@outlook.com",
                                             $"{HappyStudio.UwpToolsLibrary.Information.AppInfo.Name} {HappyStudio.UwpToolsLibrary.Information.AppInfo.Version} {Models.CharacterLibrary.ErrorDialog.GetString("ErrorReport")}",
                                             $"请说明你做了什么\nPlease introduce what you do\n\n\n\n设备名：{SystemInfo.DeviceName}\n设备类型：{SystemInfo.DeviceType}\n系统版本：{SystemInfo.BuildVersion}\n{error.ToString()}"));

            await MessageBox.ShowAsync(CharacterLibrary.ErrorDialog.GetString("Title"), error.Content, buttons, CharacterLibrary.ErrorDialog.GetString("Close"));
            Application.Current.Exit();
        }

        private void EnsureSyncContext()
        {
            ExceptionHandlingSynchronizationContext.Register().UnhandledException += OnSynchronizationContextUnhandledException;
        }

        private void OnSynchronizationContextUnhandledException(object sender, WinRTExceptions.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            settings.SettingObject.Values["IsCollapse"] = true;
            ShowErrorDialog(e.Exception);
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            settings.SettingObject.Values["IsCollapse"] = true;
            ShowErrorDialog(e.Exception);
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                    IsSuspend = false;
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
            CreateKeySubscription();
            EnsureSyncContext();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            CreateKeySubscription();
            EnsureSyncContext();
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame is null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content is null)
            {
                rootFrame.Navigate(typeof(MainPage));

                CreateKeySubscription();
                EnsureSyncContext();
            }
            Window.Current.Activate();
            Tools.LyricFileTools.ChangeFile(args.Files[0] as StorageFile);
        }

        private void Window_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Control)
                IsPressCtrl = true;
            if (args.VirtualKey == Windows.System.VirtualKey.Shift)
                IsPressShift = true;
        }

        private void Window_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Control)
                IsPressCtrl = false;
            if (args.VirtualKey == Windows.System.VirtualKey.Shift)
                IsPressShift = false;
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            IsSuspend = true;
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            IsSuspend = false;
        }

    }
}

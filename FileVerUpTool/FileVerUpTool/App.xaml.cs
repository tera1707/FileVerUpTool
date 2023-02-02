// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using FileVerUpTool.Interface;
using FileVerUpTool.Logic;
using FileVerUpTool.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FileVerUpTool
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // ServiceCollectionを作成
            var services = new ServiceCollection();

            // インスタンスを登録
            services.AddSingleton<IProjMetaDataHandler[]>(x => new IProjMetaDataHandler[] { new SdkTypeCsprojHandler(), new DotnetFrameworkProjHandler(), new CppProjHandler() });
            services.AddSingleton<ISearchSpecifiedExtFile, SearchSpecifiedExtFile>();
            services.AddSingleton<IVersionReadWrite, VersionReadWrite>();
            services.AddSingleton<MainWindow>();

            // サービスプロバイダ、サービスを作成
            var provider = services.BuildServiceProvider();
            Ioc.Default.ConfigureServices(provider);

            // 登録したインスタンスを使う
            var mw = Ioc.Default.GetRequiredService<MainWindow>();

            mw.Activate();
        }

        private Window m_window;
    }
}

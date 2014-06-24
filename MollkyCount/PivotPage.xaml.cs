using MollkyCount.Common;
using MollkyCount.DAL;
using MollkyCount.DataModel;
using MollkyCount.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace MollkyCount
{
    public sealed partial class PivotPage : BasePage
    {
        private readonly GlobalViewModel defaultViewModel = App.ViewModelLocator.Main;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage() : base()
        {
            this.InitializeComponent();

            //Windows.ApplicationModel.Resources.Core.ResourceManager.Current.MainResourceMap.look(resourceId);
            //var map = ResourceManager.Current.MainResourceMap;
            //RuleWebView.NavigateToString("toto");
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public GlobalViewModel DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        
        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        public override async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            await this.DefaultViewModel.MapViewModel();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            defaultViewModel.ShowSavedGameCommand.Execute(e.ClickedItem);
        }

        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState != HoldingState.Started) return;

            FrameworkElement element = sender as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            FlyoutBase.ShowAttachedFlyout(element);
        }
    }
}

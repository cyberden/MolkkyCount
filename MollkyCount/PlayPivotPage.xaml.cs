using MollkyCount.Common;
using MollkyCount.DAL;
using MollkyCount.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MollkyCount
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayPivotPage : BasePage
    {
        private GameViewModel defaultViewModel = null;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PlayPivotPage()
            : base()
        {
            NavigationHelper.GoBackCommand = new RelayCommand(
                        (object p) => { if (this.defaultViewModel != null) this.defaultViewModel.GoBack(); },
                        (object p) => { if (this.defaultViewModel != null) return this.defaultViewModel.CanGoBack(); else return true; });
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public GameViewModel DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }


        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var allPlayers = await DataSourceProvider.GetPlayers();
            var allPlayersVm = allPlayers.Select(p => new PlayerViewModel() { Id = p.Id, Name = p.Name });

            var game = await DataSourceProvider.GetGame((Guid)e.Parameter);

            defaultViewModel = GameViewModel.GetViewModel(game, allPlayersVm);
            this.DataContext = defaultViewModel;


            //await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            ((RelayCommand)defaultViewModel.UndoLastCommand).RaiseCanExecuteChanged();
            ((RelayCommand)defaultViewModel.NextPlayCommand).RaiseCanExecuteChanged();
            //});

            DataTransferManager.GetForCurrentView().DataRequested += PivotPage_DataRequested;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            DataTransferManager.GetForCurrentView().DataRequested -= PivotPage_DataRequested;
        }

        protected void PivotPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            defaultViewModel.HandleDataRequests(sender, args);
        }

        #endregion
    }
}

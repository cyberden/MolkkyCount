using MollkyCount.Common;
using MollkyCount.DAL;
using MollkyCount.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MollkyCount
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewGamePage : BasePage
    {
        public NewGamePage()
            : base()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            CreateGameViewModel vm = null;
            if (e.NavigationMode == NavigationMode.New)
            {
                
                if (e.Parameter is Guid && (Guid)e.Parameter != Guid.Empty)
                {
                    vm = new CreateGameViewModel();
                    vm.LoadFromExistingGame((Guid)e.Parameter);

                }
                else
                {
                    vm = new CreateGameViewModel() { Game = new GameViewModel() { Id = Guid.NewGuid(), Players = new ObservableCollection<GamePlayerViewModel>(), Date = DateTime.Now, Status = DataModel.Enums.GameStatus.BeeingCreated } };
                }

                await DataSourceProvider.SaveBeeingCreatedGame(vm.Game.GetGame());
                this.DataContext = vm;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                vm = new CreateGameViewModel();
                await vm.LoadFromTemporaryFile();

                this.DataContext = vm;
            }

            if (vm != null)
            {
                //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                    ((RelayCommand)vm.PlayCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)vm.ShufflePlayersCommand).RaiseCanExecuteChanged();
              //  });
                
            }
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState != HoldingState.Started) return;

            FrameworkElement element = sender as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            FlyoutBase.ShowAttachedFlyout(element);
        }
    }
}

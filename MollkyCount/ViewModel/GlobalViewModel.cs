using GalaSoft.MvvmLight;
using Microsoft.Live;
using MollkyCount.Common;
using MollkyCount.DAL;
using MollkyCount.OneDrive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MollkyCount.ViewModel
{
    public class GlobalViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<GameViewModel> _allGames;
        public ObservableCollection<GameViewModel> AllGames
        {
            get { return _allGames; }
            set
            {
                _allGames = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<StatsPlayerViewModel> _allPlayers;
        public ObservableCollection<StatsPlayerViewModel> AllPlayers
        {
            get { return _allPlayers; }
            set
            {
                _allPlayers = value;
                RaisePropertyChanged();
            }
        }

        private bool _isProgressBarVisible = false;
        public bool IsProgressBarVisible
        {
            get { return _isProgressBarVisible; }
            set 
            {
                _isProgressBarVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand NewGameCommand { get; private set; }

        public ICommand ShowSavedGameCommand { get; private set; }

        public ICommand DeleteGameCommand { get; private set; }

        public ICommand ImportDataCommand { get; private set; }

        public ICommand ExportDataCommand { get; private set; }
        #endregion

        #region Constructors
        public GlobalViewModel()
        {
            NewGameCommand = new RelayCommand(NewGameExecute);
            ShowSavedGameCommand = new RelayCommand(ShowSavedGameExecute);
            DeleteGameCommand = new RelayCommand(DeleteGameExecute);
            ImportDataCommand = new RelayCommand(ImportDataExecute);
            ExportDataCommand = new RelayCommand(ExportDataExecute);
        }
        #endregion

        #region Command Handlers
        public void NewGameExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(NewGamePage), Guid.Empty);
        }

        public void ShowSavedGameExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && parameter is GameViewModel)
                rootFrame.Navigate(typeof(PlayPivotPage), ((GameViewModel)parameter).Id);
        }

        public async void DeleteGameExecute(object parameter)
        {
            var gameVm = parameter as GameViewModel;

            if (gameVm != null)
            {
                AllGames.Remove(gameVm);
                await DataSourceProvider.DeleteGame(gameVm.Id);
            }
        }

        public async void ImportDataExecute(object parameter)
        {
            try
            {
                IsProgressBarVisible = true;

                OneDriveFileHandler handler = new OneDriveFileHandler(null, null, "MolkkyCountData");
                await handler.Initialize();

                var resPLayers = await handler.DownloadFile(DataSourceProvider.PlayersFileName);
                var resTeams = await handler.DownloadFile(DataSourceProvider.TeamsFileName);
                var resGames = await handler.DownloadFile(DataSourceProvider.GamesFileName);

                if (resPLayers == OperationStatus.Completed && resGames == OperationStatus.Completed && resTeams == OperationStatus.Completed)
                {
                    await MapViewModel(true);

                    var dlg = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ImportSuccess"));
                    await dlg.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dlg = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ImportFailure"));
                dlg.ShowAsync();
            }
            finally
            {
                IsProgressBarVisible = false;
            }
        }

        public async void ExportDataExecute(object parameter)
        {
            try
            {
                IsProgressBarVisible = true;
                
                OneDriveFileHandler handler = new OneDriveFileHandler(null, null, "MolkkyCountData");
                await handler.Initialize();
                
                var res = await handler.UploadFile(DataSourceProvider.PlayersFileName);
                var res2 = await handler.UploadFile(DataSourceProvider.TeamsFileName);
                var res3 = await handler.UploadFile(DataSourceProvider.GamesFileName);

                var dlg = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ExportSuccess"));
                await dlg.ShowAsync();
            }
            catch (Exception ex)
            {
                var dlg = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ExportFailure"));
                dlg.ShowAsync();
            }
            finally
            {
                IsProgressBarVisible = false;
            }
        }

        #endregion

        #region Mapping
        public async Task MapViewModel(bool forceRead = false)
        {
            var players = await DataSourceProvider.GetPlayers(forceRead);
            var teams = await DataSourceProvider.GetTeams(forceRead);
            var games = await DataSourceProvider.GetGames(forceRead);

            AllPlayers = new ObservableCollection<StatsPlayerViewModel>(players.OrderBy(p => p.Name).Select(g => new StatsPlayerViewModel(g, games, teams)));

            var allTeamsVm = await TeamViewModel.GetTeams(teams);

            AllGames = new ObservableCollection<GameViewModel>();
            foreach (var game in games.Where(g => g.Status != DataModel.Enums.GameStatus.Canceled).OrderByDescending(g => g.Date))
            {
                AllGames.Add(GameViewModel.GetViewModel(game, AllPlayers.Select(p => p.Player), allTeamsVm));
            }
        }
        #endregion

    }
}

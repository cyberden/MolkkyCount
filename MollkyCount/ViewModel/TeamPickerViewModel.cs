using MollkyCount.Common;
using MollkyCount.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MollkyCount.ViewModel
{
    public class TeamPickerViewModel : BaseViewModel
    {
        #region Properties
        private GameViewModel _game;
        public GameViewModel Game
        {
            get { return _game; }
            set
            {
                _game = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<SelectableObject<TeamViewModel>> _allTeams;
        public ObservableCollection<SelectableObject<TeamViewModel>> AllTeams
        {
            get { return _allTeams; }
            set
            {
                _allTeams = value;
                RaisePropertyChanged();
            }
        }
        #endregion

         #region Commands
        public ICommand CreateTeamCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #endregion

        #region Constructors
        public TeamPickerViewModel()
        {
            CreateTeamCommand = new RelayCommand(CreateTeamExecute);
            OkCommand = new RelayCommand(OkExecute);
            CancelCommand = new RelayCommand(CancelExecute);
            
        }
        #endregion

        #region InitializeAsync
        public async Task InitializeAsync()
        {
            var allTeams = await DataSourceProvider.GetTeams();
            var allPlayers = await DataSourceProvider.GetPlayers();

            var unselectableVms = await TeamViewModel.GetTeams(allTeams);

            AllTeams = new ObservableCollection<SelectableObject<TeamViewModel>>(unselectableVms.Select(uvm => new SelectableObject<TeamViewModel>() { IsSelected = false, Item = uvm }));

            var allPlayersVm = allPlayers.Select(p => new PlayerViewModel() { Id = p.Id, Name = p.Name });

            var game = await DataSourceProvider.RetrieveBeeingCreatedGame();

            Game = GameViewModel.GetViewModel(await DataSourceProvider.RetrieveBeeingCreatedGame(), allPlayersVm, unselectableVms);

            if (Game != null)
            {
                var beeingCreatedTeam = await DataSourceProvider.RetrieveBeeingCreatedTeam();

                foreach(var team in AllTeams.Where(p => Game.Players.Any(gp => gp.Player.Id == p.Item.Id)
                                                        || p.Item.Id == beeingCreatedTeam.Id))
                    team.IsSelected = true;


            }
        }
        #endregion

        #region Command Handlers
        public async void CreateTeamExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(CreateTeamPage), "CreateTeam");
        }

        public async void OkExecute(object parameter)
        {
            if (this.Game != null)
            {
                foreach (var elt in AllTeams.Where(t => t.IsSelected && !Game.Players.Any(gp => gp.Player.Id == t.Item.Id)))
                {
                    Game.Players.Add(new GamePlayerViewModel(Game) { Player = elt.Item, Rank = Game.Players.Count + 1, CurrentTeamPlayerRank = 1 });
                }

                foreach (var elt in AllTeams.Where(t => !t.IsSelected && Game.Players.Any(gp => gp.Player.Id == t.Item.Id)))
                {
                    var eltToRemove = Game.Players.First(p => p.Player.Id == elt.Item.Id);

                    Game.Players.Remove(eltToRemove);
                    foreach (var sourceElt in Game.Players.Where(s => s.Rank > eltToRemove.Rank))
                        sourceElt.Rank--;
                }

                await DataSourceProvider.SaveBeeingCreatedGame(Game.GetGame());
            }

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
                rootFrame.GoBack();
        }

        public void CancelExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.GoBack();
        }
        #endregion
    }
}

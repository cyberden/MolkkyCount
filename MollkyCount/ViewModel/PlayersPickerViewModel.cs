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
    public class PlayersPickerViewModel : BaseViewModel
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

        private TeamViewModel _team;
        public TeamViewModel Team
        {
            get { return _team; }
            set
            {
                _team = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<SelectableObject<PlayerViewModel>> _allPlayers;
        public ObservableCollection<SelectableObject<PlayerViewModel>> AllPlayers
        {
            get { return _allPlayers; }
            set
            {
                _allPlayers = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand CreatePlayerCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #endregion

        #region Constructors
        public PlayersPickerViewModel()
        {
            CreatePlayerCommand = new RelayCommand(CreatePlayerExecute);
            OkCommand = new RelayCommand(OkExecute);
            CancelCommand = new RelayCommand(CancelExecute);
            
        }
        #endregion

        #region InitializeAsync
        public async Task InitializeAsync(string parameter)
        {
            var allTeams = await DataSourceProvider.GetTeams();
            var allPlayers = await DataSourceProvider.GetPlayers();

            var allTeamsVms = await TeamViewModel.GetTeams(allTeams);

            var allPlayersVm = allPlayers.Select(p => new PlayerViewModel() { Id = p.Id, Name = p.Name });

            AllPlayers = new ObservableCollection<SelectableObject<PlayerViewModel>>(allPlayers.Select(p => new SelectableObject<PlayerViewModel>() { IsSelected = false, Item = allPlayersVm.First(pvm => pvm.Id == p.Id)}));

            if (parameter == "CreateGame")
            {
                var game = await DataSourceProvider.RetrieveBeeingCreatedGame();

                Game = GameViewModel.GetViewModel(await DataSourceProvider.RetrieveBeeingCreatedGame(), allPlayersVm, allTeamsVms);

                if (Game != null)
                {
                    foreach (var player in AllPlayers.Where(p => Game.Players.Any(gp => gp.Player.Id == p.Item.Id)))
                        player.IsSelected = true;

                    var unSelectablePlayers = Game.Players.Where(p => p.Player is TeamViewModel).SelectMany(p => ((TeamViewModel)p.Player).Players.Select(pp => pp.Player.Id));
                    AllPlayers = new ObservableCollection<SelectableObject<PlayerViewModel>>(AllPlayers.Where(p => !unSelectablePlayers.Any(up => up == p.Item.Id)));
                }
            }
            else if (parameter == "CreateTeam")
            {
                var team = await DataSourceProvider.RetrieveBeeingCreatedTeam();

                var teams = await TeamViewModel.GetTeams(new List<Team>() { team });
                Team = teams.First();

                if (Team != null)
                {
                    foreach (var player in AllPlayers.Where(p => Team.Players.Any(gp => gp.Player.Id == p.Item.Id)))
                        player.IsSelected = true;
                }
            }
        }
        #endregion

        #region Overriden methods
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "Game" && AllPlayers != null)
            {
                foreach (var elt in AllPlayers.Where(ap => Game.Players.Any(p => p.Player.Id == ap.Item.Id)))
                    elt.IsSelected = true;
            }
        }
        #endregion

        #region Command handlers
        public async void CreatePlayerExecute(object parameter)
        {
            var newPlayer = new PlayerViewModel() { Id = Guid.NewGuid() };

            var createPlayerDialog = new CreatePlayerDialog(newPlayer);
            var messageDialogResult = await createPlayerDialog.ShowAsync();

            if (messageDialogResult == ContentDialogResult.Primary)
            {
                await DataSourceProvider.SavePlayer(newPlayer.GetPlayer());
                AllPlayers.Add(new SelectableObject<PlayerViewModel>() { IsSelected = true, Item = newPlayer });
            }
        }
        public async void OkExecute(object parameter)
        {
            if (this.Game != null)
            {
                foreach(var elt in AllPlayers.Where(p => p.IsSelected && !Game.Players.Any(gp => gp.Player.Id == p.Item.Id)))
                {
                    Game.Players.Add(new GamePlayerViewModel(Game) { Player = elt.Item, Rank = Game.Players.Count + 1 });
                }

                foreach (var elt in AllPlayers.Where(p => !p.IsSelected && Game.Players.Any(gp => gp.Player.Id == p.Item.Id)))
                {
                    var eltToRemove = Game.Players.First(p => p.Player.Id == elt.Item.Id);

                    Game.Players.Remove(eltToRemove);
                    foreach (var sourceElt in Game.Players.Where(s => s.Rank > eltToRemove.Rank))
                        sourceElt.Rank--;
                }

                await DataSourceProvider.SaveBeeingCreatedGame(Game.GetGame());
            }
            else if (this.Team != null)
            {
                foreach (var elt in AllPlayers.Where(p => p.IsSelected && !Team.Players.Any(gp => gp.Player.Id == p.Item.Id)))
                {
                    Team.Players.Add(new TeamPlayerViewModel() { Player = elt.Item, Rank = Team.Players.Count + 1 });
                }

                foreach (var elt in AllPlayers.Where(p => !p.IsSelected && Team.Players.Any(gp => gp.Player.Id == p.Item.Id)))
                {
                    var eltToRemove = Team.Players.First(p => p.Player.Id == elt.Item.Id);

                    Team.Players.Remove(eltToRemove);
                    foreach (var sourceElt in Team.Players.Where(s => s.Rank > eltToRemove.Rank))
                        sourceElt.Rank--;
                }

                await DataSourceProvider.SaveBeeingCreatedTeam(Team.GetTeam());
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

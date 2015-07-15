using MollkyCount.Common;
using MollkyCount.DAL;
using MollkyCount.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MollkyCount.ViewModel
{
    [DataContract]
    public class CreateGameViewModel : BaseViewModel
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
        #endregion

        #region Commands
        public ICommand AddPlayerCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand ShufflePlayersCommand { get; private set; }
        public ICommand AddTeamCommand { get; private set; }
        #endregion

        #region Constructor
        public CreateGameViewModel()
        {   
            AddPlayerCommand = new RelayCommand(AddPlayerExecute);
            AddTeamCommand = new RelayCommand(AddTeamExecute);
            PlayCommand = new RelayCommand(PlayExecute, PlayCanExecute);
            ShufflePlayersCommand = new RelayCommand(ShufflePlayersExecute, ShufflePlayersCanExecute);
        }

        #endregion

        #region custom logic
        public async void LoadFromExistingGame(Guid sourceGameId)
        {
            var games = await DataSourceProvider.GetGames();
            var game = games.First(g => g.Id == sourceGameId);

            var players = await DataSourceProvider.GetPlayers();
            var teams = await DataSourceProvider.GetTeams();

            Game = new GameViewModel()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Now,
                    Status = GameStatus.BeeingCreated,
                    Players = new ObservableCollection<GamePlayerViewModel>()
                };

            // L'ordre de jeu d'une partie n'étant pas une "première partie" est suivant le score total initial, du plus petit au plus grand.
            int rank = 1;
            foreach(var player in game.Players.OrderBy(p => p.TotalScore))
            {
                if (player.PlayerId.HasValue)
                {
                    Game.Players.Add(new GamePlayerViewModel() { Player = new PlayerViewModel() { Id = player.PlayerId.Value, Name = players.First(pp => pp.Id == player.PlayerId).Name }, Rank = rank });
                }
                else
                {
                    var team = teams.First(pp => pp.Id == player.TeamId);

                    Game.Players.Add(new GamePlayerViewModel() 
                        { 
                            Player = new TeamViewModel() 
                                { 
                                    Id = player.TeamId.Value, 
                                    Name = team.Name, 
                                    Players = new ObservableCollection<TeamPlayerViewModel>(team.Players.Select(tp => new TeamPlayerViewModel() 
                                        { 
                                            Player = new PlayerViewModel() 
                                                { 
                                                    Id = players.First(pp => pp.Id == tp.PlayerId).Id, 
                                                    Name = players.First(pp => pp.Id == tp.PlayerId).Name 
                                                }, 
                                                Rank = tp.Rank 
                                        })) 
                                },
                                Rank = rank,
                                CurrentTeamPlayerRank = 1
                        });
                }

                rank++;
            }
        }

        public async Task LoadFromTemporaryFile()
        {
            var game = await DataSourceProvider.RetrieveBeeingCreatedGame();
            var players = await DataSourceProvider.GetPlayers();
            var teams = await DataSourceProvider.GetTeams();

            Game = new GameViewModel()
            {
                Id = game.Id,
                Date = DateTime.Now,
                Status = game.Status,
                Players = new ObservableCollection<GamePlayerViewModel>()
            };

            foreach (var player in game.Players)
            {
                if (player.PlayerId.HasValue)
                {
                    Game.Players.Add(new GamePlayerViewModel(Game) { Player = new PlayerViewModel() { Id = player.PlayerId.Value, Name = players.First(pp => pp.Id == player.PlayerId).Name }, Rank = player.Rank });
                }
                else
                {
                    var team = teams.First(pp => pp.Id == player.TeamId);

                    Game.Players.Add(new GamePlayerViewModel(Game)
                    {
                        Player = new TeamViewModel()
                        {
                            Id = player.TeamId.Value,
                            Name = team.Name,
                            Players = new ObservableCollection<TeamPlayerViewModel>(team.Players.Select(tp => new TeamPlayerViewModel()
                            {
                                Player = new PlayerViewModel()
                                {
                                    Id = players.First(pp => pp.Id == tp.PlayerId).Id,
                                    Name = players.First(pp => pp.Id == tp.PlayerId).Name
                                },
                                Rank = tp.Rank
                            }))
                        },
                        CurrentTeamPlayerRank = player.TeamPlayerRank,
                        Rank = player.Rank
                    });
                }
            }
        }
        #endregion

        #region Command handlers

        public void AddPlayerExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(PlayerPickerPage), "CreateGame");
        }

        public void AddTeamExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(TeamPickerPage), "CreateGame");
        }

        

        public bool PlayCanExecute(object parameter)
        {
            return Game != null && Game.Players.Count > 1;
        }

        public async void PlayExecute(object parameter)
        {
            Game.CurrentPlayer = Game.Players.OrderBy(p => p.Rank).First();
            Game.Status = GameStatus.BeeingPlayed;

            await DataSourceProvider.SaveGame(Game.GetGame());

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(PlayPivotPage), Game.Id);
        }

        public bool ShufflePlayersCanExecute(object parameter)
        {
            return Game != null && Game.Players.Count > 1;
        }
        public async void ShufflePlayersExecute(object parameter)
        {
            Random r = new Random(DateTime.Now.Ticks.GetHashCode());

            var sourcePlayers = Game.Players;
            Game.Players = new ObservableCollection<GamePlayerViewModel>();

            int rank = 1;
            while(sourcePlayers.Any())
            {
                var currentPlayer = sourcePlayers.ElementAt(r.Next(sourcePlayers.Count));
                currentPlayer.Rank = rank;
                Game.Players.Add(currentPlayer);
                sourcePlayers.Remove(currentPlayer);

                rank++;
            }

            await DataSourceProvider.SaveBeeingCreatedGame(Game.GetGame());
        }
        
        #endregion

    }
}

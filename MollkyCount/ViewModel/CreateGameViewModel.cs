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
        public ICommand CreatePlayerCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand ShufflePlayersCommand { get; private set; }
        #endregion

        #region Constructor
        public CreateGameViewModel()
        {   
            AddPlayerCommand = new RelayCommand(AddPlayerExecute);
            CreatePlayerCommand = new RelayCommand(CreatePlayerExecute);
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
                Game.Players.Add(new GamePlayerViewModel() { Player = new PlayerViewModel() { Id = player.PlayerId, Name = players.First(pp => pp.Id == player.PlayerId).Name }, Rank = rank });
                rank++;
            }
        }

        public async Task LoadFromTemporaryFile()
        {
            var game = await DataSourceProvider.RetrieveBeeingCreatedGame();
            var players = await DataSourceProvider.GetPlayers();

            Game = new GameViewModel()
            {
                Id = game.Id,
                Players = new ObservableCollection<GamePlayerViewModel>(game.Players.Select(p => new GamePlayerViewModel() { Player = new PlayerViewModel() { Id = p.PlayerId, Name = players.First(pp => pp.Id == p.PlayerId).Name }, Rank = p.Rank })),
                Date = DateTime.Now,
                Status = game.Status
            };
        }
        #endregion

        #region Command handlers

        public void AddPlayerExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(PlayerPickerPage), "CreateGame");
        }

        public async void CreatePlayerExecute(object parameter)
        {
            var newPlayer = new PlayerViewModel() { Id = Guid.NewGuid() };

            var createPlayerDialog = new CreatePlayerDialog(newPlayer);
            var messageDialogResult = await createPlayerDialog.ShowAsync();

            if (messageDialogResult == ContentDialogResult.Primary)
            {
                // TODO : Save player
                Game.Players.Add(new GamePlayerViewModel(this.Game) { Player = newPlayer, Rank = Game.Players.Count + 1 });
                await DataSourceProvider.SavePlayer(newPlayer.GetPlayer());
                await DataSourceProvider.SaveBeeingCreatedGame(Game.GetGame());

                ((RelayCommand)PlayCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ShufflePlayersCommand).RaiseCanExecuteChanged();
            }
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

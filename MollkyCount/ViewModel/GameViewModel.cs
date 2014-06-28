using MollkyCount.Common;
using MollkyCount.DAL;
using MollkyCount.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MollkyCount.ViewModel
{
    public class GameViewModel : BaseViewModel
    {
        #region Properties
        public ResourceLoader CurrentResourceLoader { get; set; }

        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        private GameStatus _status;
        public GameStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged();

                CanPlay = (value == GameStatus.BeeingPlayed);
                RaisePropertyChanged("CanPlay");
            }
        }

        private ObservableCollection<GamePlayerViewModel> _players;
        public ObservableCollection<GamePlayerViewModel> Players
        {
            get { return _players; }
            set
            {
                _players = value;
                RaisePropertyChanged();
            }
        }

        private GamePlayerViewModel _currentPlayer;
        public GamePlayerViewModel CurrentPlayer
        {
            get { return _currentPlayer; }
            set
            {
                _currentPlayer = value;
                RaisePropertyChanged();
            }
        }

        private TeamPlayerViewModel _currentTeamPlayer;
        public TeamPlayerViewModel CurrentTeamPlayer
        {
            get { return _currentTeamPlayer; }
            set
            {
                _currentTeamPlayer = value;
                RaisePropertyChanged();
            }
        }
        

        private GamePlayerViewModel _selectedScorePlayer;
        public GamePlayerViewModel SelectedScorePlayer
        {
            get { return _selectedScorePlayer; }
            set
            {
                _selectedScorePlayer = value;
                RaisePropertyChanged();
            }
        }

        private bool _canPlay;
        public bool CanPlay
        {
            get { return _canPlay; }
            set
            {
                _canPlay = value;
                RaisePropertyChanged();
            }
        }

        private bool _canUndoLast;
        public bool CanUndoLast
        {
            get { return _canUndoLast; }
            set
            {
                _canUndoLast = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<GameRoundViewModel> Rounds { get; set; }

        private bool _isScoreInputOpened;
        public bool IsScoreInputOpened
        {
            get { return _isScoreInputOpened; }
            set
            {
                _isScoreInputOpened = value;
                RaisePropertyChanged();
            }
        }

        private bool _isVictoryPopupOpened;
        public bool IsVictoryPopupOpened
        {
            get { return _isVictoryPopupOpened; }
            set
            {
                _isVictoryPopupOpened = value;
                RaisePropertyChanged();
            }
        }

        public int CurrentRoundRank { get; set; }

        #endregion

        #region Commands
        public ICommand NextPlayCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public ICommand SetScoreCommand { get; private set; }

        public ICommand EndGameCommand { get; private set; }

        public ICommand StartNewGameCommand { get; private set; }

        public ICommand UndoLastCommand { get; private set; }

        public ICommand ShareCommand { get; private set; }
        #endregion

        #region Constructor
        public GameViewModel()
        {
            CurrentResourceLoader = ResourceLoader.GetForCurrentView();

            Rounds = new ObservableCollection<GameRoundViewModel>();
            CurrentRoundRank = 0;

            NextPlayCommand = new RelayCommand(NextPlayExecute, NextPlayCanExecute);
            CancelCommand = new RelayCommand(CancelExecute);
            SetScoreCommand = new RelayCommand(SetScoreExecute);
            EndGameCommand = new RelayCommand(EndGameExecute);
            StartNewGameCommand = new RelayCommand(StartNewGameExecute);
            UndoLastCommand = new RelayCommand(UndoLastExecute, UndoLastCanExecute);
            ShareCommand = new RelayCommand(ShareExecute);
        }
        #endregion

        #region Command Handlers

        public bool NextPlayCanExecute(object parameter)
        {
            return Status == GameStatus.BeeingPlayed;
        }

        public void NextPlayExecute(object parameter)
        {
            IsScoreInputOpened = true;
        }

        public async void CancelExecute(object parameter)
        {
            if (parameter as bool? == true)
            {
                this.Status = GameStatus.Canceled;
                await DataSourceProvider.SaveGame(GetGame());

                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame != null)
                    rootFrame.Navigate(typeof(PivotPage));
            }
            else
            {
                MessageDialog dialog = new MessageDialog(CurrentResourceLoader.GetString("ExitGameConfirmation"));
                dialog.Commands.Add(new UICommand(CurrentResourceLoader.GetString("Ok"), (x) =>
                {
                    this.CancelCommand.Execute(true);
                }));
                dialog.Commands.Add(new UICommand(CurrentResourceLoader.GetString("Cancel"), (x) =>
                {
                }));

                await dialog.ShowAsync();

            }
        }

        public async void SetScoreExecute(object parameter)
        {
            IsScoreInputOpened = false;

            // On incremente le rang global si on repasse au début.
            if (CurrentPlayer.Rank == Players.OrderBy(p => p.Rank).First(p => !p.IsExcluded).Rank)
                CurrentRoundRank++;

            int score = int.Parse((string)parameter);
            CurrentPlayer.TotalScore += score;

            var gameRound = new GameRoundViewModel() { Player = CurrentPlayer.Player.Id, Score = score, RoundRank = CurrentRoundRank };
            this.Rounds.Add(gameRound);
            this.CurrentPlayer.Rounds.Add(gameRound);

            var currentPlayerRounds = this.Rounds.Where(r => r.Player == CurrentPlayer.Player.Id).OrderByDescending(r => r.RoundRank);
            if (currentPlayerRounds.Count() >= 3 && currentPlayerRounds.Take(3).Sum(r => r.Score) == 0)
            {
                CurrentPlayer.IsExcluded = true;
            }

            if (CurrentPlayer.TotalScore > 50)
                CurrentPlayer.TotalScore = 25;

            gameRound.NewTotalScore = CurrentPlayer.TotalScore;

            var player = GetNextPlayer();

            if (CurrentPlayer.TotalScore == 50
                || player == null)
            {
                this.Status = GameStatus.Finished;
                IsVictoryPopupOpened = true;
            }
            else
            {
                CurrentPlayer = player;
                if (CurrentPlayer.Player is TeamViewModel)
                {
                    CurrentTeamPlayer = CurrentPlayer.GetNextTeamPlayer();
                }

                SelectedScorePlayer = CurrentPlayer;

                if (Players.Where(p => p != CurrentPlayer).All(p => p.IsExcluded))
                {
                    this.Status = GameStatus.Finished;
                    IsVictoryPopupOpened = true;
                }
            }

            await DataSourceProvider.SaveGame(GetGame());

            ((RelayCommand)UndoLastCommand).RaiseCanExecuteChanged();
        }

        public bool UndoLastCanExecute(object parameter)
        {
            CanUndoLast = Status == GameStatus.BeeingPlayed && this.Rounds.Any();
            return CanUndoLast;
        }

        public async void UndoLastExecute(object parameter)
        {
            if (this.Rounds.Any() && this.Status == GameStatus.BeeingPlayed)
            {
                var round = this.Rounds.Last();
                var player = this.Players.First(p => p.Player.Id == round.Player);

                this.Rounds.Remove(round);
                player.Rounds.Remove(round);

                CurrentPlayer = player;

                if (player.Rounds.Any())
                    CurrentPlayer.TotalScore = player.Rounds.Last().NewTotalScore;
                else
                    CurrentPlayer.TotalScore = 0;

                await DataSourceProvider.SaveGame(GetGame());

                ((RelayCommand)UndoLastCommand).RaiseCanExecuteChanged();
            }
        }

        public void EndGameExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(PivotPage));
        }

        public void StartNewGameExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(NewGamePage), Id);
        }

        public void ShareExecute(object parameter)
        {
            DataTransferManager.ShowShareUI();
        }

        #endregion

        #region Custom logic
        public void MovePlayerUp(GamePlayerViewModel player)
        {
            if (player.Rank > 1)
            {
                player.Rank--;
                Players.First(p => p != player && p.Rank == player.Rank).Rank++;

                Players = new ObservableCollection<GamePlayerViewModel>(Players.OrderBy(p => p.Rank));
            }
        }

        public void MovePlayerDown(GamePlayerViewModel player)
        {
            if (player.Rank < Players.Count)
            {
                player.Rank++;
                Players.First(p => p != player && p.Rank == player.Rank).Rank--;

                Players = new ObservableCollection<GamePlayerViewModel>(Players.OrderBy(p => p.Rank));
            }
        }

        public void RemovePlayer(GamePlayerViewModel player)
        {
            if (Players.Contains(player))
            {
                Players.Remove(player);
            }
        }

        public GamePlayerViewModel GetNextPlayer()
        {
            int rank = 1;

            if (CurrentPlayer.Rank != Players.Count)
                rank = CurrentPlayer.Rank + 1;

            int initialRank = rank;
            int cpt = 0;

            var nextPlayer = Players.First(p => p.Rank == rank);
            while (cpt != Players.Count - 1)
            {
                if (!nextPlayer.IsExcluded)
                    return nextPlayer;

                if (nextPlayer.Rank == Players.Count)
                    rank = 1;
                else
                    rank++;

                nextPlayer = Players.First(p => p.Rank == rank);

                cpt++;
            }

            return null;
        }

        public void InitializeNextPlayer()
        {
            if (Rounds.Any())
            {
                CurrentPlayer = Players.First(p => p.Player.Id == Rounds.Last().Player);
                CurrentPlayer = GetNextPlayer();
            }
            else
                CurrentPlayer = Players.First();


        }
        #endregion

        #region Navigation
        public void GoBack()
        {
            if (IsScoreInputOpened)
                IsScoreInputOpened = false;
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame != null)
                {
                    rootFrame.Navigate(typeof(PivotPage));
                }
            }
        }
        public bool CanGoBack()
        {
            return true;
        }
        #endregion

        #region Model Mapping
        public Game GetGame()
        {
            var game = new Game()
            {
                Id = Id,
                Date = Date,
                Status = Status
            };

            game.Players = Players.Select(p => new GamePlayer() { GameId = Id, IsExcluded = p.IsExcluded, PlayerId = p.Player is PlayerViewModel ? p.Player.Id : (Guid?)null, TeamId = p.Player is TeamViewModel ? p.Player.Id : (Guid?)null, TotalScore = p.TotalScore, Rank = p.Rank, TeamPlayerRank = p.CurrentTeamPlayerRank }).ToList();
            game.Rounds = Rounds.Select(p => new GameRound() { Id = p.Id, GameId = Id, Score = p.Score, NewTotalScore = p.NewTotalScore, PlayerId = p.Player, Rank = p.RoundRank }).ToList();

            return game;
        }
        #endregion

        #region Share
        public void HandleDataRequests(DataTransferManager sender, DataRequestedEventArgs args)
        {
            string localizedTitleString = CurrentResourceLoader.GetString("ShareWinTextTitle");
            string localizedString = CurrentResourceLoader.GetString("ShareWinText");
            string formattedString = string.Format(localizedString, CurrentPlayer.Player.Name, string.Join(",", Players.Where(p => p != CurrentPlayer).Select(p => p.Player.Name)));

            var request = args.Request;

            var deferral = request.GetDeferral();

            request.Data.Properties.Title = localizedTitleString;
            request.Data.Properties.Description = "Share Mölkky win dialog";
            request.Data.SetText(formattedString);

            deferral.Complete();
        }
        #endregion

        public static GameViewModel GetViewModel(Game game, IEnumerable<PlayerViewModel> allPlayers, IEnumerable<TeamViewModel> allTeams)
        {
            var gameVm = new GameViewModel() { Date = game.Date, Id = game.Id, Status = game.Status, Rounds = new ObservableCollection<GameRoundViewModel>(), Players = new ObservableCollection<GamePlayerViewModel>() };

            if (game.Rounds != null)
            {
                foreach (var round in game.Rounds)
                {
                    gameVm.Rounds.Add(new GameRoundViewModel() { Id = round.Id, NewTotalScore = round.NewTotalScore, RoundRank = round.Rank, Score = round.Score, Player = round.PlayerId });
                }
            }

            if (game.Players != null)
            {
                foreach (var player in game.Players)
                {
                    gameVm.Players.Add(new GamePlayerViewModel()
                    {
                        IsExcluded = player.IsExcluded,
                        Rank = player.Rank,
                        TotalScore = player.TotalScore,
                        Rounds = new ObservableCollection<GameRoundViewModel>(gameVm.Rounds.Where(r => r.Player == player.PlayerId)),
                        Player = player.PlayerId.HasValue ? (IPlayerViewModel)allPlayers.First(p => p.Id == player.PlayerId.Value) : allTeams.First(t => t.Id == player.TeamId.Value),
                        CurrentTeamPlayerRank = player.TeamPlayerRank
                    });
                }
            }

            if (gameVm.Status == DataModel.Enums.GameStatus.BeeingPlayed)
            {
                gameVm.InitializeNextPlayer();
            }

            return gameVm;
        }
    }
}

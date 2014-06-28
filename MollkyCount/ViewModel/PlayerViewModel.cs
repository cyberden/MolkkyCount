using MollkyCount.Common;
using MollkyCount.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MollkyCount.ViewModel
{
    public interface IPlayerViewModel
    {
        #region Properties
        Guid Id { get; set; }

        string Name {get;set;}
        #endregion
    }

    public class PlayerViewModel : BaseViewModel, IPlayerViewModel
    {
        #region Private properties
        private GameViewModel ParentGameVm { get; set; }

        #endregion

        #region Properties
        public Guid Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Model Mapping
        public Player GetPlayer()
        {
            return new Player() { Id = Id, Name = Name };
        }
        #endregion
    }

    public class GamePlayerViewModel : BaseViewModel
    {
        #region Private properties
        private GameViewModel ParentGameVm { get; set; }
        #endregion

        #region Properties
        public IPlayerViewModel Player { get; set; }

        private ObservableCollection<GameRoundViewModel> _rounds;
        public ObservableCollection<GameRoundViewModel> Rounds 
        {
            get { return _rounds ?? (_rounds = new ObservableCollection<GameRoundViewModel>()); }
            set
            {
                _rounds = value;
                RaisePropertyChanged();
            }
        }

        public int Rank { get; set; }

        public int? CurrentTeamPlayerRank { get; set; }

        private int _totalScore;
        public int TotalScore 
        {
            get { return _totalScore; }
            set
            {
                _totalScore = value;
                RaisePropertyChanged();
            }
        }

        private bool _isExcluded;
        public bool IsExcluded
        {
            get { return _isExcluded; }
            set
            {
                _isExcluded = value;
                RaisePropertyChanged();
            }
        }
        
        #endregion

        #region Commands
        public ICommand UpCommand { get; private set; }
        public ICommand DownCommand { get; private set; }
        public ICommand RemovePlayerCommand { get; private set; }
        #endregion

        #region Constructor
        public GamePlayerViewModel()
        {
            UpCommand = new RelayCommand(UpExecute);
            DownCommand = new RelayCommand(DownExecute);
            RemovePlayerCommand = new RelayCommand(RemovePlayerExecute);
        }

        public GamePlayerViewModel(GameViewModel parentVm)
            : this()
        {
            ParentGameVm = parentVm;
        }

        #endregion

        #region Commands Handlers
        public void RemovePlayerExecute(object parameter)
        {
            if (parameter is GamePlayerViewModel)
            {
                if (ParentGameVm != null)
                    ParentGameVm.RemovePlayer((GamePlayerViewModel)parameter);
            }
        }
        public void UpExecute(object parameter)
        {
            if (ParentGameVm != null)
                ParentGameVm.MovePlayerUp(this);
        }
        public void DownExecute(object parameter)
        {
            if (ParentGameVm != null)
                ParentGameVm.MovePlayerDown(this);
        }
        #endregion

        #region Custom Logic
        public override string ToString()
        {
            if (Player is TeamViewModel)
            {
                return string.Format("{0} ({1})", Player.Name, ((TeamViewModel)Player).Players.ElementAt(CurrentTeamPlayerRank.Value - 1).Player.Name);
            }
            else return Player.Name;
        }

        public TeamPlayerViewModel GetNextTeamPlayer()
        {
            if (Player is TeamViewModel && CurrentTeamPlayerRank.HasValue)
            {
                var team = (TeamViewModel)Player;
                if (CurrentTeamPlayerRank == team.Players.Count())
                    CurrentTeamPlayerRank = 1;
                else
                    CurrentTeamPlayerRank++;

                return team.Players.ElementAt(CurrentTeamPlayerRank.Value - 1);
            }

            return null;
        }
        #endregion
    }
}

using MollkyCount.Common;
using MollkyCount.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MollkyCount.ViewModel
{
    public class TeamViewModel : BaseViewModel, IPlayerViewModel
    {
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

        public ObservableCollection<TeamPlayerViewModel> Players { get; set; }
        #endregion

        #region Ctor
        public TeamViewModel()
            : base()
        {
        }

        #endregion

        #region Mapping
        public static async Task<IEnumerable<TeamViewModel>> GetTeams(IEnumerable<Team> source)
        {
            List<TeamViewModel> vms = new List<TeamViewModel>();
            foreach (var elt in source)
            {
                var players = new ObservableCollection<TeamPlayerViewModel>();

                if (elt.Players != null)
                {
                    foreach (var p in elt.Players)
                    {
                        var tpVm = new TeamPlayerViewModel();
                        await tpVm.MapAsync(p);
                        players.Add(tpVm);

                    }
                }

                vms.Add(new TeamViewModel() { Id = elt.Id, Name = elt.Name, Players = players });
            }

            return vms;

        }

        public Team GetTeam()
        {
            return new Team() { Id = this.Id, Name = this.Name, Players = Players.Select(p => new TeamPlayer() { PlayerId = p.Player.Id, Rank = p.Rank }).ToList() };
        }
        #endregion

        #region Custom logic
        public void RemovePlayer(TeamPlayerViewModel player)
        {
            if (Players.Contains(player))
            {
                var playerRank = player.Rank;
                Players.Remove(player);

                foreach(var currentPlayer in Players.Where(p => p.Rank > playerRank))
                {
                    currentPlayer.Rank--;
                }

            }
        }
        #endregion
    }

    public class TeamPlayerViewModel : BaseViewModel
    {
        #region Props
        TeamViewModel ParentTeamVm { get; set; }

        private PlayerViewModel _player;
        public PlayerViewModel Player
        {
            get { return _player; }
            set
            {
                _player = value;
                RaisePropertyChanged();
            }
        }

        private int _rank;

        public int Rank
        {
            get { return _rank; }
            set { _rank = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand RemovePlayerCommand { get; private set; }
        #endregion

        #region Ctor
        public TeamPlayerViewModel()
            : base()
        {
            RemovePlayerCommand = new RelayCommand(RemovePlayerExecute);
        }

        public TeamPlayerViewModel(TeamViewModel parentTeamVm)
            : this()
        {
            ParentTeamVm = parentTeamVm;
        }

        #endregion

        #region Command Handlers
        public void RemovePlayerExecute(object parameter)
        {
            if (parameter is TeamPlayerViewModel)
            {
                if (ParentTeamVm != null)
                    ParentTeamVm.RemovePlayer((TeamPlayerViewModel)parameter);
            }
        }
        #endregion

        #region Mapping
        public async Task MapAsync(TeamPlayer source)
        {
            var players = await DataSourceProvider.GetPlayers();

            this.Player = new PlayerViewModel() { Id = source.PlayerId, Name = players.First(p => p.Id == source.PlayerId).Name };
            this.Rank = source.Rank;
        }
        #endregion
    }
}

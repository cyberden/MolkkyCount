using MollkyCount.Common;
using MollkyCount.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MollkyCount.ViewModel
{
    public class StatsPlayerViewModel : BaseViewModel
    {
        #region Properties
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

        private int _gamesCount;
        public int GamesCount
        {
            get { return _gamesCount; }
            set
            {
                _gamesCount = value;
                RaisePropertyChanged();
            }
        }

        private int _victoryCount;
        public int VictoryCount
        {
            get { return _victoryCount; }
            set
            {
                _victoryCount = value;
                RaisePropertyChanged();
            }
        }

        private string _victoryPercent;
        public string VictoryPercent
        {
            get { return _victoryPercent; }
            set
            {
                _victoryPercent = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public StatsPlayerViewModel(Player model, IEnumerable<Game> allGames, IEnumerable<Team> allTeams)
        {
            this.Player = new PlayerViewModel() { Id = model.Id, Name = model.Name };

            var allPlayerTeams = allTeams.Where(t => t.Players.Any(tp => tp.PlayerId == model.Id)).Select(t => t.Id);
            var allPlayerGames = allGames.Where(g => g.Status == DataModel.Enums.GameStatus.Finished 
                                                        && (g.Players.Any(p => p.PlayerId == model.Id) || (g.Players.Any(p => allPlayerTeams.Any(pt => pt == p.TeamId)))));

            GamesCount = allPlayerGames.Count();

            // Victoire si score == 50 ou seul joueur non exclu.
            VictoryCount = allPlayerGames.Where(g => 
                                    (g.Rounds.Any(r => (r.PlayerId == model.Id || allPlayerTeams.Any(pt => pt == r.PlayerId)) && r.NewTotalScore == 50)) 
                                    || (g.Players.Count(p => (p.PlayerId != model.Id && !allPlayerTeams.Any(pt => pt == p.PlayerId) && !p.IsExcluded)) == 0) ).Count();

            if (GamesCount > 0)
                VictoryPercent = string.Format("{0:P2}",(double)VictoryCount / (double)GamesCount, 2);
        }
        #endregion
    }
}

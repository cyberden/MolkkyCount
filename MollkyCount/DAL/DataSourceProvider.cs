using MollkyCount.Common;
using MollkyCount.DataModel;
using MollkyCount.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MollkyCount.DAL
{
    [DataContract]
    public class Game
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public GameStatus Status { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public List<GameRound> Rounds { get; set; }

        [DataMember]
        public List<GamePlayer> Players { get; set; }
    }

    [DataContract]
    public class Player
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class Team
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<TeamPlayer> Players { get; set; }
    }

    [DataContract]
    public class TeamPlayer
    {
        [DataMember]
        public Guid PlayerId { get; set; }

        [DataMember]
        public int Rank { get; set; }
    }

    [DataContract]
    public class GamePlayer
    {
        [DataMember]
        public Guid GameId { get; set; }

        [DataMember]
        public Guid? PlayerId { get; set; }

        [DataMember]
        public Guid? TeamId { get; set; }

        [DataMember]
        public int Rank { get; set; }

        [DataMember]
        public int TotalScore { get; set; }

        [DataMember]
        public bool IsExcluded { get; set; }

        [DataMember]
        public int? TeamPlayerRank { get; set; }
    }

    [DataContract]
    public class GameRound
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public int Rank { get; set; }

        [DataMember]
        public Guid GameId { get; set; }

        [DataMember]
        public Guid PlayerId { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember]
        public int NewTotalScore { get; set; }
    }

    public class DataSourceProvider
    {
        public static string GamesFileName = "gamesV2.xml";

        public static string PlayersFileName = "playersV2.xml";

        public static string TeamsFileName = "teamsV2.xml";

        private static DataSourceProvider _dataSourceProvider = new DataSourceProvider();

        private ObservableCollection<Game> _games;

        public ObservableCollection<Game> Games
        {
            get { return _games; }
        }

        private ObservableCollection<Player> _players;
        public ObservableCollection<Player> Players
        {
            get { return _players; }
        }

        private ObservableCollection<Team> _teams;
        public ObservableCollection<Team> Teams
        {
            get { return _teams; }
        }

        #region Games
        public static async Task<IEnumerable<Game>> GetGames(bool forceReadFromFile = false)
        {
            if ((_dataSourceProvider != null && _dataSourceProvider.Games == null)
                || forceReadFromFile)
                await _dataSourceProvider.GetGameModelAsync();

            return _dataSourceProvider.Games;
        }

        public static async Task SaveGame(Game game)
        {
            var games = await GetGames();

            var previous = games.FirstOrDefault(g => g.Id == game.Id);
            if (previous != null)
            {
                _dataSourceProvider.Games.Remove(previous);
            }

            _dataSourceProvider.Games.Add(game);

            await StorageHelper.Save<ObservableCollection<Game>>(GamesFileName, _dataSourceProvider.Games);
        }

        public static async Task SaveBeeingCreatedGame(Game game)
        {
            await StorageHelper.Save<Game>("tempGame.xml", game);
        }

        public static async Task<Game> RetrieveBeeingCreatedGame()
        {
            var game = await StorageHelper.Load<Game>("tempGame.xml");
            return game;
        }

        public static async Task SaveBeeingCreatedTeam(Team team)
        {
            await StorageHelper.Save<Team>("tempTeam.xml", team);
        }

        public static async Task<Team> RetrieveBeeingCreatedTeam()
        {
            var team = await StorageHelper.Load<Team>("tempTeam.xml");
            return team;
        }

        public static async Task<Game> GetGame(Guid id)
        {
            if (_dataSourceProvider != null && _dataSourceProvider.Games == null)
                await _dataSourceProvider.GetGameModelAsync();

            return _dataSourceProvider.Games.FirstOrDefault(g => g.Id == id);
        }

        public static async Task DeleteGame(Guid id)
        {
            var previous = _dataSourceProvider.Games.FirstOrDefault(g => g.Id == id);
            if (previous != null)
            {
                _dataSourceProvider.Games.Remove(previous);
            }

            await StorageHelper.Save<ObservableCollection<Game>>(GamesFileName, _dataSourceProvider.Games);
        }
        #endregion

        #region Players
        public static async Task<IEnumerable<Player>> GetPlayers(bool forceReadFromFile = false)
        {
            if ((_dataSourceProvider != null && _dataSourceProvider.Players == null) || forceReadFromFile)
                await _dataSourceProvider.GetPlayersModelAsync();

            return _dataSourceProvider.Players;
        }

        public static async Task SavePlayer(Player player)
        {
            _dataSourceProvider.Players.Add(player);

            await StorageHelper.Save<ObservableCollection<Player>>(PlayersFileName, _dataSourceProvider.Players);
        }
        #endregion

        #region Teams
        public static async Task<IEnumerable<Team>> GetTeams(bool forceReadFromFile = false)
        {
            if ((_dataSourceProvider != null && _dataSourceProvider.Teams == null) || forceReadFromFile)
                await _dataSourceProvider.GetTeamsModelAsync();

            return _dataSourceProvider.Teams;
        }

        public static async Task SaveTeam(Team team)
        {
            _dataSourceProvider.Teams.Add(team);

            await StorageHelper.Save<ObservableCollection<Team>>(TeamsFileName, _dataSourceProvider.Teams);
        }
        #endregion

        private async Task GetGameModelAsync()
        {
            _games = await StorageHelper.Load<ObservableCollection<Game>>(GamesFileName);
        }

        public async Task GetPlayersModelAsync()
        {

            _players = await StorageHelper.Load<ObservableCollection<Player>>(PlayersFileName);
        }

        public async Task GetTeamsModelAsync()
        {
            _teams = await StorageHelper.Load<ObservableCollection<Team>>(TeamsFileName);
        }
    }
}

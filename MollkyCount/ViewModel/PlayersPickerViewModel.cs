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
        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #endregion

        #region Constructors
        public PlayersPickerViewModel()
        {
            OkCommand = new RelayCommand(OkExecute);
            CancelCommand = new RelayCommand(CancelExecute);
            
        }
        #endregion

        #region InitializeAsync
        public async Task InitializeAsync()
        {
            var source = await DataSourceProvider.GetPlayers();
            AllPlayers = new ObservableCollection<SelectableObject<PlayerViewModel>>(source.Select(p => new SelectableObject<PlayerViewModel>() { IsSelected = false, Item = new PlayerViewModel() { Id = p.Id, Name = p.Name }}));
            var allPlayersVm = source.Select(p => new PlayerViewModel() { Id = p.Id, Name = p.Name });

            var game = await DataSourceProvider.RetrieveBeeingCreatedGame();

            Game = GameViewModel.GetViewModel(await DataSourceProvider.RetrieveBeeingCreatedGame(), allPlayersVm);

            if (Game != null)
            {
                foreach(var player in AllPlayers.Where(p => Game.Players.Any(gp => gp.Player.Id == p.Item.Id)))
                    player.IsSelected = true;
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

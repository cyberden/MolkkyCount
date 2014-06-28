using MollkyCount.Common;
using MollkyCount.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MollkyCount.ViewModel
{
    public class CreateTeamViewModel : BaseViewModel
    {
        #region Properties

        private TeamViewModel _team;
        public TeamViewModel Team
        {
            get { return _team; }
            set { _team = value; RaisePropertyChanged(); }
        }

        private bool _canTeamBeValidated;
        public bool CanTeamBeValidated
        {
            get { return _canTeamBeValidated; }
            set
            {
                _canTeamBeValidated = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands
        public ICommand AddPlayerCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        #endregion

        #region Commands
        #endregion

        #region Ctor
        public CreateTeamViewModel() : base()
        {
            CanTeamBeValidated = false;

            AddPlayerCommand = new RelayCommand(AddPlayerExecute);
            OkCommand = new RelayCommand(OkExecute);
            CancelCommand = new RelayCommand(CancelExecute);
        }
        #endregion

        #region Custom logic
        public async Task LoadFromTemporaryFile()
        {
            var team = await DataSourceProvider.RetrieveBeeingCreatedTeam();
            var teams = await TeamViewModel.GetTeams(new List<Team>() { team });

            CanTeamBeValidated = team.Players.Any();

            Team = teams.First();
        }
        #endregion

        #region Command Handlers

        public async void AddPlayerExecute(object parameter)
        {
            await DataSourceProvider.SaveBeeingCreatedTeam(Team.GetTeam());

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(PlayerPickerPage), "CreateTeam");
        }

        public async void OkExecute(object parameter)
        {
            // Save team.
            await DataSourceProvider.SaveTeam(Team.GetTeam());

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

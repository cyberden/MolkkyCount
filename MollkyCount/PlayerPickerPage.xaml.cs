﻿using MollkyCount.Common;
using MollkyCount.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MollkyCount
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayerPickerPage : BasePage
    {
        private PlayersPickerViewModel defaultViewModel;

        public PlayerPickerPage() : base()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public PlayersPickerViewModel DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public override async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var parameter = e.NavigationParameter as string;

            defaultViewModel = new PlayersPickerViewModel();
            await defaultViewModel.InitializeAsync(parameter);

            this.DataContext = defaultViewModel;
        }


        #region NavigationHelper registration


        #endregion
    }
}

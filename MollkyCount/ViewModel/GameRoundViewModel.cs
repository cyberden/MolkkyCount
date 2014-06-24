using MollkyCount.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MollkyCount.ViewModel
{
    public class GameRoundViewModel : BaseViewModel
    {
        public Guid Id { get; set; }

        public int Score { get; set; }

        public int RoundRank { get; set; }

        public Guid Player { get; set; }

        public int NewTotalScore { get; set; }
    }
}

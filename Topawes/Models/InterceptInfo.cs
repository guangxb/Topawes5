using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopModel;
using TopModel.Models;
using WinFormsClient.Models;

namespace Topawes.Models
{
    public class InterceptInfo
    {
        public string spuId;
        public Statistic statistic;
        public SuplierInfo supplier;
        public TopTrade trade;

        public InterceptInfo(SuplierInfo supplier, string spuId, TopTrade trade, Statistic statistic)
        {
            this.supplier = supplier;
            this.spuId = spuId;
            this.trade = trade;
            this.statistic = statistic;
        }
    }
}

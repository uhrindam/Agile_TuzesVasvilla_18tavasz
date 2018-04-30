using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObudaiFunctions.DTO
{
    public class ExchangeRateDto
    {
        public string Symbol { get; set; }
        public double CurrentRate { get; set; }
        public string LastRefreshed { get; set; }
        public string TimeZone { get; set; }
        public Dictionary<string, double> History { get; set; }
    }
}

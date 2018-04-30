using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObudaiFunctions.DTO
{
    public class BalanceDto
    {
        public string Token { get; set; }
        public double usd { get; set; }
        public double xrp { get; set; }
        public double eth { get; set; }
        public double btc { get; set; }
    }
}

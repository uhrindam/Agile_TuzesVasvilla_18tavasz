using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace ObudaiFunctions.TradingStrategyTrigger
{
    public class Handler
    {
        public static void Run(TimerInfo timerInfo, TraceWriter log)
        {
            log.Info("TradingStrategyTrigger invoked");

            // TODO: code here
        }

    }
}
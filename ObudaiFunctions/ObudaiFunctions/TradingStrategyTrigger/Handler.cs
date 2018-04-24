using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using ObudaiFunctions.Services;

namespace ObudaiFunctions.TradingStrategyTrigger
{
    public static class Handler
    {
        [FunctionName("Handler")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info("TradingStrategyTrigger invoked");
            TradingStrategyService.Trade(log);
        }
    }
}

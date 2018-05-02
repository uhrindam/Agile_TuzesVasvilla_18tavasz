using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Xml.Serialization;
using ObudaiFunctions.DTO;
using ObudaiFunctions.Repository;

namespace ObudaiFunctions.Services
{
    class TradingStrategyService
    {
        static TraceWriter logger;

        const double allowedDifferentePrecent = 0.015;
        static Dictionary<string, string> lastTradeDate = new Dictionary<string, string>
        {
                { "BTC", "" },
                { "ETH", "" },
                { "XRP", "" }
        };

        /// <summary>
        /// ezeket az értékeket fogjuk tologatni a tradek során
        /// </summary>
        static Dictionary<string, double> exchangeRatesAverage = new Dictionary<string, double>
        {
                { "BTC", 9300 },
                { "ETH", 677.85 },
                { "XRP", 0.85 }
        };

        /// <summary>
        /// úgy vannak belőve az értékek, hogy 250 dollárból mennyit tudnánk venni
        /// </summary>
        static Dictionary<string, double> referenceExchangeValue = new Dictionary<string, double>
        {
                { "BTC", 0.025 },
                { "ETH", 0.375 },
                { "XRP", 288 }
        };

        public static void Trade(TraceWriter log)
        {
            logger = log;
            try
            {
                Dictionary<string, ExchangeRateDto> exchangeRates = apiHandler.getExchangeRateDTOs(logger);
                Strategy(exchangeRates);
            }
            catch (Exception e)
            {
                logger.Info("An Unexpected error was occurred. The error message: " + e.Message);
            }
        }

        private static double calculateQuantity(string currency, Dictionary<string, double> exchangeRate)
        {
            List<double> exchangeRates = createListFromHistoryDatas(exchangeRate);
            double reference = referenceExchangeValue[currency];
            double quantity = reference;
            double historicalCorrectionRate = 0.01 * reference; //minden megelőző 5 perc változása 1%-ban érinti a vásárolt mennyiséget

            //azért megy exchangeRates.Count-1-ig mert az i. elemet a következővel hasonlítjuk össze
            for (int i = 0; i < exchangeRates.Count - 1; i++)
            {
                quantity += (exchangeRates[i + 1] - exchangeRates[i]) * historicalCorrectionRate;
            }

            return quantity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="CurrentRate"></param>
        /// <param name="correction">Korrekció ahhoz hogy közelebb legyen az eddigi értékhez</param>
        /// <param name="exchangeRate"></param>
        private static void calculateNeweExchangeRatesAverage(string currency, double CurrentRate, double correction)
        {
            double result = (exchangeRatesAverage[currency] * correction + CurrentRate) / 2;
            double forTheLogger = exchangeRatesAverage[currency];
            exchangeRatesAverage[currency] = result;
            logger.Info("The new exchange rate average of " + currency + " currency is changed from " + forTheLogger + " to " + result);
        }

        private static List<double> createListFromHistoryDatas(Dictionary<string, double> exchangeRate)
        {
            List<double> HistoricalExchangeRatates = new List<double>();
            int i = 0;
            Random rnd = new Random();
            //random hogy hány hisztorikus adatot használunk.
            int howManyHistoricalDataIsOnTheTable = rnd.Next(5, 21);
            foreach (var item in exchangeRate)
            {
                HistoricalExchangeRatates.Add(item.Value);
                if (i == howManyHistoricalDataIsOnTheTable)
                    break;
                i++;
            }
            return HistoricalExchangeRatates;
        }

        private static bool SellStrategy(ExchangeRateDto dto, Dictionary<string, double> currencyBalances, BalanceDto balance)
        {
            if (currencyBalances[dto.Symbol] > 0 &&
                    dto.CurrentRate * 1 + allowedDifferentePrecent > exchangeRatesAverage[dto.Symbol])
            {
                double quantity = calculateQuantity(dto.Symbol, dto.History);
                double forTheLogger = quantity;
                if (quantity > currencyBalances[dto.Symbol])
                {
                    quantity = currencyBalances[dto.Symbol];
                    logger.Info("The recommended quantity for sell " + dto.Symbol + " was " + forTheLogger + ", but we didn't have enough currency. The new quantity is " + quantity
                        + " which is costs " + quantity * dto.CurrentRate + " usd.");
                }
                else
                {
                    logger.Info("The calculated quantity for sell " + dto.Symbol + " is " + quantity + ", which is costs " + quantity * dto.CurrentRate + " usd.");
                }

                logger.Info(apiHandler.sell(dto.Symbol, quantity) + " when invoked sellCurrency method with "
                    + dto.Symbol + " currency and " + quantity + " piece parameters");
                balance.usd += quantity * dto.CurrentRate;
                lastTradeDate[dto.Symbol] = dto.LastRefreshed;
                calculateNeweExchangeRatesAverage(dto.Symbol, dto.CurrentRate, 1 + allowedDifferentePrecent);
                return true;
            }
            return false;
        }

        private static bool BuyStrategy(ExchangeRateDto dto, Dictionary<string, double> currencyBalances, BalanceDto balance)
        {
            if (balance.usd > 0
                && dto.CurrentRate * 1 - allowedDifferentePrecent < exchangeRatesAverage[dto.Symbol])
            {
                double quantity = calculateQuantity(dto.Symbol, dto.History);
                double forTheLogger = quantity;
                if (quantity * dto.CurrentRate > balance.usd)
                {
                    quantity = balance.usd / dto.CurrentRate;
                    logger.Info("The recommended quantity for buy " + dto.Symbol + " was " + forTheLogger + ", but we didn't have enough money for that. The new quantity is " + quantity
                        + " which is costs " + quantity * dto.CurrentRate + " usd.");
                }
                else
                {
                    logger.Info("The calculated quantity for buy " + dto.Symbol + " is " + quantity + ", which is costs " + quantity * dto.CurrentRate + " usd.");
                }

                logger.Info(apiHandler.buy(dto.Symbol, quantity) + " when invoked buyCurrency method with "
                        + dto.Symbol + " currency and " + quantity + " quantity parameters");
                balance.usd -= quantity * dto.CurrentRate;
                lastTradeDate[dto.Symbol] = dto.LastRefreshed;
                calculateNeweExchangeRatesAverage(dto.Symbol, dto.CurrentRate, 1 - allowedDifferentePrecent);
                return true;
            }
            return false;
        }

        private static void Strategy(Dictionary<string, ExchangeRateDto> exchangeRates)
        {
            BalanceDto balance = apiHandler.getBalanceDTO(logger);
            Dictionary<string, double> currencyBalances = new Dictionary<string, double>
            {
                { "BTC", balance.btc},
                { "ETH", balance.eth},
                { "XRP", balance.xrp}
            };
            Random rnd = new Random();
            foreach (var item in exchangeRates)
            {
                ExchangeRateDto dto = item.Value;
                //random hogy előbb eladni, vagy venni próbál
                //egy körben egyszerre csak eladni vagy venni tud
                if (dto.LastRefreshed != lastTradeDate[dto.Symbol])
                {
                    if (rnd.Next(2) == 0)
                    {
                        if (!SellStrategy(dto, currencyBalances, balance))
                            BuyStrategy(dto, currencyBalances, balance);
                    }
                    else
                    {
                        if (!BuyStrategy(dto, currencyBalances, balance))
                            SellStrategy(dto, currencyBalances, balance);
                    }
                }
                else
                    logger.Info("The datas were not refreshed.");
            }
        }
    }
}

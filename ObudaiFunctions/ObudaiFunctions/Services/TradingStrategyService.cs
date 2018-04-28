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

namespace ObudaiFunctions.Services
{
    class TradingStrategyService
    {
        static TraceWriter logger;
        const string SECRET_KEY = "8422F77A-866B-4665-B069-DD69AE3D0D23";
        const string API_URL = "https://obudai-api.azurewebsites.net/api/";
        const int howManyHistoricalDataIsOnTheTable = 10;
        const double allowedDifferentePrecent = 0.015;

        static Dictionary<string, double> exchangeRatesAverage = new Dictionary<string, double>
        {
                { "BTC", 9400 },
                { "ETH", 700 },
                { "XRP", 0.86 }
        };

        static Dictionary<string, string> currencies = new Dictionary<string, string>
            {
                { "BTC", "Bitcoin" },
                { "ETH", "Ethereum" },
                { "XRP", "Ripple" }
            };

        //úgy vannak belőve az értékek, hogy 250 dollárból mennyit tudnánk venni
        static Dictionary<string, double> referenceExchangeValue = new Dictionary<string, double>
        {
                { "BTC", 0.025 },
                { "ETH", 0.375 },
                { "XRP", 288 }
        };

        public static void Trade(TraceWriter log)
        {
            logger = log;
            Dictionary<string, ExchangeRateDto> exchangeRates = getExchangeRates();
            Strategy(exchangeRates);
        }

        static async Task<string> getBalance()
        {
            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, API_URL + "account");
            request.Headers.Add("X-Access-Token", SECRET_KEY);
            HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        static async Task<string> getExchangeRate(string currency)
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, API_URL + "exchange/" + currency);
            request.Headers.Add("X-Access-Token", SECRET_KEY);
            HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        static async Task<string> sellCurrency(string currency, double amount)
        {
            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, API_URL + "account/sell");
            SellDto contentJson = new SellDto { Symbol = currency, Amount = amount };
            string serializedObject = JsonConvert.SerializeObject(contentJson);
            StringContent content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            request.Content = content;
            request.Headers.Add("X-Access-Token", SECRET_KEY);

            HttpResponseMessage response = await client.SendAsync(request);
            return response.StatusCode.ToString();
        }

        static async Task<string> buyCurrency(string currency, double amount)
        {
            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, API_URL + "account/purchase");
            BuyDto contentJson = new BuyDto { Symbol = currency, Amount = amount };
            string serializedObject = JsonConvert.SerializeObject(contentJson);
            StringContent content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            request.Content = content;
            request.Headers.Add("X-Access-Token", SECRET_KEY);

            HttpResponseMessage response = await client.SendAsync(request);
            return response.StatusCode.ToString();
        }

        static Dictionary<string, ExchangeRateDto> getExchangeRates()
        {
            Dictionary<string, ExchangeRateDto> exchangeRates = new Dictionary<string, ExchangeRateDto>();

            int i = 0;
            foreach (KeyValuePair<string, string> entry in currencies)
            {
                exchangeRates.Add(entry.Key, JsonConvert.DeserializeObject<ExchangeRateDto>(getExchangeRate(entry.Key).Result));
            }
            return exchangeRates;
        }

        static BalanceDto getBalanceDTO()
        {
            return JsonConvert.DeserializeObject<BalanceDto>(getBalance().Result);
        }

        private static double calculateQuantity(string currency, Dictionary<string, double> exchangeRate)
        {
            List<double> exchangeRates = createListFromHistoryDatas(exchangeRate);
            double reference = referenceExchangeValue[currency];
            double quantity = reference;
            double historicalCorrectionRate = 0.01 * reference; //minden megelőző 5 perc változása 1%-ban érinti a vásárolt mennyiséget

            //azért megy exchangeRates.Count-1-ig mert az i. elemet a következővel hasonlítjuk össze
            for (int i = 0; i < exchangeRates.Count-1; i++)
            {
                quantity+= (exchangeRates[i + 1] - exchangeRates[i]) * historicalCorrectionRate;
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
            foreach (var item in exchangeRate)
            {
                HistoricalExchangeRatates.Add(item.Value);
                if (i == howManyHistoricalDataIsOnTheTable)
                    break;
                i++;
            }
            return HistoricalExchangeRatates;
        }

        private static void SellStrategy(ExchangeRateDto dto, Dictionary<string, double> currencyBalances, BalanceDto balance)
        {
            if (currencyBalances[dto.Symbol] > 0 &&
                    dto.CurrentRate * 1 + allowedDifferentePrecent > exchangeRatesAverage[dto.Symbol])
            {
                double quantity = calculateQuantity(dto.Symbol, dto.History);
                double forTheLogger = quantity;
                if (quantity > currencyBalances[dto.Symbol])
                {
                    quantity = currencyBalances[dto.Symbol];
                    logger.Info("The recommended quantity for sell " + dto.Symbol +" was " + forTheLogger + ", but we didn't have enough currency. The new quantity is " + quantity
                        + " which is costs " + quantity * dto.CurrentRate + " usd.");
                }
                else
                {
                    logger.Info("The calculated quantity for sell " + dto.Symbol + " is " + quantity + ", which is costs " + quantity * dto.CurrentRate + " usd.");
                }

                logger.Info(sellCurrency(dto.Symbol, quantity).Result + " when invoked sellCurrency method with "
                    + dto.Symbol + " currency and " + quantity + " piece parameters");
                balance.usd += quantity * dto.CurrentRate;
                calculateNeweExchangeRatesAverage(dto.Symbol, dto.CurrentRate, 1 + allowedDifferentePrecent);
            }
        }

        private static void BuyStrategy(ExchangeRateDto dto, Dictionary<string, double> currencyBalances, BalanceDto balance)
        {
            if (balance.usd > 0 && dto.CurrentRate * 1 - allowedDifferentePrecent < exchangeRatesAverage[dto.Symbol])
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

                logger.Info(buyCurrency(dto.Symbol, quantity).Result + " when invoked buyCurrency method with "
                        + dto.Symbol + " currency and " + quantity + " quantity parameters");
                balance.usd -= quantity * dto.CurrentRate;
                calculateNeweExchangeRatesAverage(dto.Symbol, dto.CurrentRate, 1 - allowedDifferentePrecent);
            }
        }

        private static void Strategy(Dictionary<string, ExchangeRateDto> exchangeRates)
        {
            BalanceDto balance = getBalanceDTO();
            Dictionary<string, double> currencyBalances = new Dictionary<string, double>
            {
                { "BTC", balance.btc},
                { "ETH", balance.eth},
                { "XRP", balance.xrp}
            };

            foreach (var item in exchangeRates)
            {
                ExchangeRateDto dto = item.Value;
                SellStrategy(dto, currencyBalances, balance);
                BuyStrategy(dto, currencyBalances, balance);
            }
        }
    }
}

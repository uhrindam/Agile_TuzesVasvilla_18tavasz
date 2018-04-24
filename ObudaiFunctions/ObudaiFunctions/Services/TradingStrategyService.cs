using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using ObudaiFunctions.DTO;

namespace ObudaiFunctions.Services
{
    class TradingStrategyService
    {
        static TraceWriter logger;
        const string SECRET_KEY = "8422F77A-866B-4665-B069-DD69AE3D0D23";
        const string API_URL = "https://obudai-api.azurewebsites.net/api/";
        static Dictionary<string, ExchangeRateDto> exchangeRates = new Dictionary<string, ExchangeRateDto>();
        const int howManyHistoricalDataIsOnTheTable = 50;

        static Dictionary<string, string> currencies =
            new Dictionary<string, string>
            {
                { "BTC", "Bitcoin" },
                { "ETH", "Ethereum" },
                { "XRP", "Ripple" }
            };

        public static void Trade(TraceWriter log)
        {
            logger = log;
            exchangeRates = getExchangeRates();
            Strategy();
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

        /// <summary>
        /// Shows the direction of the exchanging rate.
        /// </summary>
        /// <returns>true if the direction is upwarding, else return false</returns>
        private static bool directionOfAnExchangeRate(string currency)
        {
            //Eznemjó----------------------------------------------------------
            double[] relevantExchangeDatas = new double[exchangeRates[currency].History.Values.Count];
            //List<double> relevantExchangeDatas = new List<double>();
            exchangeRates[currency].History.Values.CopyTo(relevantExchangeDatas, 0);

            return false;
        }

        private static void Strategy()
        {
            BalanceDto jsonBalance = getBalanceDTO();
            if(jsonBalance.usd>0)
                logger.Info(buyCurrency("ETH", 0.1).Result + " when invoked buyCurrency method");
            if(jsonBalance.eth > 0.1)
                logger.Info(sellCurrency("ETH", 0.1).Result + " when invoked sellCurrency method");
        }

    }
}

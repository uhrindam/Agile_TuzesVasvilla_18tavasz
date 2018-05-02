using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using ObudaiFunctions.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ObudaiFunctions.Repository
{
    class apiHandler
    {
        const string SECRET_KEY = "8422F77A-866B-4665-B069-DD69AE3D0D23";
        const string API_URL = "https://obudai-api.azurewebsites.net/api/";

        /// <summary>
        /// rövidítések az URL-ekhez
        /// </summary>
        static Dictionary<string, string> currencies = new Dictionary<string, string>
            {
                { "BTC", "Bitcoin" },
                { "ETH", "Ethereum" },
                { "XRP", "Ripple" }
            };

        static Dictionary<string, string> deserialization = new Dictionary<string, string>
        {
            { "balance", "" },
            { "exchangerate", "" }
        };

        static async Task<string> getBalance()
        {
            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, API_URL + "account");
            request.Headers.Add("X-Access-Token", SECRET_KEY);
            HttpResponseMessage response = await client.SendAsync(request);
            deserialization["balance"] = response.Content.Headers.ContentType.MediaType;
            return await response.Content.ReadAsStringAsync();
        }

        static async Task<string> getExchangeRate(string currency)
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, API_URL + "exchange/" + currency);
            request.Headers.Add("X-Access-Token", SECRET_KEY);
            HttpResponseMessage response = await client.SendAsync(request);
            deserialization["exchangerate"] = response.Content.Headers.ContentType.MediaType;
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

        public static string buy(string currency, double amount)
        {
            return buyCurrency(currency, amount).Result;
        }

        public static string sell(string currency, double amount)
        {
            return sellCurrency(currency, amount).Result;
        }

        public static Dictionary<string, ExchangeRateDto> getExchangeRateDTOs(TraceWriter logger)
        {
            Dictionary<string, ExchangeRateDto> exchangeRates = new Dictionary<string, ExchangeRateDto>();

            foreach (KeyValuePair<string, string> entry in currencies)
            {
                string result = getExchangeRate(entry.Key).Result;

                if (deserialization["exchangerate"] == "application/json")
                {
                    try
                    {
                        exchangeRates.Add(entry.Key, JsonConvert.DeserializeObject<ExchangeRateDto>(result));
                    }
                    catch (Exception)
                    {
                        logger.Info("An unexpected error was occured when we tried to deserialize the response.");
                    }
                }
                else
                {
                    logger.Info("The deserialization was made from XML");
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ExchangeRateDto));
                        using (TextReader reader = new StringReader(result))
                        {
                            exchangeRates.Add(entry.Key, (ExchangeRateDto)serializer.Deserialize(reader));
                        }
                    }
                    catch (Exception)
                    {
                        logger.Info("An unexpected error was occured when we tried to deserialize the response.");
                    }
                }
            }
            return exchangeRates;
        }

        public static BalanceDto getBalanceDTO(TraceWriter logger)
        {
            BalanceDto dto = null;
            string result = getBalance().Result;
            if (deserialization["balance"] == "application/json")
            {
                try
                {
                    dto = JsonConvert.DeserializeObject<BalanceDto>(result);
                }
                catch (Exception)
                {
                    logger.Info("An unexpected error was occured when we tried to deserialize the response.");
                }
            }
            else
            {
                try
                {
                    logger.Info("The deserialization was made from XML");
                    XmlSerializer serializer = new XmlSerializer(typeof(BalanceDto));
                    using (TextReader reader = new StringReader(result))
                    {
                        dto = (BalanceDto)serializer.Deserialize(reader);
                    }
                }
                catch (Exception)
                {
                    logger.Info("An unexpected error was occured when we tried to deserialize the response.");
                }
            }
            return dto;
        }
    }
}

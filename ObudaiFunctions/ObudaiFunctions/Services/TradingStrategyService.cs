using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;


namespace ObudaiFunctions.Services
{
    class TradingStrategyService
    {
        const string teamAPIName = "teamAPI.json";

        public static void Trade()
        {
            Convert();
        }

        private static void Convert()
        {
            string path = string.Empty;
            var exePath = System.Reflection.Assembly.GetEntryAssembly().Location.Split('\\');
            for (int i = 0; i < exePath.Length - 3; i++)
            {
                path += exePath[i] + '\\';
            }


            StreamReader sr = new StreamReader(path + teamAPIName);
            var c = sr.ReadToEnd();

            dynamic api = JsonConvert.DeserializeObject<dynamic>(c);
            var id = api.item;

        }
    }
}

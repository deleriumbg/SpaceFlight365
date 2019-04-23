using Microsoft.Xrm.Sdk;
using sf365;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SF365.Plugins
{
    [CrmPluginRegistration("sf365_UpdateExchangeRates",
        "none",
        StageEnum.PostOperation,
        ExecutionModeEnum.Synchronous,
        "",
        "sf365_UpdateExchangeRates",
        1000,
        IsolationModeEnum.Sandbox
     )]
    public class ExchangeRateActionPlugin : BasePlugin
    {
        public ExchangeRateActionPlugin() : base(typeof(ExchangeRateActionPlugin))
        {

        }

        protected override void ExecuteCrmPlugin(LocalPluginContext localcontext)
        {
            var execute = Task.Run(async () => await UpdateExchangRates(localcontext));
            Task.WaitAll(execute);
        }

        private async Task UpdateExchangRates(LocalPluginContext localcontext)
        {
            var currencyList = (from c in new XrmSvc(localcontext.OrganizationService).CreateQuery<TransactionCurrency>()
                                select new TransactionCurrency
                                {
                                    CurrencySymbol = c.CurrencySymbol,
                                    TransactionCurrencyId = c.TransactionCurrencyId
                                }).ToArray();

            var baseCurrency = "EUR";
            var currencyISOCodes = string.Join(",", currencyList.Select(c => c.CurrencySymbol).Where(c => c != baseCurrency).ToArray());
            
            using (HttpClient client = new HttpClient())
            {
                //Try this API https://free.currconv.com/api/v7/convert?q=USD_PHP&compact=ultra&apiKey=705271941c489685a770
                var requestUri = new Uri("https://api.fixer.io/latest?symbols=" + String.Join(",", currencyISOCodes));
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidPluginExecutionException("Exchangerate service returned " + response);
                }
               
                var json = response.Content.ReadAsStringAsync().Result;
                var setting = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };

                ExchangeRateResult result = null;
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {         
                    var serializer = new DataContractJsonSerializer(typeof(ExchangeRateResult), setting);
                    result = (ExchangeRateResult)serializer.ReadObject(ms);
                }

                foreach (var currency in currencyList.Where(c => c.CurrencySymbol != baseCurrency))
                {
                    if (result.Rates.Keys.Contains(currency.CurrencySymbol))
                    {
                        var update = new TransactionCurrency
                        {
                            TransactionCurrencyId = currency.TransactionCurrencyId,
                            ExchangeRate = (decimal?)result.Rates[currency.CurrencySymbol]
                        };
                        localcontext.OrganizationService.Update(update);
                    }
                }
            }
        }
    }
}

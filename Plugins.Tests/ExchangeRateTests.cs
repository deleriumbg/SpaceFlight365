using System;
using Microsoft.Crm.Sdk.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using sf365;
using SF365.Plugins;

namespace Plugins.Tests
{
    [TestClass]
    public class ExchangeRateTests
    {
        [TestMethod]
        [TestCategory("Unit Tests")]
        public void TestUpdateExchangeRates()
        {

            using (var pipeline = new PluginPipeline(
                "sf365_UpdateExchangeRates", FakeStages.PreOperation, new Entity("sf365_UpdateExchangeRates")))
            {
                pipeline.FakeService.ExpectExecute((OrganizationRequest request) => new RetrieveMultipleResponse()
                {
                    ["EntityCollection"] = new EntityCollection(new Entity[]
                    {
                        new TransactionCurrency()
                        {
                            TransactionCurrencyId = Guid.NewGuid(),
                            CurrencyName = "CAD",
                            CurrencySymbol ="CAD"
                        },
                        new TransactionCurrency()
                        {
                            TransactionCurrencyId=Guid.NewGuid(),
                            CurrencyName ="GBP",
                            CurrencySymbol = "GBP"
                        }
                    })
                });

                pipeline.FakeService.ExpectUpdate((Entity record) => { });
                pipeline.FakeService.ExpectUpdate((Entity record) => { });

                var plugin = new ExchangeRateActionPlugin();

                pipeline.Execute(plugin);

                pipeline.FakeService.AssertExpectedCalls();
            }
        }
    }
}

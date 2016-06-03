using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using GreenCoinWebApi.Models;
using NBitcoin;
using NBitcoin.SPV;
using Newtonsoft.Json;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace GreenCoinWebApi.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Transfer()
        {
            //Get balance
            return View();
        }

        public ActionResult CreateWallet()
        {
            var model = new CreateWalletRequest() {UserName = "negar"};
            return View(model);
        }

        public ActionResult ViewBalance()
        {
            return View();
        }

        public ActionResult AddCoin()
        {
            var model = new List<WalletView>();
            var list = WalletData.GetWalletList("Negar");

            foreach (var name in list)
            {
                var wallet = WalletData.GetWalletbyName(name);
                var result = Get<WalletSummary>("http://tapi.qbit.ninja/balances/" + wallet.PublicKey + "/summary", null).Result;
                var amount = result.Spendable.Amount.ToUnit(MoneyUnit.BTC);
                model.Add(new WalletView() {WalletName = name , Amount = amount });
            }
            return View(model);
        }


        private Task<T> Get<T>(string path, params object[] parameters)
        {
            return Send<T>(HttpMethod.Get, null, path, parameters);
        }

        private async Task<T> Send<T>(HttpMethod method, object body, string path, params object[] parameters)
        {
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage(method, path);
                if (body != null)
                {
                    message.Content = new StringContent(Serializer.ToString(body, Network.TestNet), Encoding.UTF8, "application/json");
                }
                var result = await client.SendAsync(message).ConfigureAwait(false);
                if (result.StatusCode == HttpStatusCode.NotFound)
                    return default(T);
                if (!result.IsSuccessStatusCode)
                {
                    string error = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        try
                        {
                            var errorObject = Serializer.ToObject<QBitNinjaError>(error, Network.TestNet);
                            if (errorObject.StatusCode != 0)
                                throw new QBitNinjaException(errorObject);
                        }
                        catch (JsonSerializationException)
                        {
                        }
                        catch (JsonReaderException)
                        {
                        }
                    }
                }
                result.EnsureSuccessStatusCode();
                if (typeof(T) == typeof(byte[]))
                    return (T)(object)await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                var str = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (typeof(T) == typeof(string))
                    return (T)(object)str;
                return Serializer.ToObject<T>(str, Network.TestNet);
            }
        }

    }
}

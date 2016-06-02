using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using GreenCoinWebApi.Models;
using NBitcoin;
using NBitcoin.SPV;
using Newtonsoft.Json;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace GreenCoinWebApi.Controllers
{
    [Route("api/Wallet")] 
    public class WalletController : ApiController
    {
        private readonly Network currentNetwork;
        public WalletController()
        {
            currentNetwork = Network.TestNet;
        }

        // returns TransactionID
        [HttpPost]
        [Route("Transfer")]
        public string Transfer(TransferRequest request)
        {
            var sourceInfo = WalletData.GetWalletbyName(request.SourceWallet);
            var destinationInfo = WalletData.GetWalletbyName(request.DestinationWallet);

            var bitcoinPrivateKey = new BitcoinSecret(sourceInfo.PrivateKey);

            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            //todo: should not be hard coded
            // Parse transaction id to NBitcoin.uint256 so the client can eat it
            var transactionId = uint256.Parse("4baffd798e9149355b02d57ed91aa8a50133d8e3844077f9e63fbe5a3722f18e");
            // Query the transaction
            GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;

            List<ICoin> receivedCoins = transactionResponse.ReceivedCoins;
            
            OutPoint outPointToSpend = null;
            foreach (var coin in receivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == bitcoinPrivateKey.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }

            if (outPointToSpend == null)
                throw new Exception("TxOut doesn't contain our ScriptPubKey");
            
            //"We want to spend {0}. outpoint:", outPointToSpend.N + 1
            //new transaction
            
            var transaction = new Transaction();
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });

            var secondAddress = new BitcoinPubKeyAddress(destinationInfo.PublicKey);
            
            // How much you want to TO
            var transactionAmount = new Money((request.Amount), MoneyUnit.BTC);

            var minerFee = new Money(0.0001m, MoneyUnit.BTC);

            // How much you want to spend FROM
            var txInAmount = receivedCoins[(int)outPointToSpend.N].TxOut.Value;
            Money changeBackAmount = txInAmount - transactionAmount - minerFee;

            TxOut secondTxOut = new TxOut()
            {
                Value = transactionAmount,
                ScriptPubKey = secondAddress.ScriptPubKey
            };
            TxOut changeBackTxOut = new TxOut()
            {
                Value = changeBackAmount,
                ScriptPubKey = bitcoinPrivateKey.ScriptPubKey
            };


            transaction.Outputs.Add(secondTxOut);
            transaction.Outputs.Add(changeBackTxOut);

            var message = string.Format($"transaction from {0} to {1}!", sourceInfo.User.UserName ,destinationInfo.User.UserName) ;
            var bytes = Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });


            //
            transaction.Inputs[0].ScriptSig = bitcoinPrivateKey.ScriptPubKey;
            // Then you need to give your private key for signing:
            transaction.Sign(bitcoinPrivateKey, false);

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;
            if (!broadcastResponse.Success)
            {
                throw new Exception(broadcastResponse.Error.ErrorCode + broadcastResponse.Error.Reason);
            }
            else
            {
               // You can check out the hash of the transaciton in any block explorer
                return (transaction.GetHash().ToString());
            }

        }

        [HttpPost]
        [Route("Wallet")]
        public bool CreateWallet(CreateWalletRequest request)
        {
            Key key = new Key();
            BitcoinSecret bitcoinPrivateKey = key.GetWif(currentNetwork);
            //"WIF is : bitcoinPrivateKey
            PubKey pubKey = key.PubKey;
           // address
            BitcoinPubKeyAddress address = pubKey.GetAddress(currentNetwork);
          return  WalletData.Add(request.WalletName, request.UserName , address.ToString() , bitcoinPrivateKey.ToString());
        }

        [HttpGet]
        [Route("Wallet/{walletName}/balance")]
        public decimal GetBalance(string walletName)
        {
            var wallet = WalletData.GetWalletbyName(walletName);
           var result = Get<WalletSummary>("http://tapi.qbit.ninja/balances/" + wallet.PublicKey +"/summary",null).Result;
           return result.Spendable.Amount.ToUnit(MoneyUnit.BTC);
        }

        [HttpGet]
        [Route("Wallet/{userName}")]
        public IList<String> GetWalletList(string userName)
        {
            return WalletData.GetWalletList(userName);
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
                    message.Content = new StringContent(Serializer.ToString(body, currentNetwork), Encoding.UTF8, "application/json");
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
                            var errorObject = Serializer.ToObject<QBitNinjaError>(error, currentNetwork);
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
                return Serializer.ToObject<T>(str, currentNetwork);
            }
        }


    }
}

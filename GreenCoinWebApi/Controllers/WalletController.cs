using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using GreenCoinWebApi.Models;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace GreenCoinWebApi.Controllers
{
    [Route("api/Wallet")] 
    public class WalletController : ApiController
    {
        // returns TransactionID
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

            var message = string.Format($"transaction from {0} to {1}!", sourceInfo.Person ,destinationInfo.Person) ;
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GreenCoinWebApi.Models;

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

            return "";

        }

    }
}

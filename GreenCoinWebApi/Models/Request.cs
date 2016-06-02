using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GreenCoinWebApi.Models
{
    public class TransferRequest
    {
        public string SourceWallet { get; set; }
        public string DestinationWallet { get; set; }
        public decimal Amount { get; set; }

    }

    public class CreateWalletRequest
    {
        public string UserName { get; set; }

        public string WalletName { get; set;}

    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GreenCoinWebApi.Models
{
    public class TransferRequest
    {
        public String SourceWallet { get; set; }
        public String DestinationWallet { get; set; }
        public decimal Amount { get; set; }

    }

   
}
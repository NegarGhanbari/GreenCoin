using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GreenCoinWebApi.Models
{
    public class Wallet
    {

        public int ID { get; set; }

        public string Name { get; set; }

        public string  PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public User User { get; set; }
    }

}
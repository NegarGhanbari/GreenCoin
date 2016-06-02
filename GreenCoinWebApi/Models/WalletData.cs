using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenCoinWebApi.Models
{
    public static  class WalletData
    {

        private static Dictionary<String, WalletInformation> Wallets { get; set; }
        static WalletData()
        {
            Wallets = new Dictionary<string, WalletInformation>()
            {
                {"NegarWallet" , new WalletInformation() {Person ="Negar.Ghanbari@fairfaxmedia.com.au",PublicKey = "n49AJCcJwBGQZRRf8gwhaKokV598isEgtN" , PrivateKey = "cSFG7VxzKokyCrUmwQpY6TbdVYJ4sYeujpT8w8R9LLjJPAGpwrgD" } },
                {"CynthiaWallet" , new WalletInformation() {Person ="Cynthia@fairfaxmedia.com.au",PublicKey = "n13x7n9WE91kxxTUpGrYpiQy1bBm5HJ9Pc" , PrivateKey = "cVSbkJmmbm5EiZtoopHWtDEYCZjcionWGsDvtmC3HVW4k3nzxDr8" } },
            };
        }

        public static IList<WalletInformation> GetWalletList(string key)
        {
            return Wallets.Where(w => w.Key == key).Select(x => x.Value).ToList();
        }

        public static WalletInformation GetWalletbyName(string walletName)
        {
            return Wallets.Where(w => w.Key == walletName).Select(x => x.Value).FirstOrDefault();
        }


    }

    public class WalletInformation
    {
        public string Person { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }
    }
}
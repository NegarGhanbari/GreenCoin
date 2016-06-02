using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GreenCoinWebApi.Data;

namespace GreenCoinWebApi.Models
{
    public static  class WalletData
    {

        private static Dictionary<String, WalletInformation> Wallets { get; set; }
        static WalletData()
        {
            Wallets = new Dictionary<string, WalletInformation>()
            {
                {"NegarWallet" , new WalletInformation() {UserName ="Negar.Ghanbari@fairfaxmedia.com.au",PublicKey = "n49AJCcJwBGQZRRf8gwhaKokV598isEgtN" , PrivateKey = "cSFG7VxzKokyCrUmwQpY6TbdVYJ4sYeujpT8w8R9LLjJPAGpwrgD" } },
                {"CynthiaWallet" , new WalletInformation() {UserName ="Cynthia@fairfaxmedia.com.au",PublicKey = "n13x7n9WE91kxxTUpGrYpiQy1bBm5HJ9Pc" , PrivateKey = "cVSbkJmmbm5EiZtoopHWtDEYCZjcionWGsDvtmC3HVW4k3nzxDr8" } },
            };
        }

        public static IList<string> GetWalletList(string userName)
        {
            //return Wallets.Where(w => w.Value.UserName == userName).Select(x => x.Key).ToList();
            IList<string> result;
            using (var context = new GreenCoinDbContext())
            {
                result = context.Wallets.Where(w => w.User.UserName == userName).Select(x => x.Name).ToList();
            }

            return result;
        }

        public static Wallet GetWalletbyName(string walletName)
        {
            // return Wallets.Where(w => w.Key == walletName).Select(x => x.Value).FirstOrDefault();

            Wallet result;
            using (var context = new GreenCoinDbContext())
            {
                result = context.Wallets.FirstOrDefault(w => w.Name == walletName );
            }

            return result;
        }

        public static bool Add(string walletName, string userName , string publicKey , string privateKey)
        {
            try
            {
                using (var context = new GreenCoinDbContext())
                {
                    var currentUser = context.Users.FirstOrDefault(x => x.UserName == userName);
                    context.Wallets.Add(new Wallet()
                    {
                        Name = walletName,
                        User = currentUser,
                        PublicKey = publicKey,
                        PrivateKey = privateKey
                    });

                    context.SaveChanges();
                }
               
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


    }

    public class WalletInformation
    {
        public string UserName { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }
    }
}
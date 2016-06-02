using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using GreenCoinWebApi.Models;

namespace GreenCoinWebApi.Data
{
    public class GreenCoinDbContext : DbContext 
    {

        public GreenCoinDbContext(string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer<GreenCoinDbContext>(null);
            Database.Log = System.Console.Write;

        }

        public GreenCoinDbContext()
            : this("name=GreenCoinConnection")
        {

        }

        public IDbSet<User> Users { get; set; }

        public IDbSet<Wallet> Wallets  { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            modelBuilder.Entity<User>();
            modelBuilder.Entity<Wallet>();

        }

    }
}
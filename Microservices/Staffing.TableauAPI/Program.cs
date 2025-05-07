using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Staffing.TableauAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.TableauAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
           CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        //public static void Main(string[] args)
        //{
        //    var userId = ConfigurationUtility.GetValue("TabCredentials:UserId");
        //    var password = ConfigurationUtility.GetValue("TabCredentials:Password");
        //    var key = ConfigurationUtility.GetValue("TabCredentials:Key");

        //    var encrypteduserId = Encryption.EncryptString(userId, key);
        //    Console.WriteLine(encrypteduserId);
        //    var decrypteduserId = Encryption.DecryptString(encrypteduserId, key);
        //    Console.WriteLine(decrypteduserId);

        //    var encryptedpwd = Encryption.EncryptString(password, key);
        //    Console.WriteLine(encryptedpwd);
        //    var decryptedpwd = Encryption.DecryptString(encryptedpwd, key);
        //    Console.WriteLine(decryptedpwd);

        //    Console.ReadLine();

        //    //CreateHostBuilder(args).Build().Run();
        //}
    }
}

using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using System;
using System.Net.Http;
using System.Threading;

namespace WebProxy.Tests
{
    [TestClass()]
    public class AssemblyInitializer
    {
        static IDisposable _server;

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            _server = WebApp.Start<Startup>(url: Constants.BaseUri);

            // validate that it's running
            HttpClient client = new HttpClient();

            var response = client.GetAsync(Constants.BaseUri + Constants.SwaggerPath).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);

            //while(true)
            //{
            //    Thread.Sleep(10000);
            //}
        }
    }
}

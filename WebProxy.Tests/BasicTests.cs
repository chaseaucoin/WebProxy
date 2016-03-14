using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Linq;

namespace WebProxy.Tests
{
    [TestClass]
    public class BasicTests
    {
        IValuesController proxy;

        public BasicTests()
        {
            proxy = new WebProxy<IValuesController>()
                .CreateProxy(Constants.BaseUri, Constants.SwaggerPath);
        }

        /// <summary>
        /// Determines whether this instance [can create proxy].
        /// </summary>
        [TestMethod]
        public void CanCreateProxy()
        {
            Assert.IsNotNull(proxy);
        }

        /// <summary>
        /// Determines whether this instance [can create proxy].
        /// </summary>
        [TestMethod]
        public void CanProxyGetList()
        {
            var expectedResult = new string[] { "value1", "value2" };
            
            var results = proxy.Get().ToArray();

            for( int i=0; i < expectedResult.Count(); i++)
            {
                Assert.AreEqual(results[i], expectedResult[i]);
            }
        }

        [TestMethod]
        public void CanProxyGet()
        {
            var expectedResult = "value";
            
            var result = proxy.Get(12);            
                Assert.AreEqual(result, expectedResult);            
        }

        [TestMethod]
        public void CanProxyPost()
        {
            var expectedResult = Guid.NewGuid().ToString();

            var result = proxy.Post(expectedResult);
            Assert.AreEqual(result, expectedResult);
        }
    }
}

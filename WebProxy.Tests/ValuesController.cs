using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebProxy.Tests
{
    public class ValuesController : ApiController, IValuesController
    {
        [HttpGet]
        public IEnumerable<string> GetList()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        public string BasicValue(int id)
        {
            return "value";
        }

        [HttpGet]
        public Task DoSomeStuff()
        {
            return Task.Run(() => { });
        }

        [HttpGet]
        public Task<string> GetStringAsync()
        {
            return  Task.FromResult("value");
        }

        [HttpPost]
        // POST api/values 
        public string Post([FromBody]string value)
        {
            return value;
        }

        [HttpPut]
        // PUT api/values/5 
        public string Put([FromBody]string value)
        {
            return value;
        }
        
        [HttpPost]
        public Task<string> PostStringAsync([FromBody] string value)
        {
            return Task.FromResult(value);
        }

        [HttpDelete]
        public string Delete(string value)
        {
            return value;
        }

        [HttpDelete]
        public Task<string> DeleteStringAsync([FromBody] string value)
        {
            return Task.FromResult(value);
        }

        [HttpPut]
        public Task<string> PutStringAsync([FromBody] string value)
        {
            return Task.FromResult(value);
        }
    }
}

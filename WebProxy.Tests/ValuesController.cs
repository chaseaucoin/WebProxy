﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebProxy.Tests
{
    public class ValuesController : ApiController, IValuesController
    {
        // GET api/values 
        public IEnumerable<string> GetList()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5 
        public string BasicValue(int id)
        {
            return "value";
        }

        public Task DoSomeStuff()
        {
            return Task.Run(() => { });
        }

        public Task<string> GetStringAsync()
        {
            return  Task.FromResult("value");
        }

        // POST api/values 
        public string Post([FromBody]string value)
        {
            return value;
        }

        // PUT api/values/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5 
        public void Delete(int id)
        {
        }
    }
}

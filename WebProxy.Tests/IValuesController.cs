﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebProxy.Tests
{
    public interface IValuesController
    {
        void Delete(int id);
        IEnumerable<string> GetList();
        string BasicValue(int id);
        string Post([FromBody]string value);
        void Put(int id, [FromBody] string value);

        Task<string> GetStringAsync();
    }
}
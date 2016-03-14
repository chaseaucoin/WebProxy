using System.Collections.Generic;
using System.Web.Http;

namespace WebProxy.Tests
{
    public interface IValuesController
    {
        void Delete(int id);
        IEnumerable<string> Get();
        string Get(int id);
        string Post([FromBody]string value);
        void Put(int id, [FromBody] string value);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebProxy.Tests
{
    public interface IValuesController
    {
        string Delete(string value);

        Task<string> DeleteStringAsync([FromBody] string value);

        IEnumerable<string> GetList();

        string BasicValue(int id);

        string Post([FromBody]string value);

        Task<string> PostStringAsync([FromBody]string value);

        string Put([FromBody] string value);
        
        Task<string> PutStringAsync([FromBody] string value);

        Task<string> GetStringAsync();
    }
}
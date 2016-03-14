using CupcakeFactory.SimpleProxy;
using JsonNet.GenericTypes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using WebProxy.Swagger;

namespace WebProxy
{
    /// <summary>
    /// A proxy that uses swagger to bind a service at runtime to a given contract
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebProxy<T>
    {
        ProxyDefinition _proxyDef;
        SimpleProxy<T> _proxy;
        HttpClient client;

        string _root;

        [PermissionSet(SecurityAction.LinkDemand)]
        public WebProxy()
        {
            client = new HttpClient();
            _proxy = new SimpleProxy<T>(ProxyViaSwagger);
        }

        public T CreateProxy(string root, string swaggerPath)
        {
            _root = root;

            var parser = new SwaggerParser();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new GenericTypeResolver() }
            };

            string swaggerDoc = string.Empty;

            string page = root + swaggerPath;

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(page).Result)
            using (HttpContent content = response.Content)
            {   
                swaggerDoc = content.ReadAsStringAsync().Result;                
            }

            _proxyDef = parser.ParseSwaggerDocument(swaggerDoc);

            return (T)_proxy.GetTransparentProxy();
        }

        private object ProxyViaSwagger(MethodBase methodBase, MethodParameterCollection parameterCollection)
        {
            var swaggerOperation = _proxyDef
                .Operations
                .Where(operation =>
                    operation.OperationId == methodBase.Name)
                .Where(operation =>
                    ( parameterCollection.Count() == 0 && operation.Parameters.Count == 0 )
                    ||
                    parameterCollection.Select(x => x.Name)
                        .Any(x => operation.Parameters.Select(y => y.Type.Name).Contains(x))
                )
                .FirstOrDefault();

            var methodInfo = methodBase as MethodInfo;

            var page = _root + swaggerOperation.Path;

            var serializedResponse = string.Empty;

            var queryParams = swaggerOperation
                    .Parameters
                    .Where(x => x.ParameterIn == ParameterIn.Query || x.ParameterIn == ParameterIn.Path)
                    .Select(x => parameterCollection.FirstOrDefault(arg => arg.Name == x.Type.Name))
                    .ToList();

            var bodyParam = swaggerOperation
                    .Parameters
                    .Where(x => x.ParameterIn == ParameterIn.Body)
                    .Select(x => parameterCollection.FirstOrDefault(arg => arg.Name == x.Type.Name))
                    .FirstOrDefault();            

            if (swaggerOperation.Method.ToLower() == "get")
            {
                serializedResponse = Dispatch(HttpMethod.Get, page, queryParams, null, bodyParam).Result;
            }

            if (swaggerOperation.Method.ToLower() == "post")
            {                
                serializedResponse = Dispatch(HttpMethod.Post, page, queryParams, null, bodyParam).Result;
            }

            var returnObj = JsonConvert.DeserializeObject(serializedResponse, methodInfo.ReturnType);
            
            return returnObj;
        }

        private async Task<string> Dispatch(HttpMethod method, string methodPath, IEnumerable<MethodParameter> queryParams = null, IEnumerable<MethodParameter> headers = null, MethodParameter body = null)
        {
            if (queryParams != null)
            {
                methodPath = CreatePath(methodPath, queryParams);
            }

            var requestMessage = new HttpRequestMessage(method, methodPath);

            var responseBody = string.Empty;

            if (body != null)
            {
                var serializedBody = JsonConvert.SerializeObject(body.Value);
                requestMessage.Content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
            }            

            responseBody = await SendRequestMessage(requestMessage);

            return responseBody;
        }

        private async Task<string> SendRequestMessage(HttpRequestMessage requestMessage)
        {
            var responseBody = string.Empty;

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));            

            using (HttpResponseMessage response = await client.SendAsync(requestMessage))
            using (HttpContent content = response.Content)
            {
                responseBody = await content.ReadAsStringAsync();
            }

            return responseBody;
        }

        private string CreatePath(string methodPath, IEnumerable<MethodParameter> queryParams)
        {
            var serializedQueryParams = queryParams.Select
                (param => param.Name + "=" + WebUtility.UrlEncode(param.Value.ToString()));

            methodPath = methodPath + "?" + string.Join("&", serializedQueryParams);

            return methodPath;
        }
    }
}

using CupcakeFactory.SimpleProxy;
using JsonNet.GenericTypes;
using Newtonsoft.Json;
using System;
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
                    (parameterCollection.Count() == 0 && operation.Parameters.Count == 0)
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

            HttpMethod httpmethod = null;
            
            switch (swaggerOperation.Method.ToLower())
            {
                case "post" :
                    httpmethod = HttpMethod.Post;
                    break;

                case "get" :
                    httpmethod = HttpMethod.Get;
                    break;

                default: break;
            }

            object returnObj = null;

            if (methodInfo.ReturnType.IsSubclassOf(typeof(Task)) 
                && 
                methodInfo.ReturnType.GetGenericArguments().Count() > 0)
            {
                var genericReturnType = methodInfo.ReturnType.GetGenericArguments()[0];

                object[] args = new object[] { httpmethod, page, queryParams, null, bodyParam };

                var dispatchMethod = this.GetType()
                    .GetMethod("Dispatch", BindingFlags.NonPublic | BindingFlags.Instance);

                returnObj = dispatchMethod
                    .MakeGenericMethod(genericReturnType)
                    .Invoke(this, args);
            }
            else
            {
                object[] args = new object[] { httpmethod, page, queryParams, null, bodyParam };

                var dispatchMethod = this.GetType()
                    .GetMethod("Dispatch", BindingFlags.NonPublic | BindingFlags.Instance);

                var dispatchTask = dispatchMethod
                    .MakeGenericMethod(methodInfo.ReturnType)
                    .Invoke(this, args) as Task;

                returnObj = dispatchTask.GetType()
                    .GetProperty("Result")
                    .GetValue(dispatchTask);
            }
            
            return returnObj;
        }

        private async Task<K> Dispatch<K>(HttpMethod method, string methodPath, IEnumerable<MethodParameter> queryParams = null, IEnumerable<MethodParameter> headers = null, MethodParameter body = null)
        {
            if (queryParams != null && queryParams.Count() > 0)
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

            var deserializedResponse = JsonConvert.DeserializeObject<K>(responseBody);

            return deserializedResponse;
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

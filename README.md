# WebProxy
A runtime proxy for calling into http services by leveraging swagger. Note the example uses WebAPI but so long as the objects match the objects in the swagger document you can use this against any API endpoint that has swagger documentation. 

## Example Interface
```csharp
public interface ISomeService
{
	SomeObject GetSomeObject();

	Task<SomeOtherObject> GetSomeOtherObjectAsync(SomeInput input);
}
```

## Example Controller
```csharp
public class SomeController : ApiController, ISomeService
{
	SomeObject GetSomeObject();

	Task<SomeOtherObject> GetSomeOtherObjectAsync(SomeInput input);
}
```

## Example WebApi Stratup
```csharp
public void Configuration(IAppBuilder appBuilder)
{
    // Configure Web API for self-host. 
    HttpConfiguration config = new HttpConfiguration();            

    config.EnableSwagger(c =>
    {
        c.SingleApiVersion("v1", "A title for your API");
        c.UseFullTypeNameInSchemaIds();
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.FirstOrDefault());
    })
        .EnableSwaggerUi(); //for debugging purposes

    config.Routes.MapHttpRoute(
        name: "DefaultApi",
        routeTemplate: "api/{controller}/{action}/{id}",
        defaults: new { id = RouteParameter.Optional }
    );
    appBuilder.UseWebApi(config);
}
```

## Example Client

```csharp
public async Task DoTheThing()
{
	var proxy = new WebProxy<IValuesController>()
            .CreateProxy("http://somerooturl.org/", "swagger/docs/v1");

	SomeObject someObject = proxy.GetSomeObject();
		
	SomeInput someInput = GetInput();
		
	SomeOtherObject someOtherObject = await proxy.GetSomeOtherObjectAsync(someInput);
}
```

**Project leverages code from https://github.com/BirchCommunications/Birch.Swagger.ProxyGenerator for parsing the swagger document into C#
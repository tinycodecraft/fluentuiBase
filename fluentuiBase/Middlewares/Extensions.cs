using fluentuiBase.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace fluentuiBase.Middlewares;


public static class HttpClientJsonExtensions
{
    /// <summary>
    /// Sends a GET request to the specified URI, and parses the JSON response body
    /// to create an object of the generic type.
    /// </summary>
    /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="requestUri">The URI that the request will be sent to.</param>
    /// <returns>The response parsed as an object of the generic type.</returns>
    public static async Task<T> GetJsonAsync<T>(this HttpClient httpClient, string requestUri)
    {
        var stringContent = await httpClient.GetStringAsync(requestUri);
        return JsonSerializer.Deserialize<T>(stringContent, JsonSerializerOptionsProvider.Options);
    }

    /// <summary>
    /// Sends a POST request to the specified URI, including the specified <paramref name="content"/>
    /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="requestUri">The URI that the request will be sent to.</param>
    /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
    /// <returns>The response parsed as an object of the generic type.</returns>
    public static Task PostJsonAsync(this HttpClient httpClient, string requestUri, object content)
        => httpClient.SendJsonAsync(HttpMethod.Post, requestUri, content);

    /// <summary>
    /// Sends a POST request to the specified URI, including the specified <paramref name="content"/>
    /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
    /// </summary>
    /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="requestUri">The URI that the request will be sent to.</param>
    /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
    /// <returns>The response parsed as an object of the generic type.</returns>
    public static Task<T> PostJsonAsync<T>(this HttpClient httpClient, string requestUri, object content)
        => httpClient.SendJsonAsync<T>(HttpMethod.Post, requestUri, content);

    /// <summary>
    /// Sends a PUT request to the specified URI, including the specified <paramref name="content"/>
    /// in JSON-encoded format.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="requestUri">The URI that the request will be sent to.</param>
    /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
    public static Task PutJsonAsync(this HttpClient httpClient, string requestUri, object content)
        => httpClient.SendJsonAsync(HttpMethod.Put, requestUri, content);

    /// <summary>
    /// Sends a PUT request to the specified URI, including the specified <paramref name="content"/>
    /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
    /// </summary>
    /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="requestUri">The URI that the request will be sent to.</param>
    /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
    /// <returns>The response parsed as an object of the generic type.</returns>
    public static Task<T> PutJsonAsync<T>(this HttpClient httpClient, string requestUri, object content)
        => httpClient.SendJsonAsync<T>(HttpMethod.Put, requestUri, content);

    /// <summary>
    /// Sends an HTTP request to the specified URI, including the specified <paramref name="content"/>
    /// in JSON-encoded format.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="method">The HTTP method.</param>
    /// <param name="requestUri">The URI that the request will be sent to.</param>
    /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
    public static Task SendJsonAsync(this HttpClient httpClient, HttpMethod method, string requestUri, object content)
        => httpClient.SendJsonAsync<IgnoreResponse>(method, requestUri, content);

    /// <summary>
    /// Sends an HTTP request to the specified URI, including the specified <paramref name="content"/>
    /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
    /// </summary>
    /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="method">The HTTP method.</param>
    /// <param name="requestUri">The URI that the request will be sent to.</param>
    /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
    /// <returns>The response parsed as an object of the generic type.</returns>
    public static async Task<T> SendJsonAsync<T>(this HttpClient httpClient, HttpMethod method, string requestUri, object content)
    {
        var requestJson = JsonSerializer.Serialize(content, JsonSerializerOptionsProvider.Options);
        var response = await httpClient.SendAsync(new HttpRequestMessage(method, requestUri)
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        });

        // Make sure the call was successful before we
        // attempt to process the response content
        response.EnsureSuccessStatusCode();

        if (typeof(T) == typeof(IgnoreResponse))
        {
            return default;
        }
        else
        {
            var stringContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(stringContent, JsonSerializerOptionsProvider.Options);
        }
    }

    class IgnoreResponse { }
}

internal static class JsonSerializerOptionsProvider
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
}

//let the controller to use this.RazorView<T>(model) instead of View(model)
public static class ControllerExtensions
{
    public static IResult RazorView<T>(this Controller controller) where T : IComponent
    {
        return new RazorComponentResult<T>();
    }

    public static IResult RazorView<T>(this Controller controller, IReadOnlyDictionary<string, object?> parameters) where T : IComponent
    {
        return new RazorComponentResult<T>(parameters);
    }

    public static IResult RazorView<T>(this Controller controller, object parameters) where T : IComponent
    {
        return new RazorComponentResult<T>(parameters);
    }

    public static IResult RazorView(this Controller controller, Type componentType)
    {
        return new RazorComponentResult(componentType);
    }

    public static IResult RazorView(this Controller controller, Type componentType, IReadOnlyDictionary<string, object?> parameters)
    {
        return new RazorComponentResult(componentType, parameters);
    }

    public static IResult RazorView(this Controller controller, Type componentType, object parameters)
    {
        return new RazorComponentResult(componentType, parameters);
    }
}

public static class HelperExtensions
{
    public static void SetLangCookie(this HttpContext ctx,string? lang="en-US",int year=0,int month=0,int day=1)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(year).AddMonths(month).AddDays(day),
            IsEssential = true,
        };

        ctx.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang ?? "en-US")),
            cookieOptions
        );
        
    }
}

public static class ExceptionHandlerExtensions
{
    //Simple handler
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var error = context.Features.Get<IExceptionHandlerFeature>();
                var exDetails = new ExceptionDetails((int)HttpStatusCode.InternalServerError, error?.Error.Message ?? "");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = exDetails.StatusCode;
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Application-Error", exDetails.Message);
                context.Response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");

                await context.Response.WriteAsync(exDetails.ToString());
            });
        });

        return app;
    }

    //custom handler with logging
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ApiExceptionHandlingMiddleware>();
}

public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {


        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
public static class ServiceCollectionExtensions
{




    public static IServiceCollection AddCustomLocalization(this IServiceCollection services, params string[] langs)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        //without country code "-xx" suffix => culture invariant

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture(langs[0])
            .AddSupportedCultures(langs)
                .AddSupportedUICultures(langs);

        });

        return services;
    }
}
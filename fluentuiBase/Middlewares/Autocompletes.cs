
using DocumentFormat.OpenXml.Spreadsheet;
using fluentuiBase.Store.Commands;
using fluentuiBase.Store.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fluentuiBase.Middlewares;

public enum AutocompleteGroup
{
    weathers,

}

public static class Autocompletes
{
    public static RouteGroupBuilder MapApiFor(this RouteGroupBuilder builder,AutocompleteGroup group)
    {
        switch(group)
        {
            case AutocompleteGroup.weathers:
                builder.MapGet("/all/{userid}", GetAllWeathers).Produces(200, typeof(List<WeatherForecastDto>));

                break;


        }

        return builder;
    }

    internal static async Task<IResult> GetAllWeathers(IMediator commander,ILogger<Program> logger,string userid, [FromQuery(Name = "start")]int? start = 1, [FromQuery(Name ="size")]int? size= 10, [FromQuery(Name = "total")]int? total=100)
    {
        var result = await commander.Send(new GetWeatherForecastsQuery(total ?? 100, start ?? 1, total ?? 100));

        if (result.IsError)
        {
            logger.LogDebug(result.FirstError.Description);
            return TypedResults.Ok(new List<WeatherForecastDto>());
        }

        return TypedResults.Ok( result.Value);
    }
}

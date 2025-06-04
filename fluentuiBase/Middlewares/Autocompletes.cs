
using DocumentFormat.OpenXml.Spreadsheet;
using fluentuiBase.Store.Commands;
using fluentuiBase.Store.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fluentuiBase.Middlewares;



public static class Autocompletes
{
    public static RouteGroupBuilder MapApiFor(this RouteGroupBuilder builder,CN.AutocompleteGroup group)
    {
        switch(group)
        {
            case CN.AutocompleteGroup.suggests:
                builder.MapGet("/{userid}/{search?}", GetAllSuggestions).Produces(200, typeof(KeyValuePair<string, string>[]));
                break;

            case CN.AutocompleteGroup.weathers:
                builder.MapGet("/all/{userid}", GetAllWeathers).Produces(200, typeof(List<WeatherForecastDto>));

                break;


        }

        return builder;
    }

    internal static async Task<IResult> GetAllSuggestions(IMediator commander,ILogger<Program> logger, string userid, [FromQuery(Name = "wanted")]string wanted, string? search = null)
    {
        var wantedtype = Ss.GetEnum<CN.AutoSuggestType>(wanted);
        var result = await commander.Send(new GetAutoCompleteQuery(wantedtype, userid,search));

        if(result.IsError)
        {
            logger.LogDebug(result.FirstError.Description);
            return TypedResults.Ok(new KeyValuePair<string, string>[] { });
        }

        return TypedResults.Ok(result.Value);
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

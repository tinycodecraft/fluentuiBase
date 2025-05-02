
using Microsoft.EntityFrameworkCore;

namespace fluentuiBase.Middlewares;

public enum AutocompleteGroup
{
    weathers,

}

public static class Autocompletes
{
    public static RouteGroupBuilder MapApiFor(this RouteGroupBuilder builder)
    {

        return builder;
    }


}

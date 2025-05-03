using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using fluentuiBase.Models;
using fluentuiBase.Middlewares;
using fluentuiBase.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using MediatR;
using fluentuiBase.Store.Commands;
using fluentuiBase.Store.Dtos;

using fluentuiBase.Components.Pages;
using fluentuiBase.Shared.Tools;
using AutoMapper;

namespace fluentuiBase.Controllers;

public class HomeController : Controller
{
    private readonly ISender commander;
    private readonly ILogger<HomeController> _logger;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IN.ITokenService tokener;
    private readonly IMapper mapper;

    public HomeController(ILogger<HomeController> logger,IStringLocalizerFactory stringFactory,IMediator mediator,IN.ITokenService tokenHelper,IMapper itmapper )
    {
        _logger = logger;
        //using Factory instead of Dummy type fluentuiBase.SharedResource as generic type of IStringLocalizer<>
        _stringLocalizer = stringFactory.Create(typeof(fluentuiBase.Resources.SharedResource).Name, typeof(Program).Assembly.GetName().Name!);
        commander = mediator;
        tokener = tokenHelper;
        mapper = itmapper;
    }

    public async Task<IActionResult> Index(GetUsersQuery query)
    {
        var cn = new CancellationToken();
        var user = await commander.Send( new GetUserQuery("UXKBS"),cn);
        if (user.IsError)
        {
            _logger.LogDebug(user.FirstError.Description);

        }
        var authuser = mapper.Map<AuthUserModel>(user.Value);
        var token = tokener.CreateToken(authuser);
        var resultuser = tokener.DecodeTokenToUser(token);

        var result = await commander.Send(query, cn);

        if(result.IsError)
        {
            _logger.LogDebug(result.FirstError.Description);
            return View(new List<UserDto>());
        }

        return View(result.Value);
    }


    public IActionResult Weather(int total =100)
    {

        return View(new GetWeatherForecastsQuery(total,1, 20));

    }

    public IResult Sample(bool hideSideBar = false)
    {
        return this.RazorView<Sample>(new { HideSideBar=hideSideBar });
    }




    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

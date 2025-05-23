﻿using AutoMapper;
using fluentuiBase.Store.Setup;
using fluentuiBase.Store.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fluentuiBase.Shared.ErrorOr;

namespace fluentuiBase.Store.Commands
{
    public record GetWeatherForecastsQuery(int Total, int Start = 1, int Size = 0) : IRequest<ErrorOr<List<WeatherForecastDto>>>;

    public class GetWeatherForcecastsQueryHandler: IRequestHandler<GetWeatherForecastsQuery,ErrorOr<List<WeatherForecastDto>>>
    {
        public readonly IBlazeLogDbContext context;
        public readonly IMapper mapper;

        public GetWeatherForcecastsQueryHandler(IBlazeLogDbContext ctx, IMapper mp)
        {
            context = ctx;
            mapper = mp;
        }


        public async Task<ErrorOr<List<WeatherForecastDto>>> Handle(GetWeatherForecastsQuery request, CancellationToken cancellationToken)
        {
            var rng = new Random();
            var size = request.Size == 0 ? CN.Setting.PageSize : request.Size;
            var start = request.Start == 0 ? CN.Setting.PageStart : request.Start;

            return await Task.FromResult(Enumerable.Range(start, request.Size == 0 ? 5 : request.Size).Select(index => new WeatherForecastDto
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToList());
        }
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

    }
}

using AutoMapper;
using fluentuiBase.Shared.ErrorOr;
using fluentuiBase.Shared.Tools;
using fluentuiBase.Store.Commands;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Principal;

namespace fluentuiBase.Middlewares;

public class AuthService : IN.IAuthService
{
    ISender commander;
    IN.ITokenService tokener;
    IMapper mapper;
    IHttpContextAccessor context;
    public AuthService(IMediator sender, IN.ITokenService tokenservice, IMapper itmapper, IHttpContextAccessor accessor)
    {

        commander = sender;
        tokener = tokenservice;
        mapper = itmapper;
        context = accessor;
    }
    public async Task<ErrorOr<IPrincipal>> Authenticate(string userid, string password)
    {
        var result = await commander.Send(new GetUserQuery(userid));
        if (result.IsError)
            return Error.NotFound();
        var user = result.Value;
        var hasher = new PasswordHasher();
        var hashresult = hasher.VerifyHashedPassword(user.EncPassword, password);
        if (hashresult == PasswordVerificationResult.Success)
        {
            var authuser = mapper.Map<AuthUserModel>(user);
            var token = tokener.CreateToken(authuser);

            var userClaims = new List<Claim>
                {
                new Claim(ClaimTypes.NameIdentifier, userid),

                // specify custom claims
                new Claim("token", token) };

            var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            // creating the principal object with the identity
            var principal = new ClaimsPrincipal(identity);



            return principal;

        }


        return Error.Custom(1, "AuthFail", $"{userid} could not be found or Password invalid!");


    }
}
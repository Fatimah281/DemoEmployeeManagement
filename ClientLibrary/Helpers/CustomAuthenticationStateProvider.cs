﻿using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;


namespace ClientLibrary.Helpers
{
    public class CustomAuthenticationStateProvider(LocalStorgeService localStorgeService) : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var stringToken = await localStorgeService.GetToken();
            if (string.IsNullOrEmpty(stringToken)) return await Task.FromResult(new AuthenticationState(anonymous));

            var deserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
            if (deserializeToken == null) return await Task.FromResult(new AuthenticationState(anonymous));

            var getUserClaims = DecryptToken(deserializeToken.Token!);
            if (getUserClaims == null) return await Task.FromResult(new AuthenticationState(anonymous));


            var claimsPrincipal = SetclaimsPrincipal(getUserClaims);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }

        private static ClaimsPrincipal SetclaimsPrincipal(CustomUserClaims claims)
        {
            if (claims.Email is null) return new ClaimsPrincipal();
            return new ClaimsPrincipal(new ClaimsIdentity(
              new List<Claim>
              {
                  new (ClaimTypes.NameIdentifier, claims.Id!),
                  new (ClaimTypes.Name, claims.Name!),
                  new (ClaimTypes.Email, claims.Email!),
                  new (ClaimTypes.Role, claims.Role!),

              }, "JwtAuth"));
        }

        public async Task UpdateAuthenticationState(UserSession userSession)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            if (userSession.Token != null || userSession.RefreshToken != null)
            {
                var serializeSession = Serializations.SerializObj(userSession);
                await localStorgeService.SetToken(serializeSession);
                var getUserClaims = DecryptToken(userSession.Token!);
                claimsPrincipal = SetclaimsPrincipal(getUserClaims);
            }
            else
            {
                await localStorgeService.RemoveToken();
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        private static CustomUserClaims DecryptToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken)) return new CustomUserClaims();
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);

            var userId = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier);
            var name = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Name);
            var email = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email);
            var role = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Role);
            return new CustomUserClaims(userId!.Value!, name!.Value!, email!.Value!, role!.Value!);
        }
    }
}

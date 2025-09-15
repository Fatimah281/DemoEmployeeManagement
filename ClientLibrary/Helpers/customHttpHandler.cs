using BaseLibrary.DTOs;
using ClientLibrary.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
	public class customHttpHandler(GetHttpClient getHttpClient, LocalStorgeService localStorgeService, IUserAccountService userAccountService) : DelegatingHandler
	{
		override protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			bool LoginUrl = request.RequestUri!.AbsoluteUri.Contains("login");
			bool RegisterUrl = request.RequestUri!.AbsoluteUri.Contains("register");
			bool RefreshTokenUrl = request.RequestUri!.AbsoluteUri.Contains("refreshtoken");

			if (LoginUrl || RegisterUrl || RefreshTokenUrl) return await base.SendAsync(request, cancellationToken);
			
			var result = await base.SendAsync(request, cancellationToken);
			if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
			{
				string stringToken = await localStorgeService.GetToken();
				if (stringToken == null) return result;

				string token = string.Empty;
				try
				{	
					token = request.Headers.Authorization!.Parameter!;
				}
				catch (Exception)
				{
					return result;
				}
				var deserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
				if (deserializeToken == null) return result;

				if (string.IsNullOrEmpty(token))
				{ 
					request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", deserializeToken.Token);
					return await base.SendAsync(request, cancellationToken);
				}

				var newJwtToken = await GetRefreshToken(deserializeToken.RefreshToken!);
				if (string.IsNullOrEmpty(newJwtToken)) return result;

				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newJwtToken);
				result = await base.SendAsync(request, cancellationToken);
			}

			return result;
		}

		private async Task<string> GetRefreshToken(string refreshToken)
		{
			var result = await userAccountService.RefreshTokenAsync(new RefreshToken() { Token = refreshToken });
			string serializeToken = Serializations.SerializObj(new UserSession(){
				Token = result.Token, RefreshToken = result.RefreshToken
			});
			await localStorgeService.SetToken(serializeToken);
			if (result == null) return null!;
		    return result.Token!;
		}
	}
}

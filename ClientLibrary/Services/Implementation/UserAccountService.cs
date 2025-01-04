using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Contracts;
using System.Net.Http.Json;


namespace ClientLibrary.Services.Implementation
{
    public class UserAccountService(GetHttpClient getHttpClient) : IUserAccountService
    {
        public const string AuthUrl = "api/authentication";
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            var httpClinet = getHttpClient.GetPublicHttpClient();
            var result = await httpClinet.PostAsJsonAsync($"{AuthUrl}/register", user);
            if (result.IsSuccessStatusCode) return new GeneralResponse(false, "Error Occured");
            return await result.Content.ReadFromJsonAsync<GeneralResponse>();
        }
        public async Task<LoginResponse> SignInAsync(Login user)
        {
            var httpClinet = getHttpClient.GetPublicHttpClient();
            var result = await httpClinet.PostAsJsonAsync($"{AuthUrl}/login", user);
            if (result.IsSuccessStatusCode) return new LoginResponse(false, "Error Occured");
            return await result.Content.ReadFromJsonAsync<LoginResponse>();
        }
        public Task<LoginResponse> RefreshToken(RefreshToken token)
        {
            throw new NotImplementedException();

        }

        public async Task<WeatherForecast[]> GetWeatherForecasts()
        {
            var httpClinet = await getHttpClient.GetPrivateHttpClient();
            var result = await httpClinet.GetFromJsonAsync<WeatherForecast[]>("api/weatherforecast");

            return  result!;
        }
    }
}

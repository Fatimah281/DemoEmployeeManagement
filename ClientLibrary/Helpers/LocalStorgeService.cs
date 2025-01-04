using Blazored.LocalStorage;

namespace ClientLibrary.Helpers
{
    public class LocalStorgeService(ILocalStorageService localStorgeService)
    {
        private const string storgeKey = "Authentication-token";
        public async Task<string> GetToken() => await localStorgeService.GetItemAsStringAsync(storgeKey);
        public async Task SetToken(string item) => await localStorgeService.SetItemAsStringAsync(storgeKey, item);
        public async Task RemoveToken() => await localStorgeService.RemoveItemAsync(storgeKey);
    }

 
}

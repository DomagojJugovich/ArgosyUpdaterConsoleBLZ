using System.Net.Http.Json;
using ArgosyUpdaterConsoleBLZ.Shared;

namespace ArgosyUpdaterConsoleBLZ.Client
{
    // Holds the sidebar counters; pages call RefreshAsync after changing data (e.g. delete).
    public class MachineStatsService
    {
        private readonly HttpClient _http;

        public MachineStatsService(HttpClient http)
        {
            _http = http;
        }

        public Machines.MachineStats? Stats { get; private set; }

        public event Action? Changed;

        public async Task RefreshAsync()
        {
            try
            {
                Stats = await _http.GetFromJsonAsync<Machines.MachineStats>("machines/stats");
            }
            catch (HttpRequestException)
            {
                Stats = null;
            }
            Changed?.Invoke();
        }
    }
}

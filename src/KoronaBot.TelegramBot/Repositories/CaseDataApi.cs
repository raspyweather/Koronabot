using Newtonsoft.Json;
using Refit;
using System.Linq;
using System.Threading.Tasks;

namespace KoronaBot.TelegramBot.Repositories
{
    public class CaseDataApi
    {
        public async Task<CountyEntry[]> GetEntries()
        {
            var service = RestService.For<ICaseDataApi>("https://opendata.wuerzburg.de");
            return (await service.GetAll()).Entries.Select(y=>y.Data).ToArray();
        }
    }

    public interface ICaseDataApi
    {
        [Get("/api/records/1.0/search/?dataset=rki_corona_landkreise&lang=DE&rows=500&facet=cases7_per_100k")]
        Task<ResponseObject> GetAll();
    }

    public class ResponseObject
    {
        [JsonProperty(PropertyName = "records")]

        public CountyWrapper[] Entries { get; set; }
    }
    public class CountyWrapper
    {
        [JsonProperty(PropertyName = "fields")]
        public CountyEntry Data { get; set; }
    }

    public class CountyEntry
    {
        [JsonProperty(PropertyName = "county")]

        public string CountyName { get; set; }

        [JsonProperty(PropertyName = "cases7_per_100k")]

        public double CasesPer100k { get; set; }
    }
}

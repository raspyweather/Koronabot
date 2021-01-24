using System.Threading.Tasks;

namespace KoronaBot.TelegramBot.Repositories
{
    public interface ICaseDataRepository
    {
        public Task Fetch();

        public Task<double?> Get(string countyId);

        public Task<string[]> FindCounties(string search);
    }
}
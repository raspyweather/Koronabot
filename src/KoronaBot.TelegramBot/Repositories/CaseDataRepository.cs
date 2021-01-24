using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronaBot.TelegramBot.Repositories
{
    public class CaseDataRepository : ICaseDataRepository
    {
        private bool hasData = false;
        private List<string> _counties = new List<string>();
        private Dictionary<string, double?> _caseData = new Dictionary<string, double?>();

        public async Task Fetch()
        {
            var caseApi = new CaseDataApi();
            var entries = await caseApi.GetEntries();
            this._counties = entries.Select(x => x.CountyName).Distinct().ToList();
            this._caseData = new Dictionary<string, double?>();
            entries.ToList().ForEach(x => this._caseData[x.CountyName] = x.CasesPer100k);
            this.hasData = true;
        }

        public async Task<string[]> FindCounties(string search)
        {
            if (this._counties.Count == 0)
            {
                await this.Fetch();
            }

            // first strategy: Equals
            var results1 = this._counties.FindAll(x => x.Equals(search));

            if (results1.Count == 1) { return results1.ToArray(); }

            // second strategy: Contains
            var results2 = this._counties.FindAll(x => x.Contains(search));

            return results1.Concat(results2).Take(5).ToArray();
        }

        public async Task<double?> Get(string countyId)
        {
            if (!hasData) { await this.Fetch(); }
            return _caseData.GetValueOrDefault(countyId, default);
        }
    }
}

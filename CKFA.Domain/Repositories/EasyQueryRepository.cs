using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CKFA.Domain.Enum;
using CKFA.Domain.Repositories.Interfaces;
using CKFA.Domain.Services;

namespace CKFA.Domain.Repositories
{
    public class EasyQueryRepository : IEasyQueryRepository
    {
        private EasyQueryService _service;

        public EasyQueryRepository()
        {
            _service = new EasyQueryService();
        }

        public async Task<string> EasyQuery(EDatabaseLanguage language, List<string>? fields, List<string>? fieldValues, string databaseName, string tableName)
        {
            var query = await _service.GetQuery(language, fields, fieldValues, databaseName, tableName);

            return query;
        }
    }
}

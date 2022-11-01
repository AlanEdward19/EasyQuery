using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CKFA.Domain.Enum;

namespace CKFA.Domain.Repositories.Interfaces
{
    public interface IEasyQueryRepository
    {
        public Task<string> EasyQuery(EDatabaseLanguage language, List<string>? fields, List<string>? fieldValues, string databaseName, string tableName);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CKFA.EasyQuery.ValueObjects
{
    public class BuildedQuery
    {
        public string Query { get; set; }
        public DynamicParameters Parameters { get; set; }
    }
}

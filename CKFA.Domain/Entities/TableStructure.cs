using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CKFA.Domain.Entities
{
    public record TableStructure
    {
        public string Tablename { get; set; }
        public string DatabaseName { get; set; }
        public List<string> Fields { get; set; }
    }
}

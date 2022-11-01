using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CKFA.Domain.Enum;

namespace CKFA.Domain.Entities
{
    public class Input
    {
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public List<string>? Fields { get; set; }
        public List<string>? FieldsValues { get; set; }
        public EDatabaseLanguage DatabaseLanguage { get; set; }
    }
}

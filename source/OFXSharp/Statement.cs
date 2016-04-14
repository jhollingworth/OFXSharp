using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFXSharp
{
    public class Statement
    {
        public DateTime StatementStart { get; set; }

        public DateTime StatementEnd { get; set; }

        public AccountType AccType { get; set; }

        public string Currency { get; set; }
        
        public Account Account { get; set; }

        public Balance Balance { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace OFXSharp
{
    public class OFXDocument
    {
        public DateTime StatementStart { get; set; }

        public DateTime StatementEnd { get; set; }

        public AccountType AccType { get; set; }

        public string Currency { get; set; }

        public SignOn SignOn { get; set; }

        public Account Account { get; set; }

        public Balance Balance { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}
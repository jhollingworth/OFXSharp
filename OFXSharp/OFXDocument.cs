using System;
using System.Collections.Generic;

namespace OFXSharp
{
    public class OFXDocument
    {
        public AccountType AccType { get; set; }

        public List<OFXStatement> Statements { get; set; }
    }

    public class OFXStatement
    {
        public DateTime StatementStart { get; set; }

        public DateTime StatementEnd { get; set; }

        public string Currency { get; set; }

        public SignOn SignOn { get; set; }

        public Account Account { get; set; }

        public Balance Balance { get; set; }

        public List<Transaction> Transactions { get; set; }

    }
}

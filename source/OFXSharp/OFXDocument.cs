using System;
using System.Collections.Generic;

namespace OFXSharp
{
    public class OFXDocument
    {
        public SignOn SignOn { get; set; }
        public IList<Statement> Statements { get; set; }
    }
}
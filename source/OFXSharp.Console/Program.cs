using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFXSharp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new OFXDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"F:\Sandboxes\TM\OFXSharp\source\OFXSharp.Tests\itau.ofx", FileMode.Open));
        }
    }
}

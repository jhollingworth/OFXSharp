using OFXSharp;
using System;
using System.Collections;
using System.IO;
using Sgml;
using System.Xml;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            OFXDocumentParser doc = new OFXDocumentParser();

            OFXDocument OfxDocument = new OFXDocument();

            //doc.Import(new FileStream("D:\\Barclays.qbo", FileMode.Open));
            OfxDocument = doc.Import(new FileStream("D:\\Amex.qfx", FileMode.Open));
            Console.ReadLine();
        }


    }
}

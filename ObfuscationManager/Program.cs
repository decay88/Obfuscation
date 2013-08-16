﻿using CommonModule;
using ExchangeFormat;
using System;
using System.Xml;


namespace ObfuscationManager
{
    class Manager
    {
        protected static void Example()
        {
            // Getting input from InputProvider
            XmlDocument doc = new XmlDocument();
            InputProvider ip = new InputProvider();
            doc = ip.Read(InputType.PseudoCode, PlatformType.x86);

            // Validating XML by Schema
            try
            {
                Console.WriteLine("\nValidating XML . . .");
                Internal.Validator.ValidateXML(doc);
            }
            catch (Obfuscator.ValidatorException exc)
            {
                Console.WriteLine(exc.Message);
                if (exc.InnerException != null)
                    throw exc.InnerException;
                else throw exc;
            }
            Console.WriteLine("XML document is well-formed and complies with XSD.");
            
            // Converting XML to Exchange
            Exchange exch = Exchange.LoadFromString(doc.InnerXml);

            // For debugging
            exch.SaveToFile("Exchange1.xml", true);

            // Sending Exchange format to obfuscator
            Obfuscator.ILObfuscator.Obfuscate(exch);


            


            //   ...
            //   doc.SaveToFile("Exchange1.xml", true);
            //
            // Example code to load and save a structure:
            //   Exchange.Exchange2 doc = Exchange.Exchange2.LoadFromFile("Exchange1.xml");
            //   Exchange.VariableType root = doc.Variable.First;
            //   ...
            //   doc.SaveToFile("Exchange1.xml", true);

            
        }



        public static System.Collections.Generic.List<int> TestFunc(System.Collections.Generic.List<int> param)
        {
            int zz = param[1];
            return param;
        }




        [STAThread]
        static int Main(string[] args)
        {
            //System.Collections.Generic.List<int> a = new System.Collections.Generic.List<int>();
            //System.Collections.Generic.List<int> b = new System.Collections.Generic.List<int>();
            //a.Add(1); a.Add(2); a.Add(3);

            //b = TestFunc(DeepClone(a) as System.Collections.Generic.List<int>);
            //b = TestFunc(a);
            
            //b.Add(88);
            //a.Add(101);
            
            try
            {
                Example();
                Console.WriteLine("Program has finished successfully.");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception data:");
                Console.WriteLine(e);
                return 1;
            }

        }



    }
}

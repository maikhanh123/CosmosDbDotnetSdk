using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoDotnetSdk
{
    class Program
    {
        private static IDictionary<string, Func<Task>> DemoMethods;
        static void Main(string[] args)
        {
            DemoMethods = new Dictionary<string, Func<Task>>();
            DemoMethods.Add("DB", DatabasesDemo.Run);
            DemoMethods.Add("CO", CollectionsDemo.Run);

            Task.Run(async () =>
           {
               ShowMenu();
               var check = true;
               while (check)
               {
                   Console.WriteLine("Selection: ");
                   var input = Console.ReadLine();
                   var demoId = input.ToUpper().Trim();
                   switch (demoId)
                   {
                       case "DB":
                           var demoMethod = DemoMethods[demoId];
                           await RunDemo(demoMethod);
                           break;
                       case "Q":
                           check = false;
                           break;
                       default:
                           Console.WriteLine($"?{input}");
                           break;
                   }



                   //if (demoId == "Q")
                   //{
                   //    break;
                   //}
                   //else
                   //{
                   //    Console.WriteLine($"?{input}");
                   //}
               }

           }).Wait();

        }

        private async static Task RunDemo(Func<Task> demoMethod)
        {
            try
            {
                await demoMethod();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                while(ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    message += Environment.NewLine + ex.Message;
                }
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.Write("Done. Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            ShowMenu();
        }

        private static void ShowMenu()
        {
            Console.WriteLine(@"Cosmos DB SQL API .NET SDK demos

DB Databases
CO Collections
DO Documents
IX Indexing
UP Users & Permissions

C  Cleanup

Q  Quit
");
        }



    }
}

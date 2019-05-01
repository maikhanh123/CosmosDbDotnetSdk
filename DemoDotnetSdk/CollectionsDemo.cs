using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DemoDotnetSdk
{
    public class CollectionsDemo
    {
        public static Uri DatabaseUri => UriFactory.CreateDatabaseUri("FamiliesDb");

        public async static Task Run()
        {
            Debugger.Break();

            var endpoint = ConfigurationManager.AppSettings["CosmosDbEndpoint"];
            var masterKey = ConfigurationManager.AppSettings["CosmosDbMasterKey"];

            using (var client = new DocumentClient(new Uri(endpoint), masterKey))
            {
                ViewCollections(client);


            }

        }

        private static void ViewCollections(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> View Collections in database <<<");
            var collections = client.CreateDocumentCollectionQuery(DatabaseUri).ToList();

            var i = 0;
            foreach (var collection in collections)
            {
                i++;
                Console.WriteLine();
                Console.WriteLine($"Collection #{i}");
                ViewCollection(collection);
            }
        }

        private static void ViewCollection(DocumentCollection collection)
        {
            Console.WriteLine($"Collection ID: {collection.Id}");
            Console.WriteLine($"Resource ID: {collection.ResourceId}");
            Console.WriteLine($"Self Link: {collection.SelfLink}");
            Console.WriteLine($"E-tag: {collection.ETag}");
            Console.WriteLine($"Timestamp: {collection.Timestamp}");
        }
    }
}

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

                //Create collection
                var checkCreate = true;
                var collections = client.CreateDocumentCollectionQuery(DatabaseUri).ToList();
                var collectionIdCreate = "MyCollection1";
                foreach (var item in collections)
                {
                    if(item.Id == collectionIdCreate)
                    {
                        checkCreate = false;
                    }
                }

                if (checkCreate)
                {
                    await CreateCollection(client, collectionIdCreate);
                }

                //Delete collection
                if (!checkCreate)
                {
                    await DeleteCollection(client, collectionIdCreate);
                }
                
                

            }

        }

        private async static Task DeleteCollection(DocumentClient client, string collectionId)
        {
            Console.WriteLine();
            Console.WriteLine($">>> Delete Collection {collectionId} in database <<<");
            var collectionUri = UriFactory.CreateDocumentCollectionUri("FamiliesDb", collectionId);
            await client.DeleteDocumentCollectionAsync(collectionUri);

            Console.WriteLine($"Deleted collection {collectionId} from database name FamiliesDb");

        }

        private async static Task CreateCollection(DocumentClient client, string collectionId, int reservedRUs = 1000, string partitionKey = "/partitionKey")
        {
            Console.WriteLine();
            Console.WriteLine($">>> Create Collection {collectionId} in database <<<");
            Console.WriteLine();
            Console.WriteLine($" Throughput: {reservedRUs} RU/sec");
            Console.WriteLine($" Partition Key: {partitionKey}");

            var partitionKeyDefinition = new PartitionKeyDefinition();
            partitionKeyDefinition.Paths.Add(partitionKey);

            var collectionDefinition = new DocumentCollection
            {
                Id = collectionId,
                PartitionKey = partitionKeyDefinition
            };

            var options = new RequestOptions { OfferThroughput = reservedRUs };

            var result = await client.CreateDocumentCollectionAsync(DatabaseUri, collectionDefinition, options);
            var collection = result.Resource;

            Console.WriteLine("Created new collection");
            ViewCollection(collection);
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

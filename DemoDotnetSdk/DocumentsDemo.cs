using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DemoDotnetSdk
{
    public class DocumentsDemo
    {
        public static Uri MyStoreCollectionUri => UriFactory.CreateDocumentCollectionUri("FamiliesDb", "FamiliesColection");

        public async static Task Run()
        {
            Debugger.Break();
            var endpoint = ConfigurationManager.AppSettings["CosmosDbEndpoint"];
            var masterKey = ConfigurationManager.AppSettings["CosmosDbMasterKey"];

            using (var client = new DocumentClient(new Uri(endpoint), masterKey))
            {
                //await CreateDocuments(client);

                //Query document with SQL
                //QueryDocumentsWithSql(client);

                //Query document with paging
                //await QueryDocumentsWithPagingAsync(client);

                //Query document with linq
                //QueryDocumentsWithLinq(client);

                await ReplaceDocuments(client);

                await DeleteDocuments(client);
            }

        }

        private async static Task DeleteDocuments(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> Delete Documents <<<");
            Console.WriteLine();

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true };

            Console.WriteLine("Querying for documents to be deleted");
            var sql = "SELECT c._self, c.address.zipCode FROM c WHERE STARTSWITH(c.name, 'New Customer 1') = true";
            var documentKeys = client.CreateDocumentQuery(MyStoreCollectionUri, sql, feedOptions).ToList();

            Console.WriteLine($"Found {documentKeys.Count} documents to be deleted");
            foreach (var documentKey in documentKeys)
            {
                var requestOptions = new RequestOptions
                                                { PartitionKey = new PartitionKey(documentKey.zipCode) };
                await client.DeleteDocumentAsync(documentKey._self, requestOptions);
            }

            Console.WriteLine($"Deleted {documentKeys.Count} new customer documents");
            Console.WriteLine();
        }

        private async static Task ReplaceDocuments(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> Replace Documents <<<");
            Console.WriteLine();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            Console.WriteLine("Querying for documents with 'name' flag");
            var sql = "SELECT VALUE COUNT(c) FROM c WHERE c.name = 'new Customer 1'";
            var count = client.CreateDocumentQuery(MyStoreCollectionUri, sql, options).AsEnumerable().First();
            Console.WriteLine($"Documents with 'name' flag: {count}");
            Console.WriteLine();

            Console.WriteLine("Querying for documents to be updated");
            sql = "SELECT * FROM c WHERE STARTSWITH(c.name, 'new Customer') = true";
            var documents = client.CreateDocumentQuery(MyStoreCollectionUri, sql, options).ToList();
            Console.WriteLine($"Found {documents.Count} documents to be updated");
            foreach (var document in documents)
            {
                document.name = "New Customer 1";
                var result = await client.ReplaceDocumentAsync(document._self, document);
                var updatedDocument = result.Resource;
                Console.WriteLine($"Updated document 'name' flag: {updatedDocument.name}");
            }
            Console.WriteLine();

        }

        private static void QueryDocumentsWithLinq(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> Query Documents (LINQ) <<<");
            Console.WriteLine();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            Console.WriteLine("Querying for UK customers (LINQ)");
            var q =
                from d in client.CreateDocumentQuery<Customer>(MyStoreCollectionUri, options)
                where d.Address.CountryRegionName == "United States"
                select new
                {
                    Id = d.Id,
                    Name = d.Name,
                    City = d.Address.Location.City
                };

            var documents = q.ToList();

            Console.WriteLine($"Found {documents.Count} UK customers");
        }

        private static async Task QueryDocumentsWithPagingAsync(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> Query Documents (paged results) <<<");
            Console.WriteLine();

            Console.WriteLine("Querying for all documents");
            var sql = "SELECT * FROM c";
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            var query = client
                .CreateDocumentQuery(MyStoreCollectionUri, sql, options)
                .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                var documents = await query.ExecuteNextAsync();
                foreach (var document in documents)
                {
                    Console.WriteLine($" Id: {document.id}; Name: {document.name};");
                }
            }
            Console.WriteLine();

        }

        private static void QueryDocumentsWithSql(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> Query Documents (SQL) <<<");
            Console.WriteLine();

            Console.WriteLine("Querying for new customer documents (SQL)");
            var sql = "SELECT * FROM c WHERE STARTSWITH(c.name, 'New Customer') = true";
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            // Query for dynamic objects
            var documents = client.CreateDocumentQuery(MyStoreCollectionUri, sql, options).ToList();
            Console.WriteLine($"Found {documents.Count} new documents");
            foreach (var document in documents)
            {
                Console.WriteLine($" Id: {document.id}; Name: {document.name};");

                // Dynamic object can be converted into a defined type...
                var customer = JsonConvert.DeserializeObject<Customer>(document.ToString());
                Console.WriteLine($" City: {customer.Address.Location.City}");
            }
            Console.WriteLine();

            // Or query for defined types; e.g., Customer
            var customers = client.CreateDocumentQuery<Customer>(MyStoreCollectionUri, sql, options).ToList();
            Console.WriteLine($"Found {customers.Count} new customers");
            foreach (var customer in customers)
            {
                Console.WriteLine($" Id: {customer.Id}; Name: {customer.Name};");
                Console.WriteLine($" City: {customer.Address.Location.City}");
            }
            Console.WriteLine();

            Console.WriteLine("Querying for all documents (SQL)");
            sql = "SELECT * FROM c";
            documents = client.CreateDocumentQuery(MyStoreCollectionUri, sql, options).ToList();

            Console.WriteLine($"Found {documents.Count} documents");
            foreach (var document in documents)
            {
                Console.WriteLine($" Id: {document.id}; Name: {document.name};");
            }
            Console.WriteLine();

        }

        private async static Task CreateDocuments(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> Create Documents <<<");
            Console.WriteLine();

            //Create document object
            dynamic document1DefinitionDynamic = new
            {
                name = "new Customer 1",
                address = new
                {
                    addressType = "Main Office",
                    addressLine = "123 Main Street",
                    location = new
                    {
                        city = "Brooklyn",
                        stateProvinceName = "New York"
                    },
                    postalCode = "11229",
                    countryRegionName = "United States",
                    zipCode = "999"
                },
            };

            Document document1 = await CreateDocument(client, document1DefinitionDynamic);
            Console.WriteLine($"Created document {document1.Id} from dynamic object");
            Console.WriteLine();

            //Create document json string
            var document2DefinitionJson = @"
                {
                    ""name"":""New Customer2"",
                    ""address"": 
                        {
					        ""addressType"": ""Main Office"",
					        ""addressLine1"": ""123 Main Street"",
					        ""location"": 
                                {
						            ""city"": ""Brooklyn"",
						            ""stateProvinceName"": ""New York""
					            },
					        ""postalCode"": ""11229"",
					        ""countryRegionName"": ""United States"",
                            ""zipCode"":""998""
				        }
                }";
            var document2Object = JsonConvert.DeserializeObject(document2DefinitionJson);
            Document document2 = await CreateDocument(client, document2Object);
            Console.WriteLine($"Create document {document2.Id} from JSON string");
            Console.WriteLine();

            //Create document with class POCO
            var document3DefinitionPoco = new Customer
            {
                Name = "New Customer 3",
                Address = new Address
                {
                    AddressType = "Main Office",
                    AddressLine1 = "123 Main Street",
                    Location = new Location
                    {
                        City = "Brooklyn",
                        StateProvinceName = "New York"
                    },
                    PostalCode = "11229",
                    CountryRegionName = "United States",
                    ZipCode = "997"
                },
            };

            var document3 = await CreateDocument(client, document3DefinitionPoco);
            Console.WriteLine($"Create document {document3.Id} from type object");
            Console.WriteLine();
        }

        private async static Task<Document> CreateDocument(DocumentClient client, object documentObject)
        {
            var result = await client.CreateDocumentAsync(MyStoreCollectionUri, documentObject);
            var document = result.Resource;
            Console.WriteLine($"Created new document: {document.Id}");
            Console.WriteLine(document);

            return result;
        }
    }
}

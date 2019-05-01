using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
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
                await CreateDocuments(client);
            }

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

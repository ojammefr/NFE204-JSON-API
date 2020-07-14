using System;
using Newtonsoft.Json;
using System.Collections.Generic;
//using System.Net;
using System.IO;
using System.Net.Http;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;

namespace Test_newtonsoft
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = new DocumentStore
            {
                Urls = new[] { "http://localhost:8081" },
                Database = "Objets_Trouves"
            };
            store.Initialize();

            HttpClient client = new HttpClient();
            String url = "https://data.sncf.com/api/records/1.0/search/?dataset=objets-trouves-gares&rows=10000&facet=date&facet=gc_obo_gare_origine_r_name&facet=gc_obo_type_c";
            int i = 0;

            using (BulkInsertOperation bulkInsert = store.BulkInsert())
            using (Stream s = client.GetStreamAsync(url).Result)
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();

                // read the json from a stream
                RootObject p = serializer.Deserialize<RootObject>(reader);
                List<Record> rec = p.records;
                foreach (var repo in rec)
                {
                    var recdb = new RecDB
                    {

                        datasetid = repo.datasetid,
                        recordid = repo.recordid,
                        record_timestamp = repo.record_timestamp,
                        gc_obo_gare_origine_r_code_uic_c = repo.fields.gc_obo_gare_origine_r_code_uic_c,
                        gc_obo_nature_c = repo.fields.gc_obo_nature_c,
                        gc_obo_gare_origine_r_name = repo.fields.gc_obo_gare_origine_r_name,
                        date = repo.fields.date,
                        gc_obo_nom_recordtype_sc_c = repo.fields.gc_obo_nom_recordtype_sc_c,
                        gc_obo_type_c = repo.fields.gc_obo_type_c

                    };
                    bulkInsert.Store(recdb);
                    i++;
                    Console.WriteLine("i="+i);
                }
                Console.WriteLine("FIN");
            }

        }
    }
}

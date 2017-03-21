using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace CodeAnalyze.Business
{
    public struct RemoteFile
    {
        public RemoteFile(string sha, string name, string content)
        {
            SHA = sha;
            Name = name;
            Content = content;
        }

        public string SHA { get; private set; }
        public string Name { get; private set; }

        public string Content { get; private set; }

    }

    public class RemoteFileReader
    {
        public static async Task<List<RemoteFile>> ReadAll(dynamic payload, CancellationToken cancellationToken)
        {
            var result = new List<RemoteFile>();
            var filesLink = (payload.pull_request.url + "/files").ToString();
            using (var ht = new HttpClient())
            {
                ht.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                ht.DefaultRequestHeaders.Add("User-Agent", "Teknolot");

                HttpResponseMessage filesResponse = await ht.GetAsync(filesLink, cancellationToken);
                var filesJson = await filesResponse.Content.ReadAsStringAsync();
                var files = JsonConvert.DeserializeObject<dynamic>(filesJson);

                foreach (var file in files)
                {
                    if (!((string)file.filename).EndsWith(".cs")) { continue; }

                    HttpResponseMessage contentResponse = await ht.GetAsync(file.raw_url.ToString(), cancellationToken);
                    var content = await contentResponse.Content.ReadAsStringAsync();
                    result.Add(new RemoteFile(file.sha.ToString(), file.filename.ToString(), content));
                }
            }
            return result;
        }
    }
}

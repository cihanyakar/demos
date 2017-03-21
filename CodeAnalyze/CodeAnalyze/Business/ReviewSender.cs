using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalyze.Extensions;
using Newtonsoft.Json;

namespace CodeAnalyze.Business
{
    public static class ReviewSender
    {
        
        public static async Task<bool> Send(IList<ErrorMessage> errors, string url, CancellationToken cancellationToken)
        {
            using (var ht = new HttpClient())
            {
              
                ht.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.black-cat-preview+json"));
                ht.DefaultRequestHeaders.Add("User-Agent", "Teknolot");
                var r = JsonConvert.SerializeObject(new
                {
                    body = "Kod hatalarını düzeltin " +  string.Join(Environment.NewLine,errors.Select(e => $"=> '{e.File.Name}'  satır:{e.LineNumber}, {e.Message}; "))   ,
                    @event = "REQUEST_CHANGES",
                    comments = errors.Select(e => new
                    {
                        path = e.File.Name,
                        position = e.LineNumber,
                        body = e.Where + "->" + e.Message
                    })
                });

                var response = await ht.PostAsync(url + "/reviews".AddToken(), new StringContent(r, System.Text.Encoding.UTF8, "application/json"), cancellationToken);
                return response.IsSuccessStatusCode;
            }
        }
    }
}

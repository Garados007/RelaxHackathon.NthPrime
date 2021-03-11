using System.Threading.Tasks;
using MaxLib.WebServer;
using MaxLib.WebServer.Post;
using System.Text;

namespace NthPrime
{
    public class PrimeService : WebService
    {
        public PrimeService() 
            : base(ServerStage.CreateDocument)
        {
        }

        public override bool CanWorkWith(WebProgressTask task)
        {
            return task.Request.Location.IsUrl(new[] { "n-th-prime" });
        }

        private ulong? GetN(HttpPost post)
        {
            switch (post.Data)
            {
                case MultipartFormData formData:
                    foreach (var entry in formData.Entries)
                    {
                        if (entry is not MultipartFormData.FormData data || data.Name != "n")
                            continue;
                        var @string = Encoding.UTF8.GetString(data.Content.Span);
                        return ulong.TryParse(@string, out ulong value) ? value : null;
                    }
                    break;
                case UrlEncodedData urlData:
                {
                    return urlData.Parameter.TryGetValue("n", out string? @string) &&
                        ulong.TryParse(@string, out ulong value) ? value : null;
                }
                case UnknownPostData postData:
                {
                    var @string = Encoding.UTF8.GetString(postData.Data.Span);
                    return ulong.TryParse(@string, out ulong value) ? value : null;
                }
            }
            return null;
        }

        private async Task<string> CalcN(ulong? n)
        {
            if (n == null)
                return "\"invalid input\"";
            var prime = await Solver.GetNthPrimeAsync(n.Value).ConfigureAwait(false);
            return prime == null ? "\"error\"" : prime.Value.ToString();
        }

        public override async Task ProgressTask(WebProgressTask task)
        {
            var n = task.Request.ProtocolMethod switch
            {
                "POST" => GetN(task.Request.Post),
                "GET" => task.Request.Location.GetParameter.TryGetValue("n", out string? svalue) &&
                    ulong.TryParse(svalue, out ulong value) ? value : null,
                _ => null,
            };
            var result = await CalcN(n).ConfigureAwait(false);

            task.Document.DataSources.Add(new HttpStringDataSource(result)
            {
                MimeType = MimeType.ApplicationJson
            });
        }
    }
}
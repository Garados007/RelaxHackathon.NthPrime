using System.Reflection;
using System.Threading.Tasks;
using MaxLib.WebServer;
using System.IO;

namespace NthPrime
{
    public class Swagger : WebService
    {
        readonly string path;

        public Swagger()
            : base(ServerStage.CreateDocument)
        {
            path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
                "doc.html"
            );
            if (!File.Exists(path))
                throw new FileNotFoundException("required swagger doc file not found", path);
        }

        public override bool CanWorkWith(WebProgressTask task)
        {
            return task.Request.Location.IsUrl(System.Array.Empty<string>());
        }

        public override Task ProgressTask(WebProgressTask task)
        {
            task.Document.DataSources.Add(new HttpFileDataSource(path)
            {
                MimeType = MimeType.TextHtml
            });
            return Task.CompletedTask;
        }
    }
}
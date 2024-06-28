using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

internal class ApiEndpoint
{
    public static async Task Run()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();
        Console.WriteLine("Listening for connections on http://localhost:5000/");

        while (true)
        {
            var context = await listener.GetContextAsync();
            await HandleRequest(context);
        }
    }

    private static async Task HandleRequest(HttpListenerContext context)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
            return;
        }

        var response = context.Response;
        if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/upload")
        {
            try
            {
                var filePath = Path.Combine("uploads", Guid.NewGuid().ToString() + ".wav");

                if (!Directory.Exists("uploads"))
                {
                    Directory.CreateDirectory("uploads");
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await context.Request.InputStream.CopyToAsync(fileStream);
                }

                string translation = await PiperLing.ProcessAudioFile(filePath, "auto");

                var responseString = $"{{\"translation\": \"{translation}\"}}";
                var buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                response.ContentType = "application/json";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                var responseString = $"{{\"error\": \"{ex.Message}\"}}";
                var buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                response.ContentType = "application/json";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }
        else
        {
            var buffer = Encoding.UTF8.GetBytes("PiperLing is running.");

            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}

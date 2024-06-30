using AI_Dollmetscher;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

internal class ApiEndpoint
{
    static bool HeaderAdded = false;
    public static async Task Run()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://*:5000/");
        listener.Start();
        Console.WriteLine("Listening for connections on http://0.0.0.0:5000/");

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
                if(!HeaderAdded)
                {
                    HeaderAdded= true;
                    Header.Add("web app");
                    Console.WriteLine();
                }
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

                if (File.Exists(filePath))
                    File.Delete(filePath);
                var buffer = Encoding.UTF8.GetBytes(translation);

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

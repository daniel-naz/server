using System.Formats.Asn1;
using System.Net;

public static partial class Utils
{
    public static void SendJson(this HttpListenerResponse response, string json)
    {
        // Set the response content type to JSON
        response.ContentType = "application/json";
        response.SendString(json);
    }

    public static void SendText(this HttpListenerResponse response, string str)
    {
        // Set the response content type to JSON
        response.ContentType = "application/text";
        response.SendString(str);
    }

    public static void SendString(this HttpListenerResponse response, string str)
    {
        response.StatusCode = (int)HttpStatusCode.OK;

        // Write the JSON string to the response stream
        using (var writer = new System.IO.StreamWriter(response.OutputStream))
        {
            writer.Write(str);
        }

        // Close the response to complete it
        response.Close();
    }

    public static void SendImage(this HttpListenerResponse response, string filePath)
    {
        string fileExtension = Path.GetExtension(filePath).ToLower();

        // Set the appropriate Content-Type based on the file extension
        if (fileExtension == ".jpg" || fileExtension == ".jpeg")
        {
            response.ContentType = "image/jpeg";
        }
        else
        {
            response.ContentType = "image/png";
        }

        response.SendFile(filePath);
    }

    public static void SendHtml(this HttpListenerResponse response, string filePath)
    {
        response.ContentType = "text/html";
        response.SendFile(filePath);
    }

    public static void SendFile(this HttpListenerResponse response, string filePath)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

        if (File.Exists(fullPath))
        {
            response.StatusCode = (int)HttpStatusCode.OK;

            // Write the image to the response stream
            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(response.OutputStream);
            }
        }

        response.Close();
    }

    public static Dictionary<string, string>? ReadPOST(this HttpListenerRequest request)
    {
        if (request.HttpMethod == "POST")
        {
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string postData = reader.ReadToEnd();
                var parsedData = System.Web.HttpUtility.ParseQueryString(postData);

                Dictionary<string, string> result = [];

                foreach (string? key in parsedData.AllKeys)
                {
                    result.Add(key!, parsedData[key]!);
                }

                return result;
            }
        }
        return null;
    }
}

public abstract class Server : IDisposable
{
    public readonly string Address;

    public readonly string Host;

    public readonly int Port;

    private HttpListener listener;

    private bool disablePrint;


    public bool IsRunnning => listener.IsListening;


    public Server(string host = "localhost", int port = 8080, bool disablePrint = false)
    {
        this.Host = host;
        this.Port = port;
        this.Address = $"http://{host}:{port}/";

        this.listener = new HttpListener();
        listener.Prefixes.Add(this.Address);

        this.disablePrint = disablePrint;
    }

    public void Start()
    {
        listener.Start();
        System.Console.WriteLine($"Server started on {this.Address}.");
        Receive();
    }

    public void Stop()
    {
        listener.Stop();
    }

    private void Receive()
    {
        var result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener); ; // Non-blocking call
    }

    private void ListenerCallback(IAsyncResult result)
    {
        HttpListener listener = (HttpListener)result.AsyncState!;

        if (listener.IsListening)
        {
            var context = listener.EndGetContext(result);

            Receive();

            var request = context.Request;
            var response = context.Response;

            if (!disablePrint)
            {
                Console.WriteLine($"Recieved request from {context.Request.UserHostName} for: {request.Url}");
            }

            HandleConnectionAsync(request, response);
        }
    }

    public abstract Task HandleConnectionAsync(HttpListenerRequest request, HttpListenerResponse response);

    public void Dispose()
    {
        this.Stop();
    }
}
# C# Server & Database

This project allows you to host a server on you local machine. It supports mutli-threaded async server calls and
database integration.  

## Server

To create your own server, inherit from [```Server```](Server\Server.cs) and override ```HandleConnectionAsync```.
Add your custom server logic inside of the function.

Then you can run the server using the Start function.
```cs
static void Main()
{
    Server server = new MyServer();
    server.Start();

    while (true) ;
}
```
Notice : the server is ran asynchronously.

There are multiple helper functions added to ```HttpListenerResponse``` that allow easy reponse handling.
 - SendJson - Send a json string.
 - SendText - Send a text string.
 - SendString - Send any string datatype.
 - SendImage - Send Jpg/Png images.
 - SendHtml - Send an html file.
 - SendFile - Send any file type.
 
These can be used inside of the server request/response logic, ```HandleConnectionAsync```, for example:
```cs
public override Task HandleConnectionAsync(HttpListenerRequest request, HttpListenerResponse response)
{
    string filePath = request.Url!.AbsolutePath.TrimStart('/');
    string fileExtension = Path.GetExtension(filePath).ToLower();

    if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
    {
        response.SendImage(filePath);
    }
    else
    {
        response.SendFile($"{filePath}");
    }

    return Task.CompletedTask;
}
```

## Database

The database is based on [C# Efcore](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli).

To create a database you need to inherit the [```Database```](Server\Database.cs) class and add a ```DbSet``` of the database table data. Read more on the efcore documentation.

Efcore doesn't support multi-threaded apllications, so use the ```DatabaseFactory``` class to create a new instance with every call.

```cs
class MyServer : Server
{
    readonly MyDatabase db;
    readonly DatabaseFactory<MyDatabase> factory;

    public MyServer()
    {
        db = new MyDatabase("database");
        factory = new DatabaseFactory<MyDatabase>(db);
    }

    public override Task HandleConnectionAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        var database = factory.CreateInstance();

        // do things
    }
}
```

If the database takes more arguments than just a filepath string - add an override to ```GetInOrderConstructorArgs``` and return the necessary arguments for creating an instance (in order).

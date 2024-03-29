# Publitio-NET

Publitio-NET is a C# user library for the
[Publitio](https://publit.io) website.

## Installing

You can install this package via NuGet.
If you're using the .NET CLI tools, run

```sh
dotnet add package Publitio-NET
```

See the [package page](https://www.nuget.org/packages/Publitio-NET) for instructions on how to install with different package managers.

## Documentation

You may read this library's documentation
[here](https://ennmichael.github.io/Publitio-NET/docs/html/namespacePublitio.html). Check out the [official API documentation](https://publit.io/docs)
for more detailed info about the Publitio API.

## Example usage

```cs
using System;
using Publitio;
using Newtonsoft.Json.Linq;

// ...

var publitio = new PublitioApi("ZlndDin6v4zo0QgH9pAn", "ZSPqQ7kG8QyypfBTyrWifQAqjaJryzbw");

// Get file info
var res = await publitio.GetAsync("/files/show/MvHX8Zx5");
Console.WriteLine(res);

// Upload a file
using (var file = File.OpenRead("/home/mogwai/ya.png"))
{
    res = await publitio.UploadFileAsync(
        "files/create",
        new Dictionary<string, object>{ ["title"] = "XX" },
        file);
    Console.WriteLine(res);
}

// List files
res = await publitio.GetAsync("files/list", new Dictionary<string, object>{ ["limit"] = 3 });
Console.WriteLine(res);

// Get the ID of the first file
var files = (JArray)res["files"];
var firstFile = (JObject)files[0];
var id = (string)firstFile["id"];
Console.WriteLine(id)
```

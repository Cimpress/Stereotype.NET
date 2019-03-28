# Stereotype.NET

This is a client package making use of Stereotype service easier from C# code base.

## Usage

Install the package

    dotnet add package Stereotype.NET
    
and make sure to add the import    

    using Cimpress.Stereotype;

and then 

    var stereotypeClient = new StereotypeClient(accessToken);
    stereotypeClient.Request().SetTemplateId("demo.html").Materialize(data).Result.FetchString();
            
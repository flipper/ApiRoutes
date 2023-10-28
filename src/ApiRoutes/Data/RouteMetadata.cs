using System.Net;

namespace ApiRoutes;

public struct RouteMetadata
{
    public RouteMetadata()
    {
    }

    public enum RequestBodyType
    {
        None,
        Json,
        Form
    }

    public class Property
    {
        public string Name { get; }
        public Type Type { get; }

        public bool Required { get; }

        public PropertyFetchMethod Method { get; }
        public string Description { get; set; }

        public bool IsHidden { get; set; }

        public Property(string name, Type type, bool required, PropertyFetchMethod method, string description, bool isHidden)
        {
            Name = name;
            Type = type;
            Required = required;
            Method = method;
            Description = description;
            IsHidden = isHidden;
        }
    }
    
    public string Template { get; set; } = null!;
    public Method Method { get; set; } = Method.GET;

    public string Summary { get; set; } = null!;
    public string Description { get; set; } = null!;

    public Type Request { get; init; } = null!;
    public Type Response { get; init; } = null!;
    public Type Handler { get; init; } = null!;
    
    public Type? Validator { get; init; } = null!;

    public RequestBodyType Type { get; init; }

    public bool RequiresAuth { get; set; }
    public string AuthPolicy { get; set; } = null!;


    public List<Property> Properties { get; init; } = null!;

    public Dictionary<HttpStatusCode, string?> Responses { get; init; } = null!;
}
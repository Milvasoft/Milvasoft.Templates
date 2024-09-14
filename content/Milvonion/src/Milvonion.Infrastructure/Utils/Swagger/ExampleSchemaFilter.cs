using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Milvonion.Infrastructure.Utils.Swagger;

/// <summary>
/// Swagger examples.
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Apply examples to swagger schema
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsAssignableTo(typeof(ListRequest)))
        {
            schema.Example = new OpenApiObject()
            {
                ["pageNumber"] = new OpenApiInteger(1),
                ["rowCount"] = new OpenApiInteger(10),
                ["filtering"] = new OpenApiObject
                {
                    ["Criterias"] = new OpenApiArray {
                        new OpenApiObject
                        {
                            ["FilterBy"] = new OpenApiString("Id"),
                            ["Value"] = new OpenApiInteger(1),
                            ["OtherValue"] = new OpenApiNull(),
                            ["Type"] = new OpenApiInteger((int)FilterType.EqualTo)
                        }
                    }
                },
                ["sorting"] = new OpenApiObject
                {

                    ["SortBy"] = new OpenApiString("Id"),
                    ["Type"] = new OpenApiInteger((int)SortType.Asc)
                },
                ["aggregation"] = new OpenApiObject
                {
                    ["Criterias"] = new OpenApiArray {
                        new OpenApiObject
                        {
                            ["AggregateBy"] = new OpenApiString("Id"),
                            ["Type"] = new OpenApiInteger((int)AggregationType.Max)
                        }
                    }
                }
            };
        }
        else if (context.Type.IsAssignableTo(typeof(LookupRequest)))
        {
            schema.Example = new OpenApiObject()
            {
                ["parameters"] = new OpenApiArray {
                     new OpenApiObject{

                         ["entityName"] = new OpenApiString("User"),
                         ["requestedPropertyNames"] = new OpenApiArray {

                              new OpenApiString("Name")
                         },
                         ["filtering"] = new OpenApiObject
                         {
                             ["Criterias"] = new OpenApiArray {
                                 new OpenApiObject
                                 {
                                     ["FilterBy"] = new OpenApiString("Name"),
                                     ["Value"] = new OpenApiString("Buğra"),
                                     ["OtherValue"] = new OpenApiNull(),
                                     ["Type"] = new OpenApiInteger((int)FilterType.EqualTo)
                                 }
                             }
                         },
                         ["sorting"] = new OpenApiObject
                         {
                             ["SortBy"] = new OpenApiString("Id"),
                             ["Type"] = new OpenApiInteger((int)SortType.Asc)
                         }
                     }
                }
            };
        }
    }
}
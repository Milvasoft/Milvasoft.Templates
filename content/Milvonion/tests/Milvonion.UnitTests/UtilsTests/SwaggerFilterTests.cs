using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Milvonion.Api.Controllers;
using Milvonion.Infrastructure.Utils.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace Milvonion.UnitTests.UtilsTests;

[Trait("Utils Unit Tests", "Swagger filters unit tests.")]
public class SwaggerFilterTests
{

    [Fact]
    public void Apply_ShouldAddEnumDescriptionsToModel()
    {
        // Arrange
        var schemaFilter = new EnumSchemaFilter();
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(TestEnumFixture), null, null, null);

        // Act
        schemaFilter.Apply(schema, context);

        // Assert
        schema.Enum.Should().HaveCount(2);
    }

    [Fact]
    public void Apply_ListRequest_ShouldSetExampleCorrectly()
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(ListRequest), null, null, null, null);
        var filter = new ExampleSchemaFilter();

        // Act
        filter.Apply(schema, context);

        // Assert
        var example = schema.Example as OpenApiObject;
        example.Should().NotBeNull();
        example["pageNumber"].Should().BeEquivalentTo(new OpenApiInteger(1));
        example["rowCount"].Should().BeEquivalentTo(new OpenApiInteger(10));

        var filtering = example["filtering"] as OpenApiObject;
        filtering.Should().NotBeNull();
        var criterias = filtering["Criterias"] as OpenApiArray;
        criterias.Should().NotBeNull();
        var criteria = criterias[0] as OpenApiObject;
        criteria.Should().NotBeNull();
        criteria["FilterBy"].Should().BeEquivalentTo(new OpenApiString("Id"));
        criteria["Value"].Should().BeEquivalentTo(new OpenApiInteger(1));
        criteria["OtherValue"].Should().BeOfType<OpenApiNull>();
        criteria["Type"].Should().BeEquivalentTo(new OpenApiInteger((int)FilterType.EqualTo));

        var sorting = example["sorting"] as OpenApiObject;
        sorting.Should().NotBeNull();
        sorting["SortBy"].Should().BeEquivalentTo(new OpenApiString("Id"));
        sorting["Type"].Should().BeEquivalentTo(new OpenApiInteger((int)SortType.Asc));

        var aggregation = example["aggregation"] as OpenApiObject;
        aggregation.Should().NotBeNull();
        var aggregationCriterias = aggregation["Criterias"] as OpenApiArray;
        aggregationCriterias.Should().NotBeNull();
        var aggregationCriteria = aggregationCriterias[0] as OpenApiObject;
        aggregationCriteria.Should().NotBeNull();
        aggregationCriteria["AggregateBy"].Should().BeEquivalentTo(new OpenApiString("Id"));
        aggregationCriteria["Type"].Should().BeEquivalentTo(new OpenApiInteger((int)AggregationType.Max));
    }

    [Fact]
    public void Apply_LookupRequest_ShouldSetExampleCorrectly()
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(LookupRequest), null, null, null, null);
        var filter = new ExampleSchemaFilter();

        // Act
        filter.Apply(schema, context);

        // Assert
        var example = schema.Example as OpenApiObject;
        example.Should().NotBeNull();

        var parameters = example["parameters"] as OpenApiArray;
        parameters.Should().NotBeNull();
        var parameter = parameters[0] as OpenApiObject;
        parameter.Should().NotBeNull();
        parameter["entityName"].Should().BeEquivalentTo(new OpenApiString("User"));

        var requestedPropertyNames = parameter["requestedPropertyNames"] as OpenApiArray;
        requestedPropertyNames.Should().NotBeNull();
        requestedPropertyNames[0].Should().BeEquivalentTo(new OpenApiString("Name"));

        var filtering = parameter["filtering"] as OpenApiObject;
        filtering.Should().NotBeNull();
        var criterias = filtering["Criterias"] as OpenApiArray;
        criterias.Should().NotBeNull();
        var criteria = criterias[0] as OpenApiObject;
        criteria.Should().NotBeNull();
        criteria["FilterBy"].Should().BeEquivalentTo(new OpenApiString("Name"));
        criteria["Value"].Should().BeEquivalentTo(new OpenApiString("Buğra"));
        criteria["OtherValue"].Should().BeOfType<OpenApiNull>();
        criteria["Type"].Should().BeEquivalentTo(new OpenApiInteger((int)FilterType.EqualTo));

        var sorting = parameter["sorting"] as OpenApiObject;
        sorting.Should().NotBeNull();
        sorting["SortBy"].Should().BeEquivalentTo(new OpenApiString("Id"));
        sorting["Type"].Should().BeEquivalentTo(new OpenApiInteger((int)SortType.Asc));
    }

    [Fact]
    public void Apply_ShouldNotAddAcceptLanguageHeader_WhenControllerIsWeather()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var context = new OperationFilterContext(
            new ApiDescription(),
            new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions())),
            new SchemaRepository(),
            typeof(LanguagesController).GetMethod("GetLanguagesAsync")
        );
        var filter = new RequestHeaderFilter();

        // Act
        filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().BeNullOrEmpty();
    }
}

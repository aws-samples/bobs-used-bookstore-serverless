using Amazon.CDK.AWS.APIGateway;
using Constructs;

namespace AppStack.Models
{
    public class CreateBookDtoModel : Model
    {
        public CreateBookDtoModel(Construct scope, RestApi api) : base(scope, "CreateBookDtoModel", new ModelProps
        {
            ContentType = "application/json",
            RestApi = api,
            Schema = new JsonSchema
            {
                Type = JsonSchemaType.OBJECT,
                Properties = new Dictionary<string, IJsonSchema> {                   
                    { "author", new JsonSchema() { Type = JsonSchemaType.STRING } },
                    { "bookType", new JsonSchema { Type = JsonSchemaType.STRING } },
                    { "condition", new JsonSchema { Type = JsonSchemaType.STRING } },
                    { "coverImage", new JsonSchema { Type = JsonSchemaType.STRING | JsonSchemaType.NULL} },
                    { "coverImageFileName", new JsonSchema { Type = JsonSchemaType.STRING | JsonSchemaType.NULL } },
                    { "genre", new JsonSchema { Type = JsonSchemaType.STRING } },
                    { "isbn", new JsonSchema { Type = JsonSchemaType.STRING } },
                    { "name", new JsonSchema { Type = JsonSchemaType.STRING } },
                    { "price", new JsonSchema { Type = JsonSchemaType.NUMBER } },
                    { "publisher", new JsonSchema { Type = JsonSchemaType.STRING } },
                    { "quantity", new JsonSchema { Type = JsonSchemaType.INTEGER } },
                    { "summary", new JsonSchema { Type = JsonSchemaType.STRING } },
                    { "year", new JsonSchema { Type = JsonSchemaType.INTEGER| JsonSchemaType.NULL } }
                }   
            }            
        })
        {
        }
    }
}

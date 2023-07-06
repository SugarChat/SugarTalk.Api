using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SugarTalk.Api.Filters.Swagger;

public class SwaggerShowJsonPropertyFilter : IOperationFilter
{
    private readonly Type _propertyType;

    public SwaggerShowJsonPropertyFilter(Type propertyType)
    {
        _propertyType = propertyType;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!_propertyType.Name.Equals(operation.RequestBody?.Content?.FirstOrDefault().Value.Schema.Reference?.Id)) return;

        var stack = new Stack<Type>();
        
        stack.Push(_propertyType);

        while (stack.Count > 0)
        {
            var subType = stack.Pop();

            var subSchema = context.SchemaRepository.Schemas.FirstOrDefault(x => subType.Name.Equals(x.Key));

            if(subSchema.Key == null && subSchema.Value == null) continue;
            
            var subPropertiesSchema = subSchema.Value?.Properties;
            
            if (subPropertiesSchema == null) continue;

            foreach (var subProperty in subType.GetProperties())
            {
                var needChangeProperty = subPropertiesSchema.SingleOrDefault(x => 
                    string.Equals(subProperty.Name, x.Key, StringComparison.OrdinalIgnoreCase));

                if (needChangeProperty.Key.IsNullOrEmpty()) continue;
                
                var jsonPropertyAttribute = subProperty.GetCustomAttribute<JsonPropertyAttribute>();

                if (subProperty.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                {
                    subPropertiesSchema.Remove(needChangeProperty.Key);
                }
                else if (jsonPropertyAttribute?.PropertyName != null)
                {
                    subPropertiesSchema.Remove(needChangeProperty.Key);
                    subPropertiesSchema.Add(jsonPropertyAttribute.PropertyName, needChangeProperty.Value);
                    
                    if (subProperty.GetCustomAttribute<RequiredAttribute>() != null)
                    {
                        subSchema.Value?.Required.Add(jsonPropertyAttribute.PropertyName);
                    }
                }

                if (subPropertiesSchema.Any(x => x.Value.Reference?.Id == subProperty.PropertyType.Name)
                    && subProperty.PropertyType.GetProperties().Length > 0)
                {
                    stack.Push(subProperty.PropertyType);
                }
            }
        }
    }
}


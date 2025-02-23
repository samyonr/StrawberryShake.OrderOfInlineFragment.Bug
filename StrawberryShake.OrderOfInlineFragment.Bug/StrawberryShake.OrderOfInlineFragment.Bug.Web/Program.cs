using StrawberryShake.OrderOfInlineFragment.Bug.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddInterfaceType<IPet>()
    .AddType<Dog>()
    .AddType<Cat>()
    .AddType<Bird>()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

var app = builder.Build();

app.MapGraphQL();

await app.RunAsync();

public partial class Program;
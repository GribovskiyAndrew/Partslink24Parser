using Partslink24Parser;
using Z.EntityFramework.Extensions;

var context = ApplicationContext.GetSqlLiteContext();

EntityFrameworkManager.ContextFactory = context => ApplicationContext.GetSqlLiteContext();

await context.Database.EnsureCreatedAsync();

RequestManager requestManager = new RequestManager();

Parser parser = new Parser(requestManager, context);

parser.Run();
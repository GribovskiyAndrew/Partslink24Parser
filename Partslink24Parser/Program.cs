using Partslink24Parser;
using Partslink24Parser.Entities;
using Z.EntityFramework.Extensions;

var context = ApplicationContext.GetSqlLiteContext();

EntityFrameworkManager.ContextFactory = context => ApplicationContext.GetSqlLiteContext();

await context.Database.EnsureCreatedAsync();

if (!context.VinNumbers.Any())
    await context.VinNumbers.BulkInsertAsync(NumberSource.NumberList.Select(x => new VinNumber { Vin = x }));

RequestManager requestManager = new RequestManager();

Parser parser = new Parser(requestManager, context);

await parser.Run();

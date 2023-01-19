using Newtonsoft.Json.Linq;
using Partslink24Parser.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Partslink24Parser
{
    public class Parser
    {
        protected readonly RequestManager _requestManager;
        protected readonly ApplicationContext _context;

        private readonly ConcurrentQueue<dynamic> _queue;

        public Parser(RequestManager requestManager, ApplicationContext context)
        {
            _requestManager = requestManager;
            _context = context;

            _queue = new ConcurrentQueue<dynamic>(
                _context.VinNumbers
                .Where(x => !x.Done)
                .Select(x => new { x.Vin, x.Id })
                .ToList());
        }

        //private void extractDataFromRequest(JObject? path, List<PartInformation> list, int id)
        //{
        //    foreach (var data in path)
        //        list.Add(new PartInformation
        //        {
        //            PartNumber = data["partno"] != null ? data["partno"].ToString() : "-",
        //            Description = data["description"] != null ? data["description"].ToString() : "-",
        //            Price = data["values"]["price"]["price"] != null ? data["values"]["price"]["price"].ToString() : "-",
        //            Type = "optional",
        //            PartId = id
        //        });
        //}

        private List<PartInformation> extractDataFromRequest(string filter, JObject? partInfo, Part part)
        {
            List<PartInformation> partInformationList = new List<PartInformation>();

            foreach (var data in partInfo["data"]["segments"][filter]["records"])
                partInformationList.Add(new PartInformation
                {
                    PartNumber = data["partno"] != null ? data["partno"].ToString() : "-",
                    Description = data["description"] != null ? data["description"].ToString() : "-",
                    Price = data["values"]["price"]["price"] != null ? data["values"]["price"]["price"].ToString() : "-",
                    Type = filter,
                    PartId = part.Id
                });

            return partInformationList;
        }

        public async Task Run()
        {

            while (!_queue.IsEmpty)
            {
                var ok = _queue.TryDequeue(out dynamic vin);

                if (ok)
                {
                    try
                    {
                        _requestManager.Vin = vin.Vin;

                        await _requestManager.AddHeaders();
           
                        var vehicle = await _requestManager.Get($"https://www.partslink24.com/p5vwag/extern/directAccess?lang=en&serviceName=skoda_parts&q={vin.Vin}&p5v=1.7.18&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

                        var vehicleValues = vehicle["data"]["segments"]["vinfoBasic"]["records"];

                        VehicleData vehicleData = new VehicleData()
                        {
                            Model = vehicleValues[1]["values"]["value"].ToString(),
                            DateOfProduction = vehicleValues[2]["values"]["value"].ToString(),
                            Year = Convert.ToInt32(vehicleValues[3]["values"]["value"]),
                            SalesType = vehicleValues[4]["values"]["value"].ToString(),
                            EngineCode = vehicleValues[5]["values"]["value"].ToString(),
                            TransmissionCode = vehicleValues[6]["values"]["value"].ToString(),
                            AxleDrive = vehicleValues[7]["values"]["value"].ToString(),
                            Equipment = vehicleValues[8]["values"]["value"].ToString(),
                            RoofColor = vehicleValues[9]["values"]["value"].ToString(),
                            ExteriorColorAndPaintCode = vehicleValues[10]["values"]["value"].ToString(),
                            VinNumberId = vin.Id,
                            Done = true
                        };

                        await _context.VehicleData.AddAsync(vehicleData);
                        await _context.SaveChangesAsync();



                        //logs = driver.Manage().Logs;

                        //perf = logs.GetLog(LogType.Performance);

                        //item = perf.Select(x => x.Message).Where(x => x != null && x.Contains("/groups/vin_maingroups") && x.Contains("authorization") && x.Contains("cookie") && x.Contains("authority") && x.Contains("accept-language")).FirstOrDefault();

                        //result = JObject.Parse(item);

                        //headers = result["message"]["params"]["headers"];

                        //_headers = new Dictionary<string, string>();

                        //foreach (JProperty prop in headers.OfType<JProperty>())
                        //{
                        //    if (prop.Name != "content-length" && prop.Name != "content-type" && prop.Name != ":method" && prop.Name != ":path" && prop.Name != ":scheme" && prop.Name != "accept-encoding")
                        //    {
                        //        if (prop.Name == ":authority")
                        //            _headers.Add("authority", prop.Value.ToString());
                        //        else
                        //            _headers.Add(prop.Name, prop.Value.ToString());
                        //    }

                        //    //_headers.Add(prop.Name, prop.Value.ToString());
                        //}

                        //_requestManager.AddHeaders(_headers);

                        var majorCategoryData = await _requestManager.Get($"https://www.partslink24.com/p5vwag/extern/groups/vin_maingroups?lang=en&serviceName=skoda_parts&upds=2022-11-18--00-05&vin={vin.Vin}&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

                        List<MajorCategory> majorCategories = new List<MajorCategory>();

                        for (int i = 0; i < 10; i++)
                        {
                            var majorCategory = majorCategoryData["data"]["records"][i];

                            majorCategories.Add(new MajorCategory
                            {
                                Type = majorCategory["values"]["caption"].ToString(),
                                Done = true,
                                VehicleDataId = vehicleData.Id,
                                Path = majorCategory["link"]["path"].ToString(),
                            });
                        }

                        await _context.MajorСategories.BulkInsertAsync(majorCategories);



                        List<MinorCategory> minorCategoryList = new List<MinorCategory>();

                        foreach (var majorCategory in majorCategories)
                        {
                            var minorCategory = await _requestManager.Get($"https://www.partslink24.com/" + majorCategory.Path);

                            foreach (var data in minorCategory["data"]["records"])
                            {
                                if (data["unavailable"] != null)
                                    continue;

                                minorCategoryList.Add(new MinorCategory
                                {
                                    SubGroup = data["values"]["subgroup"].ToString(),
                                    Illustration = data["values"]["illustrationNumber"].ToString(),
                                    Description = data["values"]["captions"].ToString(),
                                    Remark = data["values"]["remarks"] != null ? data["values"]["remarks"].ToString() : "-",
                                    Model = data["values"]["modelDescriptions"] != null ? data["values"]["modelDescriptions"].ToString() : "-",
                                    Done = true,
                                    MajorCategoryId = majorCategory.Id,
                                    Path = data["link"]["path"].ToString()
                                });

                            }
                        }

                        await _context.MinorCategories.BulkInsertAsync(minorCategoryList);



                        List<Part> partsList = new List<Part>();

                        var count = 0;

                        foreach (var minorCategory in minorCategoryList)
                        {
                            Console.WriteLine(count++);

                            var part = await _requestManager.Get($"https://www.partslink24.com/" + minorCategory.Path + "&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                            var imageName = minorCategory.Id + ".png";

                            if (part["data"]["images"] != null)
                            {
                                var imagePath = part["data"]["images"][0]["uri"].ToString() + "M&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                                var img = await _requestManager.Get($"https://www.partslink24.com/" + imagePath + "M&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                                var base64String = img["image"].ToString();

                                byte[] imgByte = Convert.FromBase64String(base64String);

                                await File.WriteAllBytesAsync("C:\\Users\\lifebookE\\source\\repos\\Partslink24Parser\\Partslink24Parser\\Images\\" + imageName, imgByte);

                                List<Point> coordinateList = new List<Point>();

                                foreach (var i in img["hotspots"])
                                {
                                    foreach (var j in i["areas"])
                                    {
                                        coordinateList.Add(new Point
                                        {
                                            Left = Convert.ToInt32(j["left"]),
                                            Top = Convert.ToInt32(j["top"]),
                                            Width = Convert.ToInt32(j["widht"]),
                                            Height = Convert.ToInt32(j["height"]),
                                            Label = i["key"].ToString(),
                                            MinorCategoryId = minorCategory.Id
                                        });
                                    }
                                }

                                await _context.Points.BulkInsertAsync(coordinateList);
                            }
                            else
                            {
                                imageName = "-";
                            }

                            foreach (var data in part["data"]["records"])
                            {
                                partsList.Add(new Part
                                {
                                    Position = data["values"]["pos"] != null ? data["values"]["pos"].ToString() : "-",
                                    PartNumber = data["values"]["partno"] != null ? data["values"]["partno"].ToString() : "-",
                                    Description = data["values"]["description"] != null ? data["values"]["description"].ToString() : "-",
                                    Remark = data["values"]["remark"] != null ? data["values"]["remark"].ToString() : "-",
                                    Unit = data["values"]["qty"] != null ? data["values"]["qty"].ToString() : "-",
                                    Model = data["values"]["modelDescription"] != null ? data["values"]["modelDescription"].ToString() : "-",
                                    Path = data["link"] != null ? data["link"]["path"].ToString() : "-",
                                    Unavailable = data["unavailable"] != null ? true : false,
                                    ImageName = imageName,
                                    Done = true,
                                    MinorCategoryId = minorCategory.Id
                                });
                            }
                        }

                        await _context.Parts.BulkInsertAsync(partsList);

                        List<PartInformation> partInformationList = new List<PartInformation>();

                        foreach (var part in partsList)
                        {
                            if (part.Path == "-")
                                continue;

                            var partInfo = await _requestManager.Get($"https://www.partslink24.com/" + part.Path + "&_=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                            foreach (var data in partInfo["data"]["records"])
                                partInformationList.Add(new PartInformation
                                {
                                    PartNumber = data["partno"] != null ? data["partno"].ToString() : "-",
                                    Description = data["description"] != null ? data["description"].ToString() : "-",
                                    Price = data["values"]["price"]["price"] != null ? data["values"]["price"]["price"].ToString() : "-",
                                    Type = "main",
                                    PartId = part.Id
                                });

                            if (partInfo["data"]["segments"] != null)
                            {
                                if (partInfo["data"]["segments"]["fitting"] != null)
                                    foreach (var data in partInfo["data"]["segments"]["fitting"]["records"])
                                        partInformationList.Add(new PartInformation
                                        {
                                            PartNumber = data["partno"] != null ? data["partno"].ToString() : "-",
                                            Description = data["description"] != null ? data["description"].ToString() : "-",
                                            Price = data["values"]["price"]["price"] != null ? data["values"]["price"]["price"].ToString() : "-",
                                            Type = "fitting",
                                            PartId = part.Id
                                        });

                                else if (partInfo["data"]["segments"]["optional"] != null)
                                    foreach (var data in partInfo["data"]["segments"]["optional"]["records"])
                                        partInformationList.Add(new PartInformation
                                        {
                                            PartNumber = data["partno"] != null ? data["partno"].ToString() : "-",
                                            Description = data["description"] != null ? data["description"].ToString() : "-",
                                            Price = data["values"]["price"]["price"] != null ? data["values"]["price"]["price"].ToString() : "-",
                                            Type = "optional",
                                            PartId = part.Id
                                        });

                                else if (partInfo["data"]["segments"]["proposed"] != null)
                                    foreach (var data in partInfo["data"]["segments"]["proposed"]["records"])
                                        partInformationList.Add(new PartInformation
                                        {
                                            PartNumber = data["partno"] != null ? data["partno"].ToString() : "-",
                                            Description = data["description"] != null ? data["description"].ToString() : "-",
                                            Price = data["values"]["price"]["price"] != null ? data["values"]["price"]["price"].ToString() : "-",
                                            Type = "proposed",
                                            PartId = part.Id
                                        });
                                else if (partInfo["data"]["segments"]["interpretations"] != null)
                                    foreach (var data in partInfo["data"]["segments"]["interpretations"]["records"])
                                        partInformationList.Add(new PartInformation
                                        {
                                            PartNumber = data["partno"] != null ? data["partno"].ToString() : "-",
                                            Description = data["description"] != null ? data["description"].ToString() : "-",
                                            Price = data["values"]["price"]["price"] != null ? data["values"]["price"]["price"].ToString() : "-",
                                            Type = "interpretations",
                                            PartId = part.Id
                                        });
                            }

                        }

                        await _context.PartInformation.BulkInsertAsync(partInformationList);

                        await _context.Database.ExecuteSqlRawAsync($@"UPDATE Numbers SET Done = 1,
                                                                         ItemId = {vehicleData.Id} Where Id = {vin.Id}");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

        }
    }
}

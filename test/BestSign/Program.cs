var token = await BestSign.GetToken();
var result = await BestSign.Post<object>("/api/contract-center/search", new
{
    sender = "jiakuo.zhang@quest-global.com"
});


Console.ReadLine();
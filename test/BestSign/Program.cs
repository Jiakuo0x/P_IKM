var token = await BestSign.GetToken();
Stream result = await BestSign.PostAsStream("/api/contracts/download-file", new
{
    contractIds = new[] { "3343501061511825416" }
});

MemoryStream ms = new MemoryStream();
result.CopyTo(ms);
await File.WriteAllBytesAsync("test.zip", ms.ToArray());

Console.ReadLine();
using yoomoney_api.account;
using yoomoney_api.authorize;



string Host = System.Net.Dns.GetHostName();
string IP = System.Net.Dns.GetHostByName(Host).AddressList[0].ToString();
Console.WriteLine(Host +"   "+ IP);

//Authorize authorize = new(clientId: "E14983CB011BF260231EFB6B6EADFED71371F76CC42270AC2940BDE7B5977047", redirectUri: "https://t.me/NASA_daily_pic_bot", scope: new[]
//{
//    "account-info",
//    "operation-history",
//    "operation-details",
//    "incoming-transfers",
//    "payment-p2p",
//});

//var token = await authorize.GetAccessToken(code: "https://t.me/NASA_daily_pic_bot?code=3411810A2263BEBAD044946074D2856380AD89BB949D30794FC17EC3B715D3E67BBD5673C9CB582DCA4AEAB1DCC6A2FB8F1873AD1754BE91FC9BABF1C9E963298980F89CB3F44FAD94DDE7FCD99B18F43A97EFFEB04515A065D27CE4F8A7A19F9AC3AE0833FCB684B8764A3E21C1760EC52C5A7A4275495BE0D6175B1B6B451C", 
//    clientId: "E14983CB011BF260231EFB6B6EADFED71371F76CC42270AC2940BDE7B5977047", 
//    redirectUri: "https://t.me/NASA_daily_pic_bot");

//Console.WriteLine(token);


//var client = new Client(token: authorize.TokenUrl);
//var accountInfo = client.GetAccountInfo(token: "4100118707521253.DF7EFADAAE1158B7D40D6AF10B1758E24D0F31971166BB7D414A1BEF02AEB715DBC8CAB7F0F79A9061B3968DFEF4EB55879B748635E819A4EA4818C8A0D5321BC819954D005DAD8A5820E696713B5EFD0E67289B6649E8AA878FE8456537574F7BE6BFA71498CA8446A74B93191C2F200477FCF28707133B9EAD4CA174A9188F");
//accountInfo.Print();

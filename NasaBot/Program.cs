using Telegram.Bot;
using System.Text;
using NasaBot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NasaBot.Objects;
using System.Security.Cryptography;



public class Program
{

    public static async Task Main(string[] args)
    {

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient<NasaService>();
                services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("7120302934:AAE05ronqHxg-WmF-RCkd7K524clyhJ_VXw"));
                services.AddSingleton<TelegramService>();

                services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlServer("Server=217.28.223.127,17160;User Id=user_0badf;Password=p$6MZf*98_;Database=db_611e8;TrustServerCertificate=True;"));
            })
            .Build();


        var telegramService = host.Services.GetRequiredService<TelegramService>();
        await telegramService.StartReceivingAsync(CancellationToken.None);


        Console.ReadKey();
    }

}

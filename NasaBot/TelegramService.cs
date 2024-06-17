using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using NasaBot.Objects;
using System.Diagnostics;
using Telegram.Bot.Types.Payments;
using System.Resources;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.Text;

namespace NasaBot
{
    public class TelegramService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly NasaService _nasaService;
        private readonly AppDbContext _nasaDB;
        int[] prices = {5,100, 150,500, 750};

        public TelegramService(ITelegramBotClient botClient, NasaService nasaService, AppDbContext nasaDB, HttpClient httpClient)
        {
            _botClient = botClient;
            _nasaService = nasaService;
            _nasaDB = nasaDB;
        }
       
        public async Task StartReceivingAsync(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cancellationToken
            );

            var botInfo = await _botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{botInfo.Username}");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient,Update update, CancellationToken cancellationToken)
        {

            User user = null;
            string messageText = null;
            var message = update.Message;

            if (update.ShippingQuery != null) 
            {
                ShippingQuery shippingQuery = update.ShippingQuery;
                user = shippingQuery.From;//при оплате сюда не заходил 
                await Console.Out.WriteLineAsync(user.ToString());
                await Console.Out.WriteLineAsync(shippingQuery.ToString());
                await Console.Out.WriteLineAsync(shippingQuery.InvoicePayload+" "+ shippingQuery.Id);

            }
            
            if (update.PreCheckoutQuery != null)
            {
                PreCheckoutQuery precheckoutQuery = update.PreCheckoutQuery;
                user = precheckoutQuery.From;
                await Console.Out.WriteLineAsync(user.ToString());
                await Console.Out.WriteLineAsync(precheckoutQuery.ToString());

                await Console.Out.WriteLineAsync(precheckoutQuery.InvoicePayload + " " + precheckoutQuery.Id);

                PreCheckoutRequest(update.PreCheckoutQuery, cancellationToken);
            }

            if (update.CallbackQuery != null)
            {
                Bot_OnCallbackQuery(botClient, update.CallbackQuery,cancellationToken);
                messageText = update.CallbackQuery.Data;
                user = update.CallbackQuery.From;
                return; // Выходим из метода, так как CallbackQuery уже обработан
            }
            if (message != null)
            {
                user = message.From;

                // Проверяем успешную оплату
                if (message.SuccessfulPayment != null)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо за ваше пожертвование! 🙏 Ваш вклад не напрасен — все средства пойдут на поддержку моего проекта и реализацию новых идей!💗", cancellationToken: cancellationToken);
                    await SendStartMessage(message.Chat.Id);
                    return; // Выходим из метода, так как успешная оплата уже обработана
                }

                // Проверяем тип сообщения
                if (message.Type == MessageType.Text)
                {
                    messageText = message.Text.ToLower();
                }
            }

            if (messageText != null) 
            {
                switch (messageText)
                {
                    case "/start":
                        await SendStartMessage(message.Chat.Id);
                        break;
                    //case "/test":

                    //    await Console.Out.WriteLineAsync("Got it");
                    //    SendPaymentAsync(user, message.Chat.Id, "vafls", "sup", cancellationToken);
                        //break;

                    default:
                        await _botClient.SendTextMessageAsync(message.Chat.Id, "Команда не распознана.", cancellationToken: cancellationToken);
                        break;
                }
            }
        }
        private async void Bot_OnCallbackQuery(object sender, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            //Создает уыедомление поверх
            //await _botClient.AnswerCallbackQueryAsync(
            //    callbackQueryId: callbackQuery.Id,
            //    text: $"Received {callbackQuery.Data}"
            //);

            //удаляет сообщение
            await _botClient.EditMessageReplyMarkupAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            replyMarkup: null);

            //await _botClient.SendTextMessageAsync(
            //    chatId: callbackQuery.Message.Chat.Id,
            //    text: $"You selected: {callbackQuery.Data}"
            //);

            switch (callbackQuery.Data)
            {
                case "/start":
                    //await SendStartMessage(callbackQuery.Message.Chat.Id);

                    await SendStartMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    break;

                case "/test":

                    break;

                case "/nasapicture":

                    await _botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: "Ожидание..." // Используем невидимый символ вместо текста
                    );

                    await SendNasaPicture(callbackQuery.Message.Chat.Id, callbackQuery.Message, cancellationToken);
                    await SendStartMessage(callbackQuery.Message.Chat.Id);
                    break;
                case "/donate":
                    //await _botClient.EditMessageTextAsync(
                    //chatId: callbackQuery.Message.Chat.Id,
                    //messageId: callbackQuery.Message.MessageId,
                    //text: "Выберите способ оплаты" // Используем невидимый символ вместо текста
                    //);
                    SendPaymentListAsync(callbackQuery.From,  callbackQuery.Message.Chat.Id, callbackQuery.Message, cancellationToken);
                    break;
                case "/payinstars":
                    SendPaymentAsyncInStars(callbackQuery.From, callbackQuery.Message.Chat.Id, callbackQuery.Message, "Пожертвование", "Пожертвование в NASA_daily_pic_bot", prices, cancellationToken);
                    break;
                case "/nasatodaypicture":
                    await _botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: "Ожидание..." // Используем невидимый символ вместо текста
                    );

                    await SendTodayNasaPicture(callbackQuery.Message.Chat.Id, callbackQuery.Message, cancellationToken);
                    await SendStartMessage(callbackQuery.Message.Chat.Id);
                    break;
                default:
                    await _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "Неизвестная команда",
                        cancellationToken: cancellationToken
                    );
                    break;
            }
        }
        private async Task SendNasaPicture(long chatId, Message message, CancellationToken cancellationToken)
        {
            var nasaData = await _nasaService.GetAstronomyPictureAsync();
            if (nasaData != null)
            {
                //await botClient.SendTextMessageAsync(message.Chat.Id, $"<a href='{nasaData.Url}'>NASA Picture</a>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                // Сохраняем данные в базу данных
                _nasaDB.Nasas.Add(nasaData);
                await _nasaDB.SaveChangesAsync();
                await Console.Out.WriteAsync(nasaData.Title + $" ({Translator.Translatesentence(nasaData.Title, "ru")})");
                //await Console.Out.WriteLineAsync(nasaData.Explanation);
                //await Console.Out.WriteLineAsync("///");
                //await Console.Out.WriteLineAsync(Translator.Translatesentence(nasaData.Title, "ru"));
                //await Console.Out.WriteLineAsync(Translator.TranslateText(nasaData.Explanation, "ru"));
                await Console.Out.WriteLineAsync();
                Console.WriteLine($"сохранено {nasaData.Title}\n");
                try
                {
                    await _botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: message.MessageId,
                    text: "Случайное фото Nasa"
                    );

                    Message messa = await _botClient.SendPhotoAsync(
                        message.Chat.Id, InputFile.FromUri(nasaData.Url),
                        caption: $"<b>Фото дня NASA \"{nasaData.Title}\"({Translator.TranslateText(nasaData.Title, "ru")}) в {nasaData.Date}</b>",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                    await _botClient.SendTextMessageAsync(message.Chat.Id, $"{Translator.TranslateText(nasaData.Explanation, "ru")} ", parseMode: ParseMode.Html);
                    //await _botClient.SendTextMessageAsync(
                    //chatId: chatId,
                    //text: $"выберите действие",
                    //parseMode: ParseMode.Html,
                    //replyMarkup: Markups.GetStartMarkup());
                    

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("message caption is too long"))
                    {
                        // Если ошибка связана с длинной подписью, разбиваем сообщение на более короткие части
                        //int maxCaptionLength = 4096; // Максимальная длина подписи в Telegram
                        //var captionChunks = SplitCaption(nasaData.Explanation, maxCaptionLength);

                        //foreach (var chunk in captionChunks)
                        //{
                        //    await _botClient.SendTextMessageAsync(message.Chat.Id, chunk);

                        //}
                        await _botClient.EditMessageTextAsync(
                     chatId: chatId,
                     messageId: message.MessageId,
                     text: "Ошибка сервера"
                     );

                    }
                    else
                    {
                        // Если это другая ошибка, выводим её
                        Console.WriteLine($"Error: {ex.Message}");
                        //await _botClient.SendTextMessageAsync(message.Chat.Id, $"{ex.Message}");
                        await _botClient.EditMessageTextAsync(
                     chatId: chatId,
                     messageId: message.MessageId,
                     text: "Ошибка сервера"
                     );
                    }
                    static string[] SplitCaption(string caption, int maxLength)
                    {
                        // Разбиваем текст на части, учитывая максимальную длину
                        int index = 0;
                        var chunks = new System.Collections.Generic.List<string>();

                        while (index < caption.Length)
                        {
                            int length = Math.Min(maxLength, caption.Length - index);
                            chunks.Add(caption.Substring(index, length));
                            index += length;
                        }

                        return chunks.ToArray();
                    }
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, "Не удалось получить данные от NASA.", cancellationToken: cancellationToken);
                await _botClient.EditMessageTextAsync(
                     chatId: chatId,
                     messageId: message.MessageId,
                     text: "Не удалось получить данные от NASA."
                     );
            }


        }
        private async Task SendTodayNasaPicture(long chatId, Message message, CancellationToken cancellationToken)
        {
            var nasaData = await _nasaService.GetTodayAstronomyPictureAsync();
            if (nasaData != null)
            {
                //await botClient.SendTextMessageAsync(message.Chat.Id, $"<a href='{nasaData.Url}'>NASA Picture</a>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                // Сохраняем данные в базу данных
                _nasaDB.Nasas.Add(nasaData);
                await _nasaDB.SaveChangesAsync();
                await Console.Out.WriteAsync(nasaData.Title + $" ({Translator.Translatesentence(nasaData.Title, "ru")})");
                //await Console.Out.WriteLineAsync(nasaData.Explanation);
                //await Console.Out.WriteLineAsync("///");
                //await Console.Out.WriteLineAsync(Translator.Translatesentence(nasaData.Title, "ru"));
                //await Console.Out.WriteLineAsync(Translator.TranslateText(nasaData.Explanation, "ru"));
                await Console.Out.WriteLineAsync();
                Console.WriteLine($"сохранено {nasaData.Title}\n");
                try
                {
                    await _botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: message.MessageId,
                    text: "Сегодняшнее фото NASA"
                    );

                    Message messa = await _botClient.SendPhotoAsync(
                        message.Chat.Id, InputFile.FromUri(nasaData.Url),
                        caption: $"<b>Фото дня NASA \"{nasaData.Title}\"({Translator.TranslateText(nasaData.Title, "ru")}) в {nasaData.Date}</b>",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                    await _botClient.SendTextMessageAsync(message.Chat.Id, $"{Translator.TranslateText(nasaData.Explanation, "ru")} ", parseMode: ParseMode.Html);
                    //await _botClient.SendTextMessageAsync(
                    //chatId: chatId,
                    //text: $"выберите действие",
                    //parseMode: ParseMode.Html,
                    //replyMarkup: Markups.GetStartMarkup());


                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("message caption is too long"))
                    {
                        // Если ошибка связана с длинной подписью, разбиваем сообщение на более короткие части
                        //int maxCaptionLength = 4096; // Максимальная длина подписи в Telegram
                        //var captionChunks = SplitCaption(nasaData.Explanation, maxCaptionLength);

                        //foreach (var chunk in captionChunks)
                        //{
                        //    await _botClient.SendTextMessageAsync(message.Chat.Id, chunk);

                        //}
                        await _botClient.EditMessageTextAsync(
                     chatId: chatId,
                     messageId: message.MessageId,
                     text: "Ошибка сервера"
                     );

                    }
                    else
                    {
                        // Если это другая ошибка, выводим её
                        Console.WriteLine($"Error: {ex.Message}");
                        //await _botClient.SendTextMessageAsync(message.Chat.Id, $"{ex.Message}");
                        await _botClient.EditMessageTextAsync(
                     chatId: chatId,
                     messageId: message.MessageId,
                     text: "Фото дня пока недоступно"
                     );
                    }
                    static string[] SplitCaption(string caption, int maxLength)
                    {
                        // Разбиваем текст на части, учитывая максимальную длину
                        int index = 0;
                        var chunks = new System.Collections.Generic.List<string>();

                        while (index < caption.Length)
                        {
                            int length = Math.Min(maxLength, caption.Length - index);
                            chunks.Add(caption.Substring(index, length));
                            index += length;
                        }

                        return chunks.ToArray();
                    }
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, "Не удалось получить данные от NASA.", cancellationToken: cancellationToken);
                await _botClient.EditMessageTextAsync(
                     chatId: chatId,
                     messageId: message.MessageId,
                     text: "Не удалось получить данные от NASA."
                     );
            }


        }
        public async Task SendPaymentListAsync(User user, long chatId,Message message, CancellationToken cancellationToken)
        {
            string text = "Выберите способ оплаты";
            await _botClient.EditMessageTextAsync(
               chatId: chatId,
               text: text,
               messageId: message.MessageId,
               replyMarkup: Markups.GetPaymentsMarkup(),
               disableWebPagePreview: false,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
           );
        }
        public async Task SendPaymentAsyncInStars(User user, long chatId, Message message, string description, string subscription,int[] prices, CancellationToken cancellationToken)
        {
            string text = "Выберите способ оплаты";
            await _botClient.EditMessageTextAsync(
               chatId: chatId,
               text: text,
               messageId: message.MessageId,
               replyMarkup: Markups.GetTelegramStarsPriceList(await GetPayinStarsUrlPriceDictionary(user,description,"XTR",prices,cancellationToken)),
               disableWebPagePreview: false,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
           );
        }
        private async Task<Dictionary<int, string>> GetPayinStarsUrlPriceDictionary(User user, string description, string currency, int[] prices, CancellationToken cancellationToken)
        {
            var links = new Dictionary<int, string>();

            foreach (var price in prices)
            {
                var payUrl = await _botClient.CreateInvoiceLinkAsync(
                    title: "Пожертвование в бот",
                    description: description,
                    payload: user.Id.ToString(),
                    providerToken: String.Empty,
                    currency: currency,
                    prices: new List<LabeledPrice> { new LabeledPrice(currency, price) },
                    cancellationToken: cancellationToken
                );
                links.Add(price, payUrl);
            }

            return links;
        }
        private async Task PreCheckoutRequest(PreCheckoutQuery preCheckoutQuery, CancellationToken cancellationToken) 
        {
            await Console.Out.WriteLineAsync("старт PreCheckoutRequest");
            await _botClient.AnswerPreCheckoutQueryAsync(preCheckoutQuery.Id);
            await Console.Out.WriteLineAsync(preCheckoutQuery.ToString());
            await Console.Out.WriteLineAsync("конец PreCheckoutRequest");
            return;
        }
        private async Task BotOnPreCheckoutQuery(PreCheckoutQuery preCheckoutQuery, CancellationToken cancellationToken) 
        {
            await _botClient.AnswerPreCheckoutQueryAsync(preCheckoutQuery.Id);
            return;
        }
        private  async Task SendStartMessage(long chatId)
        {
            await _botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "Выберите действие",
               replyMarkup: Markups.GetStartMarkup()

           );

        }
        private async Task SendStartMessage(long chatId, int messageId)
        {
                    await _botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: messageId,
                text: "Выберите действие", // Используем невидимый символ вместо текста
                replyMarkup: Markups.GetStartMarkup()
            );
        }
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }

       
    }
}


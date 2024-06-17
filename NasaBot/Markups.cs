using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NasaBot
{
    public static class Markups
    {
        public static InlineKeyboardMarkup GetStartMarkup()
        {
            return new InlineKeyboardMarkup(new[]
            {
            new []
            {
                InlineKeyboardButton.WithCallbackData("Вызвать случайное фото NASA", "/nasapicture"),
            },
             new []
            {
                InlineKeyboardButton.WithCallbackData("Вызвать сегодняшнее фото NASA", "/nasatodaypicture"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("Пожертвование", "/donate"),
            }
        });
        }
        public static InlineKeyboardMarkup GetPaymentsMarkup()
        {
            return new InlineKeyboardMarkup(new[]
            {
            new []
            {
                //InlineKeyboardButton.WithUrl("\"👛 Telegram Stars\"", payurl),
                InlineKeyboardButton.WithCallbackData("\"👛 Telegram Stars\"", "/payinstars")
            },
            //new []
            //{
            //    InlineKeyboardButton.WithCallbackData("\"👛 Другой способ оплаты\"", "/start")
            //},
            new []
            {
                InlineKeyboardButton.WithCallbackData("Назад", "/start")
            }
        });
        }
        public static InlineKeyboardMarkup GetTelegramStarsPriceList(Dictionary<int, string> priceUrls)
        {
            var buttons = priceUrls.Select(priceUrl =>
            new[]
            {
                InlineKeyboardButton.WithUrl($"👛 Заплатить {priceUrl.Key} 🌟", priceUrl.Value)
            }
            ).ToList();

            // Добавляем кнопку "Go Back"
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад", "/donate")
            });

            return new InlineKeyboardMarkup(buttons);
        }

    }
}

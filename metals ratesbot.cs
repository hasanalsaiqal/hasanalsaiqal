using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace MetalsRatesBot
{
    class Program
    {
        private static TelegramBotClient? Bot;

        public static async Task Main()
        {
            Bot = new TelegramBotClient("5017478606:AAEzGf2TaUaQk_XZCnkk58QIj2CjfAca3jQ");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "Metals Rates Bot";
            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }


        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }



        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;


            var action = message.Text switch
            {
                "rates" => GetMetalsRates(botClient, message),
                "/help" or "/start" => help(botClient, message),
                _ => help(botClient, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");


            static async Task<Message> help(ITelegramBotClient botClient, Message message)
            {


                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text:
                                                                  "/help - Get help\n" +
                                                                  "Type 'rates' to find out Precious metals rates\n"+
                                                                  "The API will return real-time metals rates data updated every 60 minutes"
                                                            );
            }



            static async Task<Message> GetMetalsRates(ITelegramBotClient botClient, Message message)
            {
                /*
                string msg = message.Text;
                string pattern = "dd/MM/yyyy";
                DateTime dt;
                DateTime maxDate = DateTime.UtcNow.Date;
                //string date = DateTime.Now.ToString("ddMMyyyy");
               
                */
                        string URL = "https://www.metals-api.com/api/latest";
                        string urlParameters = "?access_key=0bs07w107i48yqzf728d117jl1kok4foj8p31gz7210hbue2kap2qud1y72b&base=USD";
                        HttpClient client = new HttpClient(new HttpClientHandler
                        {
                            AllowAutoRedirect = true,
                        }
                        );
                        client.BaseAddress = new Uri(URL);


                        //client.DefaultRequestHeaders.Add("x-auth-token", "goldapi-2rdpxtkxtfoiaf-io");


                        // Add an Accept header for JSON format.
                        client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                        

                        // List data response.
                        HttpResponseMessage response = client.GetAsync(urlParameters).Result;

                        //Console.WriteLine(response);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            //Console.WriteLine("dddd");
                            var datsaas = response.Content.ReadAsStringAsync().Result;
                            //Console.WriteLine(datsaas);
                            JObject json = JObject.Parse(datsaas);
                            //Console.WriteLine(json);
                            string rates = json["rates"].ToString();
                            Console.WriteLine(rates);
                            if (rates.Length > 4096)
                            {
                                for(int i =0; i <= rates.Length; i=i + 4096)
                                {
                                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                   text: rates.Substring(i,i+4096));
                                }
                           
                            }
                            else
                            {
                                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                   text: rates);
                            }




                        }


                else
                {
                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                               text:
                                                                     "An error occurred in the server response \n" +
                                                                     "/help - Get help\n");
                }



                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text:
                                                                      "An error occurred in the server response \n" +
                                                                      "/help - Get help\n");






            }
        }

    }

}

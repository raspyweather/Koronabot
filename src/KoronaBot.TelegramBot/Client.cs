using KoronaBot.TelegramBot.Models;
using KoronaBot.TelegramBot.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KoronaBot.TelegramBot
{
    public class Client
    {
        private readonly TelegramBotClient _bot;
        private readonly BotConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ICaseDataRepository _caseRepository;
        private readonly ILogger<Client> _logger;
        private CronlikeTimer _dailyScheduler;

        public Client(IOptions<BotConfiguration> configuration,
                      ILogger<Client> logger,
                      IUserRepository userRepository,
                      ICaseDataRepository caseRepository)
        {
            _configuration = configuration.Value;
            _bot = new TelegramBotClient(_configuration.BotToken);
            _userRepository = userRepository;
            _caseRepository = caseRepository;
            _logger = logger;
        }

        public async Task Start()
        {
            var me = await _bot.GetMeAsync();
            Console.Title = me.Username;

            _dailyScheduler = new CronlikeTimer("* * * * *", () => SendDailyNotification().Start());
            //            _dailyScheduler = new CronlikeTimer("0 19 * * *", () => SendDailyNotification().Start());

            _dailyScheduler.Start();

            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnMessageEdited += BotOnMessageReceived;
            // _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            _bot.OnReceiveError += BotOnReceiveError;

            _bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
        }

        public Task Stop()
        {
            _bot.StopReceiving();
            return Task.CompletedTask;
        }


        private async Task SendDailyNotification()
        {
            try
            {
                foreach (var user in await this._userRepository.GetUsers())
                {
                    await NotifyUser(user);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not send daily tasks");
            }
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            _logger.LogInformation("{ChatId} {UserId} {messageText}", messageEventArgs.Message.Chat.Id, messageEventArgs.Message.From.Id, messageEventArgs.Message.Text);

            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            switch (GetMessageCommand(message.Text))
            {
                // starts county selection
                case "/start":
                    {
                        await _bot.SendTextMessageAsync(message.Chat, "😎");
                        await Usage(message);
                        await ShowCountySelection(message);
                        break;
                    }
                case "/county":
                    await ShowCountySelection(message);
                    break;
                case "/usage":
                    await Usage(message);
                    break;
                case "/get":
                    await NotifyUser(message);
                    break;
                case "/cancel":
                    await _bot.SendTextMessageAsync(message.Chat, "😎");
                    break;
                default:
                    await CountySelectionList(message);
                    break;
            }
        }

        private async Task ShowCountySelection(Message message)
        {
            // send current county
            // if answered ask if correct if 1 is found, else list and ask again
            var user = await this._userRepository.GetUser(message.Chat.Id.ToString());

            string selectionMessage = "Please type the name of your county.";
            if (user != null)
            {
                var region = user.CountyId;
                selectionMessage += $"\n\nCurrently selected: {region}";
            }
            await _bot.SendTextMessageAsync(message.Chat, selectionMessage);
        }

        private async Task CountySelectionList(Message message)
        {
            string answer = message.Text;
            var counties = await this._caseRepository.FindCounties(answer);
            if (counties.Length == 1)
            {
                await this._userRepository.UpsertUser(new Models.UserData
                {
                    CountyId = counties.First(),
                    UserId = message.Chat.Id.ToString()
                });
                await this._bot.SendTextMessageAsync(message.Chat, $"{counties.First().ToString()} selected");
                await this.NotifyUser(message);
                return;
            }
            if (counties.Length > 0)
            {
                await this._bot.SendTextMessageAsync(message.Chat, $"Please be more Found following matches: {string.Join("\n", counties)}");
            }

        }

        private async Task NotifyUser(Message message)
        {
            try
            {
                var userId = message.Chat.Id.ToString();
                var user = await this._userRepository.GetUser(userId);
                await NotifyUser(user);
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
            }
        }

        private async Task NotifyUser(UserData user)
        {
            try
            {
                var caseData = await this._caseRepository.Get(user.CountyId);

                string file = System.IO.Path.Combine("Assets",
                    !caseData.HasValue ?
                        "error.jpg" :
                        $"{Math.Max(0, Math.Min(400, caseData.Value - 12.5 + (400 - caseData.Value + 12.5) % 25))}.jpg");

                var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                await this._bot.SendPhotoAsync(user.UserId,
                    new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, caseData.ToString()),
                    caption: $"{user.CountyId}: {caseData.Value}");
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
            }
        }

        private async Task Usage(Message message)
        {
            string usage = string.Join(Environment.NewLine,
                                   "Usage:",
                                   "/county - gets or sets current county",
                                   "/get - get today's data if you missed it");
            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        private string GetMessageCommand(string messageString)
        {
            return messageString.Split(' ')[0].Split('@')[0];
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}


using System;
using System.Threading;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Configurations;
using KNFA.Bots.MTB.Events.Telegram;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KNFA.Bots.MTB.Services.Telegram
{
    public interface ITelegramMessageSender
    {
        Task SendTextMessage(string text, CancellationToken ct);
    }
    
    internal class TelegramService : IHostedService, ITelegramMessageSender
    {
        private readonly TelegramConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<TelegramService> _logger;
        private readonly TelegramBotClient _telegramBotClient;

        public TelegramService(TelegramConfiguration configuration, IMessageBus messageBus, ILogger<TelegramService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telegramBotClient = new TelegramBotClient(configuration.BotToken);
            _telegramBotClient.OnMessage += ProcessMessage;
            _telegramBotClient.OnReceiveError += LogReceiveError;
            _telegramBotClient.OnReceiveGeneralError += OnReceiveGeneralError;
        }

        public async Task SendTextMessage(string text, CancellationToken ct)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var chatId = new ChatId(_configuration.HostGroupId);

            var formattedText = text.Contains("\r\n")
                ? $"```\r\n{text}```"
                : $"`{text}`";

            await _telegramBotClient.SendTextMessageAsync(chatId, formattedText, ParseMode.Markdown, disableNotification: true,
                cancellationToken: ct);
        }

        private void ProcessMessage(object? sender, MessageEventArgs e)
        {
            var message = e.Message;
            var messageText = message.Text;

            if (message.Chat.Id != _configuration.HostGroupId)
            {
                _logger.LogInformation("Skipping message {message} from unknown chat {chatId} ({chatUsername})",
                    messageText, message.Chat.Id, message.Chat.Username);
                return;
            }

            var requester = message.From.Username ?? message.From.FirstName ?? message.From.Id.ToString();

            _logger.LogInformation("Incoming message {message} from host chat", messageText);

            var botHighlightPart = $"@{_configuration.BotUsername}";
            var commandText = messageText.Replace(botHighlightPart, string.Empty);

            switch (commandText)
            {
                case "/users":
                    _messageBus.Publish(new UserListRequested(requester));
                    break;
            }
        }

        private void LogReceiveError(object? sender, ReceiveErrorEventArgs e)
            => _logger.LogError(e.ApiRequestException, "Receive error occured: {errorMessage}", e.ApiRequestException.Message);

        private void OnReceiveGeneralError(object? sender, ReceiveGeneralErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "Receive error occured: {errorMessage}", e.Exception.Message);
            Environment.FailFast("Telegram service failed");
        }

        public Task StartAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _telegramBotClient.StartReceiving(new[] { UpdateType.Message });
            _logger.LogInformation("Telegram service started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _telegramBotClient.StopReceiving();
            return Task.CompletedTask;
        }
    }
}

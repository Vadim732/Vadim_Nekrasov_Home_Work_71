using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeBot;

public class Bot
{
    private readonly TelegramBotClient _botClient;

    public Bot()
    {
        _botClient = new TelegramBotClient("7694688524:AAH_ShYb0PgWU9kwxjzmP6zbYMo8lloE2ak");
    }

    public void StartBot()
    {
        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        while (true)
        {
            Console.WriteLine("Bot is running...");
            Thread.Sleep(int.MaxValue);
        }
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                await HandleMessageAsync(botClient, update.Message, cancellationToken);
                break;
            case UpdateType.CallbackQuery:
                await HandleCallBackQueryAsync(botClient, update, cancellationToken);
                break;
        }
    }

    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        if (message == null || message.From == null) return;
        
        var user = message.From;
        var text = message.Text ?? string.Empty;
        Console.WriteLine($"{user.Username} ({user.FirstName}): {message.Text}");

        switch (text)
        {
            case "/start":
                await CommandStart(message);
                break;
            case "/help":
                await CommandHelp(message);
                break;
            case "/game":
                await CommandGame(message);
                break;
            case "/rock":
            case "/scissors":
            case "/paper":
                await RPS(message, text.Substring(1));
                break;
            case "/repeat":
                await CommandGame(message);
                break;
            case "/end":
                await ChoiceEnt(message);
                break;
            default:
                await _botClient.SendTextMessageAsync(message.Chat.Id, "Error: Unknown command! (If you want to start, enter the command \"/start\".)");
                break;
        }
    }

    private async Task CommandStart(Message mes)
    {
        var user = mes.From;
        var replyMarkup = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Start", "start"),
                InlineKeyboardButton.WithCallbackData("Rules", "help")
            }
        });

        await _botClient.SendTextMessageAsync(mes.Chat.Id,
            $"Hello, {user.FirstName}!\n" +
            "To start the game, click the \"Start\" button or enter the command \"/game\".\n" +
            "If you want to read the rules of the game, click on the \"Rules\" button, or enter the command \"/help\".",
            replyMarkup: replyMarkup);
    }

    private async Task CommandHelp(Message message)
    {
        var rules = "Rules of the game \"Rock, Paper, Scissors\":\n" +
                    "1. Rock beats scissors.\n" +
                    "2. Scissors beat paper.\n" +
                    "3. Paper beats stone.";
        await _botClient.SendTextMessageAsync(message.Chat.Id, rules);
    }

    private async Task CommandGame(Message message)
    {
        var replyMarkup = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Rock", "rock"),
                InlineKeyboardButton.WithCallbackData("Scissors", "scissors"),
                InlineKeyboardButton.WithCallbackData("Paper", "paper")
            }
        });

        await _botClient.SendTextMessageAsync(message.Chat.Id, "Select an action:", replyMarkup: replyMarkup);
    }

    private async Task RPS(Message message, string userChoice)
    {
        var botChoice = GetRandomChoice();
        var result = RockPaperScissors(userChoice, botChoice);

        var replyMarkup = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Repeat", "repeat"),
                InlineKeyboardButton.WithCallbackData("End", "end")
            }
        });

        await _botClient.SendTextMessageAsync(message.Chat.Id,
            $"{result}\n" +
            $"\nYou: {userChoice}\n" +
            $"Bot: {botChoice}",
            replyMarkup: replyMarkup);
    }

    private async Task ChoiceEnt(Message message)
    {
        var user = message.From;
        await _botClient.SendTextMessageAsync(message.Chat.Id,
            $"{user.FirstName}, thanks for playing!");
    }

    public async Task HandleCallBackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery == null || update.CallbackQuery.Data == null)
            return;

        var callbackQuery = update.CallbackQuery;
        var callbackData = callbackQuery.Data;
        var chatId = callbackQuery.Message.Chat.Id;

        switch (callbackData)
        {
            case "start":
                await CommandGame(callbackQuery.Message);
                break;
            case "help":
                await CommandHelp(callbackQuery.Message);
                break;
            case "rock":
            case "scissors":
            case "paper":
                await RPS(callbackQuery.Message, callbackData);
                break;
            case "repeat":
                await CommandGame(callbackQuery.Message);
                break;
            case "end":
                await ChoiceEnt(callbackQuery.Message);
                break;
            default:
                await botClient.SendTextMessageAsync(chatId, "Error: Unknown command!");
                break;
        }

        await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, null);
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
    }

    private string GetRandomChoice()
    {
        var choices = new[] { "rock", "scissors", "paper" };
        var random = new Random();
        return choices[random.Next(choices.Length)];
    }

    public static string RockPaperScissors(string first, string second) => (first, second) switch
    {
        ("rock", "paper") => "rock is covered by paper. Paper wins.",
        ("rock", "scissors") => "rock breaks scissors. Rock wins.",
        ("paper", "rock") => "paper covers rock. Paper wins.",
        ("paper", "scissors") => "paper is cut by scissors. Scissors wins.",
        ("scissors", "rock") => "scissors is broken by rock. Rock wins.",
        ("scissors", "paper") => "scissors cuts paper. Scissors wins.",
        (_, _) => "tie"
    };

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
    }
}

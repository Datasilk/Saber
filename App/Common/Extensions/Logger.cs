using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
namespace Saber.Common.Extensions
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder AddSaberLogFormatter(this ILoggingBuilder builder) =>
            builder.AddConsole(options => options.FormatterName = nameof(LoggingFormatter))
                .AddConsoleFormatter<LoggingFormatter, ConsoleFormatterOptions>();
    }

    public sealed class LoggingFormatter : ConsoleFormatter, IDisposable
    {
        private readonly IDisposable _optionsReloadToken;
        private ConsoleFormatterOptions _formatterOptions;
        public LoggingFormatter(IOptionsMonitor<ConsoleFormatterOptions> options): base(nameof(LoggingFormatter)) => (_optionsReloadToken, _formatterOptions) = (options.OnChange(ReloadLoggerOptions), options.CurrentValue);
        private void ReloadLoggerOptions(ConsoleFormatterOptions options) => _formatterOptions = options;

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            string message =
                logEntry.Formatter?.Invoke(
                    logEntry.State, logEntry.Exception);

            if (message is null)
            {
                return;
            }

            textWriter.WriteLine($"{message}");
        }
        public void Dispose() => _optionsReloadToken?.Dispose();
    }
}


namespace Own.BlockchainExplorer.Common.Framework
{
    public enum AlertType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class Alert
    {
        public AlertType AlertType { get; }
        public string Message { get; }

        private Alert(AlertType alertType, string message)
        {
            AlertType = alertType;
            Message = message;
        }

        public static Alert Info(string message, params object[] args) =>
            new Alert(AlertType.Info, string.Format(message, args));
        public static Alert Success(string message, params object[] args) =>
            new Alert(AlertType.Success, string.Format(message, args));
        public static Alert Warning(string message, params object[] args) =>
            new Alert(AlertType.Warning, string.Format(message, args));
        public static Alert Error(string message, params object[] args) =>
            new Alert(AlertType.Error, string.Format(message, args));

        public override string ToString() => string.Format("[{0}] {1}", AlertType, Message);
    }
}

namespace DaresGameBot.Web.Models
{
    public sealed class ErrorViewModel
    {
        public string RequestId { get; internal set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
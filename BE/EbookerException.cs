namespace BE
{
    internal class EbookerException : Exception
    {
        public EbookerException(string message) : base (message) { }
        public EbookerException(string message, Exception ex) : base(message, ex) { }
    }
}

using System;

namespace Saber
{
    public class PageErrorException : Exception
    {
        public PageErrorException(string message) { }
    }

    public class PageDeniedException : Exception
    {
        public PageDeniedException(string message) { }
    }

    public class ServiceErrorException : Exception
    {
        public ServiceErrorException(string message) { }
    }

    public class ServiceDeniedException : Exception
    {
        public ServiceDeniedException(string message) { }
    }
}

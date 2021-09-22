using System;

namespace MementoHealth.Exceptions
{
    public class ExceededPinAccessFailedCountException : Exception
    {
        public ExceededPinAccessFailedCountException(int maxCount) : base("User exceeded max PIN access fails of " + maxCount) { }
    }
}
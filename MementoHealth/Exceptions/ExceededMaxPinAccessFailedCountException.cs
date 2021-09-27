using System;

namespace MementoHealth.Exceptions
{
    public class ExceededMaxPinAccessFailedCountException : Exception
    {
        public ExceededMaxPinAccessFailedCountException(int maxCount) 
            : base("User exceeded the maximum PIN access fails of " + maxCount + ".") { }
    }
}
using System;

namespace Cimpress.Stereotype.Exceptions
{
    public class StereotypeException : Exception
    {
        private readonly string _additional;

        public StereotypeException(string message, string additional)
            : base(message)
        {
            _additional = additional;
        }

        public string AdditionInfo()
        {
            return _additional;
        }
    }
}

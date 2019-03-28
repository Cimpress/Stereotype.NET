using System;

namespace Cimpress.Stereotype.Exceptions
{
    public class AuthorizationException : Exception
    {
        private readonly string _additional;

        public AuthorizationException(string message)
            : base(message)
        {
           
        }
       
    }
}

using System;

namespace Cimpress.Stereotype.Exceptions
{
    public class AuthenticationException : Exception
    {
        private readonly string _additional;

        public AuthenticationException(string message)
            : base(message)
        {
           
        }
       
    }
}

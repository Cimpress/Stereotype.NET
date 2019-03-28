using System;

namespace Cimpress.Stereotype.Exceptions
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message)
            : base(message)
        {
           
        }
       
    }
}

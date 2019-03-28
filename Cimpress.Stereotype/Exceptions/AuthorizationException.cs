using System;

namespace Cimpress.Stereotype.Exceptions
{
    public class AuthorizationException : Exception
    {
        public AuthorizationException(string message)
            : base(message)
        {
           
        }
       
    }
}

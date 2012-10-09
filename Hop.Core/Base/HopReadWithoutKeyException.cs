using System;

namespace Hop.Core.Base
{
    public class HopReadWithoutKeyException : Exception
    {
        public HopReadWithoutKeyException(object target)
        {
            Target = target;
        }

        public object Target { get; set; }
    }
}
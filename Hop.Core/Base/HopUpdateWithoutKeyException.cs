using System;

namespace Hop.Core.Base
{
    public class HopUpdateWithoutKeyException : Exception
    {
        public HopUpdateWithoutKeyException(object target)
        {
            Target = target;
        }

        public object Target { get; set; }
    }
}
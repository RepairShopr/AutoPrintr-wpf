using System;

namespace AutoPrintr.Core.Models
{
    public abstract class BaseModel : ICloneable
    {
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
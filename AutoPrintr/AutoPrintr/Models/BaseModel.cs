using System;

namespace AutoPrintr.Models
{
    public abstract class BaseModel : ICloneable
    {
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
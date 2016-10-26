using System;

namespace AutoPrintr.Core.Models
{
    [Serializable]
    public abstract class BaseModel : ICloneable
    {
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
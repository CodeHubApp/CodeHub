using System;

namespace CodeFramework.Core.ViewModels
{
    [Serializable]
    public abstract class FilterModel<TF>
    {
        public abstract TF Clone();
    }
}


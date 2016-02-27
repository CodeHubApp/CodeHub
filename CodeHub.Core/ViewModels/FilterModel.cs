using System;

namespace CodeHub.Core.ViewModels
{
    [Serializable]
    public abstract class FilterModel<TF>
    {
        public abstract TF Clone();
    }
}


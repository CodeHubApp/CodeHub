using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using Splat;

namespace CodeHub.Core.Services
{
    public class ViewModelViewService : IViewModelViewService, IEnableLogger
    {
        private readonly Dictionary<Type, Type> _viewModelViewDictionary = new Dictionary<Type, Type>();

        public void RegisterViewModels(System.Reflection.Assembly asm)
        {
            foreach (var type in asm.DefinedTypes.Where(x => !x.IsAbstract && x.ImplementedInterfaces.Any(y => y == typeof(IViewFor))))
            {
                var viewForType = type.ImplementedInterfaces.FirstOrDefault(
                                      x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IViewFor<>));
                Register(viewForType.GenericTypeArguments[0], type.AsType());
            }
        }

        public void Register(Type viewModelType, Type viewType)
        {
            this.Log().Debug("Registering view model {0} to view {1}", viewModelType.Name, viewType.Name);
            _viewModelViewDictionary.Add(viewModelType, viewType);
        }

        public Type GetViewFor(Type viewModel)
        {
            Type viewType;
            return !_viewModelViewDictionary.TryGetValue(viewModel, out viewType) ? null : viewType;
        }
    }
}


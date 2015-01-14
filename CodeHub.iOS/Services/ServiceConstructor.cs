using System;
using System.Linq;
using System.Collections.Generic;
using Splat;
using CodeHub.Core.Services;
using System.Diagnostics;

namespace CodeHub.iOS.Services
{
    public class ServiceConstructor : IServiceConstructor
    {
        public object Construct(Type type)
        {
            var constructor = type.GetConstructors().First();
            var parameters = constructor.GetParameters();
            var args = new List<object>(parameters.Length);
            foreach (var p in parameters)
            {
                var argument = Locator.Current.GetService(p.ParameterType);
                if (argument == null)
                    Debugger.Break();
                args.Add(argument);
            }
            return Activator.CreateInstance(type, args.ToArray(), null);
        }
    }
}


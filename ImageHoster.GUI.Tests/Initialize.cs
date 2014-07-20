using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sogyo.CQRS.Library.Infrastructure.Persistence;
using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.ReadModel.Eventhandlers;

namespace ImageHoster.GUI.Tests
{
    [TestClass]
    public class InitializeClass
    {
        public static void RegisterHandlers(FakeBus bus, object obj)
        {
            var interfaces = obj.GetType().GetInterfaces().SelectMany(i => i.GetGenericArguments());

            foreach (var i in interfaces)
            {
                var handleMethod = obj.GetType().GetMethod("Handle", new Type[] { i });
                var del = Delegate.CreateDelegate(Expression.GetActionType(i), obj, handleMethod);

                MethodInfo register = bus.GetType().GetMethod("RegisterHandler").MakeGenericMethod(i);
                register.Invoke(bus, new object[] { del });
            }
        }
    }
}

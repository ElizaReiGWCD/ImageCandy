using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sogyo.CQRS.Library.Infrastructure.Bus
{
    public class AsynchronousBus : IBus
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<IMessage>>> _routes = new ConcurrentDictionary<Type, ConcurrentBag<Action<IMessage>>>();
        public readonly FakeBus SynchronousBus = new FakeBus();
        //private delegate void handle(IMessage message);

        public async void RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            await Task.Run(() =>
            {
                ConcurrentBag<Action<IMessage>> handlers;
                if (!_routes.TryGetValue(typeof(T), out handlers))
                {
                    handlers = new ConcurrentBag<Action<IMessage>>();
                    _routes.TryAdd(typeof(T), handlers);
                }
                handlers.Add(DelegateAdjuster.CastArgument<IMessage, T>(x => handler(x)));
                SynchronousBus.RegisterHandler(handler);
            });
        }

        public async void Send<T>(T command) where T : Command
        {
            await Task.Run(() =>
            {
                ConcurrentBag<Action<IMessage>> handlers;
                if (_routes.TryGetValue(typeof(T), out handlers))
                {
                    if (handlers.Count != 1) 
                        throw new InvalidOperationException("cannot send to more than one handler");
                    
                    Task.Run(() => handlers.First()(command));
                }
                else
                {
                    throw new InvalidOperationException("no handler registered");
                }
            });
        }

        public async void Publish<T>(T @event) where T : Event
        {
            await Task.Run(() =>
            {
                ConcurrentBag<Action<IMessage>> handlers;
                if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;
                foreach (var handler in handlers)
                {
                    //dispatch on thread pool for added awesomeness
                    var handler1 = handler;
                    Task.Run(() => handler1(@event));
                }
            });
        }

        public FakeBus GenerateSynchronousBus()
        {
            FakeBus bus = new FakeBus();

            foreach (var kv in _routes)
            {
                foreach (var v in kv.Value)
                {
                    bus.RegisterHandler(v);
                }
            }

            return bus;
        }
    }
}

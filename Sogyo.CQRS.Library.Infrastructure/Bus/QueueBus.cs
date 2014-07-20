using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sogyo.CQRS.Library.Infrastructure.Bus
{
    public class QueueBus : IBus
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<IMessage>>> _routes = new ConcurrentDictionary<Type, ConcurrentBag<Action<IMessage>>>();

        private readonly ConcurrentQueue<Command> Commands = new ConcurrentQueue<Command>();
        private Task CommandTask { get; set; }

        private readonly ConcurrentQueue<Event> Events = new ConcurrentQueue<Event>();
        private Task EventTask { get; set; }

        public readonly FakeBus SynchronousBus = new FakeBus();
        //private delegate void handle(IMessage message);

        public void RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            ConcurrentBag<Action<IMessage>> handlers;
            if (!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new ConcurrentBag<Action<IMessage>>();
                _routes.TryAdd(typeof(T), handlers);
            }
            handlers.Add(DelegateAdjuster.CastArgument<IMessage, T>(x => handler(x)));
            SynchronousBus.RegisterHandler(handler);
        }

        public void Send<T>(T command) where T : Command
        {
            this.Commands.Enqueue(command);

            if (CommandTask == null || CommandTask.IsCompleted)
                CommandTask = SendCommands();
        }

        public async Task SendCommands()
        {
            await Task.Run(() =>
            {
                Command command;

                while (Commands.TryDequeue(out command))
                {
                    Debug.WriteLine(command.GetType());

                    ConcurrentBag<Action<IMessage>> handlers;
                    if (_routes.TryGetValue(command.GetType(), out handlers))
                    {
                        if (handlers.Count != 1)
                            throw new InvalidOperationException("cannot send to more than one handler");

                        Task.Run(() => handlers.First()(command));
                    }
                    else
                    {
                        throw new InvalidOperationException("no handler registered");
                    }
                }
            });
        }

        public async void Publish<T>(T @event) where T : Event
        {
            this.Events.Enqueue(@event);

            if (EventTask == null || EventTask.IsCompleted)
                EventTask = SendEvents();
        }


        public async Task SendEvents()
        {
            await Task.Run(() =>
            {
                Event @event;

                while (Events.TryDequeue(out @event))
                {
                    ConcurrentBag<Action<IMessage>> handlers;
                    if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;
                    foreach (var handler in handlers)
                    {
                        //dispatch on thread pool for added awesomeness
                        var handler1 = handler;
                        Task.Run(() => handler1(@event));
                    }
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

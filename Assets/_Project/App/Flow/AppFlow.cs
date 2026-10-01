using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Template.App.Flow
{
    public sealed class AppFlow : IAppFlow
    {
        private readonly Dictionary<AppScreen, IFlowHandler> _handlers;
        public AppScreen Current { get; private set; }
        private AppScreen _transitionTarget;
        private bool _isTransitioning;
        private IFlowHandler _currentHandler;

        public AppFlow(IEnumerable<IFlowHandler> handlers)
        {
            if (handlers == null)
                throw new ArgumentNullException(nameof(handlers));

            _handlers = new Dictionary<AppScreen, IFlowHandler>();

            foreach (IFlowHandler handler in handlers)
            {
                if (_handlers.ContainsKey(handler.Screen))
                    throw new InvalidOperationException($"Duplicate flow handler registered for {handler.Screen}.");

                _handlers.Add(handler.Screen, handler);
            }
        }

        public async UniTask StartAsync()
        {
            await GoToAsync(AppScreen.Boot);
            await GoToAsync(AppScreen.MainMenu);
        }

        public async UniTask GoToAsync(AppScreen target)
        {
            if (_isTransitioning)
            {
                if (_transitionTarget == target)
                    return;

                throw new InvalidOperationException($"Cannot transition to {target} while transition to {_transitionTarget} is in progress.");
            }

            if (!_handlers.TryGetValue(target, out IFlowHandler nextHandler))
                throw new InvalidOperationException($"No flow handler registered for {target}.");

            _isTransitioning = true;
            _transitionTarget = target;

            try
            {
                if (_currentHandler != null)
                {
                    await _currentHandler.ExitAsync();
                }

                await nextHandler.EnterAsync();

                Current = target;
                _currentHandler = nextHandler;
            } 
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}
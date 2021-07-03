using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Events;
using Rg.Plugins.Popup.Exceptions;
using Rg.Plugins.Popup.Pages;

using Xamarin.Forms;

namespace Rg.Plugins.Popup.Services
{
    internal class PopupNavigationImpl : IPopupNavigation
    {
        private readonly object _locker = new object();

        private readonly List<PopupPage> _popupStack = new List<PopupPage>();

        public event EventHandler<PopupNavigationEventArgs>? Pushing;

        public event EventHandler<PopupNavigationEventArgs>? Pushed;

        public event EventHandler<PopupNavigationEventArgs>? Popping;

        public event EventHandler<PopupNavigationEventArgs>? Popped;

        private static IPopupPlatform PopupPlatform
        {
            get
            {
                var popupPlatform = DependencyService.Get<IPopupPlatform>();

                if (popupPlatform == null)
                    throw new RGInitialisationException("You MUST install Rg.Plugins.Popup to each project and call Rg.Plugins.Popup.Popup.Init(); prior to using it.\nSee more info: " + Config.InitializationDescriptionUrl);

                if (!popupPlatform.IsInitialized)
                    throw new RGInitialisationException("You MUST call Rg.Plugins.Popup.Popup.Init(); prior to using it.\nSee more info: " + Config.InitializationDescriptionUrl);

                return popupPlatform;
            }
        }

        public IReadOnlyList<PopupPage> PopupStack => _popupStack;

        public PopupNavigationImpl()
        {
            PopupPlatform.OnInitialized += OnInitialized;
        }

        private async void OnInitialized(object sender, EventArgs e)
        {
            if (PopupStack.Count > 0)
                await PopAllAsync(false);
        }

        public Task PushAsync(PopupPage page, bool animate = true)
        {
            lock (_locker)
            {
                if (_popupStack.Contains(page))
                    return Task.FromResult(false);

                Pushing?.Invoke(this, new PopupNavigationEventArgs(page, animate));

                _popupStack.Add(page);

                var task = InvokeThreadSafe(async () =>
                {
                    animate = CanBeAnimated(animate);

                    if (animate)
                    {
                        page.PreparingAnimation();
                        await AddAsync(page);
                        await page.AppearingAnimation();
                    }
                    else
                    {
                        await AddAsync(page);
                    }

                    page.AppearingTransactionTask = null;

                    Pushed?.Invoke(this, new PopupNavigationEventArgs(page, animate));
                });

                page.AppearingTransactionTask = task;

                return task;
            }
        }

        public Task PopAsync(bool animate = true)
        {
            lock (_locker)
            {
                animate = CanBeAnimated(animate);
                if (!PopupStack.Any())
                    return Task.FromResult(false);

                return RemovePageAsync(PopupStack.Last(), animate);
            }
        }

        public Task PopAllAsync(bool animate = true)
        {
            lock (_locker)
            {
                animate = CanBeAnimated(animate);

                if (!PopupStack.Any())
                    return Task.FromResult(false);

                var popupTasks = PopupStack.ToList().Select(page => RemovePageAsync(page, animate));

                return Task.WhenAll(popupTasks);
            }
        }

        public Task PopAllAsync<T>(bool animate = true)
        {
            lock (_locker)
            {
                animate = CanBeAnimated(animate);

                if (!PopupStack.Any(p => p is T))
                    return Task.FromResult(false);

                var popupTasks = PopupStack.Where(p => p is T).Select(page => RemovePageAsync(page, animate));

                return Task.WhenAll(popupTasks);
            }
        }

        public Task RemovePageAsync(PopupPage page, bool animate = true)
        {
            lock (_locker)
            {
                if (page == null)
                    throw new RGPageInvalidException("Page cannot be null");

                if (!_popupStack.Contains(page))
                    return Task.FromResult(false);
                if (page.DisappearingTransactionTask != null)
                    return page.DisappearingTransactionTask;

                var task = InvokeThreadSafe(async () =>
                {
                    if (page.AppearingTransactionTask != null)
                        await page.AppearingTransactionTask;

                    lock (_locker)
                    {
                        if (!_popupStack.Contains(page))
                            return;
                    }

                    Popping?.Invoke(this, new PopupNavigationEventArgs(page, animate));

                    animate = CanBeAnimated(animate);

                    if (animate)
                        await page.DisappearingAnimation();

                    await RemoveAsync(page);

                    if (animate)
                        page.DisposingAnimation();

                    lock (_locker)
                    {
                        _popupStack.Remove(page);
                        page.DisappearingTransactionTask = null;

                        Popped?.Invoke(this, new PopupNavigationEventArgs(page, animate));
                    }
                });

                page.DisappearingTransactionTask = task;

                return task;
            }
        }

        // Private

        private static Task AddAsync(PopupPage page)
        {
            return PopupPlatform.AddAsync(page);
        }

        private static Task RemoveAsync(PopupPage page)
        {
            return PopupPlatform.RemoveAsync(page);
        }

        // Internal 

        internal void RemovePopupFromStack(PopupPage page)
        {
            if (_popupStack.Contains(page))
                _popupStack.Remove(page);
        }

        #region Animation

        private static bool CanBeAnimated(bool animate)
        {
            return animate && PopupPlatform.IsSystemAnimationEnabled;
        }

        #endregion

        #region Helpers

        private static Task InvokeThreadSafe(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<bool>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await action.Invoke();
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }

        #endregion
    }
}

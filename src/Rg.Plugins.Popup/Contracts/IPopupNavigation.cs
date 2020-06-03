using System.Collections.Generic;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;

namespace Rg.Plugins.Popup.Contracts
{
    public interface IPopupNavigation
    {
        IReadOnlyList<PopupPage> PopupStack { get; }

        /// <summary>
        /// Push a popup
        /// </summary>
        /// <param name="page"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task PushAsync(PopupPage page, bool animate = true);

        /// <summary>
        /// Pop a popup - Removes the last popup from the stack
        /// </summary>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task PopAsync(bool animate = true);

        /// <summary>
        /// Remove all popups
        /// </summary>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task PopAllAsync(bool animate = true);

        /// <summary>
        /// remove all specific type of popups
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task PopAllAsync<T>(bool animate = true);

        /// <summary>
        /// Remove a popup from the PopupStack
        /// </summary>
        /// <param name="page"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task RemovePageAsync(PopupPage page, bool animate = true);
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;

namespace Demo
{
    public class PromptDialogs
    {   
        /// <summary>
        /// clear navigation if there is any popup and pus a new one
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task PushNewPopup(Rg.Plugins.Popup.Pages.PopupPage page, Xamarin.Forms.INavigation _navigation = null)
        {
            try
            {
                //remove all popups first
                if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Any())
                {
                    await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAllAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }

            if (_navigation == null)
                _navigation = DependencyService.Get<INavigation>();

            await _navigation.PushPopupAsync(page);
        }

        public static async Task PopAllPopupAsync(INavigation navigation, bool animate = false)
        {
            try
            {
                if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Any())
                {
                    await Task.Delay(100);
                    await navigation.PopAllPopupAsync(animate);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }

        }


    }
}

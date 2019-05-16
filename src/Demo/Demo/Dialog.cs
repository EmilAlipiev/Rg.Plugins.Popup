using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.Pages;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;

namespace Demo
{
    public class Dialogs : System.IDisposable
    {
        public static async Task<Dialogs> Create(string message, INavigation navigation = null)
        {
            var myClass = new Dialogs();
            await myClass.Initialize(message, navigation);
            return myClass;
        }

        private Dialogs()
        {

        }

        private async Task Initialize(string message, INavigation navigation = null)
        {
            await LoadingAsync(message, navigation);
        }

        public static async Task HideLoading(INavigation _navigation = null)
        {
            try
            {
                if (_navigation == null)
                    _navigation = DependencyService.Get<INavigation>();

                await PromptDialogs.PopAllPopupAsync(_navigation);

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task ShowLoading(string Message = null, INavigation _navigation = null)
        {
            try
            {
                LoadingPopupPage page = new LoadingPopupPage();
                await  PromptDialogs.PushNewPopup(page);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task LoadingAsync(string Message = null, INavigation _navigation = null)
        {
            try
            {
                if (_navigation == null)
                    _navigation = DependencyService.Get<INavigation>();

                if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Any())
                {
                    await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAllAsync();
                }
                LoadingPopupPage page = new LoadingPopupPage();
                await _navigation.PushPopupAsync(page);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public async void Dispose()
        {
            try
            {
                var _navigation = DependencyService.Get<Xamarin.Forms.INavigation>();
                await HideLoading(_navigation);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

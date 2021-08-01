using System;
using System.Windows.Controls;

namespace TBotHamsti.Services
{
    public class PageService
    {
        public event Action<Page> OnPageChanged;
        public void ChangePage(Page page) => (OnPageChanged ?? throw new ArgumentNullException(nameof(OnPageChanged))).Invoke(page);
    }
}

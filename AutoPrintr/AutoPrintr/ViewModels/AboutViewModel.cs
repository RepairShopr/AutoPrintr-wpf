using AutoPrintr.Helpers;
using GalaSoft.MvvmLight.Views;
using System.Linq;
using System.Reflection;

namespace AutoPrintr.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        #region Properties
        public string Product { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }
        public string Copyright { get; private set; }
        public string Licence { get; private set; }
        public string About { get; private set; }

        public override ViewType Type => ViewType.About;
        #endregion

        #region Constructors
        public AboutViewModel(INavigationService navigationService)
            : base(navigationService)
        { }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);

            var assembly = Assembly.GetExecutingAssembly();
            Product = assembly.GetCustomAttributes(false).OfType<AssemblyProductAttribute>().Select(x => x.Product).First();
            Version = assembly.GetName().Version.ToString();
            Description = assembly.GetCustomAttributes(false).OfType<AssemblyDescriptionAttribute>().Select(x => x.Description).First();
            Copyright = $"{assembly.GetCustomAttributes(false).OfType<AssemblyCopyrightAttribute>().Select(x => x.Copyright).First()} {assembly.GetCustomAttributes(false).OfType<AssemblyCompanyAttribute>().Select(x => x.Company).First()}";

            Licence = Resources.License;
            About = Resources.About;
        }
        #endregion
    }
}
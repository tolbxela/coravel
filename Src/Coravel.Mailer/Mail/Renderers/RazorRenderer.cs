using System;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RazorLight;

namespace Coravel.Mailer.Mail.Renderers
{
    public class RazorRenderer
    {
        private IServiceProvider _serviceProvider;
        private string _logoSrc;
        private string _companyName;
        private string _companyAddress;
        private string _primaryColor;
        private IRazorLightEngine _razorEngine;

        public RazorRenderer(
            IServiceProvider serviceProvider,
            IConfiguration config,
            IRazorLightEngine razorEngine)
        {
            this._serviceProvider = serviceProvider;
            this._logoSrc = config?.GetValue<string>("Coravel:Mail:LogoSrc");
            this._companyName = config?.GetValue<string>("Coravel:Mail:CompanyName");
            this._companyAddress = config?.GetValue<string>("Coravel:Mail:CompanyAddress");
            this._primaryColor = config?.GetValue<string>("Coravel:Mail:PrimaryColor");
            this._razorEngine = razorEngine;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewPath, TModel model)
        {
            var viewBag = new ExpandoObject();
            this.BindConfigurationToViewBag(viewBag);

            return await this.RenderAsync(viewPath, model, viewBag);               
        }

        private async Task<string> RenderAsync<TModel>(string viewPath, TModel model, ExpandoObject viewBag)
        {
            var cached = this._razorEngine.Handler.Cache.RetrieveTemplate(viewPath);

            if(cached.Success)
            {
                return await this._razorEngine.RenderTemplateAsync(cached.Template.TemplatePageFactory(), model, viewBag);
            }            
            return await this._razorEngine.CompileRenderAsync(viewPath, model, viewBag);
        }

        private void BindConfigurationToViewBag(dynamic viewBag)
        {
            viewBag.LogoSrc = this._logoSrc;
            viewBag.CompanyName = this._companyName;
            viewBag.CompanyAddress = this._companyAddress;
            viewBag.PrimaryColor = this._primaryColor;
        }
    }
}
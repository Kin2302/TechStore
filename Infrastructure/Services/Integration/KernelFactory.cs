using Application.Interfaces.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using TechStore.Infrastructure.Plugins;

namespace TechStore.Infrastructure.Services.Integration
{
    public class KernelFactory : IKernelFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ProductPlugin _productPlugin;
        private readonly CartPlugin _cartPlugin;
        private readonly OrderPlugin _orderPlugin;
        private readonly StoreInfoPlugin _storeInfoPlugin;
        private readonly AdminReportPlugin _adminReportPlugin;
        private readonly AdminProductPlugin _adminProductPlugin;
        private readonly AdminOrderPlugin _adminOrderPlugin;

        public KernelFactory(
            IConfiguration configuration,
            ProductPlugin productPlugin,
            CartPlugin cartPlugin,
            OrderPlugin orderPlugin,
            StoreInfoPlugin storeInfoPlugin,
            AdminReportPlugin adminReportPlugin,
            AdminProductPlugin adminProductPlugin,
            AdminOrderPlugin adminOrderPlugin)
        {
            _configuration = configuration;
            _productPlugin = productPlugin;
            _cartPlugin = cartPlugin;
            _orderPlugin = orderPlugin;
            _storeInfoPlugin = storeInfoPlugin;
            _adminReportPlugin = adminReportPlugin;
            _adminProductPlugin = adminProductPlugin;
            _adminOrderPlugin = adminOrderPlugin;
        }

        public Kernel CreateCustomerKernel()
        {
            var apiKey = _configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API Key not found");

            var builder = Kernel.CreateBuilder();
            builder.AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", apiKey: apiKey);

            // Customer plugins only
            builder.Plugins.AddFromObject(_productPlugin, "ProductTools");
            builder.Plugins.AddFromObject(_cartPlugin, "CartTools");
            builder.Plugins.AddFromObject(_orderPlugin, "OrderTools");
            builder.Plugins.AddFromObject(_storeInfoPlugin, "StoreInfoTools");

            return builder.Build();
        }

        public Kernel CreateAdminKernel()
        {
            var apiKey = _configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API Key not found");

            var builder = Kernel.CreateBuilder();
            builder.AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", apiKey: apiKey);

            // ALL plugins (customer + admin)
            builder.Plugins.AddFromObject(_productPlugin, "ProductTools");
            builder.Plugins.AddFromObject(_cartPlugin, "CartTools");
            builder.Plugins.AddFromObject(_orderPlugin, "OrderTools");
            builder.Plugins.AddFromObject(_storeInfoPlugin, "StoreInfoTools");
            builder.Plugins.AddFromObject(_adminReportPlugin, "AdminReportTools");
            builder.Plugins.AddFromObject(_adminProductPlugin, "AdminProductTools");
            builder.Plugins.AddFromObject(_adminOrderPlugin, "AdminOrderTools");

            return builder.Build();
        }
    }
}

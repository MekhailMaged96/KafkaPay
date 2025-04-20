using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KafkaPay.AccountingService.Application
{
    public static class DependencyInjection
    {
        public static void AddAccountApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace KafkaPay.TransferService.Application.Handlers
{
    public class TelemetryHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            //var activity = Activity.Current;
            //if (activity != null)
            //{
            //    var context = new PropagationContext(activity.Context, Baggage.Current);
            //    Propagators.DefaultTextMapPropagator.Inject(
            //        context,
            //        request.Headers,
            //        (headers, key, value) => headers.TryAddWithoutValidation(key, value)
            //    );
            //}
            return await base.SendAsync(request, cancellationToken);
        }
    }

}

using Microsoft.Extensions.Options;

namespace QuakeNote.Shared.Web.Auth;

public sealed class ServiceSecretHandler(IOptions<ServiceAuthOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove(ServiceAuthConstants.HeaderName);
        request.Headers.TryAddWithoutValidation(ServiceAuthConstants.HeaderName, options.Value.Secret);
        return base.SendAsync(request, cancellationToken);
    }
}
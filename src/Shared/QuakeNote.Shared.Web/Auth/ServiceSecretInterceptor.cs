using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;

namespace QuakeNote.Shared.Web.Auth;

public sealed class ServiceSecretInterceptor(IOptions<ServiceAuthOptions> options) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var provided = context.RequestHeaders.GetValue(ServiceAuthConstants.HeaderName.ToLowerInvariant());

        if (!ServiceSecretComparer.AreEqual(provided, options.Value.Secret))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid service credentials"));

        return continuation(request, context);
    }
}
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.Interception.Interceptors.Response;
using System.Net;

namespace Milvonion.Application.Behaviours;

/// <summary>
/// Validation behavior for return value is <see cref="Response"/> typed requests.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="validators"></param>
/// <param name="serviceProvider"></param>
public sealed class ValidationBehaviorForResponse<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators,
                                                                       IServiceProvider serviceProvider) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationFailures = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context)));

        var errors = validationFailures.Where(validationResult => !validationResult.IsValid)
                                       .SelectMany(validationResult => validationResult.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();

        if (errors.Count != 0)
            return CreateFailureResponse(errors);

        var response = await next();

        return response;
    }

    private TResponse CreateFailureResponse(List<string> errors)
    {
        if (typeof(TResponse) == typeof(Response))
        {
            var response = new Response();

            AddMessagesToResponse(response, errors);

            return response as TResponse;
        }
        else if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Response<>))
        {
            return BuildGenericResponse(typeof(Response<>), errors);
        }
        else if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ListResponse<>))
        {
            return BuildGenericResponse(typeof(ListResponse<>), errors);
        }

        var localizer = _serviceProvider.GetService<IMilvaLocalizer>();

        throw new MilvaUserFriendlyException(localizer[errors[0]]);
    }

    private TResponse BuildGenericResponse(Type type, List<string> errors)
    {
        var responseType = typeof(TResponse).GetGenericArguments()[0];

        var response = Activator.CreateInstance(type.MakeGenericType(responseType));

        if (response is Response r)
        {
            AddMessagesToResponse(r, errors);

            var interceptionOptions = _serviceProvider.GetService<IResponseInterceptionOptions>();

            var metadataGenerator = new ResponseMetadataGenerator(interceptionOptions, _serviceProvider);

            metadataGenerator.GenerateMetadata(response as IHasMetadata);

            if (interceptionOptions.TranslateResultMessages && !r.Messages.IsNullOrEmpty())
            {
                var localizer = _serviceProvider.GetService<IMilvaLocalizer>();

                if (localizer != null)
                    foreach (var message in r.Messages)
                        message.Message = localizer[message.Message];
            }
        }

        return (TResponse)response;
    }

    private static void AddMessagesToResponse(Response response, List<string> errors)
    {
        response.StatusCode = (int)HttpStatusCode.BadRequest;
        response.IsSuccess = false;

        foreach (var error in errors.Distinct())
            response.AddMessage(error, MessageType.Validation);
    }
}

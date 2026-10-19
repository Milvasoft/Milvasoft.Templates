using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Helpers;
using Milvasoft.Interception.Decorator;
using System.Globalization;

namespace Milvonion.Application.Utils.Aspects.UserActivityLogAspect;

/// <summary>
/// Specifies that the method marked with this attribute will be added to as activity to database by the <see cref="UserActivityLogInterceptor"/>.
/// The affected record is captured too: a handler may call <c>IHttpContextAccessor.SetUserActivityRelated(id, data)</c>,
/// otherwise the id is taken from the handler's <c>Response&lt;int&gt;</c> result, so the log can deep-link to the record.
/// </summary>
/// <param name="serviceProvider"></param>
public partial class UserActivityLogInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IMilvonionRepositoryBase<ActivityLog> _activityLogRepository = serviceProvider.GetService<IMilvonionRepositoryBase<ActivityLog>>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

    /// <inheritdoc/>
    public int InterceptionOrder { get; set; } = 999;

    /// <inheritdoc/>
    public async Task OnInvoke(Call call)
    {
        await call.NextAsync();

        if (call.ReturnType.CanAssignableTo(typeof(IResponse<>)))
        {
            var response = call.ReturnValue as IResponse;

            if (response.IsSuccess)
            {

                try
                {
                    var activityLogAttribute = call.GetInterceptorAttribute<UserActivityTrackAttribute>();

                    var entity = new ActivityLog
                    {
                        Activity = activityLogAttribute.Activity,
                        ActivityDate = DateTimeOffset.UtcNow,
                        UserName = User.GetCurrentUser(_serviceProvider),
                        // The handler's explicit related record wins; otherwise fall back to the returned id.
                        RelatedId = _httpContextAccessor.GetUserActivityRelatedId() ?? GetResponseId(call.ReturnValue),
                        RelatedData = _httpContextAccessor.GetUserActivityRelatedData(),
                    };

                    await _activityLogRepository.AddAsync(entity);
                }
                catch (Exception)
                {
                    // Ignored because the log should not affect the main process.
                }
            }
        }
    }

    // Most command handlers return Response<int> whose Data is the affected record's id, so it is used as the related id.
    private static string GetResponseId(object response)
    {
        var data = response?.GetType().GetProperty("Data")?.GetValue(response);

        return data switch
        {
            int i and > 0 => i.ToString(CultureInfo.InvariantCulture),
            long l and > 0 => l.ToString(CultureInfo.InvariantCulture),
            _ => null
        };
    }
}
using Lab1.Task2.Interfaces;
using Lab1.Task2.Models;
using System.Collections.Concurrent;

namespace Lab1.Task2.Implementation;

public class RequestClient : IRequestClient, ILibraryOperationHandler
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>> _requests = new();

    private readonly ILibraryOperationService _service;

    public RequestClient(ILibraryOperationService service)
    {
        _service = service;
    }

    public async Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<ResponseModel>(cancellationToken);
        }

        var requestId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<ResponseModel>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using CancellationTokenRegistration reg = cancellationToken.Register(() =>
        {
            if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? pending))
            {
                pending.SetCanceled(cancellationToken);
            }
        });

        _requests.TryAdd(requestId, tcs);

        _service.BeginOperation(requestId, request, cancellationToken);

        return await tcs.Task;
    }

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? request))
        {
            request.TrySetResult(new ResponseModel(data));
        }
    }

    public void HandleOperationError(Guid requestId, Exception exception)
    {
        if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? request))
        {
            request.TrySetException(exception);
        }
    }
}
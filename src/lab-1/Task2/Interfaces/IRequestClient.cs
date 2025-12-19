using Lab1.Task2.Models;

namespace Lab1.Task2.Interfaces;

public interface IRequestClient
{
    Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken);
}

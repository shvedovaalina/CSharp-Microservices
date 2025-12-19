using Lab1.Task2.Models;

namespace Lab1.Task2.Interfaces;

public interface ILibraryOperationService
{
    void BeginOperation(Guid requestId, RequestModel model, CancellationToken cancellationToken);
}
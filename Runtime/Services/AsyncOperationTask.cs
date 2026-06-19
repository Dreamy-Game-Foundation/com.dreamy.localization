using System;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Dreamy.Localization
{
    internal static class AsyncOperationTask
    {
        public static Task Await(AsyncOperationHandle handle)
        {
            if (handle.IsDone)
            {
                return handle.Status == AsyncOperationStatus.Succeeded
                    ? Task.CompletedTask
                    : Task.FromException(
                        handle.OperationException ??
                        new InvalidOperationException("Unity async operation failed."));
            }

            var source = new TaskCompletionSource<bool>();
            handle.Completed += completed =>
            {
                if (completed.Status == AsyncOperationStatus.Succeeded)
                {
                    source.TrySetResult(true);
                }
                else
                {
                    source.TrySetException(
                        completed.OperationException ??
                        new InvalidOperationException("Unity async operation failed."));
                }
            };

            return source.Task;
        }

        public static Task<T> Await<T>(AsyncOperationHandle<T> handle)
        {
            if (handle.IsDone)
            {
                return handle.Status == AsyncOperationStatus.Succeeded
                    ? Task.FromResult(handle.Result)
                    : Task.FromException<T>(
                        handle.OperationException ??
                        new InvalidOperationException("Unity async operation failed."));
            }

            var source = new TaskCompletionSource<T>();
            handle.Completed += completed =>
            {
                if (completed.Status == AsyncOperationStatus.Succeeded)
                {
                    source.TrySetResult(completed.Result);
                }
                else
                {
                    source.TrySetException(
                        completed.OperationException ??
                        new InvalidOperationException("Unity async operation failed."));
                }
            };

            return source.Task;
        }
    }
}

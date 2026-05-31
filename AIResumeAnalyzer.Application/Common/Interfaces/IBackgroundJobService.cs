using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    string Enqueue(Expression<Func<Task>> methodCall);
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
}

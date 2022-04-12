using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tricycle.Utilities
{
    public static class TaskUtility
    {
        public static void RunSync(this Task task) => task.GetAwaiter().GetResult();

        public static TResult RunSync<TResult>(this Task<TResult> task) => task.GetAwaiter().GetResult();
    }
}

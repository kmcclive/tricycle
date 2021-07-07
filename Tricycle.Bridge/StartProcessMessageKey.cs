using System;
using System.Collections.Generic;
using System.Text;

namespace Tricycle.Bridge
{
    public static class StartProcessMessageKey
    {
        public static string RequestFileName { get; } = "FILE_NAME";
        public static string RequestArguments { get; } = "ARGUMENTS";
        public static string ResponseExceptionType { get; } = "EXCEPTION_TYPE";
        public static string ResponseExceptionMessage { get; } = "EXCEPTION_MESSAGE";
    }
}

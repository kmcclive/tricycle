using System;
namespace Tricycle.IO
{
    public interface ISerializer<T>
    {
        T Seriialize(object obj);
        TObject Deserialize<TObject>(T data);
    }
}

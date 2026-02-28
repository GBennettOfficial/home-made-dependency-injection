

namespace DependencyInjection
{
    public class Container : IContainer
    {
        private record ContainerKey(Type Type, string Key);

        private readonly ReaderWriterLockSlim _readWriteLock;
        private readonly Dictionary<ContainerKey, object> _singletonFactoryDict;
        private readonly Dictionary<ContainerKey, object> _singletonLazyDict;
        private readonly Dictionary<ContainerKey, object> _scopedFactoryDict;
        private readonly Dictionary<ContainerKey, object> _scopedLazyDict;
        private readonly Dictionary<ContainerKey, object> _transientFactoryDict;

        public Container()
        {
            _readWriteLock = new();
            _scopedLazyDict = new();
            _singletonFactoryDict = new();
            _singletonLazyDict = new();
            _scopedFactoryDict = new();
            _transientFactoryDict = new();
        }

        private Container(Dictionary<ContainerKey, object>[] data)
        {
            _readWriteLock = new();
            _singletonFactoryDict = data[0];
            _singletonLazyDict = data[1];
            _scopedFactoryDict = data[2];
            _scopedLazyDict = data[3];
            _transientFactoryDict = data[4];
        }

        public IContainer BranchScope()
        {
            _readWriteLock.EnterReadLock();
            try
            {
                var data = new Dictionary<ContainerKey, object>[]
                {
                    _singletonFactoryDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _singletonLazyDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _scopedFactoryDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _scopedLazyDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    _transientFactoryDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };
                return new Container(data);
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
        }

        public T Inject<T>(string key = "")
        {
            ContainerKey containerKey = new(typeof(T), key);
            _readWriteLock.EnterReadLock();
            try
            {
                if (_transientFactoryDict.ContainsKey(containerKey))
                    return ((Func<T>)_transientFactoryDict[containerKey]).Invoke();

                if (_singletonLazyDict.ContainsKey(containerKey))
                    return (T)_singletonLazyDict[containerKey];

                if (_scopedLazyDict.ContainsKey(containerKey))
                    return (T)_scopedLazyDict[containerKey];
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
            _readWriteLock.EnterWriteLock();
            try
            {
                if (_scopedFactoryDict.ContainsKey(containerKey))
                {
                    _scopedLazyDict[containerKey] = ((Func<T>)_scopedFactoryDict[containerKey]).Invoke()!;
                    return (T)_scopedLazyDict[containerKey];
                }
                if (_singletonFactoryDict.ContainsKey(containerKey))
                {
                    _singletonLazyDict[containerKey] = ((Func<T>)_singletonFactoryDict[containerKey]).Invoke()!;
                    return (T)_singletonLazyDict[containerKey];
                }
                throw new InvalidOperationException($"Type {typeof(T)} with key '{key}' is not registered in the container.");
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public void RegisterSingleton<T>(Func<T> factory, string key = "")
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                ContainerKey containerKey = new(typeof(T), key);
                if (_transientFactoryDict.ContainsKey(containerKey) || _scopedFactoryDict.ContainsKey(containerKey) || _singletonFactoryDict.ContainsKey(containerKey))
                    throw new InvalidOperationException($"Type {typeof(T)} with key '{key}' is already registered in the container.");

                _singletonFactoryDict[containerKey] = factory;
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }

        }

        public void RegisterScoped<T>(Func<T> factory, string key = "")
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                ContainerKey containerKey = new(typeof(T), key);
                if (_transientFactoryDict.ContainsKey(containerKey) || _scopedFactoryDict.ContainsKey(containerKey) || _singletonFactoryDict.ContainsKey(containerKey))
                    throw new InvalidOperationException($"Type {typeof(T)} with key '{key}' is already registered in the container.");

                _scopedFactoryDict[containerKey] = factory;
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public void RegisterTransient<T>(Func<T> factory, string key = "")
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                ContainerKey containerKey = new(typeof(T), key);
                if (_transientFactoryDict.ContainsKey(containerKey) || _scopedFactoryDict.ContainsKey(containerKey) || _singletonFactoryDict.ContainsKey(containerKey))
                    throw new InvalidOperationException($"Type {typeof(T)} with key '{key}' is already registered in the container.");

                _transientFactoryDict[containerKey] = factory;
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }
    }
}

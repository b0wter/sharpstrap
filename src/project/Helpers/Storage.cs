using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpStrap.Helpers
{
    public class Storage<T> where T : new()
    {
        internal List<T> storage = new List<T>();
        internal Func<T, string> defaultToString;
        public int Count => storage.Count;

        protected Storage()
        {
            //
        }

        public void Add(T item)
        {
            this.storage.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            storage.AddRange(items);
        }

        public void Remove(T item)
        {
            this.storage.Remove(item);
        }

        public void Remove(Predicate<T> predicate)
        {
            storage.RemoveAll(predicate);
        }

        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            return storage.Where(predicate);
        }

        public string CreateTextRepresentation()
        {
            return this.CreateTextRepresentation(defaultToString);
        }

        public string CreateTextRepresentation(string delimiter)
        {
            return this.CreateTextRepresentation(defaultToString, delimiter);
        }

        public string CreateTextRepresentation(Func<T, string> toString)
        {
            return CreateTextRepresentation(toString, Environment.NewLine);
        }

        public string CreateTextRepresentation(Func<T, string> toString, string delimiter)
        {
            return string.Join(delimiter, this.storage.Select(toString));
        }

        public T First(Func<T, bool> predicate)
        {
            return this.storage.First(predicate);
        }

        public bool Contains(T item)
        {
            return this.storage.Contains(item);
        }

        public IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            return this.storage.Select(selector);
        }

        public void RemoveAt(int index)
        {
            this.storage.RemoveAt(index);
        }

        public bool Any(Func<T, bool> predicate)
        {
            return this.storage.Any(predicate);
        }

        public TAccumulator Aggregate<TAccumulator>(Func<TAccumulator, T, TAccumulator> aggregator)
        {
            return this.storage.Aggregate(default(TAccumulator), aggregator);
        }

        public IEnumerable<T> Union(IEnumerable<T> otherElements)
        {
            return this.storage.Union(otherElements);
        }

        public T this[int index]
        {
            get
            {
                return this.storage[index];
            }
            set
            {
                this.storage[index] = value;
            }
        }

        public static Storage<U> FromCollection<U>(IEnumerable<U> items, Func<U, string> toString) where U : new()
        {
            var storage = new Storage<U>();
            storage.defaultToString = toString;
            storage.AddRange(items);
            return storage;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpStrap.Helpers
{
    public class Storage<T> where T : new()
    {
        private List<T> storage = new List<T>();
        private Func<T, string> defaultToString;

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

        public static Storage<U> FromCollection<U>(IEnumerable<U> items, Func<U, string> toString) where U : new()
        {
            var storage = new Storage<U>();
            storage.defaultToString = toString;
            storage.AddRange(items);
            return storage;
        }
    }
}
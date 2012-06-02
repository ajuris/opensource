using System;
using System.Collections;
using System.Collections.Generic;
using EPiServer.Common.Attributes;

namespace Geta.Community.EntityAttributeBuilder
{
    public class CustomInterceptedList<T> : IList<T>
    {
        private readonly List<T> backingList = new List<T>();
        private readonly IAttributeExtendableEntity entity;
        private readonly string propertyName;

        public CustomInterceptedList(IAttributeExtendableEntity entity, string propertyName, IList initialValues)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            this.entity = entity;
            this.propertyName = propertyName;

            if (initialValues != null && initialValues.Count > 0)
            {
                foreach (var initialValue in initialValues)
                {
                    this.backingList.Add((T) initialValue);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.backingList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            this.backingList.Add(item);
            SetNewAttributeValue();
        }

        public void Clear()
        {
            this.backingList.Clear();
            SetNewAttributeValue();
        }

        public bool Contains(T item)
        {
            return this.backingList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.backingList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = this.backingList.Remove(item);
            SetNewAttributeValue();
            return result;
        }

        public int Count
        {
            get { return this.backingList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            return this.backingList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.backingList.Insert(index, item);
            SetNewAttributeValue();
        }

        public void RemoveAt(int index)
        {
            this.backingList.RemoveAt(index);
            SetNewAttributeValue();
        }

        public T this[int index]
        {
            get { return this.backingList[index]; }
            set
            {
                this.backingList[index] = value;
                SetNewAttributeValue();
            }
        }

        private void SetNewAttributeValue()
        {
            this.entity.SetAttributeValue(this.propertyName, (IList<T>) this.backingList);
        }
    }
}

/**************************************************************************
 * 
 * Copyright (c) Unterrainer Informatik OG.
 * This source is subject to the Microsoft Public License.
 * 
 * See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
 * All other rights reserved.
 * 
 * (In other words you may copy, use, change and redistribute it without
 * any restrictions except for not suing me because it broke something.)
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 * 
 ***************************************************************************/

using System.Threading;

namespace LockFreeQueue
{
	/// <summary>
	///     This class implements a lock-free FIFO-queue. No monitors or lock-statements are used here. The pseudo-lock is
	///     established via memory-fences built by
	///     the Interlocked library. It essentially implements a spin-lock for one atomic operation.
	///     This queue is tested for a one-producer - one-consumer - environment or a many-consumer - environment, if no items
	///     get enqueued while consuming. Thus it
	///     is not capable to handle a one-producer - many-consumer or many-producer - many-consumer - environment.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LockFreeQueue<T>
	{
		private int count;
		private SingleLinkedNode<T> head;
		private SingleLinkedNode<T> tail;

		/// <summary>
		///     Initializes a new instance of the <see cref="LockFreeQueue&lt;T&gt;" /> class.
		/// </summary>
		public LockFreeQueue()
		{
			head = new SingleLinkedNode<T>();
			tail = head;
		}

		/// <summary>
		///     Gets the count.
		/// </summary>
		public int Count
		{
			get { return count; }
		}

		/// <summary>
		/// Clears this instance by consecutively dequeuing all items until this instance is empty.
		/// </summary>
		public void Clear()
		{
			while (Count > 0)
			{
				Dequeue();
			}
		}

		/// <summary>
		///     Enqueues the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Enqueue(T item)
		{
			SingleLinkedNode<T> oldTail = null;

			SingleLinkedNode<T> node = new SingleLinkedNode<T>();
			node.Item = item;

			// Loop until we have managed to update the tail's Next link to point to our new node.
			bool updatedNewLink = false;
			while (!updatedNewLink)
			{
				// Make local copies of the tail and its Next link, but in getting the latter use the local copy of 
				// the tail since another thread may have changed the value of tail.
				oldTail = tail;

				SingleLinkedNode<T> oldNext = oldTail.Next;
				// Providing that the tail field has not changed...
				if (tail == oldTail)
				{
					// ...and its Next field is null...
					if (oldNext == null)
					{
						// ...try to update the tail's Next field.
						updatedNewLink = null == Interlocked.CompareExchange(ref tail.Next, node, null);
					}
					else
					{
						// If the tail's Next field was non-null, another thread is in the middle of enqueuing a new node, so try and 
						// advance the tail to point to its Next node.
						Interlocked.CompareExchange(ref tail, oldNext, oldTail);
					}
				}
			}
			// Try and update the tail field to point to our node; Don't worry if we can't, another thread will update it for us on
			// the next call to Enqueue().
			Interlocked.CompareExchange(ref tail, node, oldTail);
			Interlocked.Increment(ref count);
		}

		/// <summary>
		///     Dequeues the next item (the oldest in the queue).
		/// </summary>
		/// <returns></returns>
		public T Dequeue()
		{
			T result = default(T);

			// Loop until we manage to advance the head, removing a node (if there are no nodes to dequeue, we'll exit
			// the method instead).
			bool haveAdvancedHead = false;
			while (!haveAdvancedHead)
			{
				// Make local copies.
				SingleLinkedNode<T> oldHead = head;
				SingleLinkedNode<T> oldTail = tail;
				// ReSharper disable PossibleNullReferenceException
				// This exception cannot occur because _head is never set to null.
				SingleLinkedNode<T> oldHeadNext = oldHead.Next;
				// ReSharper restore PossibleNullReferenceException

				// Providing that the head field has not changed...
				if (oldHead == head)
				{
					// ...and it is equal to the tail field...
					if (oldHead == oldTail)
					{
						// ...and the head's Next field is null...
						if (oldHeadNext == null)
						{
							// ...then there is nothing to dequeue.
							return default(T);
						}

						// If the head's Next field is non-null and head was equal to the tail then we have a lagging tail. Try and update it.
						Interlocked.CompareExchange(ref tail, oldHeadNext, oldTail);
					}
					else
					{
						// Otherwise the head and tail fields are different grab the item to dequeue, and then try to advance the head reference.
						result = oldHeadNext.Item;
						haveAdvancedHead = oldHead == Interlocked.CompareExchange(ref head, oldHeadNext, oldHead);
					}
				}
			}
			Interlocked.Decrement(ref count);
			return result;
		}
	}
}
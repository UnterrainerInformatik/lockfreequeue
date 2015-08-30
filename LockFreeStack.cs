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
	///     This class implements a lock-free stack. No monitors or lock-statements are used here. The pseudo-lock is
	///     established via memory-fences built by the
	///     Interlocked library. It essentially implements a spin-lock for one atomic operation.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LockFreeStack<T>
	{
		private readonly SingleLinkedNode<T> head;

		/// <summary>
		///     Initializes a new instance of the <see cref="LockFreeStack&lt;T&gt;" /> class.
		/// </summary>
		public LockFreeStack()
		{
			head = new SingleLinkedNode<T>();
		}

		/// <summary>
		///     Pushes the specified item on to the stack.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Push(T item)
		{
			SingleLinkedNode<T> node = new SingleLinkedNode<T>();
			node.Item = item;
			do
			{
				node.Next = head.Next;
			}
			while (node.Next != Interlocked.CompareExchange(ref head.Next, node, node.Next));
		}

		/// <summary>
		///     Pops the specified item from the stack.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public bool Pop(out T item)
		{
			SingleLinkedNode<T> node;
			do
			{
				node = head.Next;
				if (node == null)
				{
					item = default(T);
					return false;
				}
			}
			while (node != Interlocked.CompareExchange(ref head.Next, node.Next, node));
			item = node.Item;
			return true;
		}

		/// <summary>
		///     Pops the last item from the stack.
		/// </summary>
		/// <returns></returns>
		public T Pop()
		{
			T result;
			Pop(out result);
			return result;
		}
	}
}
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

namespace LockFreeQueue
{
	/// <summary>
	///     This is a simple node containing an arbitrary item, a next-pointer and a prev-pointer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DoubleLinkedNode<T> : SingleLinkedNode<T>
	{
		public DoubleLinkedNode<T> Prev;
		public new DoubleLinkedNode<T> Next;
	}
}
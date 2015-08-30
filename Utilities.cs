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

using System;
using System.Threading;

namespace LockFreeQueue
{
	public class Utilities
	{
		/// <summary>
		///     Does an update given a generic update-function without requiring a lock.
		///     Interlocked.CompareExchange updates a field with a specified value if the field’s current value matches the third
		///     argument.
		///     It then returns the field’s old value, so you can test whether it succeeded by comparing that against the original
		///     snapshot.
		///     If the values differ, it means that another thread preempted you, in which case you spin and try again.
		///     CompareExchange is overloaded to work with the object type too.
		///     We can leverage this overload by writing a lock-free update method that works with all reference types.
		///     Here’s how we can use this method to write a thread-safe event without locks (this is, in fact, what the C# 4.0
		///     compiler now does by default with events):
		///     <code>
		/// EventHandler _someDelegate;
		/// 
		/// public event EventHandler SomeEvent
		/// {
		///     add { LockFreeUpdate(ref _someDelegate, d => d + value); }
		///     remove { LockFreeUpdate(ref _someDelegate, d => d - value); }
		/// }
		/// </code>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field">The field.</param>
		/// <param name="updateFunction">The update function.</param>
		public static void LockFreeUpdate<T>(ref T field, Func<T, T> updateFunction) where T : class
		{
			SpinWait spinWait = new SpinWait();
			while (true)
			{
				T snapshot1 = field;
				T calc = updateFunction(snapshot1);
				T snapshot2 = Interlocked.CompareExchange(ref field, calc, snapshot1);
				if (snapshot1 == snapshot2)
				{
					return;
				}
				spinWait.SpinOnce();
			}
		}
	}
}
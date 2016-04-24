// *************************************************************************** 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

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
            var spinWait = new SpinWait();
            while (true)
            {
                var snapshot1 = field;
                var calc = updateFunction(snapshot1);
                var snapshot2 = Interlocked.CompareExchange(ref field, calc, snapshot1);
                if (snapshot1 == snapshot2)
                {
                    return;
                }
                spinWait.SpinOnce();
            }
        }
    }
}
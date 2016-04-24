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
            var node = new SingleLinkedNode<T>();
            node.Item = item;
            do
            {
                node.Next = head.Next;
            } while (node.Next != Interlocked.CompareExchange(ref head.Next, node, node.Next));
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
            } while (node != Interlocked.CompareExchange(ref head.Next, node.Next, node));
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
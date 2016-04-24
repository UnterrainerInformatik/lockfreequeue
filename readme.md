```
/**************************************************************************
 * 
 * by Unterrainer Informatik OG.
 * This is free and unencumbered software released into the public domain.
 * Anyone is free to copy, modify, publish, use, compile, sell, or
 * distribute this software, either in source code form or as a compiled
 * binary, for any purpose, commercial or non-commercial, and by any
 * means.
 *
 * In jurisdictions that recognize copyright laws, the author or authors
 * of this software dedicate any and all copyright interest in the
 * software to the public domain. We make this dedication for the benefit
 * of the public at large and to the detriment of our heirs and
 * successors. We intend this dedication to be an overt act of
 * relinquishment in perpetuity of all present and future rights to this
 * software under copyright law.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * For more information, please refer to <http://unlicense.org>
 * 
 * (In other words you may copy, use, change, redistribute and sell it without
 * any restrictions except for not suing me because it broke something.)
 * 
 ***************************************************************************/

```

# General  

This section contains various useful projects that should help your development-process.  

This section of our GIT repositories is free. You may copy, use or rewrite every single one of its contained projects to your hearts content.  
In order to get help with basic GIT commands you may try [the GIT cheat-sheet][coding] on our [homepage][homepage].  

This repository located on our  [homepage][homepage] is private since this is the master- and release-branch. You may clone it, but it will be read-only.  
If you want to contribute to our repository (push, open pull requests), please use the copy on github located here: [the public github repository][github]  

# LockFreeQueue  

This class implements a lock-free FIFO-queue.  

No monitors or lock-statements are used here. The pseudo-lock is established via memory-fences built by the Interlocked library. It essentially implements a spin-lock for one atomic operation.  
This queue is tested for a one-producer - one-consumer - environment or a many-consumer - environment, if no items get enqueued while consuming. Thus it is not capable to handle a one-producer - many-consumer or many-producer - many-consumer - environment.  

#### Example  
    
```csharp
LockFreeQueue<int> queue = new LockFreeQueue<int>();

for (int i = 0; i < numberCount - enqueueLater; i++)
{
	queue.Enqueue(i + 1);
}
```


[homepage]: http://www.unterrainer.info
[coding]: http://www.unterrainer.info/Home/Coding
[github]: https://github.com/UnterrainerInformatik/lockfreequeue
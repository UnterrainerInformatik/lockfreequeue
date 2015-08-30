```
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
[github] : https://github.com/UnterrainerInformatik/lockfreequeue
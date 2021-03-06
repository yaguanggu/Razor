// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Chunks
{
    public class ChunkTree
    {
        public ChunkTree()
        {
            Chunks = new List<Chunk>();
        }

        public IList<Chunk> Chunks { get; set; }
    }
}

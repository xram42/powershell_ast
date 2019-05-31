// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

static class ListExtension {
    public static void Shuffle<T>(this IList<T> list, Random rng) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public static class Convert {
    public static byte[] FromBase64String(bool b) {
        return new byte[] { };
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PKCS11Explorer.Tools
{
    static class Assert
    {
        public static void AssertTrue(bool thingToAssert, string tag = null)
        {
            if (!thingToAssert)
                throw new AssertException("[" + tag + "] AssertTrue: is false, expected true");
        }

        public static void AssertFalse(bool thingToAssert, string tag = "Assert")
        {
            if (thingToAssert)
                throw new AssertException("[" + tag + "] AssertFalse: is true, expected false");
        }

        public static void AssertICollectionEmptyOrNull(ICollection thingToAssert, string tag = "Assert")
        {
            if (thingToAssert != null && thingToAssert.Count > 0)
                throw new AssertException("[" + tag + "] AssertICollectionEmptyOrNull: is populated, expected null or empty");
        }

        public static void AssertICollectionPopulated(ICollection thingToAssert, string tag = "Assert")
        {
            if (thingToAssert != null && thingToAssert.Count > 0)
                throw new AssertException("[" + tag + "] AssertICollectionPopulated: is null or empty, expected populated");
        }
    }

    public class AssertException : Exception
    {
        public AssertException(string message) : base(message) {}
    }
}

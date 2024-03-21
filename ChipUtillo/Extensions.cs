using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChipUtillo
{
    /*
     * MIT License

Copyright (c) 2020 Sebastian Amann

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
    */
    public static class IconExtensions
    {
        /// <summary>
        /// Create a Icon that calls DestroyIcon() when the Destructor is called.
        /// Unfortunatly Icon.FromHandle() initializes with the internal Icon-constructor Icon(handle, false), which sets the internal value "ownHandle" to false
        /// This way because of the false, DestroyIcon() is not called as can be seen here:
        /// https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Icon.cs,f2697049dea34e7c,references
        /// To get arround this we get the constructor internal Icon(IntPtr handle, bool takeOwnership) from Icon through reflection and initialize that way
        /// </summary>
        public static Icon BitmapToIcon(this Bitmap bitmap)
        {
            Type[] cargt = new[] { typeof(IntPtr), typeof(bool) };
            ConstructorInfo ci = typeof(Icon).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, cargt, null);
            object[] cargs = new[] { (object)bitmap.GetHicon(), true };
            Icon icon = (Icon)ci.Invoke(cargs);
            return icon;
        }
    }
}

using System;
using System.Diagnostics;
using System.Reflection;

// Creation date: 25.06.2004
// Checked: 02.07.2004
// Author: Otto Mayer (mot@root.ch)
// Version: 1.03

// Report.NET copyright 2002-2004 root-software ag, Bürglen Switzerland - O. Mayer, S. Spirig, R. Gartenmann, all rights reserved
// This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation, version 2.1 of the License.
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You
// should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA www.opensource.org/licenses/lgpl-license.html

namespace Root.Reports {
  /// <summary>Debug Tools Class</summary>
  internal sealed class DebugTools {
    #if (Checked)
    /// <summary>Method Definition</summary>
    internal class Method {
      /// <summary>Type of the class from which the method may be called</summary>
      internal Type type;

      /// <summary>Name of the method</summary>
      internal String sMethodName;

      //------------------------------------------------------------------------------------------02.07.2004
      /// <summary>Creates a method definition.</summary>
      /// <param name="type">Type of the class from which the method may be called</param>
      /// <param name="sMethodName">Name of the method</param>
      internal Method(Type type, String sMethodName) {
        this.type = type;
        this.sMethodName = sMethodName;
      }

      //------------------------------------------------------------------------------------------02.07.2004
      /// <summary>Creates a constructor definition.</summary>
      /// <param name="type">Type of the class from which the constructor may be called</param>
      internal Method(Type type) : this(type, ".ctor") {
      }
    }

    //------------------------------------------------------------------------------------------02.07.2004
    /// <summary>Checks whether this method has been called from specified method.</summary>
    /// <param name="type">Type of the class from which the method may be called</param>
    /// <param name="sMethodName">Name of the method</param>
    internal static void CheckMethodCall(Type type, String sMethodName) {
      CheckMethodCall(new Method[] { new Method(type, sMethodName) });
    }

    //------------------------------------------------------------------------------------------02.07.2004
    /// <summary>Checks whether this method has been called from specified constructor.</summary>
    /// <param name="type">Type of the class from which the method may be called</param>
    internal static void CheckMethodCall(Type type) {
      CheckMethodCall(new Method[] { new Method(type) });
    }

    //------------------------------------------------------------------------------------------02.07.2004
    /// <summary>Checks whether this method has been called from one of the specified methods.</summary>
    /// <param name="aMethod">Array of methods</param>
    internal static void CheckMethodCall(Method[] aMethod) {
      StackFrame stackFrame = new StackFrame(2);
      MethodBase methodBase = stackFrame.GetMethod();
      foreach (Method method in aMethod) {
        if (methodBase.DeclaringType == method.type && methodBase.Name == method.sMethodName) {
          return;
        }
      }

      String s = "";
      foreach (Method method in aMethod) {
        s += "[" + method.type.Name + "." + method.sMethodName + "], ";
      }

      Debug.Fail("This method has been called from " + methodBase.DeclaringType.Name + "." + methodBase.Name +
        ". It may be called from " + s.Substring(0, s.Length - 2) + " only.");
    }
    #endif
  }
}

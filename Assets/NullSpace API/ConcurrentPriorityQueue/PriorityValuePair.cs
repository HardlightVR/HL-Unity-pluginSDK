/*
Copyright (c) 2014 Axon Interactive Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ca.axoninteractive.Collections
{
	public class PriorityValuePair<TValue>
	{
		#region Instance members

		/// <summary>
		/// The double-precision floating-point value indicating the priority value of this pair. 
		/// Typically this affects how it will be sorted in a binary heap or priority queue.
		/// </summary>
		public double Priority { get; set; }


		/// <summary>
		/// A generically-typed value that may contain any kind of data.
		/// </summary>
		public TValue Value { get; set; }

		#endregion


		#region Constructors

		/// <summary>
		/// Create a new default priority-value pair.
		/// </summary>
		public PriorityValuePair()
		{
			Priority = 0f;
			Value = default( TValue );
		}


		/// <summary>
		/// Create a new priority-value pair by specifying its initial priority and value.
		/// </summary>
		/// <param name="priority">The double-precision floating-point value indicating the 
		/// priority value of this pair. Typically this affects how it will be sorted in a binary 
		/// heap or priority queue.
		/// </param>
		/// <param name="value">A generically-typed value that may contain any kind of data.
		/// </param>
		public PriorityValuePair( double priority, TValue value )
		{
			Priority = priority;
			Value = value;
		}

		#endregion
	}
}

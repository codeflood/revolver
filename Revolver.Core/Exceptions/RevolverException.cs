using System;

namespace Revolver.Core.Exceptions
{
	public class RevolverException : Exception
	{
		public RevolverException() : base() { }
		public RevolverException(string message) : base(message) { }
		public RevolverException(string message, Exception ex) : base(message, ex) { }
	}
}

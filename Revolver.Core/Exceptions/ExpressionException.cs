using System;

namespace Revolver.Core.Exceptions
{
	public class ExpressionException : RevolverException
	{
		public ExpressionException()
			: base()
		{
		}

		public ExpressionException(string message)
			: base(message)
		{
		}

		public ExpressionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}

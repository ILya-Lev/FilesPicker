using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderParser
{
	/// <summary>
	/// the interface provides means of standardizing exception notification to MainWindow class
	/// </summary>
	interface IExceptionNotifier
	{
		event Action<object, string> ExceptionOccuredEvent;
	}
}

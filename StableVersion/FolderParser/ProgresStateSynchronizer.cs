using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderParser
{
	/// <summary>
	/// the class is used to wrap a flag about status of progress - in progress or not.
	/// one can see analogy with CancellationTokenSource and CancellationToken classes.
	/// </summary>
	class ProgresStateSynchronizer
	{
		public ProgresStateSynchronizer(bool isInProgress)
		{
			InProgress = isInProgress;
		}
		public bool InProgress { get; set; }
	}
}

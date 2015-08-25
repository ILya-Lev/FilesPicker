using System;
using System.Collections.Generic;
using System.Threading;

namespace FolderParser
{
	/// <summary>
	/// base class for ones to store data (received in wrapper object Item)
	/// into some 'place' - either XML or Tree, for example.
	/// </summary>
	abstract class Filler : IExceptionNotifier
	{
		protected Queue<Item> m_itemsInQueue = new Queue<Item>();
		protected Thread m_thread;
		protected volatile ProgresStateSynchronizer m_inProgress;

		protected Filler(ProgresStateSynchronizer inProgress)
		{
			m_inProgress = inProgress;
		}

		public void ItemGrabbedHandler(Item anItem)
		{
			m_itemsInQueue.Enqueue(anItem);
		}

		public abstract void FolderStartedHandler(Item item);
		public abstract void FolderFinishedHandler(Item item);

		protected void RetreiveData()
		{
			try
			{
				while (m_itemsInQueue.Count != 0 && m_inProgress.InProgress)
				{
					Item currentItem = m_itemsInQueue.Dequeue();
					StoreData(currentItem);
				}
			}
			catch (Exception exc)
			{
				OnExceptionOccured(exc.Message);				// collects all exceptions from XML and Tree threads
			}
		}

		protected abstract void StoreData(Item anItem);

		public event Action<object, string> ExceptionOccuredEvent;
		protected void OnExceptionOccured(string message)
		{
			if (ExceptionOccuredEvent != null)
			{
				ExceptionOccuredEvent(this, message);
			}
		}
		/// <summary>
		/// cleans internal state of the object in case data processing cycle is invoked more than once per application run
		/// </summary>
		public abstract void CleanData();
	}
}

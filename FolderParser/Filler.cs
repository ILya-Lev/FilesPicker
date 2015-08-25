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
		protected string m_threadName;
		protected volatile ProgresStateSynchronizer m_inProgress;
		protected CountdownEvent m_folderEndSynch;
		protected CountdownEvent m_fileEndSynch;

		protected Filler(ProgresStateSynchronizer inProgress, CountdownEvent folderEndSynch, CountdownEvent fileEndSynch)
		{
			m_inProgress = inProgress;
			m_folderEndSynch = folderEndSynch;
			m_fileEndSynch = fileEndSynch;
		}

		public void ItemGrabbedHandler(Item anItem)
		{
			m_itemsInQueue.Enqueue(anItem);
		}

		public abstract void FolderStartedHandler(Item item);
		public abstract void FolderFinishedHandler(Item item);

		public void Start()
		{
			CleanData();
			m_thread = new Thread(RetreiveData);
			m_thread.Name = m_threadName;
			m_thread.Start();
		}

		protected void RetreiveData()
		{
			try
			{
				while (m_inProgress.InProgress)
				{
					m_fileEndSynch.Wait();
					while (m_itemsInQueue.Count != 0)
					{
						Item currentItem = m_itemsInQueue.Dequeue();
						StoreData(currentItem);
					}
					lock (m_itemsInQueue)
					{
						if (m_folderEndSynch.CurrentCount > 0)
						{
							m_fileEndSynch.Reset();
							m_folderEndSynch.Signal(1);
						}
					}
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

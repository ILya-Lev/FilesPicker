using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace FolderParser
{
	class TreeFiller : Filler
	{
		private TreeView m_tree;
		private Stack<TreeViewItem> m_itemsStack = new Stack<TreeViewItem>();

		public TreeFiller(TreeView tree, ProgresStateSynchronizer inProgress, CountdownEvent folderEndSynch, CountdownEvent fileEndByTree)
			: base(inProgress, folderEndSynch, fileEndByTree)
		{
			m_tree = tree;
			m_threadName = "Tree Thread";
		}

		public override void FolderStartedHandler(Item item)
		{
			try
			{
				m_tree.Dispatcher.Invoke(() =>
				{
					var treeItem = new TreeViewItem { Header = item.Name };
					if (m_itemsStack.Count == 0)
					{
						m_tree.Items.Clear();
						m_tree.Items.Add(treeItem);
					}
					else
					{
						m_itemsStack.Peek().Items.Add(treeItem);
					}
					m_itemsStack.Push(treeItem);
				});
			}
			catch (Exception exc)
			{
				OnExceptionOccured(exc.Message);
			}
		}

		public override void FolderFinishedHandler(Item item)
		{
			try
			{
				m_tree.Dispatcher.Invoke(() =>
				{
					if (m_itemsStack.Peek().Header.ToString() == item.Name)
					{
						m_itemsStack.Pop();
					}
					else
					{
						throw new Exception(string.Format("invalid element structure {0} != {1}", m_itemsStack.Peek(), item.Id));
					}
				});
			}
			catch (Exception exc)
			{
				OnExceptionOccured(exc.Message);
			}
		}

		protected override void StoreData(Item anItem)
		{
			m_tree.Dispatcher.Invoke(() => m_itemsStack.Peek().Items.Add(new TreeViewItem { Header = anItem.Name }));
		}

		public override void CleanData()
		{
			m_itemsInQueue.Clear();
			m_itemsStack.Clear();
		}
	}
}

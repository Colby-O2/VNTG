using System.Collections.Generic;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    SelectionState.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class SelectionState
    {
        private readonly List<int> _orderedIndices = new List<int> { 0 };
        private readonly HashSet<int> _selectedIndices = new HashSet<int> { 0 };

        public HashSet<int> Indices => _selectedIndices;

        public int GetPrimarySelection()
        {
            if (IsEmpty()) return -1;
            return _orderedIndices[0];
        }

        public int SetPrimarySelection(int index)
        {
            if (!Contains(index))
            {
                Add(index);
            }

            _orderedIndices.Remove(index);
            _orderedIndices.Insert(0, index);

            return index;
        }

        public int Add(int index)
        {
            if (_selectedIndices.Add(index))
            {
                _orderedIndices.Add(index);
            }
            return GetPrimarySelection();
        }

        public int Remove(int index)
        {
            if (_selectedIndices.Remove(index))
            {
                _orderedIndices.Remove(index);
            }
            return GetPrimarySelection();
        }

        public int UnionWith(IEnumerable<int> other)
        {
            List<int> currentRange = new List<int>(_orderedIndices);

            _selectedIndices.Clear();
            _orderedIndices.Clear();

            foreach (int index in other)
            {
                Add(index);
            }

            foreach (int index in currentRange)
            {
                Add(index);
            }

            return GetPrimarySelection();
        }

        public void Clear()
        {
            _selectedIndices.Clear();
            _orderedIndices.Clear();
        }

        public bool Contains(int index)
        {
            return _selectedIndices.Contains(index);
        }

        public bool IsEmpty()
        {
            return _orderedIndices.Count == 0;
        }
    }
}
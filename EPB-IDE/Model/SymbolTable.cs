using Computer_Simulator;
using System;
using System.Collections.Generic;

namespace EPB_IDE.Model
{
    public class SymbolTable 
    {
        //------------------------------------------------------------------------------------------------------------
        public static SymbolTable Make(int memoryMax = 100)
        {
            return new SymbolTable(memoryMax);
        }

        private List<TableEntry> _entries;
        public List<TableEntry> Entries { get { return _entries; } }
        private int _nextVarConstLocation;
        public int TempLocation { get { return _nextVarConstLocation; } }

        //------------------------------------------------------------------------------------------------------------
        private SymbolTable(int memoryMax)
        {
            _nextVarConstLocation = memoryMax-1;
            _entries = new List<TableEntry>();
        }

        //------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            string response = "SymbolTable: Entries: \n";
            response += "      Symbol    Type      Location\n";
            foreach (var entry in _entries)
            {
                response += $"{entry.Symbol(), 10}{entry.Type(),10}{entry.Location().ToString("D2"),10}\n";
            }
            return response;
        }

        //------------------------------------------------------------------------------------------------------------
        public SymbolTable AddConst(int symbol)
        {
            FindOrAddConst(symbol);
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public SymbolTable Add(int symbol, char type, int location)
        {
            _entries.Add(TableEntry.Make(symbol, type, location));
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry FindOrAdd(int symbol, char type)
        {
            TableEntry response = Find(symbol, type);
            if (response == null)
            {
                response = TableEntry.Make(symbol, type, _nextVarConstLocation);
                _entries.Add(response);
                --_nextVarConstLocation;
            }
            return response;
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry FindOrAddConst(int symbol)
        {
            return FindOrAdd(symbol, TableEntry.CONSTANT);
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry FindOrAddVar(int symbol)
        {
            return FindOrAdd(symbol, TableEntry.VARIABLE);
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry FindOrAddLine(int symbol)
        {
            return FindOrAdd(symbol, TableEntry.LINE_NUMBER);
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry GetEntry(int symbol)
        {
            foreach (TableEntry entry in _entries)
            {
                if (symbol == entry.Symbol()) { return entry; }
            }
            return null;
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry AtLocation(int location)
        {
            foreach (TableEntry entry in _entries)
            {
                if (location == entry.Location()) { return entry; }
            }
            return null;
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry Find(int symbol, char type)
        {
            foreach(TableEntry entry in _entries)
            {
                if (type == entry.Type() && symbol == entry.Symbol()) { return entry; }
            }
            return null;
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry Find(string searchFor)
        {
            if (Int32.TryParse(searchFor, out int intConstant))
            {
                // search string is a constant
                return FindConstant(intConstant);
            }
            else if (Toolbelt.IsValidVariable(searchFor))
            {
                // search string is a variable
                if (Char.TryParse(searchFor, out char chVar))
                {
                    return FindVariable((int)chVar);
                }
                else
                {
                    throw new SystemException($"SymbolTable Find({searchFor}) unexpected error.  Something bad happened");
                }
            }
            else
            {
                throw new SystemException($"SymbolTable Find({searchFor}) search string must be a variable or constant");
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry FindLineNumber(int symbol)
        {
            return Find(symbol, TableEntry.LINE_NUMBER);
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry FindVariable(int symbol)
        {
            return Find(symbol, TableEntry.VARIABLE);
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry FindConstant(int symbol)
        {
            return Find(symbol, TableEntry.CONSTANT);
        }

        //------------------------------------------------------------------------------------------------------------
        public bool HasSymbol(int symbol)
        {
            foreach (TableEntry entry in _entries)
            {
                if (symbol == entry.Symbol()) { return true; }
            }
            return false;
        }

        //------------------------------------------------------------------------------------------------------------
        public bool HasLocation(int location)
        {
            foreach (TableEntry entry in _entries)
            {
                if (location == entry.Location()) { return true; }
            }
            return false;
        }

    }
}

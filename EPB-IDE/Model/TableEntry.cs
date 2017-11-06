using System;

namespace EPB_IDE.Model
{
    public class TableEntry
    {
        //------------------------------------------------------------------------------------------------------------
        public static TableEntry Make(int symbol, char type, int location, int locationMin = 0, int locationMax = 99)
        {
            return new TableEntry(symbol, type, location, locationMin, locationMax);
        }
        public const char CONSTANT = 'C';
        public const char LINE_NUMBER = 'L';
        public const char VARIABLE = 'V';
        private static readonly char[] _allowedTypes = {
            'C',    // Constant
            'L',    // Line number
            'V'     // variable
        };
        private int _locationMin;
        private int _locationMax;
        private int _symbol;
        private char _type;
        private int _location;

        //------------------------------------------------------------------------------------------------------------
        private TableEntry(int symbol, char type, int location, int locationMin, int locationMax)
        {
            _locationMin = locationMin;
            _locationMax = locationMax;

            Symbol(symbol).Type(type).Location(location);
        }

        public char CharVal { get { return Convert.ToChar(_symbol); } }
        public int Val { get { return _symbol; } }

        public int Symbol() { return _symbol; }
        public char Type() { return _type; }
        public int Location() { return _location; }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry Symbol(int symbol)
        {
            this._symbol = symbol;
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry Type(char type)
        {
            if (Array.IndexOf(_allowedTypes, type) >= 0)
            {
                this._type = type;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"TableEntry Type '{type}' is not an allowed type.");
            }
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public TableEntry Location(int location)
        {
            if (_locationMin <= location && location <= _locationMax)
            {
                this._location = location;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"TableEntry Location ({location}) must be between {_locationMin} and {_locationMax}");
            }
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public bool IsVar()
        {
            return _type == 'V';
        }

        //------------------------------------------------------------------------------------------------------------
        public bool IsLine()
        {
            return _type == 'L';
        }

        //------------------------------------------------------------------------------------------------------------
        public bool IsConst()
        {
            return _type == 'C';
        }

        //------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return $"Symbol: {_symbol}, Type: {_type}, Location: {_location}";
        }
    }
}

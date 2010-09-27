using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SheepVMDotNet
{
    public enum SheepTokenType
    {
        None,

        Identifier,
        LocalIdentifier,

        LiteralInteger,
        LiteralFloat,
        LiteralString,

        Comma,
        Period,
        Semicolon,
        LBracket,
        RBracket,
        LBrace,
        RBrace,
        LParen,
        RParen,

        PlusEq,
        MinusEq,
        StarEq,
        DivEq,

        EqEq,
        NotEq,
        LEq,
        GEq,
        AndAnd,
        OrOr,
        PlusPlus,
        MinusMinus,

        Plus,
        Minus,
        Star,
        Slash,
        Lt,
        Gt,
        Equal,

        Int,
        Float,
        String,

        If,
        Else,
        True,
        False,

        Wait,
        Return,
        Symbols,
        Code
    }

    public struct SheepToken
    {
        public SheepTokenType Type;
        public bool IsRegex;
        public string Text;
        public Regex Regex;
    }

    public struct ScannedToken
    {
        public SheepTokenType Type;
        public BetterString Text;
        public int Position;
        public int Line;

        public ScannedToken(SheepTokenType type, BetterString text, int line)
        {
            Type = type;
            Position = text.Position;
            Line = line;
            Text = text;
        }
    }

    public struct BetterString
    {
        private string _source;
        public int Position;
        public int Length;

        public BetterString(string source, int startIndex, int length)
        {
            _source = source;
            Position = startIndex;
            Length = length;
        }

        public bool TryParseFloat(out float value)
        {
            // TODO: do this in a way that doesn't generate garbage
            string s = _source.Substring(Position, Length);
            return float.TryParse(s, out value);
        }

        public bool TryParseInt(out int value)
        {
            // TODO: do this in a way that doesn't generate garbage
            string s = _source.Substring(Position, Length);
            return int.TryParse(s, out value);
        }

        public override string ToString()
        {
            return _source.Substring(Position, Length);
        }

        public static BetterString NullString
        {
            get
            {
                return new BetterString();
            }
        }
    }

    public struct SheepScannerPosition
    {
        public int Position;
        public int Line;
    }

    public class SheepScanner
    {
        private string _text;
        private SheepScannerPosition _position;
        private List<SheepToken> _regexTokens = new List<SheepToken>();
        private List<SheepToken> _normalTokens = new List<SheepToken>();

        const int MAX_TOKEN_LENGTH = 256;
        private char[] _tokenBuffer = new char[MAX_TOKEN_LENGTH];

        public SheepScanner()
        {
            createTokenMap();
        }

        public void Begin(string text)
        {
            _position.Line = 0;
            _position.Position = 0;
            _text = text;
        }

        public void Seek(SheepScannerPosition position)
        {
            _position = position;
            clearTokenBuffer(MAX_TOKEN_LENGTH);
        }

        public ScannedToken GetNextToken()
        {
            int length = _text.Length;

            if (_position.Position >= length)
                return new ScannedToken(SheepTokenType.None, BetterString.NullString, _position.Line);

            while (isWhitespace(_text, _position.Position))
            {
                if (_text[_position.Position] == '\n')
                    _position.Line++;

                _position.Position++;
                if (_position.Position >= length)
                    return new ScannedToken(SheepTokenType.None, BetterString.NullString, _position.Line);
            }
            
            int tokenBufferPosition = 0;
            while (_position.Position < length)
            {
                _tokenBuffer[tokenBufferPosition++] = _text[_position.Position++];

                if (char.IsWhiteSpace(_tokenBuffer[tokenBufferPosition - 1]) ||
                    representsAtLeastOneValidToken(_tokenBuffer, tokenBufferPosition) == false)
                {
                    // back up
                    tokenBufferPosition--;
                    _position.Position--;
                    _tokenBuffer[tokenBufferPosition] = (char)0;

                    // get the token
                    ScannedToken token = getToken(_tokenBuffer, tokenBufferPosition);
                    clearTokenBuffer(tokenBufferPosition);

                    return token;
                }
            }

            if (_position.Position > 0)
            {
                ScannedToken token = getToken(_tokenBuffer, tokenBufferPosition);
                clearTokenBuffer(tokenBufferPosition);

                return token;
            }

            return new ScannedToken(SheepTokenType.None, BetterString.NullString, _position.Line);
        }

        public SheepScannerPosition CurrentPosition
        {
            get { return _position; }
        }

        private void clearTokenBuffer(int len)
        {
            for (int i = 0; i < len; i++)
                _tokenBuffer[i] = (char)0;
        }

        private bool isWhitespace(string text, int index)
        {
            return char.IsWhiteSpace(text[index]);
        }

        private bool isValidIdentifier(char[] tokenBuffer, int bufferLength, bool local)
        {
            if (bufferLength < 1)
                return false;

            if (!char.IsLetter(tokenBuffer[0]) && tokenBuffer[0] != '_')
                return false;

            for (int i = 1; i < bufferLength; i++)
            {
                if (!char.IsLetterOrDigit(tokenBuffer[i]) &&
                    tokenBuffer[i] != '_' && 
                    (local == false ||
                    (tokenBuffer[i] != '$' && i == bufferLength - 1)))
                    return false;
            }

            if (local && tokenBuffer[bufferLength - 1] != '$')
                return false;

            return true;
        }

        private bool isValidStringLiteral(char[] tokenBuffer, int bufferLength, bool allowIncomplete)
        {
            if (bufferLength < 1)
                return false;

            if (!allowIncomplete && bufferLength < 2)
                return false;

            // must begin with quotes
            if (tokenBuffer[0] != '"')
                return false;

            bool closingFound = false;
            for (int i = 2; i < bufferLength; i++)
            {
                if (tokenBuffer[i] == '"')
                    if (tokenBuffer[i - 1] != '\\')
                    {
                        closingFound = true;
                        break;
                    }
            }

            if (!allowIncomplete && closingFound == false)
                return false;

            if (closingFound && tokenBuffer[bufferLength - 1] != '"')
                return false;

            return true;
        }

        private bool isValidIntegerLiteral(char[] tokenBuffer, int bufferLength)
        {
            if (bufferLength < 1)
                return false;

            if (!char.IsDigit(tokenBuffer[0]) && tokenBuffer[0] != '-')
                return false; 

            for (int i = 1; i < bufferLength; i++)
            {
                if (!char.IsDigit(tokenBuffer[i]))
                    return false;
            }

            return true;
        }

        private bool isValidFloatLiteral(char[] tokenBuffer, int bufferLength, bool allowPartial)
        {
            if (bufferLength < 1)
                return false;

            if (tokenBuffer[0] != '-')
            {
                if (!char.IsDigit(tokenBuffer[0]))
                    return false;
            }
            else
            {
                if (bufferLength < 2 || !char.IsDigit(tokenBuffer[1]))
                    return false;
            }

            bool foundPeriod = false;
            for (int i = 1; i < bufferLength; i++)
            {
                if (!char.IsDigit(tokenBuffer[i]))
                {
                    if (tokenBuffer[i] == '.')
                    {
                        if (foundPeriod)
                            return false;
                        else
                            foundPeriod = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // can't end with a '.'
            if (!allowPartial && tokenBuffer[bufferLength - 1] == '.')
                return false;

            return true;
        }

        private bool representsAtLeastOneValidToken(char[] tokenBuffer, int bufferLength)
        {
            int tokenIndex = _normalTokens.FindIndex(t => startswith(t.Text, tokenBuffer, bufferLength));
            if (tokenIndex >= 0)
                return true;

            // maybe its an identifier?
            return isValidIdentifier(tokenBuffer, bufferLength, false) ||
                isValidIdentifier(tokenBuffer, bufferLength, true) ||
                isValidFloatLiteral(tokenBuffer, bufferLength, true) ||
                isValidIntegerLiteral(tokenBuffer, bufferLength) ||
                isValidStringLiteral(tokenBuffer, bufferLength, true);
        }

        private ScannedToken getToken(char[] tokenBuffer, int bufferLength)
        {
            int tokenLen = bufferLength;

            int tokenIndex = _normalTokens.FindIndex(t => compare(t.Text, tokenBuffer, bufferLength));
            if (tokenIndex >= 0)
                return new ScannedToken(_normalTokens[tokenIndex].Type, new BetterString(_text, _position.Position - tokenLen, tokenLen), _position.Line);

            // check the regexs
            for (int i = 0; i < _regexTokens.Count; i++)
            {
                if (isValidIdentifier(tokenBuffer, bufferLength, true))
                    return new ScannedToken(SheepTokenType.LocalIdentifier, new BetterString(_text, _position.Position - tokenLen, tokenLen), _position.Line);
                else if (isValidIdentifier(tokenBuffer, bufferLength, false))
                    return new ScannedToken(SheepTokenType.Identifier, new BetterString(_text, _position.Position - tokenLen, tokenLen), _position.Line);
                else if (isValidIntegerLiteral(tokenBuffer, bufferLength))
                    return new ScannedToken(SheepTokenType.LiteralInteger, new BetterString(_text, _position.Position - tokenLen, tokenLen), _position.Line);
                else if (isValidFloatLiteral(tokenBuffer, bufferLength, false))
                    return new ScannedToken(SheepTokenType.LiteralFloat, new BetterString(_text, _position.Position - tokenLen, tokenLen), _position.Line);
                else if (isValidStringLiteral(tokenBuffer, bufferLength, false))
                    return new ScannedToken(SheepTokenType.LiteralString, new BetterString(_text, _position.Position - tokenLen, tokenLen), _position.Line);
            }

            return new ScannedToken(SheepTokenType.None, BetterString.NullString, _position.Line);
        }

        private void createTokenMap()
        {
            addToTokens(SheepTokenType.Comma, false, ",");
            addToTokens(SheepTokenType.Period, false, ".");
            addToTokens(SheepTokenType.Semicolon, false, ";");
            addToTokens(SheepTokenType.LBracket, false, "[");
            addToTokens(SheepTokenType.RBracket, false, "]");
            addToTokens(SheepTokenType.LBrace, false, "{");
            addToTokens(SheepTokenType.RBrace, false, "}");
            addToTokens(SheepTokenType.LParen, false, "(");
            addToTokens(SheepTokenType.RParen, false, ")");

            addToTokens(SheepTokenType.PlusEq, false, "+=");
            addToTokens(SheepTokenType.MinusEq, false, "-=");
            addToTokens(SheepTokenType.StarEq, false, "*=");
            addToTokens(SheepTokenType.DivEq, false, "/=");
            addToTokens(SheepTokenType.EqEq, false, "==");
            addToTokens(SheepTokenType.NotEq, false, "!=");
            addToTokens(SheepTokenType.LEq, false, "<=");
            addToTokens(SheepTokenType.GEq, false, ">=");
            addToTokens(SheepTokenType.AndAnd, false, "&&");
            addToTokens(SheepTokenType.OrOr, false, "||");
            addToTokens(SheepTokenType.PlusPlus, false, "++");
            addToTokens(SheepTokenType.MinusMinus, false, "--");

            addToTokens(SheepTokenType.Plus, false, "+");
            addToTokens(SheepTokenType.Minus, false, "-");
            addToTokens(SheepTokenType.Star, false, "*");
            addToTokens(SheepTokenType.Slash, false, "/");
            addToTokens(SheepTokenType.Lt, false, "<");
            addToTokens(SheepTokenType.Gt, false, ">");
            addToTokens(SheepTokenType.Equal, false, "=");

            addToTokens(SheepTokenType.Int, false, "int");
            addToTokens(SheepTokenType.Float, false, "float");
            addToTokens(SheepTokenType.String, false, "string");

            addToTokens(SheepTokenType.Symbols, false, "symbols");
            addToTokens(SheepTokenType.Code, false, "code");

            addToTokens(SheepTokenType.LiteralInteger, true, "^[0-9]+$");
            addToTokens(SheepTokenType.Identifier, true, "^([A-Za-z]|_)([A-Za-z0-9_])*$");
        }

        private void addToTokens(SheepTokenType type, bool isRegex, string text)
        {
            SheepToken token;
            token.Type = type;
            token.IsRegex = isRegex;
            token.Text = text;

            if (isRegex)
            {
                token.Regex = new Regex(text);
                _regexTokens.Add(token);
            }
            else
            {
                token.Regex = null;
                _normalTokens.Add(token);
            }
        }

        private static bool compare(string s, char[] buffer, int bufferLength)
        {
            if (s.Length != bufferLength)
                return false;

            for (int i = 0; i < bufferLength; i++)
                if (char.ToLowerInvariant(s[i]) != char.ToLowerInvariant(buffer[i]))
                    return false;

            return true;
        }

        private static bool startswith(string s, char[] buffer, int bufferLength)
        {
            if (s.Length < bufferLength)
                return false;

            for (int i = 0; i < bufferLength; i++)
            {
                if (char.ToLowerInvariant(s[i]) != char.ToLowerInvariant(buffer[i]))
                    return false;
            }

            return true;
        }
    }
}

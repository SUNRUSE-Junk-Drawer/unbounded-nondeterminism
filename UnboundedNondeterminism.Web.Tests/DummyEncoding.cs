using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnboundedNondeterminism.Web.Tests
{
    public sealed class DummyEncoding : Encoding
    {
        public override string BodyName => throw new NotImplementedException();
        public override int CodePage => throw new NotImplementedException();
        public override string EncodingName => throw new NotImplementedException();
        public override string HeaderName => throw new NotImplementedException();
        public override bool IsBrowserDisplay => throw new NotImplementedException();
        public override bool IsBrowserSave => throw new NotImplementedException();
        public override bool IsMailNewsDisplay => throw new NotImplementedException();
        public override bool IsMailNewsSave => throw new NotImplementedException();
        public override bool IsSingleByte => throw new NotImplementedException();
        public override string WebName => throw new NotImplementedException();
        public override int WindowsCodePage => throw new NotImplementedException();

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override int GetByteCount(char[] chars)
        {
            throw new NotImplementedException();
        }

        public override int GetByteCount(string s)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetBytes(char[] chars)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetBytes(char[] chars, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetBytes(string s)
        {
            throw new NotImplementedException();
        }

        public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetCharCount(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override char[] GetChars(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override char[] GetChars(byte[] bytes, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override Decoder GetDecoder()
        {
            throw new NotImplementedException();
        }

        public override Encoder GetEncoder()
        {
            throw new NotImplementedException();
        }

        public override byte[] GetPreamble()
        {
            throw new NotImplementedException();
        }

        public override string GetString(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override string GetString(byte[] bytes, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override bool IsAlwaysNormalized(NormalizationForm form)
        {
            throw new NotImplementedException();
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetMaxByteCount(int charCount)
        {
            throw new NotImplementedException();
        }

        public override int GetMaxCharCount(int byteCount)
        {
            throw new NotImplementedException();
        }
    }
}

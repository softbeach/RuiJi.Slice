﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using FbxSharp;

namespace FbxSharpTests
{
    [TestFixture]
    public class TokenizerTest
    {
        [Test]
        public void TestWhitespace()
        {
            // given
            const string input = "    ";
            var tokenizer = new Tokenizer(input, false);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Whitespace, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestComment()
        {
            // given
            const string input = "; comment\n";
            var tokenizer = new Tokenizer(input, false);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Comment, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestStar()
        {
            // given
            const string input = "*";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Star, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestOpenBrace()
        {
            // given
            const string input = "{";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.OpenBrace, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestCloseBrace()
        {
            // given
            const string input = "}";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.CloseBrace, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestColon()
        {
            // given
            const string input = ":";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Colon, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestComma()
        {
            // given
            const string input = ",";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Comma, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestString()
        {
            // given
            const string input = "\"this is a ; string \n\"";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.String, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestName()
        {
            // given
            const string input = "asdf";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Name, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestNumber()
        {
            // given
            const string input = "123.45e67";
            var tokenizer = new Tokenizer(input);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Number, input), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestNameInWhitespace()
        {
            // given
            const string input = "   abcd   ";
            var tokenizer = new Tokenizer(input, false);

            // when
            var token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Whitespace, "   "), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Name, "abcd"), token);


            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.AreEqual(new Token(TokenType.Whitespace, "   "), token);

            // when
            token = tokenizer.GetNextToken();

            // then
            Assert.IsNull(token);
        }

        [Test]
        public void TestEnumerateTokens()
        {
            // given
            const string input = "***";
            var token = new Token(TokenType.Star, "*");
            var tokenizer = new Tokenizer(input);

            // when
            var list = tokenizer.EnumerateTokens().ToList<Token>();

            // then
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(token, list[0]);
            Assert.AreEqual(token, list[1]);
            Assert.AreEqual(token, list[2]);
        }

        [Test]
        public void TestIgnoreWhitespace()
        {
            // given
            const string input = " name 123 \"one two three\" ";
            var tokenizer = new Tokenizer(input);

            // when
            var list = tokenizer.EnumerateTokens().ToList<Token>();

            // then
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Name, "name"), list[0]);
            Assert.AreEqual(new Token(TokenType.Number, "123"), list[1]);
            Assert.AreEqual(new Token(TokenType.String, "\"one two three\""), list[2]);
        }

        [Test]
        public void TestBlock()
        {
            const string input =
                "FBXHeaderExtension:  {\n" +
                "    ; This is a comment \n" +
                "    FBXHeaderVersion: 1003\n" +
                "    FBXVersion: 6100\n" +
                "    Creator: \"FBX SDK/FBX Plugins version 2010.2\"\n" +
                "    ; This is another comment \n" +
                "}\n";
            var tokens = new Token[] {
                new Token(TokenType.Name, "FBXHeaderExtension"),
                new Token(TokenType.Colon, ":"),
                new Token(TokenType.OpenBrace, "{"),
                new Token(TokenType.Name, "FBXHeaderVersion"),
                new Token(TokenType.Colon, ":"),
                new Token(TokenType.Number, "1003"),
                new Token(TokenType.Name, "FBXVersion"),
                new Token(TokenType.Colon, ":"),
                new Token(TokenType.Number, "6100"),
                new Token(TokenType.Name, "Creator"),
                new Token(TokenType.Colon, ":"),
                new Token(TokenType.String, "\"FBX SDK/FBX Plugins version 2010.2\""),
                new Token(TokenType.CloseBrace, "}"),
            };
            var tokenizer = new Tokenizer(input);

            // when
            var list = tokenizer.EnumerateTokens().ToList<Token>();

            // then
            CollectionAssert.AreEqual(tokens, list);
        }
    }
}

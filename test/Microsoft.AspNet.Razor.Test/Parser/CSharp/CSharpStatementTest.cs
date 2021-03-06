// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Chunks.Generators;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Test.Framework;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Parser.CSharp
{
    // Basic Tests for C# Statements:
    //  * Basic case for each statement
    //  * Basic case for ALL clauses

    // This class DOES NOT contain
    //  * Error cases
    //  * Tests for various types of nested statements
    //  * Comment tests

    public class CSharpStatementTest : CsHtmlCodeParserTestBase
    {
        [Fact]
        public void ForStatement()
        {
            ParseBlockTest("@for(int i = 0; i++; i < length) { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("for(int i = 0; i++; i < length) { foo(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ForEachStatement()
        {
            ParseBlockTest("@foreach(var foo in bar) { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("foreach(var foo in bar) { foo(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void WhileStatement()
        {
            ParseBlockTest("@while(true) { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("while(true) { foo(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void SwitchStatement()
        {
            ParseBlockTest("@switch(foo) { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("switch(foo) { foo(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void LockStatement()
        {
            ParseBlockTest("@lock(baz) { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("lock(baz) { foo(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void IfStatement()
        {
            ParseBlockTest("@if(true) { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("if(true) { foo(); }")
                                   .AsStatement()
                               ));
        }

        [Fact]
        public void ElseIfClause()
        {
            ParseBlockTest("@if(true) { foo(); } else if(false) { foo(); } else if(!false) { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("if(true) { foo(); } else if(false) { foo(); } else if(!false) { foo(); }")
                                   .AsStatement()
                               ));
        }

        [Fact]
        public void ElseClause()
        {
            ParseBlockTest("@if(true) { foo(); } else { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("if(true) { foo(); } else { foo(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void TryStatement()
        {
            ParseBlockTest("@try { foo(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("try { foo(); }")
                                   .AsStatement()
                               ));
        }

        [Fact]
        public void CatchClause()
        {
            ParseBlockTest("@try { foo(); } catch(IOException ioex) { handleIO(); } catch(Exception ex) { handleOther(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("try { foo(); } catch(IOException ioex) { handleIO(); } catch(Exception ex) { handleOther(); }")
                                   .AsStatement()
                               ));
        }

        [Fact]
        public void FinallyClause()
        {
            ParseBlockTest("@try { foo(); } finally { Dispose(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("try { foo(); } finally { Dispose(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        public static TheoryData StaticUsingData
        {
            get
            {
                var factory = SpanFactory.CreateCsHtml();
                Func<string, string, DirectiveBlock> createUsing = (code, import) =>
                    new DirectiveBlock(
                        factory.CodeTransition(),
                        factory.Code(code)
                            .AsNamespaceImport(import)
                            .Accepts(AcceptedCharacters.AnyExceptNewline));

                // document, expectedResult
                return new TheoryData<string, DirectiveBlock>
                {
                    { "@using static", createUsing("using static", " static") },
                    { "@using static    ", createUsing("using static    ", " static    ") },
                    { "@using         static    ", createUsing("using         static    ", "         static    ") },
                    { "@using static System", createUsing("using static System", " static System") },
                    {
                        "@using static         System",
                        createUsing("using static         System", " static         System")
                    },
                    {
                        "@using static System.Console",
                        createUsing("using static System.Console", " static System.Console")
                    },
                    {
                        "@using static global::System.Console",
                        createUsing("using static global::System.Console", " static global::System.Console")
                    },
                    {
                        "@using   static   global::System.Console  ",
                        createUsing("using   static   global::System.Console", "   static   global::System.Console")
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(StaticUsingData))]
        public void StaticUsingImport(string document, DirectiveBlock expectedResult)
        {
            // Act & Assert
            ParseBlockTest(document, expectedResult);
        }

        [Fact]
        public void UsingStatement()
        {
            ParseBlockTest("@using(var foo = new Foo()) { foo.Bar(); }",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("using(var foo = new Foo()) { foo.Bar(); }")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void UsingTypeAlias()
        {
            ParseBlockTest("@using StringDictionary = System.Collections.Generic.Dictionary<string, string>",
                           new DirectiveBlock(
                               Factory.CodeTransition(),
                               Factory.Code("using StringDictionary = System.Collections.Generic.Dictionary<string, string>")
                                   .AsNamespaceImport(" StringDictionary = System.Collections.Generic.Dictionary<string, string>")
                                   .Accepts(AcceptedCharacters.AnyExceptNewline)
                               ));
        }

        [Fact]
        public void UsingNamespaceImport()
        {
            ParseBlockTest("@using System.Text.Encoding.ASCIIEncoding",
                           new DirectiveBlock(
                               Factory.CodeTransition(),
                               Factory.Code("using System.Text.Encoding.ASCIIEncoding")
                                   .AsNamespaceImport(" System.Text.Encoding.ASCIIEncoding")
                                   .Accepts(AcceptedCharacters.AnyExceptNewline)
                               ));
        }

        [Fact]
        public void DoStatement()
        {
            ParseBlockTest("@do { foo(); } while(true);",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.Code("do { foo(); } while(true);")
                                   .AsStatement()
                                   .Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void NonBlockKeywordTreatedAsImplicitExpression()
        {
            ParseBlockTest("@is foo",
                           new ExpressionBlock(new ExpressionChunkGenerator(),
                                               Factory.CodeTransition(),
                                               Factory.Code("is")
                                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                                   .Accepts(AcceptedCharacters.NonWhiteSpace)
                               ));
        }
    }
}

﻿using System;
using System.IO;
using SimpleScanner;
using SimpleParser;
using SimpleLang.Visitors;
using SimpleLang;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SimpleCompiler
{
    public class SimpleCompilerMain
    {
        public static void Main()
        {
            string FileName = @"../../a.txt";
            try
            {
                string Text = File.ReadAllText(FileName);

                Scanner scanner = new Scanner();
                scanner.SetSource(Text, 0);

                /*Console.WriteLine(" \nDefUseSet");
                var livev = new LiveVariableAnalysis();
                livev.FillDefUse();
                Console.WriteLine(livev.ToString());
                */


                Parser parser = new Parser(scanner);

                var b = parser.Parse();
                if (!b) Console.WriteLine("Error");
                else
                {
                    Console.WriteLine("Syntax tree built");

                    var fillParents = new FillParentsVisitor();
                    parser.root.Visit(fillParents);

                    var pp = new PrettyPrintVisitor();
                    parser.root.Visit(pp);
                    Console.WriteLine(pp.Text);

                    ASTOptimizer.Optimize(parser);
                    Console.WriteLine("\n\nAfter AST optimizations");
                    pp = new PrettyPrintVisitor();
                    parser.root.Visit(pp);
                    Console.WriteLine(pp.Text);

                    Console.WriteLine("\n\nThree address code");
                    var threeAddrCodeVisitor = new ThreeAddrGenVisitor();
                    parser.root.Visit(threeAddrCodeVisitor);
                    var threeAddressCode = threeAddrCodeVisitor.Instructions;
                    foreach (var instruction in threeAddressCode)
                        Console.WriteLine(instruction);


                    Console.WriteLine("\n\nOptimized three address code");
                    var optResult = ThreeAddressCodeOptimizer.OptimizeAll(threeAddressCode);
                    foreach (var x in optResult)
                        Console.WriteLine(x);

                    Console.WriteLine("\n\nDivided three address code");
                    var divResult = BasicBlockLeader.DivideLeaderToLeader(optResult);

                    foreach (var x in divResult)
                    {
                        foreach (var y in x.GetInstructions())
                            Console.WriteLine(y);
                        Console.WriteLine("--------------");
                    }


                    var cfg = new ControlFlowGraph(divResult);

                    foreach(var block in cfg.GetCurrentBasicBlocks())
                    {
                        Console.WriteLine($"{cfg.VertexOf(block)}  {block.GetInstructions()[0]}");
                        var children = cfg.GetChildrenBasicBlocks(cfg.VertexOf(block));
                        var childrenStr = String.Join(" | ", children.Select(v => v.Item2.GetInstructions()[0].ToString()));
                        Console.WriteLine($" children: {childrenStr}");

                        var parents = cfg.GetParentsBasicBlocks(cfg.VertexOf(block));
                        var parentsStr = String.Join(" | ", parents.Select(v => v.Item2.GetInstructions()[0].ToString()));
                        Console.WriteLine($" parents: {parentsStr}");
                    }


                    var a = new OptimizedGenericIterativeAlgorithm();
                    var tmp = new ReachingDefinitions();
                    var res = a.Analyze(cfg, tmp.Execute(cfg), new ReachingTransferFunc(cfg));

                    Console.WriteLine();
                    Console.WriteLine();

                    foreach (var Superres in res)
                    {
                        Console.WriteLine("BasicBloc---------------------");
                        foreach (var y1 in Superres.Key.GetInstructions())
                            Console.WriteLine(y1);
                        Console.WriteLine("MyOut---------------------");
                        foreach (var y2 in Superres.Value)
                            Console.WriteLine(y2);
                    }

                    Console.WriteLine(" \nDone");
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File {0} not found", FileName);
            }
            catch (LexException e)
            {
                Console.WriteLine("Lex Error. " + e.Message);
            }
            catch (SyntaxException e)
            {
                Console.WriteLine("Syntax Error. " + e.Message);
            }
            Console.ReadLine();
        }
    }
}
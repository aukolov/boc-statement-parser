﻿using System;
using System.IO;
using System.Linq;
using BankStatementParser.Banks;
using BankStatementParser.Banks.Hellenic;
using CommandLine;

namespace BankStatementParser
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bank Statement Parser");
            Console.WriteLine();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    var fileProcessor = CreateFileProcessor(o.Bank);
                    var statements = fileProcessor.Process(o.Path);

                    var transactionSerializer = new TransactionSerializer();
                    foreach (var g in statements.GroupBy(x => x.AccountNumber))
                    {
                        var accountNumber = g.Key;
                        PrintInColor($"Processing account {accountNumber}...", ConsoleColor.Green);
                        var accountStatements = g.OrderBy(s => s.FromDate).ToArray();

                        foreach (var error in StatementContinuityValidator.GetErrors(accountStatements))
                        {
                            PrintInColor(error, ConsoleColor.Red);
                        }

                        foreach (var grouping in accountStatements.GroupBy(x => x.AccountNumber))
                        {
                            var transactions = grouping
                                .SelectMany(s => s.Transactions).ToArray();
                            var serializedTransactions = transactionSerializer.Serialize(
                                transactions);
                            var fileName = File.Exists(o.Path)
                                ? $"{Path.GetFileNameWithoutExtension(o.Path)}_{grouping.Key}.csv"
                                : $"statement_{accountNumber}_gen{DateTime.Now:yyyyMMdd-HHmmss}.csv";

                            var transactionsWord = transactions.Length != 1 ? "transactions" : "transaction";
                            Console.WriteLine(
                                $"Writing {transactions.Length} {transactionsWord} to {Path.GetFullPath(fileName)}...");
                            File.WriteAllText(fileName, serializedTransactions);
                        }
                    }

                    Console.WriteLine();
                    PrintInColor("Done! Press <Enter> to exit.", ConsoleColor.Green);
                    Console.ReadLine();
                });
        }

        private static IFileProcessor CreateFileProcessor(Bank bank)
        {
            switch (bank)
            {
                case Bank.BoC:
                    return new BocFileProcessor();
                case Bank.Eurobank:
                    return new EurobankFileProcessor();
                case Bank.Eurobank2:
                    return new Eurobank2FileProcessor();
                case Bank.Eurobank3:
                    return new Eurobank3FileProcessor();
                case Bank.Revolut:
                    return new RevolutFileProcessor();
                case Bank.Hellenic:
                    return new HellenicFileProcessor();
                case Bank.Unlimint:
                    return new UnlimintFileProcessor();
                case Bank.Fibank:
                    return new FibankFileProcessor();
                default:
                    throw new ArgumentOutOfRangeException(nameof(bank), bank, null);
            }
        }

        private static void PrintInColor(string text, ConsoleColor foregroundColor)
        {
            var oldColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(text);
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
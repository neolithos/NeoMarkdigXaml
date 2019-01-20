#region -- copyright --
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
#endregion
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Markdig;
using Neo.Markdig.Xaml;

namespace MarkdownCompare
{
	class Program
	{
		static MarkdownPipeline pipeLine;
		static string content;

		static void RunMeasure(string title, Action<string, MarkdownPipeline> action, int count = 100)
		{
			var sw = Stopwatch.StartNew();
			for (var i = 0; i < count; i++)
				action(content, pipeLine);

			Console.WriteLine("{0,-20}: {1:N0}ms", title, sw.ElapsedMilliseconds);

			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
		} // proc RunMeasure

		[STAThread]
		static void Main(string[] args)
		{
			content = File.ReadAllText("Markdig-readme.md");
			pipeLine = new MarkdownPipelineBuilder()
				.UseXamlSupportedExtensions()
				.Build();

			// warm up
			MarkdigWpf.RunXaml(content, pipeLine);
			MarkdigWpf.RunWpf(content, pipeLine);
			MarkdigXaml.RunXaml(content, pipeLine);
			MarkdigXaml.RunWpf(content, pipeLine);
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

			// run tests
			RunMeasure("Markdig.Wpf-toxaml", MarkdigWpf.RunXaml);
			Thread.Sleep(1000);
			RunMeasure("Markdig.Xaml-toxaml", MarkdigXaml.RunXaml);
			Thread.Sleep(1000);

			RunMeasure("Markdig.Wpf-towpf", MarkdigWpf.RunWpf);
			Thread.Sleep(1000);
			RunMeasure("Markdig.Xaml-towpf", MarkdigXaml.RunWpf);
			Thread.Sleep(1000);

			Console.ReadLine();
		}
	}
}

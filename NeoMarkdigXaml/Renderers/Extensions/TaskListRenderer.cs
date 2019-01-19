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
using Markdig.Extensions.TaskLists;

namespace Neo.Markdig.Xaml.Renderers.Extensions
{
	/// <summary> </summary>
	public class TaskListRenderer : XamlObjectRenderer<TaskList>
    {
        /// <summary></summary>
        /// <param name="renderer"></param>
        /// <param name="taskList"></param>
        protected override void Write(XamlMarkdownWriter renderer, TaskList taskList)
        {
            //var checkBox = new CheckBox
            //{
            //    IsEnabled = false,
            //    IsChecked = taskList.Checked,
            //};

            //checkBox.SetResourceReference(FrameworkContentElement.StyleProperty, Styles.TaskListStyleKey);
            //renderer.WriteInline(new InlineUIContainer(checkBox));
        } // proc Write
	} // class TaskListRenderer
}
